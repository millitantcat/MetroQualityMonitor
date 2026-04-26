"""
Прогнозирование пассажиропотока.

Стратегия:
- Линейный уровень, SARIMA(1,1,1)(1,1,1,4) — когда ≥ 12 точек.
- Seasonal Naive baseline — при нехватке данных или ошибке оптимизации.
- Walk-forward validation (hold-out 4 квартала) для расчёта MAPE/RMSE.
"""

import logging
import uuid
import warnings
from datetime import datetime, timezone

import numpy as np
import pandas as pd
from sqlalchemy.orm import Session
from statsmodels.tsa.statespace.sarimax import SARIMAX
from statsmodels.tools.sm_exceptions import ConvergenceWarning

from app.config import settings
from app.models.orm import Forecast
from app.services.data_loader import load_line_passenger_flow

logger = logging.getLogger(__name__)

_QUARTERS = ["Q1", "Q2", "Q3", "Q4"]
_QUARTER_TO_MONTH = {"Q1": 1, "Q2": 4, "Q3": 7, "Q4": 10}
_RU_QUARTER_MAP = {
    "I квартал": "Q1", "II квартал": "Q2",
    "III квартал": "Q3", "IV квартал": "Q4",
}

# Минимум точек для SARIMA (2 полных сезона + запас)
_MIN_SARIMA_POINTS = 12


def normalize_quarter(quarter: str) -> str:
    """Приводит квартал к формату Q1-Q4 (принимает как 'I квартал', так и 'Q1')."""
    return _RU_QUARTER_MAP.get(quarter, quarter)


def quarter_to_timestamp(year: int, quarter: str) -> pd.Timestamp:
    return pd.Timestamp(year=year, month=_QUARTER_TO_MONTH[normalize_quarter(quarter)], day=1)


def next_quarters(year: int, quarter: str, n: int) -> list[tuple[int, str]]:
    """Возвращает n следующих (year, quarter) пар после заданного периода."""
    q_idx = _QUARTERS.index(normalize_quarter(quarter))
    result = []
    for i in range(1, n + 1):
        offset = q_idx + i
        result.append((year + offset // 4, _QUARTERS[offset % 4]))
    return result


def _fit_sarima(ts: pd.Series, n_steps: int) -> tuple[list[int], list[int], list[int]]:
    """
    Подгоняет SARIMA(1,1,1)(1,1,1,4) и возвращает (predicted, lower_80, upper_80).
    Все значения обрезаны снизу на 0.
    """
    with warnings.catch_warnings():
        warnings.simplefilter("ignore", ConvergenceWarning)
        warnings.simplefilter("ignore", UserWarning)
        model = SARIMAX(
            ts,
            order=(1, 1, 1),
            seasonal_order=(1, 1, 1, 4),
            enforce_stationarity=False,
            enforce_invertibility=False,
        )
        fit = model.fit(disp=False, maxiter=200)

    forecast = fit.get_forecast(steps=n_steps)
    mean = forecast.predicted_mean.values
    ci   = forecast.conf_int(alpha=0.20)  # 80% CI

    predicted = [max(0, int(v)) for v in mean]
    lower     = [max(0, int(v)) for v in ci.iloc[:, 0].values]
    upper     = [max(0, int(v)) for v in ci.iloc[:, 1].values]
    return predicted, lower, upper


def _seasonal_naive(
    ts: pd.Series, n_steps: int
) -> tuple[list[int], list[int], list[int]]:
    """
    Seasonal Naive: значение = то же квартал год назад.
    Доверительный интервал ±10 %.
    """
    predicted, lower, upper = [], [], []
    n = len(ts)
    for i in range(n_steps):
        # Берём точку на 4 квартала назад относительно первого предсказываемого
        lookback = n - 4 + (i % 4)
        val = float(ts.iloc[lookback]) if 0 <= lookback < n else float(ts.mean())
        val = max(0.0, val)
        predicted.append(int(val))
        lower.append(int(val * 0.90))
        upper.append(int(val * 1.10))
    return predicted, lower, upper


def _forecast_series(
    ts: pd.Series, n_steps: int, label: str
) -> tuple[list[int], list[int], list[int]]:
    """Выбирает SARIMA или seasonal naive и возвращает прогноз."""
    if len(ts) >= _MIN_SARIMA_POINTS:
        try:
            return _fit_sarima(ts, n_steps)
        except Exception as exc:
            logger.warning("%s: SARIMA упал (%s), используем seasonal naive.", label, exc)
    else:
        logger.info("%s: мало точек (%d), используем seasonal naive.", label, len(ts))
    return _seasonal_naive(ts, n_steps)


# ---------------------------------------------------------------------------
# Batch-запуск
# ---------------------------------------------------------------------------

def run_batch_forecast(session: Session) -> dict:
    """
    Рассчитывает прогнозы для всех линий на settings.forecast_horizon кварталов
    вперёд и сохраняет результаты в таблицу Forecasts.

    Перед записью удаляет существующие будущие прогнозы по линиям
    (year >= первого будущего квартала), чтобы не накапливать дубли.
    """
    df = load_line_passenger_flow(session)
    if df.empty:
        logger.warning("Данные о пассажиропотоке не найдены; прогноз пропущен.")
        return {"saved": 0, "skipped": 0, "lines": 0}

    df["quarter"] = df["quarter"].map(normalize_quarter)
    df["date"] = df.apply(
        lambda r: quarter_to_timestamp(int(r["year"]), r["quarter"]), axis=1
    )
    df = df.sort_values(["line_id", "date"])

    # Определяем «последний факт» глобально
    latest_row   = df.loc[df["date"].idxmax()]
    last_year    = int(latest_row["year"])
    last_quarter = str(latest_row["quarter"])
    future_periods = next_quarters(last_year, last_quarter, settings.forecast_horizon)
    min_future_year = future_periods[0][0]

    # Удаляем старые прогнозы для линий за будущие периоды
    deleted = (
        session.query(Forecast)
        .filter(Forecast.line_id.isnot(None), Forecast.year >= min_future_year)
        .delete(synchronize_session=False)
    )
    logger.info("Удалено %d устаревших прогнозов по линиям.", deleted)

    now   = datetime.now(timezone.utc)
    saved = 0
    skipped = 0
    lines_processed = 0

    for line_id, group in df.groupby("line_id"):
        group     = group.set_index("date").sort_index()
        ts_in     = group["incoming"]
        ts_out    = group["outgoing"]
        label_in  = f"line={line_id} incoming"
        label_out = f"line={line_id} outgoing"

        try:
            pred_in,  lower_in,  upper_in  = _forecast_series(ts_in,  settings.forecast_horizon, label_in)
            pred_out, lower_out, upper_out = _forecast_series(ts_out, settings.forecast_horizon, label_out)
        except Exception as exc:
            logger.error("Линия %s: не удалось построить прогноз (%s), пропуск.", line_id, exc)
            skipped += 1
            continue

        for i, (year, quarter) in enumerate(future_periods):
            session.add(Forecast(
                id=uuid.uuid4(),
                line_id=int(line_id),
                station_id=None,
                year=year,
                quarter=quarter,
                predicted_incoming=pred_in[i],
                predicted_outgoing=pred_out[i],
                confidence_lower_incoming=lower_in[i],
                confidence_upper_incoming=upper_in[i],
                confidence_lower_outgoing=lower_out[i],
                confidence_upper_outgoing=upper_out[i],
                model_name=settings.model_name,
                model_version=settings.model_version,
                create_date_time_utc=now,
            ))
            saved += 1

        lines_processed += 1

    logger.info("Сохранено %d прогнозов для %d линий.", saved, lines_processed)
    return {"saved": saved, "skipped": skipped, "lines": lines_processed}


# ---------------------------------------------------------------------------
# Walk-forward validation
# ---------------------------------------------------------------------------

def compute_validation_metrics(session: Session) -> dict:
    """
    Walk-forward validation: hold-out последних 4 квартала на каждой линии.
    Возвращает средние MAPE и RMSE по всем линиям с достаточным количеством данных.
    Используется для пояснительной записки (сравнение с seasonal naive baseline).
    """
    df = load_line_passenger_flow(session)
    if df.empty:
        return {}

    df["quarter"] = df["quarter"].map(normalize_quarter)
    df["date"] = df.apply(
        lambda r: quarter_to_timestamp(int(r["year"]), r["quarter"]), axis=1
    )
    df = df.sort_values(["line_id", "date"])

    HOLD_OUT = 4
    sarima_maes, sarima_mapers, sarima_rmses   = [], [], []
    naive_maes,  naive_mapers,  naive_rmses    = [], [], []

    for line_id, group in df.groupby("line_id"):
        group  = group.set_index("date").sort_index()
        ts_in  = group["incoming"]

        # Нужно минимум HOLD_OUT + _MIN_SARIMA_POINTS точек
        if len(ts_in) <= HOLD_OUT + _MIN_SARIMA_POINTS:
            continue

        train = ts_in.iloc[:-HOLD_OUT]
        test  = ts_in.iloc[-HOLD_OUT:].values

        # SARIMA
        try:
            with warnings.catch_warnings():
                warnings.simplefilter("ignore")
                model  = SARIMAX(train, order=(1, 1, 1), seasonal_order=(1, 1, 1, 4),
                                 enforce_stationarity=False, enforce_invertibility=False)
                result = model.fit(disp=False, maxiter=200)
            sarima_pred = result.get_forecast(steps=HOLD_OUT).predicted_mean.values
        except Exception:
            sarima_pred = np.array([float(train.mean())] * HOLD_OUT)

        # Seasonal Naive baseline
        naive_pred = np.array([
            float(train.iloc[len(train) - 4 + (i % 4)]) if len(train) >= 4 else float(train.mean())
            for i in range(HOLD_OUT)
        ])

        for pred, mae_list, mape_list, rmse_list in [
            (sarima_pred, sarima_maes, sarima_mapers, sarima_rmses),
            (naive_pred,  naive_maes,  naive_mapers,  naive_rmses),
        ]:
            pred    = np.clip(pred, 0, None)
            abs_err = np.abs(test - pred)
            denom   = np.where(test > 0, test, 1.0)
            mae_list.append(float(abs_err.mean()))
            mape_list.append(float((abs_err / denom).mean() * 100))
            rmse_list.append(float(np.sqrt((abs_err ** 2).mean())))

    if not sarima_maes:
        return {"message": "Недостаточно данных для валидации."}

    def _avg(lst: list) -> float:
        return round(float(np.mean(lst)), 2)

    return {
        "lines_evaluated": len(sarima_maes),
        "hold_out_quarters": HOLD_OUT,
        "sarima": {
            "mae":      _avg(sarima_maes),
            "mape_pct": _avg(sarima_mapers),
            "rmse":     _avg(sarima_rmses),
        },
        "seasonal_naive_baseline": {
            "mae":      _avg(naive_maes),
            "mape_pct": _avg(naive_mapers),
            "rmse":     _avg(naive_rmses),
        },
    }

"""
Детекция аномалий пассажиропотока.

Три подхода:
1. Statistical (Z-score) — per-station, порог |z| ≥ 2.5.
2. YoYDeviation — отклонение того же квартала год назад, порог ≥ 30 %.
3. IsolationForest — per-record на нормализованных признаках, contamination=0.03.

Severities: Low=1, Medium=2, High=3
AnomalyTypes: Statistical=1, IsolationForest=2, YoYDeviation=3
"""

import logging
import uuid
from datetime import datetime, timezone

import numpy as np
import pandas as pd
from sklearn.ensemble import IsolationForest
from sklearn.preprocessing import StandardScaler
from sqlalchemy.orm import Session

from app.models.orm import Anomaly
from app.services.data_loader import load_station_passenger_flow

logger = logging.getLogger(__name__)

# Целочисленные значения enum, зеркалящие C# AnomalyTypes
ANOMALY_TYPE_STATISTICAL      = 1
ANOMALY_TYPE_ISOLATION_FOREST = 2
ANOMALY_TYPE_YOY              = 3

# Целочисленные значения enum, зеркалящие C# AnomalySeverities
SEVERITY_LOW    = 1
SEVERITY_MEDIUM = 2
SEVERITY_HIGH   = 3

# Пороги
_ZSCORE_MEDIUM  = 2.5
_ZSCORE_HIGH    = 3.5
_YOY_MEDIUM     = 0.30   # 30 %
_YOY_HIGH       = 0.50   # 50 %
_IF_CONTAMINATION = 0.03


def _severity_from_zscore(z: float) -> int:
    abs_z = abs(z)
    if abs_z >= _ZSCORE_HIGH:
        return SEVERITY_HIGH
    if abs_z >= _ZSCORE_MEDIUM:
        return SEVERITY_MEDIUM
    return SEVERITY_LOW


def _severity_from_yoy(deviation: float) -> int:
    abs_dev = abs(deviation)
    if abs_dev >= _YOY_HIGH:
        return SEVERITY_HIGH
    if abs_dev >= _YOY_MEDIUM:
        return SEVERITY_MEDIUM
    return SEVERITY_LOW


# ---------------------------------------------------------------------------
# Подход 1: Z-score
# ---------------------------------------------------------------------------

def detect_statistical_anomalies(df: pd.DataFrame) -> list[dict]:
    """
    Для каждой станции вычисляет Z-score по входящим пассажирам.
    Записи с |z| ≥ 2.5 помечаются как аномалии.
    """
    anomalies: list[dict] = []

    for station_id, group in df.groupby("station_id"):
        if len(group) < 4:
            continue

        mean_in = float(group["incoming"].mean())
        std_in  = float(group["incoming"].std())

        if std_in < 1.0:
            continue

        for _, row in group.iterrows():
            z = (float(row["incoming"]) - mean_in) / std_in
            if abs(z) >= _ZSCORE_MEDIUM:
                anomalies.append({
                    "station_id":    int(station_id),
                    "year":          int(row["year"]),
                    "quarter":       str(row["quarter"]),
                    "anomaly_type":  ANOMALY_TYPE_STATISTICAL,
                    "severity":      _severity_from_zscore(z),
                    "score":         round(float(z), 4),
                    "actual_value":  int(row["incoming"]),
                    "expected_value": int(mean_in),
                    "description": (
                        f"Z-score {z:+.2f}: значение отклоняется от среднего станции "
                        f"({int(row['incoming'])} vs среднее {int(mean_in)})."
                    ),
                })

    return anomalies


# ---------------------------------------------------------------------------
# Подход 2: Year-over-Year Deviation
# ---------------------------------------------------------------------------

def detect_yoy_anomalies(df: pd.DataFrame) -> list[dict]:
    """
    Сравнивает каждый квартал с тем же кварталом предыдущего года.
    Отклонение ≥ 30 % → аномалия.
    """
    anomalies: list[dict] = []
    df = df.sort_values(["station_id", "year", "quarter"])

    for station_id, group in df.groupby("station_id"):
        group = group.copy().reset_index(drop=True)

        for _, row in group.iterrows():
            prev = group[
                (group["year"]    == row["year"] - 1)
                & (group["quarter"] == row["quarter"])
            ]
            if prev.empty:
                continue

            prev_val = float(prev.iloc[0]["incoming"])
            if prev_val < 1.0:
                continue

            deviation = (float(row["incoming"]) - prev_val) / prev_val
            if abs(deviation) >= _YOY_MEDIUM:
                anomalies.append({
                    "station_id":    int(station_id),
                    "year":          int(row["year"]),
                    "quarter":       str(row["quarter"]),
                    "anomaly_type":  ANOMALY_TYPE_YOY,
                    "severity":      _severity_from_yoy(deviation),
                    "score":         round(float(deviation), 4),
                    "actual_value":  int(row["incoming"]),
                    "expected_value": int(prev_val),
                    "description": (
                        f"Отклонение год-к-году: {deviation:+.1%} "
                        f"({int(row['incoming'])} vs {int(prev_val)} "
                        f"в {row['quarter']} {int(row['year']) - 1})."
                    ),
                })

    return anomalies


# ---------------------------------------------------------------------------
# Подход 3: Isolation Forest
# ---------------------------------------------------------------------------

def detect_isolation_forest_anomalies(df: pd.DataFrame) -> list[dict]:
    """
    Isolation Forest на двух признаках per-record:
    - normalized_in: incoming / mean_incoming по станции
    - yoy_change:    YoY-изменение входящих (по аналогичному кварталу)

    contamination = 3 % от всего массива записей.
    """
    if len(df) < 20:
        return []

    df = df.copy().sort_values(["station_id", "year", "quarter"]).reset_index(drop=True)

    # Средний поток по станции
    station_means = (
        df.groupby("station_id")["incoming"].mean().rename("mean_in").reset_index()
    )
    df = df.merge(station_means, on="station_id")
    df["normalized_in"] = df["incoming"] / df["mean_in"].clip(lower=1.0)

    # YoY-изменение (по тому же кварталу)
    df["yoy_change"] = (
        df.groupby(["station_id", "quarter"])["incoming"]
        .pct_change(fill_method=None)
        .fillna(0)
        .clip(-2.0, 2.0)
    )

    features_raw = df[["normalized_in", "yoy_change"]].values
    scaler        = StandardScaler()
    features      = scaler.fit_transform(features_raw)

    iso        = IsolationForest(contamination=_IF_CONTAMINATION, random_state=42, n_estimators=100)
    labels     = iso.fit_predict(features)       # -1 = аномалия
    raw_scores = iso.score_samples(features)     # ниже = аномальнее

    # Нормализация raw_score → [0, 1] (1 = самая аномальная)
    s_min, s_max = raw_scores.min(), raw_scores.max()
    norm_scores  = (s_max - raw_scores) / (s_max - s_min + 1e-9)

    anomalies: list[dict] = []
    for idx, (label, norm_score) in enumerate(zip(labels, norm_scores)):
        if label != -1:
            continue

        row = df.iloc[idx]
        severity = (
            SEVERITY_HIGH   if norm_score > 0.70 else
            SEVERITY_MEDIUM if norm_score > 0.40 else
            SEVERITY_LOW
        )

        anomalies.append({
            "station_id":    int(row["station_id"]),
            "year":          int(row["year"]),
            "quarter":       str(row["quarter"]),
            "anomaly_type":  ANOMALY_TYPE_ISOLATION_FOREST,
            "severity":      severity,
            "score":         round(float(norm_score), 4),
            "actual_value":  int(row["incoming"]),
            "expected_value": int(row["mean_in"]),
            "description":   f"Isolation Forest: аномальный паттерн потока (score={norm_score:.2f}).",
        })

    return anomalies


# ---------------------------------------------------------------------------
# Batch-запуск
# ---------------------------------------------------------------------------

def run_anomaly_detection(session: Session) -> dict:
    """
    Запускает все три детектора и сохраняет результаты в таблицу Anomalies.

    Перед записью удаляет все неподтверждённые аномалии (is_acknowledged=False),
    чтобы результат отражал текущее состояние данных.
    Подтверждённые аномалии (is_acknowledged=True) не трогаются.
    """
    df = load_station_passenger_flow(session)
    if df.empty:
        logger.warning("Данные о пассажиропотоке не найдены; детекция аномалий пропущена.")
        return {"saved": 0, "statistical": 0, "yoy": 0, "isolation_forest": 0}

    # Удаляем неподтверждённые аномалии
    deleted = (
        session.query(Anomaly)
        .filter(Anomaly.is_acknowledged == False)   # noqa: E712
        .delete(synchronize_session=False)
    )
    logger.info("Удалено %d неподтверждённых аномалий.", deleted)

    statistical = detect_statistical_anomalies(df)
    yoy         = detect_yoy_anomalies(df)
    if_anom     = detect_isolation_forest_anomalies(df)

    # Дедупликация по ключу (station_id, year, quarter, anomaly_type)
    seen: set[tuple] = set()
    now = datetime.now(timezone.utc)
    saved = 0

    for a in statistical + yoy + if_anom:
        key = (a["station_id"], a["year"], a["quarter"], a["anomaly_type"])
        if key in seen:
            continue
        seen.add(key)

        session.add(Anomaly(
            id=uuid.uuid4(),
            station_id=a["station_id"],
            year=a["year"],
            quarter=a["quarter"],
            anomaly_type=a["anomaly_type"],
            severity=a["severity"],
            score=a["score"],
            actual_value=a["actual_value"],
            expected_value=a.get("expected_value"),
            description=a.get("description"),
            is_acknowledged=False,
            acknowledged_date_time_utc=None,
            create_date_time_utc=now,
        ))
        saved += 1

    logger.info(
        "Сохранено %d аномалий (stat=%d, yoy=%d, if=%d).",
        saved, len(statistical), len(yoy), len(if_anom),
    )
    return {
        "saved": saved,
        "statistical": len(statistical),
        "yoy": len(yoy),
        "isolation_forest": len(if_anom),
    }

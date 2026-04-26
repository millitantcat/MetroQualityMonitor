import pandas as pd
import pytest

from app.services.anomaly_detector import (
    ANOMALY_TYPE_STATISTICAL,
    ANOMALY_TYPE_YOY,
    SEVERITY_HIGH,
    SEVERITY_LOW,
    SEVERITY_MEDIUM,
    _severity_from_yoy,
    _severity_from_zscore,
    detect_statistical_anomalies,
    detect_yoy_anomalies,
)


def _make_df(station_id: int, values_by_period: list[tuple[int, str, float]]) -> pd.DataFrame:
    """Вспомогательная фабрика DataFrame для тестов."""
    rows = [
        {"station_id": station_id, "year": y, "quarter": q, "incoming": v, "outgoing": v * 0.9}
        for y, q, v in values_by_period
    ]
    return pd.DataFrame(rows)


class TestSeverityFromZscore:
    def test_high(self):
        assert _severity_from_zscore(3.5) == SEVERITY_HIGH
        assert _severity_from_zscore(-4.0) == SEVERITY_HIGH

    def test_medium(self):
        assert _severity_from_zscore(2.5) == SEVERITY_MEDIUM
        assert _severity_from_zscore(-3.0) == SEVERITY_MEDIUM

    def test_low(self):
        assert _severity_from_zscore(1.0) == SEVERITY_LOW
        assert _severity_from_zscore(-2.0) == SEVERITY_LOW


class TestSeverityFromYoy:
    def test_high(self):
        assert _severity_from_yoy(0.6) == SEVERITY_HIGH
        assert _severity_from_yoy(-0.55) == SEVERITY_HIGH

    def test_medium(self):
        assert _severity_from_yoy(0.35) == SEVERITY_MEDIUM
        assert _severity_from_yoy(-0.40) == SEVERITY_MEDIUM

    def test_low(self):
        assert _severity_from_yoy(0.1) == SEVERITY_LOW


class TestStatisticalAnomalies:
    def test_detects_clear_outlier(self):
        # 5 лет стабильного ряда ~1000 + один явный выброс 5000.
        # Нужно достаточно точек, чтобы выброс не слишком сильно
        # завышал среднее (masking effect) — иначе Z-score не пробивает порог 2.5.
        periods = [
            (2019, "Q1", 1000), (2019, "Q2", 1050), (2019, "Q3", 980), (2019, "Q4", 1020),
            (2020, "Q1", 1030), (2020, "Q2", 990),  (2020, "Q3", 1010),(2020, "Q4", 1000),
            (2021, "Q1", 1000), (2021, "Q2", 1040), (2021, "Q3", 990), (2021, "Q4", 1010),
            (2022, "Q1", 1020), (2022, "Q2", 980),  (2022, "Q3", 1000),(2022, "Q4", 1030),
            (2023, "Q1", 990),  (2023, "Q2", 1010), (2023, "Q3", 1000),(2023, "Q4", 5000),
        ]
        df        = _make_df(1, periods)
        anomalies = detect_statistical_anomalies(df)

        assert len(anomalies) >= 1
        outlier = next(a for a in anomalies if a["year"] == 2023 and a["quarter"] == "Q4")
        assert outlier["anomaly_type"] == ANOMALY_TYPE_STATISTICAL
        assert outlier["station_id"] == 1

    def test_stable_series_no_anomalies(self):
        periods = [
            (2022, "Q1", 100), (2022, "Q2", 102), (2022, "Q3", 98),  (2022, "Q4", 101),
            (2023, "Q1", 103), (2023, "Q2", 99),  (2023, "Q3", 100), (2023, "Q4", 104),
        ]
        df = _make_df(1, periods)
        assert detect_statistical_anomalies(df) == []

    def test_skips_station_with_few_points(self):
        periods = [(2023, "Q1", 100), (2023, "Q2", 500)]  # < 4 точек
        df      = _make_df(1, periods)
        assert detect_statistical_anomalies(df) == []

    def test_skips_station_with_zero_std(self):
        periods = [(2022, "Q" + str(i + 1), 100) for i in range(4)]
        periods += [(2023, "Q" + str(i + 1), 100) for i in range(4)]
        df = _make_df(1, periods)
        assert detect_statistical_anomalies(df) == []


class TestYoyAnomalies:
    def test_detects_large_yoy_drop(self):
        periods = [
            (2022, "Q1", 1000), (2022, "Q2", 1100), (2022, "Q3", 950), (2022, "Q4", 1050),
            (2023, "Q1", 1000), (2023, "Q2", 1100), (2023, "Q3", 950), (2023, "Q4", 400),
        ]
        df        = _make_df(1, periods)
        anomalies = detect_yoy_anomalies(df)

        q4_2023 = [a for a in anomalies if a["year"] == 2023 and a["quarter"] == "Q4"]
        assert len(q4_2023) == 1
        assert q4_2023[0]["anomaly_type"] == ANOMALY_TYPE_YOY
        assert q4_2023[0]["severity"] == SEVERITY_HIGH  # (1050−400)/1050 ≈ 62 % > 50 %

    def test_small_yoy_change_no_anomaly(self):
        periods = [
            (2022, "Q1", 1000), (2022, "Q2", 1100), (2022, "Q3", 950), (2022, "Q4", 1050),
            (2023, "Q1", 1010), (2023, "Q2", 1090), (2023, "Q3", 960), (2023, "Q4", 1070),
        ]
        df = _make_df(1, periods)
        assert detect_yoy_anomalies(df) == []

    def test_no_prior_year_no_anomaly(self):
        # Только один год данных — YoY сравнивать не с чем
        periods = [(2023, "Q1", 1000), (2023, "Q2", 100)]
        df = _make_df(1, periods)
        assert detect_yoy_anomalies(df) == []

    def test_yoy_growth_anomaly(self):
        periods = [
            (2022, "Q1", 1000), (2022, "Q2", 1000), (2022, "Q3", 1000), (2022, "Q4", 1000),
            (2023, "Q1", 2000), (2023, "Q2", 1000), (2023, "Q3", 1000), (2023, "Q4", 1000),
        ]
        df        = _make_df(1, periods)
        anomalies = detect_yoy_anomalies(df)
        q1_2023   = [a for a in anomalies if a["year"] == 2023 and a["quarter"] == "Q1"]
        assert len(q1_2023) == 1
        assert q1_2023[0]["severity"] == SEVERITY_HIGH  # 100 % рост

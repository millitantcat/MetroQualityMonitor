import pandas as pd
import pytest

from app.services.forecaster import (
    _seasonal_naive,
    next_quarters,
    quarter_to_timestamp,
)


class TestQuarterToTimestamp:
    def test_q1(self):
        assert quarter_to_timestamp(2024, "Q1") == pd.Timestamp("2024-01-01")

    def test_q2(self):
        assert quarter_to_timestamp(2023, "Q2") == pd.Timestamp("2023-04-01")

    def test_q3(self):
        assert quarter_to_timestamp(2022, "Q3") == pd.Timestamp("2022-07-01")

    def test_q4(self):
        assert quarter_to_timestamp(2021, "Q4") == pd.Timestamp("2021-10-01")


class TestNextQuarters:
    def test_from_q4_wraps_to_next_year(self):
        result = next_quarters(2024, "Q4", 4)
        assert result == [(2025, "Q1"), (2025, "Q2"), (2025, "Q3"), (2025, "Q4")]

    def test_from_q2_mid_year(self):
        result = next_quarters(2024, "Q2", 2)
        assert result == [(2024, "Q3"), (2024, "Q4")]

    def test_from_q3_crosses_year(self):
        result = next_quarters(2023, "Q3", 3)
        assert result == [(2023, "Q4"), (2024, "Q1"), (2024, "Q2")]

    def test_length(self):
        for n in [1, 4, 8]:
            assert len(next_quarters(2024, "Q1", n)) == n

    def test_no_duplicates(self):
        result = next_quarters(2024, "Q1", 8)
        assert len(result) == len(set(result))


class TestSeasonalNaive:
    def _make_ts(self, values: list[float]) -> pd.Series:
        return pd.Series(values, dtype=float)

    def test_returns_correct_length(self):
        ts = self._make_ts([100, 110, 120, 130] * 2)
        predicted, lower, upper = _seasonal_naive(ts, 4)
        assert len(predicted) == 4
        assert len(lower) == 4
        assert len(upper) == 4

    def test_lower_le_predicted_le_upper(self):
        ts = self._make_ts([1000, 1100, 900, 1050] * 3)
        predicted, lower, upper = _seasonal_naive(ts, 4)
        for i in range(4):
            assert lower[i] <= predicted[i] <= upper[i]

    def test_non_negative_values(self):
        ts = self._make_ts([100.0] * 8)
        predicted, lower, upper = _seasonal_naive(ts, 4)
        assert all(v >= 0 for v in predicted)
        assert all(v >= 0 for v in lower)

    def test_uses_same_season(self):
        # 8 точек: первые 4 — базовый год, следующие 4 — текущий год
        ts = self._make_ts([100.0, 200.0, 300.0, 400.0, 110.0, 210.0, 310.0, 410.0])
        predicted, _, _ = _seasonal_naive(ts, 4)
        # seasonal naive должен вернуть значения 4 квартала назад
        assert predicted[0] == 110  # ts[4] = 110
        assert predicted[1] == 210
        assert predicted[2] == 310
        assert predicted[3] == 410

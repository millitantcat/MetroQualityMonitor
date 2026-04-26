from collections import defaultdict

import pytest

from app.services.deaggregator import (
    DAY_HOLIDAY,
    DAY_SATURDAY,
    DAY_SUNDAY,
    DAY_WEEKDAY,
    _normalize,
    build_profile_table,
)

_CATEGORIES = [1, 2, 3, 4]
_DAY_TYPES  = [DAY_WEEKDAY, DAY_SATURDAY, DAY_SUNDAY, DAY_HOLIDAY]


class TestBuildProfileTable:
    def setup_method(self):
        self.profiles = build_profile_table()

    def test_total_count(self):
        # 4 категории × 4 типа дня × 24 часа = 384
        assert len(self.profiles) == 384

    def test_all_hours_present_per_combination(self):
        hours_per_combo: dict = defaultdict(set)
        for p in self.profiles:
            key = (p["station_category"], p["day_type"])
            hours_per_combo[key].add(p["hour"])

        for (cat, dt), hours in hours_per_combo.items():
            assert hours == set(range(24)), (
                f"Отсутствуют часы для category={cat}, day_type={dt}"
            )

    def test_incoming_shares_sum_to_one(self):
        sums: dict = defaultdict(float)
        for p in self.profiles:
            sums[(p["station_category"], p["day_type"])] += p["incoming_share"]

        for (cat, dt), total in sums.items():
            assert abs(total - 1.0) < 1e-5, (
                f"incoming_share сумма ≠ 1.0 для category={cat}, day_type={dt}: {total}"
            )

    def test_outgoing_shares_sum_to_one(self):
        sums: dict = defaultdict(float)
        for p in self.profiles:
            sums[(p["station_category"], p["day_type"])] += p["outgoing_share"]

        for (cat, dt), total in sums.items():
            assert abs(total - 1.0) < 1e-5, (
                f"outgoing_share сумма ≠ 1.0 для category={cat}, day_type={dt}: {total}"
            )

    def test_all_shares_non_negative(self):
        for p in self.profiles:
            assert p["incoming_share"] >= 0, f"Отрицательный incoming_share: {p}"
            assert p["outgoing_share"] >= 0, f"Отрицательный outgoing_share: {p}"

    def test_peak_hours_have_higher_share_residential_weekday(self):
        """
        Спальная станция, будни:
        - Утренний пик входящих (8–9h): жители ВХОДЯТ в метро, едут на работу.
        - Вечерний пик исходящих (17–19h): жители ВЫХОДЯТ из метро, возвращаются домой.
        """
        residential_weekday = [
            p for p in self.profiles
            if p["station_category"] == 1 and p["day_type"] == DAY_WEEKDAY
        ]
        by_hour = {p["hour"]: p for p in residential_weekday}

        # incoming пик (8h) > дневная долина (13h)
        assert by_hour[8]["incoming_share"] > by_hour[13]["incoming_share"]
        # outgoing пик (18h) > утро (8h)
        assert by_hour[18]["outgoing_share"] > by_hour[8]["outgoing_share"]

    def test_weekend_flatter_than_weekday_residential(self):
        """Выходной профиль должен быть более равномерным: max/min меньше, чем в будни."""
        def _ratio(profiles: list[dict], direction: str) -> float:
            shares = [p[direction] for p in profiles]
            return max(shares) / max(min(shares), 1e-9)

        weekday = [p for p in self.profiles if p["station_category"] == 1 and p["day_type"] == DAY_WEEKDAY]
        saturday = [p for p in self.profiles if p["station_category"] == 1 and p["day_type"] == DAY_SATURDAY]

        # В будни разброс значительно больше
        assert _ratio(weekday, "outgoing_share") > _ratio(saturday, "outgoing_share")


class TestNormalize:
    def test_sums_to_one(self):
        raw = {h: (float(h + 1), float(h + 2)) for h in range(24)}
        result = _normalize(raw)

        total_in  = sum(v[0] for v in result.values())
        total_out = sum(v[1] for v in result.values())

        assert abs(total_in  - 1.0) < 1e-10
        assert abs(total_out - 1.0) < 1e-10

    def test_preserves_relative_order(self):
        raw = {0: (1.0, 1.0), 1: (2.0, 3.0), 2: (3.0, 2.0)}
        result = _normalize(raw)
        assert result[2][0] > result[1][0] > result[0][0]

    def test_keys_preserved(self):
        raw    = {h: (1.0, 1.0) for h in range(24)}
        result = _normalize(raw)
        assert set(result.keys()) == set(range(24))

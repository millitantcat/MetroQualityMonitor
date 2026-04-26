"""
Деагрегация квартального пассажиропотока в часовые профили.

Используется model-based подход: эмпирические коэффициенты распределения
на основе типичных паттернов московского метро (пиковые часы, выходные, типы станций).

Формула (для каждого часа h):
  hourly(station, day, h) =
      quarterly(station) / 91            ← среднесуточный поток
    × WeekdayFactor(day)                 ← поправка на тип дня
    × HourlyProfile(category, day, h)   ← доля часа

WeekdayFactor: Weekday=1.15, Saturday=0.70, Sunday/Holiday=0.55.

Профили хранятся в таблице HourlyProfiles и используются .NET при вызове
GET /api/stations/{id}/hourly.
"""

import logging

from sqlalchemy.orm import Session

from app.models.orm import HourlyProfile

logger = logging.getLogger(__name__)

# Enum-значения, зеркалящие C# StationCategories
CATEGORY_RESIDENTIAL = 1
CATEGORY_CENTRAL     = 2
CATEGORY_TRANSFER    = 3
CATEGORY_MIXED       = 4

# Enum-значения, зеркалящие C# DayTypes
DAY_WEEKDAY  = 1
DAY_SATURDAY = 2
DAY_SUNDAY   = 3
DAY_HOLIDAY  = 4


def _normalize(raw: dict[int, tuple[float, float]]) -> dict[int, tuple[float, float]]:
    """Нормализует словарь {час: (in, out)} так, чтобы суммы по дню = 1.0."""
    total_in  = sum(v[0] for v in raw.values())
    total_out = sum(v[1] for v in raw.values())
    return {h: (v[0] / total_in, v[1] / total_out) for h, v in raw.items()}


# ---------------------------------------------------------------------------
# Профили будних дней по категориям станций
# ---------------------------------------------------------------------------

def _residential_weekday() -> dict[int, tuple[float, float]]:
    """
    Спальная станция: утренний пик входящих (рабочие уезжают в центр),
    вечерний пик входящих (возвращаются домой).
    """
    raw = {
        0: (0.3, 0.2),  1: (0.1, 0.1),  2: (0.1, 0.1),  3: (0.1, 0.1),
        4: (0.3, 0.2),  5: (1.0, 0.5),
        6: (5.0, 2.5),
        7: (9.0, 3.0),   # утренний пик выходящих (на работу)
        8: (14.0, 3.5),  # утренний пик выходящих
        9: (8.0, 3.0),
        10: (4.0, 3.5), 11: (3.5, 3.5), 12: (3.5, 4.0), 13: (3.5, 4.0),
        14: (3.5, 3.5), 15: (3.5, 4.0),
        16: (4.0, 6.0),
        17: (5.0, 9.0),  # вечерний пик входящих (домой)
        18: (6.0, 13.0), # вечерний пик входящих
        19: (5.0, 9.0),
        20: (4.0, 6.0),  21: (3.0, 5.0), 22: (2.0, 3.5), 23: (1.0, 2.0),
    }
    return _normalize(raw)


def _central_weekday() -> dict[int, tuple[float, float]]:
    """
    Центральная станция: утренний пик входящих (работники приезжают),
    вечерний пик выходящих (разъезжаются по домам).
    """
    raw = {
        0: (0.3, 0.2),  1: (0.1, 0.1),  2: (0.1, 0.1),  3: (0.1, 0.1),
        4: (0.2, 0.2),  5: (0.8, 0.5),
        6: (3.5, 2.0),
        7: (6.0, 3.5),
        8: (8.0, 5.0),   # входящий утренний пик
        9: (13.0, 6.0),  # входящий утренний пик (смещённый)
        10: (7.0, 5.0), 11: (6.0, 5.5), 12: (6.5, 6.0), 13: (6.5, 6.0),
        14: (6.0, 5.5), 15: (5.5, 6.0),
        16: (5.0, 8.0),
        17: (5.0, 12.0), # выходящий вечерний пик
        18: (5.5, 13.0), # выходящий вечерний пик
        19: (4.5, 9.0),
        20: (3.5, 6.5), 21: (2.5, 5.0), 22: (2.0, 3.5), 23: (1.3, 2.5),
    }
    return _normalize(raw)


def _transfer_weekday() -> dict[int, tuple[float, float]]:
    """
    Пересадочная станция (у вокзалов): два симметричных пика,
    высокая нагрузка в течение всего дня.
    """
    raw = {
        0: (0.5, 0.5),  1: (0.2, 0.2),  2: (0.2, 0.2),  3: (0.2, 0.2),
        4: (0.5, 0.5),  5: (1.5, 1.5),
        6: (4.0, 4.0),
        7: (7.0, 6.5),
        8: (10.0, 9.5),  # утренний пик
        9: (11.0, 10.5), # утренний пик (длиннее из-за транзита)
        10: (6.5, 6.5), 11: (5.5, 5.5), 12: (5.5, 5.5), 13: (5.5, 5.5),
        14: (5.5, 5.5), 15: (6.0, 6.0),
        16: (7.0, 7.0),
        17: (10.0, 10.5), # вечерний пик
        18: (11.0, 11.0), # вечерний пик
        19: (7.0, 7.0),
        20: (5.0, 5.0),  21: (3.5, 3.5), 22: (2.5, 2.5), 23: (1.5, 1.5),
    }
    return _normalize(raw)


def _mixed_weekday() -> dict[int, tuple[float, float]]:
    """Смешанная станция: среднее между residential и central."""
    raw = {
        0: (0.3, 0.3),  1: (0.1, 0.1),  2: (0.1, 0.1),  3: (0.1, 0.1),
        4: (0.3, 0.2),  5: (0.9, 0.5),
        6: (4.0, 2.5),
        7: (7.0, 5.0),
        8: (11.0, 4.0),  # утренний пик
        9: (10.0, 5.0),
        10: (5.5, 4.5), 11: (5.0, 5.0), 12: (5.0, 5.0), 13: (5.0, 5.0),
        14: (4.5, 5.0), 15: (4.5, 5.5),
        16: (5.0, 7.0),
        17: (5.5, 11.0), # вечерний пик
        18: (5.8, 11.5),
        19: (4.5, 8.0),
        20: (3.5, 5.5), 21: (2.5, 4.0), 22: (2.0, 3.0), 23: (1.2, 2.0),
    }
    return _normalize(raw)


def _weekend() -> dict[int, tuple[float, float]]:
    """
    Выходной/праздник: пиков нет, поток размазан по дню с небольшим
    вечерним подъёмом (досуг, шопинг). Профиль одинаков для всех категорий.
    """
    raw = {
        0: (0.5, 0.4),  1: (0.2, 0.2),  2: (0.1, 0.1),  3: (0.1, 0.1),
        4: (0.2, 0.2),  5: (0.5, 0.4),
        6: (2.0, 1.5),  7: (3.5, 2.5),  8: (4.5, 3.5),  9: (5.5, 4.5),
        10: (6.5, 5.5), 11: (7.0, 6.0), 12: (7.0, 6.5), 13: (7.0, 6.5),
        14: (6.5, 6.5), 15: (6.5, 6.5),
        16: (6.0, 7.0),
        17: (6.0, 7.5),
        18: (6.5, 7.5), # небольшой вечерний пик
        19: (5.5, 6.5),
        20: (4.5, 5.5), 21: (3.5, 4.5), 22: (2.5, 3.5), 23: (1.5, 2.5),
    }
    return _normalize(raw)


# ---------------------------------------------------------------------------
# Сборка таблицы профилей
# ---------------------------------------------------------------------------

def build_profile_table() -> list[dict]:
    """
    Возвращает список из 384 записей (4 категории × 4 типа дня × 24 часа),
    готовых к вставке в таблицу HourlyProfiles.
    """
    profiles: list[dict] = []

    weekday_builders = {
        CATEGORY_RESIDENTIAL: _residential_weekday,
        CATEGORY_CENTRAL:     _central_weekday,
        CATEGORY_TRANSFER:    _transfer_weekday,
        CATEGORY_MIXED:       _mixed_weekday,
    }

    for category, builder in weekday_builders.items():
        shares = builder()
        for hour in range(24):
            in_s, out_s = shares[hour]
            profiles.append({
                "station_category": category,
                "day_type":         DAY_WEEKDAY,
                "hour":             hour,
                "incoming_share":   round(in_s, 6),
                "outgoing_share":   round(out_s, 6),
            })

    # Выходные и праздники — одинаковый профиль для всех категорий
    for category in weekday_builders:
        shares = _weekend()
        for day_type in [DAY_SATURDAY, DAY_SUNDAY, DAY_HOLIDAY]:
            for hour in range(24):
                in_s, out_s = shares[hour]
                profiles.append({
                    "station_category": category,
                    "day_type":         day_type,
                    "hour":             hour,
                    "incoming_share":   round(in_s, 6),
                    "outgoing_share":   round(out_s, 6),
                })

    return profiles


def seed_hourly_profiles(session: Session, overwrite: bool = False) -> dict:
    """
    Заполняет таблицу HourlyProfiles эмпирическими профилями.
    При overwrite=True удаляет существующие записи перед вставкой.
    """
    existing = session.query(HourlyProfile).count()
    if existing > 0 and not overwrite:
        logger.info("Профили уже заполнены (%d записей), пропуск.", existing)
        return {"skipped": True, "existing": existing}

    if overwrite:
        deleted = session.query(HourlyProfile).delete()
        logger.info("Удалено %d профилей.", deleted)

    profiles = build_profile_table()
    for p in profiles:
        session.add(HourlyProfile(
            station_category=p["station_category"],
            day_type=p["day_type"],
            hour=p["hour"],
            incoming_share=p["incoming_share"],
            outgoing_share=p["outgoing_share"],
        ))

    logger.info("Заполнено %d профилей пассажиропотока.", len(profiles))
    return {"seeded": len(profiles)}

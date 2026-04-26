from sqlalchemy.orm import Session
from fastapi import APIRouter, Depends, Query

from app.db import get_db
from app.services.deaggregator import seed_hourly_profiles

router = APIRouter(prefix="/profiles", tags=["Profiles"])


@router.post(
    "/seed",
    summary="Заполнить или перезаполнить таблицу HourlyProfiles",
)
def seed(
    overwrite: bool = Query(False, description="Удалить существующие профили перед вставкой"),
    db: Session = Depends(get_db),
):
    """
    Записывает в HourlyProfiles 384 строки эмпирических часовых профилей
    (4 категории × 4 типа дня × 24 часа).

    При overwrite=false пропускает вставку, если таблица уже заполнена.
    """
    return seed_hourly_profiles(db, overwrite=overwrite)

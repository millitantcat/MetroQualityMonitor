from sqlalchemy.orm import Session
from fastapi import APIRouter, Depends

from app.db import get_db
from app.services.anomaly_detector import run_anomaly_detection

router = APIRouter(prefix="/anomalies", tags=["Anomalies"])


@router.post(
    "/detect",
    summary="Запустить детекцию аномалий (Z-score + YoY + Isolation Forest)",
)
def detect(db: Session = Depends(get_db)):
    """
    Запускает три детектора аномалий, удаляет неподтверждённые аномалии
    и записывает новые результаты в таблицу Anomalies.
    """
    return run_anomaly_detection(db)

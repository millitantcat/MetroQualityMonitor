from sqlalchemy.orm import Session
from fastapi import APIRouter, Depends

from app.db import get_db
from app.services.forecaster import compute_validation_metrics, run_batch_forecast

router = APIRouter(prefix="/forecast", tags=["Forecast"])


@router.post(
    "/run-batch",
    summary="Пересчитать прогнозы для всех линий на 4 квартала вперёд",
)
def run_batch(db: Session = Depends(get_db)):
    """
    Запускает SARIMA-прогнозирование для каждой линии метро.
    Удаляет устаревшие будущие прогнозы, записывает новые в таблицу Forecasts.
    """
    return run_batch_forecast(db)


@router.get(
    "/validate",
    summary="Walk-forward валидация моделей (hold-out 4 квартала)",
)
def validate(db: Session = Depends(get_db)):
    """
    Сравнивает SARIMA с Seasonal Naive baseline по метрикам MAE, MAPE, RMSE.
    Используется для пояснительной записки ВКР.
    """
    return compute_validation_metrics(db)

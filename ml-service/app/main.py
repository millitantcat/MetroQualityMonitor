import logging

from fastapi import FastAPI

from app.config import settings
from app.routes import anomalies, clusters, forecast, health, profiles

logging.basicConfig(
    level=getattr(logging, settings.log_level.upper(), logging.INFO),
    format="%(asctime)s [%(levelname)s] %(name)s: %(message)s",
)

app = FastAPI(
    title="MetroQualityMonitor ML Service",
    description=(
        "Python ML-сервис: прогнозирование пассажиропотока (SARIMA), "
        "детекция аномалий (Z-score + Isolation Forest + YoY), "
        "кластеризация станций (K-Means), деагрегация часовых профилей."
    ),
    version="1.0.0",
)

app.include_router(health.router)
app.include_router(forecast.router)
app.include_router(anomalies.router)
app.include_router(clusters.router)
app.include_router(profiles.router)

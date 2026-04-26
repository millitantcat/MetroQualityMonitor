from sqlalchemy.orm import Session
from fastapi import APIRouter, Depends

from app.db import get_db
from app.services.clusterer import recompute_clusters

router = APIRouter(prefix="/clusters", tags=["Clusters"])


@router.post(
    "/recompute",
    summary="Пересчитать кластеризацию станций (K-Means, k=4)",
)
def recompute(db: Session = Depends(get_db)):
    """
    Запускает K-Means на признаках (mean_incoming, cv_incoming, yoy_avg_growth).
    Добавляет новые строки в StationClusters; история не удаляется.
    """
    return recompute_clusters(db)

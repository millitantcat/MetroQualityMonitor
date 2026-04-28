"""
Кластеризация станций метро по характеру пассажиропотока.

Алгоритм: K-Means, k=4.
Признаки (per station): mean_incoming, cv_incoming, yoy_avg_growth.
Присвоение меток по центроидам:
  - наивысший mean_incoming → Central
  - наинизший mean_incoming → Residential
  - среди оставшихся: наивысший cv_incoming → Transfer (нестабильный поток у вокзалов)
  - остальное → Mixed

Результат пишется в таблицу StationClusters (история не удаляется).
"""

import logging
from datetime import datetime, timezone

import numpy as np
import pandas as pd
from sklearn.cluster import KMeans
from sklearn.preprocessing import StandardScaler
from sqlalchemy.orm import Session

from app.config import settings
from app.models.orm import StationCluster
from app.services.data_loader import load_station_passenger_flow

logger = logging.getLogger(__name__)

_FEATURE_NAMES = ["mean_incoming", "cv_incoming", "yoy_avg_growth"]
_LABEL_CENTRAL     = "Central"
_LABEL_RESIDENTIAL = "Residential"
_LABEL_TRANSFER    = "Transfer"
_LABEL_MIXED       = "Mixed"


def _assign_labels_by_centroids(
    km: KMeans, feature_names: list[str]
) -> dict[int, str]:
    """
    Назначает семантические метки кластерам на основе значений центроидов.

    Порядок приоритетов:
    1. Кластер с максимальным mean_incoming → Central
    2. Кластер с минимальным mean_incoming → Residential
    3. Среди оставшихся: максимальный cv_incoming → Transfer
    4. Остаток → Mixed
    """
    centroids = km.cluster_centers_
    mean_idx  = feature_names.index("mean_incoming")
    cv_idx    = feature_names.index("cv_incoming")
    n         = len(centroids)
    all_ids   = list(range(n))

    sorted_by_mean = sorted(all_ids, key=lambda c: centroids[c][mean_idx], reverse=True)

    labels:   dict[int, str] = {}
    assigned: set[int]       = set()

    # Central — самый нагруженный
    labels[sorted_by_mean[0]] = _LABEL_CENTRAL
    assigned.add(sorted_by_mean[0])

    # Residential — наименее нагруженный
    labels[sorted_by_mean[-1]] = _LABEL_RESIDENTIAL
    assigned.add(sorted_by_mean[-1])

    remaining = [c for c in all_ids if c not in assigned]
    if remaining:
        # Transfer — самый нестабильный (высокий CV) из оставшихся
        transfer = max(remaining, key=lambda c: centroids[c][cv_idx])
        labels[transfer] = _LABEL_TRANSFER
        assigned.add(transfer)
        remaining = [c for c in remaining if c not in assigned]

    for c in remaining:
        labels[c] = _LABEL_MIXED

    return labels


def recompute_clusters(session: Session) -> dict:
    """
    Запускает K-Means кластеризацию и добавляет новые строки в StationClusters.
    Старые записи не удаляются — .NET всегда берёт последнюю по ComputedAtDateTimeUtc.
    """
    df = load_station_passenger_flow(session)
    if df.empty:
        logger.warning("Данные о пассажиропотоке не найдены; кластеризация пропущена.")
        return {"saved": 0}

    # Признаки per-station
    stats = (
        df.groupby("station_id")
        .agg(
            mean_incoming=("incoming", "mean"),
            std_incoming=("incoming", "std"),
            count=("incoming", "count"),
        )
        .reset_index()
    )
    stats["std_incoming"] = stats["std_incoming"].fillna(0.0)
    stats["cv_incoming"]  = stats["std_incoming"] / stats["mean_incoming"].clip(lower=1.0)

    # Среднее YoY-изменение по станции
    df_sorted = df.sort_values(["station_id", "year", "quarter"])
    yoy_change = (
        df_sorted.groupby(["station_id", "quarter"])["incoming"]
        .pct_change(fill_method=None)
    )
    yoy_avg = (
        yoy_change.groupby(df_sorted["station_id"])
        .mean()
        .reset_index()
        .rename(columns={"incoming": "yoy_avg_growth"})
    )
    stats = stats.merge(yoy_avg, on="station_id", how="left")
    stats["yoy_avg_growth"] = stats["yoy_avg_growth"].fillna(0.0).clip(-1.0, 2.0)

    # Оставляем только станции с достаточным количеством точек
    stats = stats[stats["count"] >= 4].reset_index(drop=True)

    n_clusters = min(settings.n_clusters, len(stats))
    if n_clusters < 2:
        logger.warning("Недостаточно станций для кластеризации (нужно ≥ 2).")
        return {"saved": 0}

    # K-Means
    X       = stats[_FEATURE_NAMES].values
    scaler  = StandardScaler()
    X_scaled = scaler.fit_transform(X)

    km = KMeans(n_clusters=n_clusters, random_state=42, n_init=10, max_iter=300)
    km.fit(X_scaled)

    stats["cluster_id"]    = km.labels_
    label_map              = _assign_labels_by_centroids(km, _FEATURE_NAMES)
    stats["cluster_label"] = stats["cluster_id"].map(label_map)

    # Запись
    now   = datetime.now(timezone.utc)
    saved = 0

    for _, row in stats.iterrows():
        session.add(StationCluster(
            station_id=int(row["station_id"]),
            cluster_id=int(row["cluster_id"]),
            cluster_label=str(row["cluster_label"]),
            computed_at_date_time_utc=now,
        ))
        saved += 1

    dist = stats["cluster_label"].value_counts().to_dict()
    logger.info("Кластеризация завершена: %d станций, распределение: %s.", saved, dist)
    return {"saved": saved, "label_distribution": dist}

import numpy as np
import pytest
from sklearn.cluster import KMeans

from app.services.clusterer import (
    _FEATURE_NAMES,
    _LABEL_CENTRAL,
    _LABEL_RESIDENTIAL,
    _LABEL_TRANSFER,
    _assign_labels_by_centroids,
)


def _fit_km_on_structured_data() -> KMeans:
    """
    Четыре хорошо разделённых кластера:
    - очень высокий mean_incoming, низкий cv → Central
    - очень низкий mean_incoming, низкий cv  → Residential
    - средний mean_incoming, высокий cv      → Transfer
    - средний mean_incoming, средний cv      → Mixed
    """
    X = np.array([
        [10_000, 0.05, 0.02],   # Central
        [10_100, 0.06, 0.01],
        [10_200, 0.04, 0.02],
        [300,    0.10, 0.00],   # Residential
        [280,    0.12, 0.01],
        [320,    0.09, 0.00],
        [4_000,  0.90, 0.03],   # Transfer (высокий CV)
        [3_800,  0.85, 0.02],
        [4_200,  0.95, 0.03],
        [5_000,  0.35, 0.02],   # Mixed
        [4_800,  0.38, 0.03],
        [5_200,  0.33, 0.01],
    ])
    km = KMeans(n_clusters=4, random_state=42, n_init=10)
    km.fit(X)
    return km


class TestAssignLabelsByCentroids:
    def test_central_has_highest_mean(self):
        km     = _fit_km_on_structured_data()
        labels = _assign_labels_by_centroids(km, _FEATURE_NAMES)

        centroids  = km.cluster_centers_
        mean_idx   = _FEATURE_NAMES.index("mean_incoming")
        central_id = next(k for k, v in labels.items() if v == _LABEL_CENTRAL)
        res_id     = next(k for k, v in labels.items() if v == _LABEL_RESIDENTIAL)

        assert centroids[central_id][mean_idx] > centroids[res_id][mean_idx]

    def test_residential_has_lowest_mean(self):
        km        = _fit_km_on_structured_data()
        labels    = _assign_labels_by_centroids(km, _FEATURE_NAMES)
        centroids = km.cluster_centers_
        mean_idx  = _FEATURE_NAMES.index("mean_incoming")

        res_id = next(k for k, v in labels.items() if v == _LABEL_RESIDENTIAL)
        all_means = [centroids[c][mean_idx] for c in range(len(centroids))]
        assert centroids[res_id][mean_idx] == min(all_means)

    def test_all_four_labels_present(self):
        km     = _fit_km_on_structured_data()
        labels = _assign_labels_by_centroids(km, _FEATURE_NAMES)
        assert set(labels.values()) == {"Central", "Residential", "Transfer", "Mixed"}

    def test_returns_mapping_for_every_cluster(self):
        km     = _fit_km_on_structured_data()
        labels = _assign_labels_by_centroids(km, _FEATURE_NAMES)
        assert set(labels.keys()) == set(range(4))

    def test_transfer_has_higher_cv_than_mixed(self):
        km     = _fit_km_on_structured_data()
        labels = _assign_labels_by_centroids(km, _FEATURE_NAMES)

        centroids   = km.cluster_centers_
        cv_idx      = _FEATURE_NAMES.index("cv_incoming")
        transfer_id = next(k for k, v in labels.items() if v == _LABEL_TRANSFER)
        mixed_ids   = [k for k, v in labels.items() if v not in (_LABEL_CENTRAL, _LABEL_RESIDENTIAL, _LABEL_TRANSFER)]

        for mid in mixed_ids:
            assert centroids[transfer_id][cv_idx] >= centroids[mid][cv_idx]

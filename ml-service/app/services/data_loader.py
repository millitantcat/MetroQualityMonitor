"""Загрузка сырых данных из PostgreSQL для ML-моделей."""

import pandas as pd
from sqlalchemy.orm import Session

from app.models.orm import PassengerFlowRecord, Station


def load_station_passenger_flow(session: Session) -> pd.DataFrame:
    """
    Загружает квартальный пассажиропоток по станциям.
    Возвращает DataFrame с колонками:
    station_id, line_id, year, quarter, incoming, outgoing.
    """
    rows = (
        session.query(
            PassengerFlowRecord.station_id,
            PassengerFlowRecord.line_id,
            PassengerFlowRecord.year,
            PassengerFlowRecord.quarter,
            PassengerFlowRecord.incoming_passengers,
            PassengerFlowRecord.outgoing_passengers,
        )
        .filter(PassengerFlowRecord.station_id.isnot(None))
        .all()
    )

    if not rows:
        return pd.DataFrame(
            columns=["station_id", "line_id", "year", "quarter", "incoming", "outgoing"]
        )

    df = pd.DataFrame(
        rows,
        columns=["station_id", "line_id", "year", "quarter", "incoming", "outgoing"],
    )
    df["incoming"] = df["incoming"].astype(float)
    df["outgoing"] = df["outgoing"].astype(float)
    return df


def load_line_passenger_flow(session: Session) -> pd.DataFrame:
    """
    Агрегирует пассажиропоток по линиям (суммирует станции).
    Возвращает DataFrame с колонками:
    line_id, year, quarter, incoming, outgoing.
    """
    df = load_station_passenger_flow(session)
    if df.empty or df["line_id"].isna().all():
        return pd.DataFrame(columns=["line_id", "year", "quarter", "incoming", "outgoing"])

    df = df.dropna(subset=["line_id"])
    aggregated = (
        df.groupby(["line_id", "year", "quarter"])
        .agg(incoming=("incoming", "sum"), outgoing=("outgoing", "sum"))
        .reset_index()
    )
    return aggregated


def load_stations(session: Session) -> pd.DataFrame:
    """Загружает справочник станций."""
    rows = session.query(Station.id, Station.name).all()
    if not rows:
        return pd.DataFrame(columns=["id", "name"])
    return pd.DataFrame(rows, columns=["id", "name"])

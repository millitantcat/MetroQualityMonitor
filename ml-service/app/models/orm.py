"""
SQLAlchemy ORM-зеркало EF Core сущностей.

Все имена таблиц и колонок — точное отражение PascalCase-идентификаторов,
которые EF Core + Npgsql создаёт в кавычках (quoted identifiers).
quoted_name(..., quote=True) гарантирует, что SQLAlchemy отправляет их
в двойных кавычках: SELECT "Id" FROM "Stations" — и не сворачивает в lower-case.

Отношения (relationships) не определены — они не нужны:
сервис пишет только в аналитические таблицы и читает данные
простыми SELECT/GROUP BY без JOIN-навигации через ORM.
"""

import uuid

from sqlalchemy import Boolean, Column, DateTime, Double, Integer, SmallInteger, String
from sqlalchemy.dialects.postgresql import UUID as PG_UUID
from sqlalchemy.orm import DeclarativeBase
from sqlalchemy.sql.expression import quoted_name


def _qn(name: str) -> quoted_name:
    """Возвращает SQLAlchemy-идентификатор с принудительными кавычками."""
    return quoted_name(name, quote=True)


class Base(DeclarativeBase):
    pass


# ---------------------------------------------------------------------------
# Справочные таблицы (только чтение)
# ---------------------------------------------------------------------------

class Station(Base):
    __tablename__ = _qn("Stations")

    id   = Column(_qn("Id"),   SmallInteger, primary_key=True)
    name = Column(_qn("Name"), String(100),  nullable=False)


class Line(Base):
    __tablename__ = _qn("Lines")

    id   = Column(_qn("Id"),   SmallInteger, primary_key=True)
    name = Column(_qn("Name"), String(100),  nullable=False)


class PassengerFlowRecord(Base):
    __tablename__ = _qn("PassengerFlowRecords")

    id                  = Column(_qn("Id"),                  Integer,      primary_key=True)
    global_id           = Column(_qn("GlobalId"),            Integer,      nullable=False)
    year                = Column(_qn("Year"),                Integer,      nullable=False)
    quarter             = Column(_qn("Quarter"),             String(20),   nullable=False)
    incoming_passengers = Column(_qn("IncomingPassengers"),  Integer,      nullable=False)
    outgoing_passengers = Column(_qn("OutgoingPassengers"),  Integer,      nullable=False)
    station_id          = Column(_qn("StationId"),           SmallInteger, nullable=True)
    line_id             = Column(_qn("LineId"),              SmallInteger, nullable=True)


# ---------------------------------------------------------------------------
# Аналитические таблицы (чтение + запись)
# ---------------------------------------------------------------------------

class Forecast(Base):
    __tablename__ = _qn("Forecasts")

    id                        = Column(_qn("Id"),                       PG_UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    line_id                   = Column(_qn("LineId"),                   SmallInteger,           nullable=True)
    station_id                = Column(_qn("StationId"),                SmallInteger,           nullable=True)
    year                      = Column(_qn("Year"),                     Integer,                nullable=False)
    quarter                   = Column(_qn("Quarter"),                  String(20),             nullable=False)
    predicted_incoming        = Column(_qn("PredictedIncoming"),        Integer,                nullable=False)
    predicted_outgoing        = Column(_qn("PredictedOutgoing"),        Integer,                nullable=False)
    confidence_lower_incoming = Column(_qn("ConfidenceLowerIncoming"),  Integer,                nullable=True)
    confidence_upper_incoming = Column(_qn("ConfidenceUpperIncoming"),  Integer,                nullable=True)
    confidence_lower_outgoing = Column(_qn("ConfidenceLowerOutgoing"),  Integer,                nullable=True)
    confidence_upper_outgoing = Column(_qn("ConfidenceUpperOutgoing"),  Integer,                nullable=True)
    model_name                = Column(_qn("ModelName"),                String(50),             nullable=False)
    model_version             = Column(_qn("ModelVersion"),             String(20),             nullable=False)
    create_date_time_utc      = Column(_qn("CreateDateTimeUtc"),        DateTime(timezone=True), nullable=False)


class Anomaly(Base):
    __tablename__ = _qn("Anomalies")

    id                         = Column(_qn("Id"),                       PG_UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    station_id                 = Column(_qn("StationId"),                SmallInteger,           nullable=False)
    year                       = Column(_qn("Year"),                     Integer,                nullable=False)
    quarter                    = Column(_qn("Quarter"),                  String(20),             nullable=False)
    # AnomalyTypes:    Statistical=1, IsolationForest=2, YoYDeviation=3
    anomaly_type               = Column(_qn("AnomalyType"),              SmallInteger,           nullable=False)
    # AnomalySeverities: Low=1, Medium=2, High=3
    severity                   = Column(_qn("Severity"),                 SmallInteger,           nullable=False)
    score                      = Column(_qn("Score"),                    Double(),               nullable=False)
    actual_value               = Column(_qn("ActualValue"),              Integer,                nullable=False)
    expected_value             = Column(_qn("ExpectedValue"),            Integer,                nullable=True)
    description                = Column(_qn("Description"),              String(500),            nullable=True)
    is_acknowledged            = Column(_qn("IsAcknowledged"),           Boolean,                nullable=False, default=False)
    acknowledged_date_time_utc = Column(_qn("AcknowledgedDateTimeUtc"), DateTime(timezone=True), nullable=True)
    create_date_time_utc       = Column(_qn("CreateDateTimeUtc"),        DateTime(timezone=True), nullable=False)


class HourlyProfile(Base):
    __tablename__ = _qn("HourlyProfiles")

    id               = Column(_qn("Id"),              Integer,      primary_key=True, autoincrement=True)
    # StationCategories: Residential=1, Central=2, Transfer=3, Mixed=4
    station_category = Column(_qn("StationCategory"), SmallInteger, nullable=False)
    # DayTypes: Weekday=1, Saturday=2, Sunday=3, Holiday=4
    day_type         = Column(_qn("DayType"),          SmallInteger, nullable=False)
    hour             = Column(_qn("Hour"),             Integer,      nullable=False)
    incoming_share   = Column(_qn("IncomingShare"),    Double(),     nullable=False)
    outgoing_share   = Column(_qn("OutgoingShare"),    Double(),     nullable=False)


class StationCluster(Base):
    __tablename__ = _qn("StationClusters")

    id                        = Column(_qn("Id"),                      Integer,      primary_key=True, autoincrement=True)
    station_id                = Column(_qn("StationId"),               SmallInteger, nullable=False)
    cluster_id                = Column(_qn("ClusterId"),               Integer,      nullable=False)
    cluster_label             = Column(_qn("ClusterLabel"),            String(50),   nullable=False)
    computed_at_date_time_utc = Column(_qn("ComputedAtDateTimeUtc"),   DateTime(timezone=True), nullable=False)

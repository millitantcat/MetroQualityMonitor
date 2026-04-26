from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(
        env_file=".env",
        env_file_encoding="utf-8",
        case_sensitive=False,
    )

    database_url: str = (
        "postgresql+psycopg2://postgres:postgres@localhost:5432/MetroQualityMonitor"
    )
    forecast_horizon: int = 4
    model_name: str = "SARIMA"
    model_version: str = "v1.0"
    log_level: str = "INFO"
    n_clusters: int = 4


settings = Settings()

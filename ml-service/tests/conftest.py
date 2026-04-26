"""
Общая конфигурация pytest.

Все тесты в этом проекте — unit-тесты без подключения к БД.
Импорты app.config/db производятся через переменную среды DATABASE_URL.
"""

import os

# Устанавливаем заглушку до любого импорта, чтобы pydantic-settings
# не пыталась подключиться к реальной БД.
os.environ.setdefault(
    "DATABASE_URL",
    "postgresql+psycopg2://test:test@localhost:5432/test",
)

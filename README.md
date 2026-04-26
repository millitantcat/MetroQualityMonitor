# MetroQualityMonitor

Магистерская ВКР: **«Разработка веб-приложения для мониторинга и повышения качества пассажирских перевозок в метро на основе методов искусственного интеллекта»**

---

## Назначение

Система собирает открытые данные о пассажиропотоке метро Москвы, строит прогнозы на SARIMA-модели, обнаруживает аномалии методами Z-score и Isolation Forest, и представляет результаты в веб-интерфейсе с интерактивной картой, часовым heatmap и графиком прогноза с доверительным интервалом.

---

## Архитектура

```
┌──────────────────────────────────────────────────────────────┐
│                      Angular 18 SPA                          │
│   Dashboard │ Карта станций │ Детали станции │ Аномалии      │
└──────────────────────┬───────────────────────────────────────┘
                       │ REST/JSON (proxy → :5000)
                       ▼
┌──────────────────────────────────────────────────────────────┐
│               .NET 9 Web API (:5000)                         │
│   /api/dashboard  /api/stations  /api/anomalies  /api/...    │
│                                                              │
│   BackgroundService — раз в 24 ч вызывает ML-сервис         │
└──────────────────────┬───────────────────────────────────────┘
                       │ EF Core / SQLAlchemy
                       ▼
┌──────────────────────────────────────────────────────────────┐
│                    PostgreSQL                                 │
│   Lines, Stations, Vestibules, PassengerFlow                 │
│   Forecasts, Anomalies, StationClusters, HourlyProfiles      │
└──────────────────────▲───────────────────────────────────────┘
                       │ SQLAlchemy (write)
                       │
┌──────────────────────┴───────────────────────────────────────┐
│              Python FastAPI ML-сервис (:8000)                │
│   POST /forecast/run-batch                                   │
│   POST /anomalies/detect                                     │
│   POST /clusters/recompute                                   │
└──────────────────────────────────────────────────────────────┘
```

**Ключевые принципы:**
- ML-сервис **не в hot-path**: пишет результаты в БД, .NET читает через EF Core.
- BackgroundService дёргает Python раз в сутки. Если ML недоступен — UI работает на последних данных.
- Никаких очередей, gRPC, MediatR.

---

## Стек

| Слой | Технологии |
|---|---|
| Frontend | Angular 18 (standalone), Angular Material (azure-blue), Chart.js, Leaflet |
| Backend | .NET 9, ASP.NET Core Web API, EF Core, Npgsql |
| ML-сервис | Python 3.11, FastAPI, SQLAlchemy 2, statsmodels (SARIMA), scikit-learn |
| БД | PostgreSQL 16 |

---

## Структура репозитория

```
MetroQualityMonitor/
├── MetroQualityMonitor.Domain/          # Сущности и enum'ы, без инфраструктуры
├── MetroQualityMonitor.Application/     # Интерфейсы сервисов, DTO
├── MetroQualityMonitor.Infrastructure/  # EF Core, миграции, реализации сервисов
├── MetroQualityMonitor.DbMigrator/      # Консоль: применяет миграции + сидинг
├── MetroQualityMonitor.Web/             # ASP.NET Core Web API (контроллеры)
├── scripts/
│   ├── DataFetcher/                     # Выкачивает GeoJSON с apidata.mos.ru
│   └── DataImporter/                    # Импортирует datasets/*.json в БД
├── ml-service/                          # Python FastAPI: прогнозы, аномалии, кластеры
│   ├── app/
│   │   ├── main.py
│   │   ├── services/                    # forecaster, anomaly_detector, clusterer, deaggregator
│   │   └── routes/                      # forecast, anomalies, clusters, health
│   └── tests/
├── web/                                 # Angular 18 SPA
│   └── src/app/
│       ├── core/                        # ApiService, TypeScript-модели
│       ├── features/
│       │   ├── dashboard/               # KPI, графики, топ станций
│       │   ├── map/                     # Leaflet карта с кластерами
│       │   ├── station-details/         # Прогноз + часовой heatmap
│       │   └── anomalies/               # Таблица с фильтрами
│       └── shared/                      # KpiCardComponent и др.
├── datasets/                            # Выгруженные JSON'ы (gitignored)
├── notepads/                            # Jupyter-ноутбуки EDA
└── docs/                                # Roadmap, источники данных
```

---

## Предварительные требования

- **.NET 9 SDK** — `dotnet --version`
- **PostgreSQL 16** — запущен локально или через Docker
- **Python 3.11** — `python3 --version`
- **Node.js 20+** — `node --version`
- **Angular CLI 18** — `npm install -g @angular/cli`

---

## Быстрый старт

### 1. База данных

```bash
# Создать БД в PostgreSQL (psql или pgAdmin)
createdb metro_quality_monitor

# Строка подключения — в MetroQualityMonitor.Web/appsettings.Development.json
# и MetroQualityMonitor.DbMigrator/appsettings.Development.json:
# "DefaultConnection": "Host=localhost;Database=metro_quality_monitor;Username=postgres;Password=..."
```

### 2. Миграции и сидинг

```bash
dotnet run --project MetroQualityMonitor.DbMigrator
```

### 3. Загрузка данных

```bash
# Создать scripts/DataFetcher/appsettings.local.json с ключом API:
# { "MosApiKey": "<your_key>" }

dotnet run --project scripts/DataFetcher    # скачать JSON'ы в datasets/
dotnet run --project scripts/DataImporter   # залить в БД
```

### 4. .NET Web API

```bash
dotnet run --project MetroQualityMonitor.Web
# Swagger UI: http://localhost:5000/swagger
```

### 5. Python ML-сервис

```bash
cd ml-service
python3 -m venv .venv
source .venv/bin/activate          # Windows: .venv\Scripts\activate
pip install -r requirements.txt

# Создать .env (пример в .env.example):
# DATABASE_URL=postgresql+psycopg2://postgres:password@localhost/metro_quality_monitor

uvicorn app.main:app --reload --port 8000
# Health: http://localhost:8000/health
```

### 6. Первый пересчёт ML

```bash
# После запуска обоих сервисов:
curl -X POST http://localhost:5000/api/admin/recompute
# Или через Swagger: POST /api/admin/recompute
```

### 7. Angular SPA

```bash
cd web
npm install
ng serve
# Открыть: http://localhost:4200
```

---

## Команды разработки

### Backend (.NET)

```bash
# Сборка
dotnet build MetroQualityMonitor.sln

# Запуск API (dev-режим, с hot-reload)
dotnet watch --project MetroQualityMonitor.Web

# Добавить миграцию EF Core
dotnet ef migrations add <Name> \
  --project MetroQualityMonitor.Infrastructure \
  --startup-project MetroQualityMonitor.DbMigrator

# Применить миграции напрямую через EF
dotnet ef database update \
  --project MetroQualityMonitor.Infrastructure \
  --startup-project MetroQualityMonitor.DbMigrator

# Тесты
dotnet test
```

### Python ML-сервис

```bash
cd ml-service
source .venv/bin/activate

# Запуск сервиса
uvicorn app.main:app --reload --port 8000

# Тесты
pytest

# Принудительный пересчёт через API
curl -X POST http://localhost:8000/forecast/run-batch
curl -X POST http://localhost:8000/anomalies/detect
curl -X POST http://localhost:8000/clusters/recompute
```

### Frontend (Angular)

```bash
cd web

# Dev-сервер (с proxy на :5000)
ng serve

# Production-сборка
ng build

# Сборка в dev-режиме (без оптимизаций)
ng build --configuration=development

# Линтинг
ng lint
```

### Python-исследования (Jupyter)

```bash
source notepads/.venv/bin/activate
jupyter lab notepads/
```

---

## API endpoints (краткий обзор)

| Метод | Путь | Описание |
|---|---|---|
| GET | `/api/dashboard/kpi` | KPI-карточки главного экрана |
| GET | `/api/dashboard/top-stations` | Топ-N станций по метрике |
| GET | `/api/dashboard/seasonality` | Сезонность по кварталам |
| GET | `/api/lines` | Список линий |
| GET | `/api/stations` | Список станций (lite) |
| GET | `/api/stations/{id}` | Детали станции |
| GET | `/api/stations/{id}/flow` | Пассажиропоток по кварталам |
| GET | `/api/stations/{id}/forecast` | Прогноз на 4 квартала |
| GET | `/api/stations/{id}/hourly` | Часовая разбивка (heatmap) |
| GET | `/api/stations/{id}/anomalies` | Аномалии станции |
| GET | `/api/vestibules` | Вестибюли (с координатами) |
| GET | `/api/anomalies` | Все аномалии с фильтрами |
| PATCH | `/api/anomalies/{id}/acknowledge` | Подтвердить аномалию |
| GET | `/api/clusters` | Станции с кластерами (для карты) |
| POST | `/api/admin/recompute` | Принудительный пересчёт ML |

Полная документация: **http://localhost:5000/swagger**

---

## Источники данных

Портал открытых данных Москвы ([data.mos.ru](https://data.mos.ru)):

| Датасет | ID | Файл |
|---|---|---|
| Вестибюли и входы | 624 | `datasets/vestibules.json` |
| Линии метро | 2278 | `datasets/metro_lines.json` |
| Квартальный пассажиропоток | 62743 | `datasets/passenger_flow.json` |

API-ключ (`MosApiKey`) кладётся в `scripts/DataFetcher/appsettings.local.json` и не коммитится.

**Важно про гранулярность:** публичных дневных/часовых данных нет — только квартальные. Часовая аналитика строится через model-based деагрегацию с эмпирическими профилями (спальные/центральные/пересадочные станции × тип дня × час).

---

## Чеклист готовности к защите

- [ ] `dotnet run --project MetroQualityMonitor.Web` — Swagger открывается
- [ ] `uvicorn app.main:app --port 8000` — `/health` возвращает `{"status":"ok"}`
- [ ] `POST /api/admin/recompute` — отрабатывает без ошибок
- [ ] Таблицы `Forecasts`, `Anomalies`, `StationClusters` содержат данные
- [ ] `ng serve` — все 4 экрана открываются без ошибок в консоли
- [ ] График прогноза показывает доверительный интервал
- [ ] Часовой heatmap переключается между типами дней
- [ ] Карта показывает вестибюли с раскраской по кластерам
- [ ] Список аномалий фильтруется, подтверждение работает
- [ ] `dotnet test` — тесты проходят
- [ ] `pytest` — тесты проходят

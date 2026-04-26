using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MetroQualityMonitor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalyticsEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Anomalies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор записи"),
                    StationId = table.Column<short>(type: "smallint", nullable: false, comment: "Идентификатор станции метро"),
                    Year = table.Column<int>(type: "integer", nullable: false, comment: "Год наблюдения аномалии"),
                    Quarter = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "Квартал наблюдения аномалии (например, «Q1», «Q2»)"),
                    AnomalyType = table.Column<short>(type: "smallint", nullable: false, comment: "Тип аномалии"),
                    Severity = table.Column<short>(type: "smallint", nullable: false, comment: "Уровень серьёзности аномалии"),
                    Score = table.Column<double>(type: "double precision", nullable: false, comment: "Числовой показатель аномальности (например, Z-score или IF score)"),
                    ActualValue = table.Column<int>(type: "integer", nullable: false, comment: "Фактическое значение пассажиропотока"),
                    ExpectedValue = table.Column<int>(type: "integer", nullable: true, comment: "Ожидаемое значение пассажиропотока (расчётное)"),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "Текстовое описание аномалии"),
                    IsAcknowledged = table.Column<bool>(type: "boolean", nullable: false, comment: "Признак подтверждения аномалии оператором"),
                    AcknowledgedDateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Дата и время подтверждения аномалии (UTC)"),
                    CreateDateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Дата и время создания записи (UTC)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anomalies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Anomalies_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Обнаруженная аномалия пассажиропотока по станции за квартал");

            migrationBuilder.CreateTable(
                name: "Forecasts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор записи"),
                    LineId = table.Column<short>(type: "smallint", nullable: true, comment: "Идентификатор линии метро (заполняется для прогноза на уровне линии)"),
                    StationId = table.Column<short>(type: "smallint", nullable: true, comment: "Идентификатор станции метро (заполняется для прогноза на уровне станции)"),
                    Year = table.Column<int>(type: "integer", nullable: false, comment: "Год прогноза"),
                    Quarter = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "Квартал прогноза (например, «Q1», «Q2»)"),
                    PredictedIncoming = table.Column<int>(type: "integer", nullable: false, comment: "Прогнозируемое количество входящих пассажиров"),
                    PredictedOutgoing = table.Column<int>(type: "integer", nullable: false, comment: "Прогнозируемое количество исходящих пассажиров"),
                    ConfidenceLowerIncoming = table.Column<int>(type: "integer", nullable: true, comment: "Нижняя граница доверительного интервала для входящих пассажиров"),
                    ConfidenceUpperIncoming = table.Column<int>(type: "integer", nullable: true, comment: "Верхняя граница доверительного интервала для входящих пассажиров"),
                    ConfidenceLowerOutgoing = table.Column<int>(type: "integer", nullable: true, comment: "Нижняя граница доверительного интервала для исходящих пассажиров"),
                    ConfidenceUpperOutgoing = table.Column<int>(type: "integer", nullable: true, comment: "Верхняя граница доверительного интервала для исходящих пассажиров"),
                    ModelName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "Название модели прогнозирования (например, «SARIMA», «Prophet»)"),
                    ModelVersion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "Версия модели прогнозирования (например, «v1.0»)"),
                    CreateDateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Дата и время создания записи (UTC)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Forecasts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Forecasts_Lines_LineId",
                        column: x => x.LineId,
                        principalTable: "Lines",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Forecasts_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id");
                },
                comment: "Прогноз пассажиропотока на квартал по линии или станции");

            migrationBuilder.CreateTable(
                name: "HourlyProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор записи")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StationCategory = table.Column<short>(type: "smallint", nullable: false, comment: "Категория станции"),
                    DayType = table.Column<short>(type: "smallint", nullable: false, comment: "Тип дня"),
                    Hour = table.Column<int>(type: "integer", nullable: false, comment: "Час суток (0–23)"),
                    IncomingShare = table.Column<double>(type: "double precision", nullable: false, comment: "Доля входящих пассажиров для данного часа (сумма по всем часам дня = 1.0)"),
                    OutgoingShare = table.Column<double>(type: "double precision", nullable: false, comment: "Доля исходящих пассажиров для данного часа (сумма по всем часам дня = 1.0)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HourlyProfiles", x => x.Id);
                },
                comment: "Справочник эмпирических часовых профилей пассажиропотока по категории станции и типу дня");

            migrationBuilder.CreateTable(
                name: "StationClusters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор записи")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StationId = table.Column<short>(type: "smallint", nullable: false, comment: "Идентификатор станции метро"),
                    ClusterId = table.Column<int>(type: "integer", nullable: false, comment: "Числовой идентификатор кластера"),
                    ClusterLabel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "Метка кластера (например, «Residential», «Central»)"),
                    ComputedAtDateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Дата и время вычисления кластеризации (UTC)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StationClusters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StationClusters_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Результат кластеризации станции метро по характеру пассажиропотока");

            migrationBuilder.CreateIndex(
                name: "IX_Anomalies_CreateDateTimeUtc",
                table: "Anomalies",
                column: "CreateDateTimeUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Anomalies_IsAcknowledged",
                table: "Anomalies",
                column: "IsAcknowledged");

            migrationBuilder.CreateIndex(
                name: "IX_Anomalies_Severity",
                table: "Anomalies",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_Anomalies_StationId_Year_Quarter",
                table: "Anomalies",
                columns: new[] { "StationId", "Year", "Quarter" });

            migrationBuilder.CreateIndex(
                name: "IX_Forecasts_CreateDateTimeUtc",
                table: "Forecasts",
                column: "CreateDateTimeUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Forecasts_LineId_Year_Quarter",
                table: "Forecasts",
                columns: new[] { "LineId", "Year", "Quarter" });

            migrationBuilder.CreateIndex(
                name: "IX_Forecasts_StationId_Year_Quarter",
                table: "Forecasts",
                columns: new[] { "StationId", "Year", "Quarter" });

            migrationBuilder.CreateIndex(
                name: "IX_HourlyProfiles_StationCategory_DayType_Hour",
                table: "HourlyProfiles",
                columns: new[] { "StationCategory", "DayType", "Hour" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StationClusters_ComputedAtDateTimeUtc",
                table: "StationClusters",
                column: "ComputedAtDateTimeUtc");

            migrationBuilder.CreateIndex(
                name: "IX_StationClusters_StationId",
                table: "StationClusters",
                column: "StationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Anomalies");

            migrationBuilder.DropTable(
                name: "Forecasts");

            migrationBuilder.DropTable(
                name: "HourlyProfiles");

            migrationBuilder.DropTable(
                name: "StationClusters");
        }
    }
}

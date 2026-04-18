using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MetroQualityMonitor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVestibulesAndPassengerFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "GlobalId",
                table: "Lines",
                type: "bigint",
                nullable: true,
                comment: "Глобальный идентификатор записи");

            migrationBuilder.AddColumn<int>(
                name: "MosDataId",
                table: "Lines",
                type: "integer",
                nullable: true,
                comment: "Идентификатор в реестре открытых данных Москвы");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Lines",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "Статус линии");

            migrationBuilder.CreateTable(
                name: "PassengerFlowRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор записи")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GlobalId = table.Column<long>(type: "bigint", nullable: false, comment: "Глобальный идентификатор записи"),
                    Year = table.Column<int>(type: "integer", nullable: false, comment: "Год"),
                    Quarter = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "Квартал"),
                    IncomingPassengers = table.Column<int>(type: "integer", nullable: false, comment: "Количество входящих пассажиров"),
                    OutgoingPassengers = table.Column<int>(type: "integer", nullable: false, comment: "Количество исходящих пассажиров"),
                    StationId = table.Column<short>(type: "smallint", nullable: true, comment: "Идентификатор станции метро"),
                    LineId = table.Column<short>(type: "smallint", nullable: true, comment: "Идентификатор линии метро")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PassengerFlowRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PassengerFlowRecords_Lines_LineId",
                        column: x => x.LineId,
                        principalTable: "Lines",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PassengerFlowRecords_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id");
                },
                comment: "Пассажиропоток по станции метро за квартал");

            migrationBuilder.CreateTable(
                name: "Vestibules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор записи")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MosDataId = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор в реестре открытых данных Москвы"),
                    GlobalId = table.Column<long>(type: "bigint", nullable: false, comment: "Глобальный идентификатор записи"),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "Наименование вестибюля"),
                    OnTerritoryOfMoscow = table.Column<bool>(type: "boolean", nullable: true, comment: "Расположен на территории Москвы"),
                    AdmArea = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true, comment: "Административный округ"),
                    District = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true, comment: "Район"),
                    LongitudeWgs84 = table.Column<double>(type: "double precision", nullable: true, comment: "Долгота (WGS84)"),
                    LatitudeWgs84 = table.Column<double>(type: "double precision", nullable: true, comment: "Широта (WGS84)"),
                    VestibuleType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Тип вестибюля"),
                    CulturalHeritageSiteStatus = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true, comment: "Статус объекта культурного наследия"),
                    ModeOnEvenDays = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true, comment: "Режим работы по чётным дням"),
                    ModeOnOddDays = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true, comment: "Режим работы по нечётным дням"),
                    FullFeaturedBPAAmount = table.Column<int>(type: "integer", nullable: true, comment: "Количество полнофункциональных БПА"),
                    LittleFunctionalBPAAmount = table.Column<int>(type: "integer", nullable: true, comment: "Количество малофункциональных БПА"),
                    BPAAmount = table.Column<int>(type: "integer", nullable: true, comment: "Общее количество БПА"),
                    ObjectStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Статус объекта"),
                    ReleaseNumber = table.Column<int>(type: "integer", nullable: true, comment: "Номер релиза набора данных"),
                    VersionNumber = table.Column<int>(type: "integer", nullable: true, comment: "Номер версии набора данных"),
                    StationId = table.Column<short>(type: "smallint", nullable: true, comment: "Идентификатор станции метро"),
                    LineId = table.Column<short>(type: "smallint", nullable: true, comment: "Идентификатор линии метро")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vestibules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vestibules_Lines_LineId",
                        column: x => x.LineId,
                        principalTable: "Lines",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Vestibules_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id");
                },
                comment: "Вестибюль (вход/выход) станции метро");

            migrationBuilder.CreateTable(
                name: "EscalatorRepairs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор записи")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GlobalId = table.Column<long>(type: "bigint", nullable: false, comment: "Глобальный идентификатор записи"),
                    RepairPeriod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "Период ремонта (вида \"27.02.2023-30.05.2023\")"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: "Признак удалённой записи"),
                    VestibuleId = table.Column<int>(type: "integer", nullable: false, comment: "Идентификатор вестибюля")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EscalatorRepairs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EscalatorRepairs_Vestibules_VestibuleId",
                        column: x => x.VestibuleId,
                        principalTable: "Vestibules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Сведения о ремонте эскалаторов вестибюля");

            migrationBuilder.CreateIndex(
                name: "IX_Lines_GlobalId",
                table: "Lines",
                column: "GlobalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lines_MosDataId",
                table: "Lines",
                column: "MosDataId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EscalatorRepairs_GlobalId",
                table: "EscalatorRepairs",
                column: "GlobalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EscalatorRepairs_VestibuleId",
                table: "EscalatorRepairs",
                column: "VestibuleId");

            migrationBuilder.CreateIndex(
                name: "IX_PassengerFlowRecords_GlobalId",
                table: "PassengerFlowRecords",
                column: "GlobalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PassengerFlowRecords_LineId",
                table: "PassengerFlowRecords",
                column: "LineId");

            migrationBuilder.CreateIndex(
                name: "IX_PassengerFlowRecords_StationId_Year_Quarter",
                table: "PassengerFlowRecords",
                columns: new[] { "StationId", "Year", "Quarter" });

            migrationBuilder.CreateIndex(
                name: "IX_Vestibules_GlobalId",
                table: "Vestibules",
                column: "GlobalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vestibules_LineId",
                table: "Vestibules",
                column: "LineId");

            migrationBuilder.CreateIndex(
                name: "IX_Vestibules_MosDataId",
                table: "Vestibules",
                column: "MosDataId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vestibules_StationId",
                table: "Vestibules",
                column: "StationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EscalatorRepairs");

            migrationBuilder.DropTable(
                name: "PassengerFlowRecords");

            migrationBuilder.DropTable(
                name: "Vestibules");

            migrationBuilder.DropIndex(
                name: "IX_Lines_GlobalId",
                table: "Lines");

            migrationBuilder.DropIndex(
                name: "IX_Lines_MosDataId",
                table: "Lines");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                table: "Lines");

            migrationBuilder.DropColumn(
                name: "MosDataId",
                table: "Lines");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Lines");
        }
    }
}

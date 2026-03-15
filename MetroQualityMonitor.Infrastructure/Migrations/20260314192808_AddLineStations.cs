using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MetroQualityMonitor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLineStations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataImportRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор записи"),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "Наименование исходного файла"),
                    BlobName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "Ключ файла в хранилище"),
                    SourceType = table.Column<short>(type: "smallint", nullable: false, comment: "Метод загрузки данных"),
                    DataType = table.Column<short>(type: "smallint", nullable: false, comment: "Тип загруженного справочника"),
                    Status = table.Column<short>(type: "smallint", nullable: false, comment: "Статус загрузки и обработки данных"),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true, comment: "Сообщение ошибки"),
                    RowCount = table.Column<int>(type: "integer", nullable: true, comment: "Количество загруженных строк"),
                    Sha256Hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "SHA256 хэш"),
                    LoadedDateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Дата и время (UTC) загрузки файла"),
                    ProcessingStartDateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Дата и время (UTC) начала обработки"),
                    ProcessingFinishedDateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Дата и время (UTC) завершения обработки"),
                    CreateDateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Дата и время (UTC) создания записи")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataImportRuns", x => x.Id);
                },
                comment: "Журнал загрузок исходных наборов данных");

            migrationBuilder.CreateTable(
                name: "Lines",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false, comment: "Идентификатор записи")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Наименование")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lines", x => x.Id);
                },
                comment: "Линия метро");

            migrationBuilder.CreateTable(
                name: "Stations",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false, comment: "Идентификатор записи")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Наименование")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stations", x => x.Id);
                },
                comment: "Станция метро");

            migrationBuilder.CreateTable(
                name: "LineStation",
                columns: table => new
                {
                    LinesId = table.Column<short>(type: "smallint", nullable: false),
                    StationsId = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineStation", x => new { x.LinesId, x.StationsId });
                    table.ForeignKey(
                        name: "FK_LineStation_Lines_LinesId",
                        column: x => x.LinesId,
                        principalTable: "Lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LineStation_Stations_StationsId",
                        column: x => x.StationsId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DataImportRuns_DataType",
                table: "DataImportRuns",
                column: "DataType");

            migrationBuilder.CreateIndex(
                name: "IX_DataImportRuns_LoadedDateTimeUtc",
                table: "DataImportRuns",
                column: "LoadedDateTimeUtc");

            migrationBuilder.CreateIndex(
                name: "IX_DataImportRuns_Sha256Hash",
                table: "DataImportRuns",
                column: "Sha256Hash");

            migrationBuilder.CreateIndex(
                name: "IX_DataImportRuns_Status",
                table: "DataImportRuns",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Lines_Name",
                table: "Lines",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LineStation_StationsId",
                table: "LineStation",
                column: "StationsId");

            migrationBuilder.CreateIndex(
                name: "IX_Stations_Name",
                table: "Stations",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataImportRuns");

            migrationBuilder.DropTable(
                name: "LineStation");

            migrationBuilder.DropTable(
                name: "Lines");

            migrationBuilder.DropTable(
                name: "Stations");
        }
    }
}

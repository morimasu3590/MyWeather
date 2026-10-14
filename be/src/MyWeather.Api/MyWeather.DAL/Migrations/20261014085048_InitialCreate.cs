using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MyWeather.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CountryId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cities_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FavoriteCities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Notes = table.Column<string>(type: "TEXT", nullable: false),
                    CityId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoriteCities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FavoriteCities_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeatherRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CityId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    WindSpeed = table.Column<double>(type: "REAL", nullable: false),
                    WindDirection = table.Column<double>(type: "REAL", nullable: false),
                    TemperatureInC = table.Column<double>(type: "REAL", nullable: false),
                    DewPoint = table.Column<double>(type: "REAL", nullable: false),
                    Humidity = table.Column<double>(type: "REAL", nullable: false),
                    Pressure = table.Column<double>(type: "REAL", nullable: false),
                    Visibility = table.Column<double>(type: "REAL", nullable: false),
                    Cloudiness = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    WeatherDescription = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeatherRecords_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { 1, "ID", "Indonesia" },
                    { 2, "SG", "Singapore" },
                    { 3, "MY", "Malaysia" },
                    { 4, "AU", "Australia" },
                    { 5, "US", "United States" },
                    { 6, "GB", "United Kingdom" },
                    { 7, "CA", "Canada" },
                    { 8, "JP", "Japan" },
                    { 9, "DE", "Germany" },
                    { 10, "FR", "France" },
                    { 11, "IN", "India" },
                    { 12, "CN", "China" },
                    { 13, "BR", "Brazil" },
                    { 14, "NO", "Norway" },
                    { 15, "NZ", "New Zealand" },
                    { 16, "TH", "Thailand" },
                    { 17, "KR", "South Korea" },
                    { 18, "IT", "Italy" },
                    { 19, "ES", "Spain" },
                    { 20, "EG", "Egypt" }
                });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Name" },
                values: new object[,]
                {
                    { 1, 1, "Jakarta" },
                    { 2, 2, "Singapore" },
                    { 3, 3, "Kuala Lumpur" },
                    { 4, 4, "Canberra" },
                    { 5, 5, "Washington, D.C." },
                    { 6, 6, "London" },
                    { 7, 7, "Ottawa" },
                    { 8, 8, "Tokyo" },
                    { 9, 9, "Berlin" },
                    { 10, 10, "Paris" },
                    { 11, 11, "New Delhi" },
                    { 12, 12, "Beijing" },
                    { 13, 13, "Brasília" },
                    { 14, 14, "Oslo" },
                    { 15, 15, "Wellington" },
                    { 16, 16, "Bangkok" },
                    { 17, 17, "Seoul" },
                    { 18, 18, "Rome" },
                    { 19, 19, "Madrid" },
                    { 20, 20, "Cairo" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cities_CountryId_Name",
                table: "Cities",
                columns: new[] { "CountryId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Code",
                table: "Countries",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteCities_CityId",
                table: "FavoriteCities",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_WeatherRecords_CityId_Date",
                table: "WeatherRecords",
                columns: new[] { "CityId", "Date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FavoriteCities");

            migrationBuilder.DropTable(
                name: "WeatherRecords");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "Countries");
        }
    }
}

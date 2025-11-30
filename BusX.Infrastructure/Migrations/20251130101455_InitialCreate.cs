using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable



namespace BusX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Stations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    City = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Pnr = table.Column<string>(type: "TEXT", nullable: false),
                    JourneyId = table.Column<int>(type: "INTEGER", nullable: false),
                    SeatId = table.Column<int>(type: "INTEGER", nullable: false),
                    PassengerName = table.Column<string>(type: "TEXT", nullable: false),
                    PassengerTc = table.Column<string>(type: "TEXT", nullable: false),
                    PassengerGender = table.Column<int>(type: "INTEGER", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Journeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FromStationId = table.Column<int>(type: "INTEGER", nullable: false),
                    ToStationId = table.Column<int>(type: "INTEGER", nullable: false),
                    Departure = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ArrivalEstimate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProviderName = table.Column<string>(type: "TEXT", nullable: false),
                    BasePrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Journeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Journeys_Stations_FromStationId",
                        column: x => x.FromStationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Journeys_Stations_ToStationId",
                        column: x => x.ToStationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Seats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JourneyId = table.Column<int>(type: "INTEGER", nullable: false),
                    SeatNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Row = table.Column<int>(type: "INTEGER", nullable: false),
                    Column = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    IsSold = table.Column<bool>(type: "INTEGER", nullable: false),
                    GenderLock = table.Column<int>(type: "INTEGER", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Seats_Journeys_JourneyId",
                        column: x => x.JourneyId,
                        principalTable: "Journeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Stations",
                columns: new[] { "Id", "City", "CreatedAt", "Name" },
                values: new object[,]
                {
                    { 1, "Istanbul", new DateTime(2025, 11, 30, 10, 14, 55, 8, DateTimeKind.Utc).AddTicks(367), "Esenler Otogarı" },
                    { 2, "Ankara", new DateTime(2025, 11, 30, 10, 14, 55, 8, DateTimeKind.Utc).AddTicks(370), "AŞTİ" },
                    { 3, "Izmir", new DateTime(2025, 11, 30, 10, 14, 55, 8, DateTimeKind.Utc).AddTicks(371), "İzotaş" }
                });

            migrationBuilder.InsertData(
                table: "Journeys",
                columns: new[] { "Id", "ArrivalEstimate", "BasePrice", "CreatedAt", "Departure", "FromStationId", "ProviderName", "ToStationId" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 12, 1, 16, 0, 0, 0, DateTimeKind.Utc), 500m, new DateTime(2025, 11, 30, 10, 14, 55, 8, DateTimeKind.Utc).AddTicks(503), new DateTime(2025, 12, 1, 10, 0, 0, 0, DateTimeKind.Utc), 1, "ProviderA", 2 },
                    { 2, new DateTime(2025, 12, 1, 18, 0, 0, 0, DateTimeKind.Utc), 450m, new DateTime(2025, 11, 30, 10, 14, 55, 8, DateTimeKind.Utc).AddTicks(523), new DateTime(2025, 12, 1, 11, 0, 0, 0, DateTimeKind.Utc), 1, "ProviderB", 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Journeys_FromStationId",
                table: "Journeys",
                column: "FromStationId");

            migrationBuilder.CreateIndex(
                name: "IX_Journeys_ToStationId",
                table: "Journeys",
                column: "ToStationId");

            migrationBuilder.CreateIndex(
                name: "IX_Seats_JourneyId",
                table: "Seats",
                column: "JourneyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Seats");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "Journeys");

            migrationBuilder.DropTable(
                name: "Stations");
        }
    }
}

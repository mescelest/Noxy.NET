using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Noxy.NET.EntityManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataElement",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    SchemaIdentifier = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataElement", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DataPropertyBoolean",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerID = table.Column<Guid>(type: "TEXT", nullable: false),
                    PropertyIdentifier = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Value = table.Column<bool>(type: "INTEGER", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataPropertyBoolean", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DataPropertyDateTime",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerID = table.Column<Guid>(type: "TEXT", nullable: false),
                    PropertyIdentifier = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Value = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataPropertyDateTime", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DataPropertyDecimal",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerID = table.Column<Guid>(type: "TEXT", nullable: false),
                    PropertyIdentifier = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Value = table.Column<decimal>(type: "TEXT", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataPropertyDecimal", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DataPropertyImage",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerID = table.Column<Guid>(type: "TEXT", nullable: false),
                    PropertyIdentifier = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 1000000, nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataPropertyImage", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DataPropertyInteger",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerID = table.Column<Guid>(type: "TEXT", nullable: false),
                    PropertyIdentifier = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Value = table.Column<long>(type: "INTEGER", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataPropertyInteger", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DataPropertyString",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerID = table.Column<Guid>(type: "TEXT", nullable: false),
                    PropertyIdentifier = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 1000000, nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataPropertyString", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DataRelation",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    RelationIdentifier = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FromElementID = table.Column<Guid>(type: "TEXT", nullable: false),
                    ToElementID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataRelation", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DataTextParameter",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TextIdentifier = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Culture = table.Column<string>(type: "TEXT", maxLength: 35, nullable: false, collation: "NOCASE"),
                    Value = table.Column<string>(type: "TEXT", maxLength: 1000000, nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataTextParameter", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "HistoryElement",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    SchemaIdentifier = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeEnded = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndedByUserID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryElement", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "HistoryPropertyBoolean",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerID = table.Column<Guid>(type: "TEXT", nullable: false),
                    PropertyIdentifier = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Value = table.Column<bool>(type: "INTEGER", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeEnded = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndedByUserID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryPropertyBoolean", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "HistoryPropertyDateTime",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerID = table.Column<Guid>(type: "TEXT", nullable: false),
                    PropertyIdentifier = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Value = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeEnded = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndedByUserID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryPropertyDateTime", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "HistoryPropertyDecimal",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerID = table.Column<Guid>(type: "TEXT", nullable: false),
                    PropertyIdentifier = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Value = table.Column<decimal>(type: "TEXT", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeEnded = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndedByUserID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryPropertyDecimal", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "HistoryPropertyImage",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerID = table.Column<Guid>(type: "TEXT", nullable: false),
                    PropertyIdentifier = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 1000000, nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeEnded = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndedByUserID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryPropertyImage", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "HistoryPropertyInteger",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerID = table.Column<Guid>(type: "TEXT", nullable: false),
                    PropertyIdentifier = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Value = table.Column<long>(type: "INTEGER", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeEnded = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndedByUserID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryPropertyInteger", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "HistoryPropertyString",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerID = table.Column<Guid>(type: "TEXT", nullable: false),
                    PropertyIdentifier = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 1000000, nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeEnded = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndedByUserID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryPropertyString", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "HistoryRelation",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    RelationIdentifier = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FromElementID = table.Column<Guid>(type: "TEXT", nullable: false),
                    ToElementID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeEnded = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndedByUserID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryRelation", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "HistoryTextParameter",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TextIdentifier = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Culture = table.Column<string>(type: "TEXT", maxLength: 35, nullable: false, collation: "NOCASE"),
                    Value = table.Column<string>(type: "TEXT", maxLength: 1000000, nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeEnded = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndedByUserID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryTextParameter", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DataElement_SchemaIdentifier_TimeCreated",
                table: "DataElement",
                columns: new[] { "SchemaIdentifier", "TimeCreated" });

            migrationBuilder.CreateIndex(
                name: "IX_DataPropertyBoolean_OwnerID_PropertyIdentifier",
                table: "DataPropertyBoolean",
                columns: new[] { "OwnerID", "PropertyIdentifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataPropertyDateTime_OwnerID_PropertyIdentifier",
                table: "DataPropertyDateTime",
                columns: new[] { "OwnerID", "PropertyIdentifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataPropertyDecimal_OwnerID_PropertyIdentifier",
                table: "DataPropertyDecimal",
                columns: new[] { "OwnerID", "PropertyIdentifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataPropertyImage_OwnerID_PropertyIdentifier",
                table: "DataPropertyImage",
                columns: new[] { "OwnerID", "PropertyIdentifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataPropertyInteger_OwnerID_PropertyIdentifier",
                table: "DataPropertyInteger",
                columns: new[] { "OwnerID", "PropertyIdentifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataPropertyString_OwnerID_PropertyIdentifier",
                table: "DataPropertyString",
                columns: new[] { "OwnerID", "PropertyIdentifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataRelation_FromElementID_RelationIdentifier",
                table: "DataRelation",
                columns: new[] { "FromElementID", "RelationIdentifier" });

            migrationBuilder.CreateIndex(
                name: "IX_DataRelation_RelationIdentifier_FromElementID_ToElementID",
                table: "DataRelation",
                columns: new[] { "RelationIdentifier", "FromElementID", "ToElementID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataRelation_ToElementID_RelationIdentifier",
                table: "DataRelation",
                columns: new[] { "ToElementID", "RelationIdentifier" });

            migrationBuilder.CreateIndex(
                name: "IX_DataTextParameter_Culture",
                table: "DataTextParameter",
                column: "Culture");

            migrationBuilder.CreateIndex(
                name: "IX_DataTextParameter_TextIdentifier_Culture",
                table: "DataTextParameter",
                columns: new[] { "TextIdentifier", "Culture" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HistoryElement_SchemaIdentifier_TimeEnded",
                table: "HistoryElement",
                columns: new[] { "SchemaIdentifier", "TimeEnded" });

            migrationBuilder.CreateIndex(
                name: "IX_HistoryPropertyBoolean_OwnerID_TimeEnded",
                table: "HistoryPropertyBoolean",
                columns: new[] { "OwnerID", "TimeEnded" });

            migrationBuilder.CreateIndex(
                name: "IX_HistoryPropertyDateTime_OwnerID_TimeEnded",
                table: "HistoryPropertyDateTime",
                columns: new[] { "OwnerID", "TimeEnded" });

            migrationBuilder.CreateIndex(
                name: "IX_HistoryPropertyDecimal_OwnerID_TimeEnded",
                table: "HistoryPropertyDecimal",
                columns: new[] { "OwnerID", "TimeEnded" });

            migrationBuilder.CreateIndex(
                name: "IX_HistoryPropertyImage_OwnerID_TimeEnded",
                table: "HistoryPropertyImage",
                columns: new[] { "OwnerID", "TimeEnded" });

            migrationBuilder.CreateIndex(
                name: "IX_HistoryPropertyInteger_OwnerID_TimeEnded",
                table: "HistoryPropertyInteger",
                columns: new[] { "OwnerID", "TimeEnded" });

            migrationBuilder.CreateIndex(
                name: "IX_HistoryPropertyString_OwnerID_TimeEnded",
                table: "HistoryPropertyString",
                columns: new[] { "OwnerID", "TimeEnded" });

            migrationBuilder.CreateIndex(
                name: "IX_HistoryRelation_FromElementID_TimeEnded",
                table: "HistoryRelation",
                columns: new[] { "FromElementID", "TimeEnded" });

            migrationBuilder.CreateIndex(
                name: "IX_HistoryRelation_ToElementID_TimeEnded",
                table: "HistoryRelation",
                columns: new[] { "ToElementID", "TimeEnded" });

            migrationBuilder.CreateIndex(
                name: "IX_HistoryTextParameter_TextIdentifier_Culture_TimeEnded",
                table: "HistoryTextParameter",
                columns: new[] { "TextIdentifier", "Culture", "TimeEnded" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataElement");

            migrationBuilder.DropTable(
                name: "DataPropertyBoolean");

            migrationBuilder.DropTable(
                name: "DataPropertyDateTime");

            migrationBuilder.DropTable(
                name: "DataPropertyDecimal");

            migrationBuilder.DropTable(
                name: "DataPropertyImage");

            migrationBuilder.DropTable(
                name: "DataPropertyInteger");

            migrationBuilder.DropTable(
                name: "DataPropertyString");

            migrationBuilder.DropTable(
                name: "DataRelation");

            migrationBuilder.DropTable(
                name: "DataTextParameter");

            migrationBuilder.DropTable(
                name: "HistoryElement");

            migrationBuilder.DropTable(
                name: "HistoryPropertyBoolean");

            migrationBuilder.DropTable(
                name: "HistoryPropertyDateTime");

            migrationBuilder.DropTable(
                name: "HistoryPropertyDecimal");

            migrationBuilder.DropTable(
                name: "HistoryPropertyImage");

            migrationBuilder.DropTable(
                name: "HistoryPropertyInteger");

            migrationBuilder.DropTable(
                name: "HistoryPropertyString");

            migrationBuilder.DropTable(
                name: "HistoryRelation");

            migrationBuilder.DropTable(
                name: "HistoryTextParameter");
        }
    }
}

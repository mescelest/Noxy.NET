using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Noxy.NET.EntityManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TableAuthentication",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Salt = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Hash = table.Column<byte[]>(type: "BLOB", nullable: false),
                    UserID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableAuthentication", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TableDataElement",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SchemaIdentifier = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableDataElement", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TableDataStyleParameter",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SchemaIdentifier = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    TimeApproved = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TimeEffective = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableDataStyleParameter", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TableDataSystemParameter",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SchemaIdentifier = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    TimeApproved = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TimeEffective = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableDataSystemParameter", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TableDataTextParameter",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SchemaIdentifier = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    TimeApproved = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TimeEffective = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableDataTextParameter", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TableSchema",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    TimeActivated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableSchema", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TableUser",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "varchar(256)", nullable: false),
                    TimeSignIn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeVerified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuthenticationID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableUser", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TableUser_TableAuthentication_AuthenticationID",
                        column: x => x.AuthenticationID,
                        principalTable: "TableAuthentication",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TableDataPropertyBoolean",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    ElementID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Value = table.Column<bool>(type: "INTEGER", nullable: true),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SchemaIdentifier = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableDataPropertyBoolean", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TableDataPropertyBoolean_TableDataElement_ElementID",
                        column: x => x.ElementID,
                        principalTable: "TableDataElement",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TableDataPropertyDateTime",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    ElementID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Value = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SchemaIdentifier = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableDataPropertyDateTime", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TableDataPropertyDateTime_TableDataElement_ElementID",
                        column: x => x.ElementID,
                        principalTable: "TableDataElement",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TableDataPropertyString",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    ElementID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SchemaIdentifier = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableDataPropertyString", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TableDataPropertyString_TableDataElement_ElementID",
                        column: x => x.ElementID,
                        principalTable: "TableDataElement",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TableSchemaParameterStyle",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsApprovalRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    SchemaIdentifier = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SchemaID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableSchemaParameterStyle", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TableSchemaParameterStyle_TableSchema_SchemaID",
                        column: x => x.SchemaID,
                        principalTable: "TableSchema",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TableSchemaParameterSystem",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsApprovalRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    SchemaIdentifier = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SchemaID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableSchemaParameterSystem", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TableSchemaParameterSystem_TableSchema_SchemaID",
                        column: x => x.SchemaID,
                        principalTable: "TableSchema",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TableSchemaParameterText",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsApprovalRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    SchemaIdentifier = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SchemaID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableSchemaParameterText", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TableSchemaParameterText_TableSchema_SchemaID",
                        column: x => x.SchemaID,
                        principalTable: "TableSchema",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TableIdentity",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Handle = table.Column<string>(type: "varchar(32)", nullable: false),
                    Username = table.Column<string>(type: "varchar(32)", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeSignIn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableIdentity", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TableIdentity_TableUser_UserID",
                        column: x => x.UserID,
                        principalTable: "TableUser",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TableSchemaContext",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    SchemaIdentifier = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SchemaID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TitleTextParameterID = table.Column<Guid>(type: "TEXT", nullable: false),
                    DescriptionTextParameterID = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableSchemaContext", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TableSchemaContext_TableSchemaParameterText_DescriptionTextParameterID",
                        column: x => x.DescriptionTextParameterID,
                        principalTable: "TableSchemaParameterText",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TableSchemaContext_TableSchemaParameterText_TitleTextParameterID",
                        column: x => x.TitleTextParameterID,
                        principalTable: "TableSchemaParameterText",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TableSchemaContext_TableSchema_SchemaID",
                        column: x => x.SchemaID,
                        principalTable: "TableSchema",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TableSchemaElement",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    SchemaIdentifier = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SchemaID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TitleTextParameterID = table.Column<Guid>(type: "TEXT", nullable: false),
                    DescriptionTextParameterID = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableSchemaElement", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TableSchemaElement_TableSchemaParameterText_DescriptionTextParameterID",
                        column: x => x.DescriptionTextParameterID,
                        principalTable: "TableSchemaParameterText",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TableSchemaElement_TableSchemaParameterText_TitleTextParameterID",
                        column: x => x.TitleTextParameterID,
                        principalTable: "TableSchemaParameterText",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TableSchemaElement_TableSchema_SchemaID",
                        column: x => x.SchemaID,
                        principalTable: "TableSchema",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TableSchemaPropertyBoolean",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    SchemaIdentifier = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SchemaID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TitleTextParameterID = table.Column<Guid>(type: "TEXT", nullable: false),
                    DescriptionTextParameterID = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableSchemaPropertyBoolean", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyBoolean_TableSchemaParameterText_DescriptionTextParameterID",
                        column: x => x.DescriptionTextParameterID,
                        principalTable: "TableSchemaParameterText",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyBoolean_TableSchemaParameterText_TitleTextParameterID",
                        column: x => x.TitleTextParameterID,
                        principalTable: "TableSchemaParameterText",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyBoolean_TableSchema_SchemaID",
                        column: x => x.SchemaID,
                        principalTable: "TableSchema",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TableSchemaPropertyCollection",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    SchemaIdentifier = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SchemaID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TitleTextParameterID = table.Column<Guid>(type: "TEXT", nullable: false),
                    DescriptionTextParameterID = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableSchemaPropertyCollection", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyCollection_TableSchemaParameterText_DescriptionTextParameterID",
                        column: x => x.DescriptionTextParameterID,
                        principalTable: "TableSchemaParameterText",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyCollection_TableSchemaParameterText_TitleTextParameterID",
                        column: x => x.TitleTextParameterID,
                        principalTable: "TableSchemaParameterText",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyCollection_TableSchema_SchemaID",
                        column: x => x.SchemaID,
                        principalTable: "TableSchema",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TableSchemaPropertyDateTime",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    SchemaIdentifier = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SchemaID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TitleTextParameterID = table.Column<Guid>(type: "TEXT", nullable: false),
                    DescriptionTextParameterID = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableSchemaPropertyDateTime", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyDateTime_TableSchemaParameterText_DescriptionTextParameterID",
                        column: x => x.DescriptionTextParameterID,
                        principalTable: "TableSchemaParameterText",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyDateTime_TableSchemaParameterText_TitleTextParameterID",
                        column: x => x.TitleTextParameterID,
                        principalTable: "TableSchemaParameterText",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyDateTime_TableSchema_SchemaID",
                        column: x => x.SchemaID,
                        principalTable: "TableSchema",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TableSchemaPropertyString",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    SchemaIdentifier = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SchemaID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TitleTextParameterID = table.Column<Guid>(type: "TEXT", nullable: false),
                    DescriptionTextParameterID = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableSchemaPropertyString", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyString_TableSchemaParameterText_DescriptionTextParameterID",
                        column: x => x.DescriptionTextParameterID,
                        principalTable: "TableSchemaParameterText",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyString_TableSchemaParameterText_TitleTextParameterID",
                        column: x => x.TitleTextParameterID,
                        principalTable: "TableSchemaParameterText",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyString_TableSchema_SchemaID",
                        column: x => x.SchemaID,
                        principalTable: "TableSchema",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TableSchemaPropertyTable",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    SchemaIdentifier = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SchemaID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TitleTextParameterID = table.Column<Guid>(type: "TEXT", nullable: false),
                    DescriptionTextParameterID = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableSchemaPropertyTable", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyTable_TableSchemaParameterText_DescriptionTextParameterID",
                        column: x => x.DescriptionTextParameterID,
                        principalTable: "TableSchemaParameterText",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyTable_TableSchemaParameterText_TitleTextParameterID",
                        column: x => x.TitleTextParameterID,
                        principalTable: "TableSchemaParameterText",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyTable_TableSchema_SchemaID",
                        column: x => x.SchemaID,
                        principalTable: "TableSchema",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TableJunctionSchemaContextHasElement",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EntityID = table.Column<Guid>(type: "TEXT", nullable: false),
                    RelationID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableJunctionSchemaContextHasElement", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TableJunctionSchemaContextHasElement_TableSchemaContext_EntityID",
                        column: x => x.EntityID,
                        principalTable: "TableSchemaContext",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TableJunctionSchemaContextHasElement_TableSchemaElement_RelationID",
                        column: x => x.RelationID,
                        principalTable: "TableSchemaElement",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TableJunctionSchemaElementHasElement",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EntityID = table.Column<Guid>(type: "TEXT", nullable: false),
                    RelationID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableJunctionSchemaElementHasElement", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TableJunctionSchemaElementHasElement_TableSchemaElement_EntityID",
                        column: x => x.EntityID,
                        principalTable: "TableSchemaElement",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TableJunctionSchemaElementHasElement_TableSchemaElement_RelationID",
                        column: x => x.RelationID,
                        principalTable: "TableSchemaElement",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TableJunctionSchemaElementHasProperty",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EntityID = table.Column<Guid>(type: "TEXT", nullable: false),
                    RelationID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableJunctionSchemaElementHasProperty", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TableJunctionSchemaElementHasProperty_TableSchemaElement_EntityID",
                        column: x => x.EntityID,
                        principalTable: "TableSchemaElement",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TableJunctionSchemaPropertyCollectionHasProperty",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EntityID = table.Column<Guid>(type: "TEXT", nullable: false),
                    RelationID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableJunctionSchemaPropertyCollectionHasProperty", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TableJunctionSchemaPropertyCollectionHasProperty_TableSchemaPropertyCollection_EntityID",
                        column: x => x.EntityID,
                        principalTable: "TableSchemaPropertyCollection",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TableJunctionSchemaPropertyTableHasProperty",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EntityID = table.Column<Guid>(type: "TEXT", nullable: false),
                    RelationID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableJunctionSchemaPropertyTableHasProperty", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TableJunctionSchemaPropertyTableHasProperty_TableSchemaPropertyTable_EntityID",
                        column: x => x.EntityID,
                        principalTable: "TableSchemaPropertyTable",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "TableDataTextParameter",
                columns: new[] { "ID", "SchemaIdentifier", "TimeApproved", "TimeCreated", "TimeEffective", "TimeUpdated", "Value" },
                values: new object[,]
                {
                    { new Guid("019764ca-25c4-7785-bd02-ebdc5a27fb39"), "ButtonActivate", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Activate" },
                    { new Guid("019764ca-25c4-7785-bd02-efa276b57b62"), "ButtonCreate", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Create" },
                    { new Guid("019764ca-25c4-7785-bd02-f1e439a3bb07"), "ButtonUpdate", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Update" },
                    { new Guid("019764ca-25c4-7785-bd02-f7c5260cb82d"), "ButtonSubmit", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Submit" },
                    { new Guid("01977fb8-8d9e-7179-b098-d37940c4d817"), "ButtonSignIn", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Sign in" },
                    { new Guid("01977fb8-8d9e-7179-b098-d431e095ba95"), "ButtonSignUp", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Sign up" },
                    { new Guid("01978309-3029-74e9-931c-436de21f95b0"), "LinkNavigationSchema", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Schemas" },
                    { new Guid("019789f9-2601-72ac-ad27-ea4b8f4855d6"), "LabelFormSchemaIdentifier", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Schema identifier" },
                    { new Guid("019789f9-2601-72ac-ad27-ed762c6df40e"), "LabelFormInputID", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Input type" },
                    { new Guid("019789f9-2601-72ac-ad27-f1f7ad078b01"), "LabelFormName", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Name" },
                    { new Guid("019789f9-2601-72ac-ad27-f7c76f0002d4"), "LabelFormNote", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Note" },
                    { new Guid("019789f9-2601-72ac-ad27-f9ef87027fec"), "LabelFormTitle", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Title" },
                    { new Guid("019789f9-2601-72ac-ad27-fceadc8f7eda"), "LabelFormDescription", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Description" },
                    { new Guid("019789f9-2601-72ac-ad28-0204b7c6d497"), "LabelFormOrder", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Order" },
                    { new Guid("019789fc-18dc-73ee-94b6-1564b2f72eb7"), "HelpFormSchemaIdentifier", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "The unique, humanly readable identifier for this entity type." },
                    { new Guid("019789fc-18dc-73ee-94b6-18a6e3314ccd"), "HelpFormInputID", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "The type of input this entity is related with." },
                    { new Guid("019789fc-18dc-73ee-94b6-1c60e96f26a9"), "HelpFormName", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "The internal name of this entity type. Should only be visible in the configuration." },
                    { new Guid("019789fc-18dc-73ee-94b6-21b290f8401d"), "HelpFormNote", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "A short note detailing how this entity type is used. Should only be visible in the configuration." },
                    { new Guid("019789fc-18dc-73ee-94b6-26ead5f18d4d"), "HelpFormTitle", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "The title used when displaying an entity of this type." },
                    { new Guid("019789fc-18dc-73ee-94b6-28dc0edf9b69"), "HelpFormDescription", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "The description used when displaying an entity of this type." },
                    { new Guid("019789fc-18dc-73ee-94b6-2ee0b27366dc"), "HelpFormOrder", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "The order in which this entity type is sorted." },
                    { new Guid("019789fc-3929-75a9-99e9-142d13f18fb0"), "DefaultEmptyValue", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "-" },
                    { new Guid("01978a17-7901-7131-8b49-0a2832bb83bd"), "LabelFormValue", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Value" },
                    { new Guid("01978a17-7901-7131-8b49-0cb8c0a70a25"), "LabelFormDefaultValue", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Default value" },
                    { new Guid("01978a17-7901-7131-8b49-1200bad01c81"), "LabelFormIsApprovalRequired", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Is approval required?" },
                    { new Guid("01978a17-7901-7131-8b49-179f268688f2"), "LabelFormIsAsynchronous", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Is asynchronous?" },
                    { new Guid("01978a17-b7b1-772f-a129-5fe41a2f1676"), "HelpFormValue", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "The code snippet to be run. Must return a value." },
                    { new Guid("01978a17-b7b1-772f-a129-6253fc96820a"), "HelpFormDefaultValue", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "The default value that will be used for this entity type." },
                    { new Guid("01978a17-b7b1-772f-a129-66dba634b0b5"), "HelpFormIsApprovalRequired", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Determines if another user must approve a text parameter value before it becomes active." },
                    { new Guid("01978a17-b7b1-772f-a129-68c0836a0759"), "HelpFormIsAsynchronous", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Determines if the method will be run as asynchronous code." },
                    { new Guid("01978a1f-f0a2-731f-b17d-1981b375a5db"), "LabelFormIsRepeatable", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Is repeatable?" },
                    { new Guid("01978a1f-f0a2-731f-b17d-1e57e7540e08"), "LabelFormAttributeType", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Attribute type" },
                    { new Guid("01978a1f-f0a2-731f-b17d-2174a6ecd4fe"), "LabelFormTextParameterType", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Text parameter type" },
                    { new Guid("01978a20-9692-72ff-be7d-ac500344bf4b"), "HelpFormIsRepeatable", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Determines if this step can be completed with multiple results." },
                    { new Guid("01978a20-9692-72ff-be7d-b08279f62f4a"), "HelpFormAttributeType", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "The type of attribute." },
                    { new Guid("01978a20-9692-72ff-be7d-b59a17facafb"), "HelpFormTextParameterType", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "The type of text parameter." },
                    { new Guid("01978a20-9692-72ff-be7d-ba24274b04c3"), "HelpFormIsList", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Determines if this input attribute can be configured with a list of values." },
                    { new Guid("019799a4-1a2b-7368-9b33-6a3f0bf60dae"), "LabelFormIsList", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Is value list?" },
                    { new Guid("019799a4-1a2b-7368-9b33-6c45805d80a3"), "LabelFormBoolean", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Boolean" },
                    { new Guid("019799a4-1a2b-7368-9b33-72817ce487a1"), "LabelFormDateTime", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "DateTime" },
                    { new Guid("019799a4-1a2b-7368-9b33-77fdd478928f"), "LabelFormDecimal", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Decimal" },
                    { new Guid("019799a4-1a2b-7368-9b33-781625aca070"), "LabelFormInteger", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Integer" },
                    { new Guid("019799a4-1a2b-7368-9b33-7c0e241b9622"), "LabelFormString", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "String" },
                    { new Guid("019799a4-1a2b-7368-9b33-809a8b75e571"), "LabelFormDynamicValueCode", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Code" },
                    { new Guid("019799a4-1a2b-7368-9b33-86038825b246"), "LabelFormDynamicValueStyleParameter", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Style parameter" },
                    { new Guid("019799a4-1a2b-7368-9b33-8bd3f0f5e948"), "LabelFormDynamicValueSystemParameter", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "System parameter" },
                    { new Guid("019799a4-1a2b-7368-9b33-8c2e6b4a0b6d"), "LabelFormDynamicValueTextParameter", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Text parameter" },
                    { new Guid("019799a6-6b05-7029-b9a7-2d147f778de8"), "HelpFormBoolean", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Represents a boolean value." },
                    { new Guid("019799a6-6b05-7029-b9a7-300f0306a227"), "HelpFormDateTime", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Represents a datetime value." },
                    { new Guid("019799a6-6b05-7029-b9a7-35f76c49841a"), "HelpFormDecimal", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Represents a decimal value." },
                    { new Guid("019799a6-6b05-7029-b9a7-3a924229a88a"), "HelpFormInteger", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Represents a integer value." },
                    { new Guid("019799a6-6b05-7029-b9a7-3ca405bc7246"), "HelpFormString", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Represents a string value." },
                    { new Guid("019799a6-6b05-7029-b9a7-40860609aed2"), "HelpFormDynamicValueCode", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Represents a dynamic code value." },
                    { new Guid("019799a6-6b05-7029-b9a7-473fbbc23ec3"), "HelpFormDynamicValueStyleParameter", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Represents a dynamic style parameter value." },
                    { new Guid("019799a6-6b05-7029-b9a7-48183a3903a4"), "HelpFormDynamicValueSystemParameter", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Represents a dynamic system parameter value." },
                    { new Guid("019799a6-6b05-7029-b9a7-4c3fb79cf9d1"), "HelpFormDynamicValueTextParameter", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Represents a dynamic text parameter value." },
                    { new Guid("01979d14-54ad-72f9-b5d8-aa11a0e1ce60"), "LabelFormDynamicValueTypeList", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Choose dynamic value type" },
                    { new Guid("01979d14-54ad-72f9-b5d8-ae413650b40c"), "LabelFormPropertyTypeList", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Choose property type" },
                    { new Guid("01979d14-54ad-72f9-b5d8-b830dc26f9f4"), "HelpFormDynamicValueTypeList", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "" },
                    { new Guid("01979d14-54ad-72f9-b5d8-bf20682e54af"), "HelpFormPropertyTypeList", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "" },
                    { new Guid("019c240d-19d8-71b0-b48d-639f8765db5a"), "Header:Form:Create:EntitySchemaContext", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Create Schema Context" },
                    { new Guid("019c240d-19d8-727f-9d4a-b6a92f70bfd9"), "Header:Form:Create:EntitySchemaPropertyDecimal", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Create Schema Decimal Property" },
                    { new Guid("019c240d-19d8-736f-87d7-71972f4848b0"), "Header:Form:Create:EntitySchemaPropertyDateTime", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Create Schema DateTime Property" },
                    { new Guid("019c240d-19d8-754f-9c95-ca256884cd81"), "Header:Form:Create:EntitySchemaElement", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Create Schema Element" },
                    { new Guid("019c240d-19d8-75f8-99cb-285ba1b2ecd8"), "Header:Form:Create:EntitySchemaPropertyBoolean", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Create Schema Boolean Property" },
                    { new Guid("019c240d-19d8-761c-804b-b56d211c5c43"), "Header:Form:Create:EntitySchemaParameterSystem", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Create Schema System Parameter" },
                    { new Guid("019c240d-19d8-7789-b384-b32c673d1f98"), "Header:Form:Create:EntitySchemaPropertyImage", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Create Schema Image Property" },
                    { new Guid("019c240d-19d8-77db-a925-13311a12f816"), "Header:Form:Create:EntitySchemaParameterStyle", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Create Schema Style Parameter" },
                    { new Guid("019c240d-19d8-79bf-af26-543a2ae1fcdf"), "Header:Form:Create:EntitySchemaParameterText", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Create Schema Text Parameter" },
                    { new Guid("019c240d-19d8-7b18-8700-7933cc2e5ac7"), "Header:Form:Create:EntitySchemaPropertyInteger", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Create Schema Integer Property" },
                    { new Guid("019c240d-19d8-7b93-9f90-de94403d000a"), "Header:Form:Create:EntitySchemaPropertyTable", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Create Schema Table Property" },
                    { new Guid("019c240d-19d8-7cb3-b84c-981fd2862f0e"), "Header:Form:Create:EntitySchemaPropertyCollection", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Create Schema Collection Property" },
                    { new Guid("019c240d-19d8-7f6e-a91d-db2e3c9e6e5e"), "Header:Form:Create:EntitySchema", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Create Schema" },
                    { new Guid("019c240d-19d8-7f86-b88c-db22de407519"), "Header:Form:Create:EntitySchemaPropertyString", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Create Schema String Property" },
                    { new Guid("019c240d-8b86-7197-954e-c80cb43b9192"), "Header:Form:Update:EntitySchemaPropertyString", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Update Schema String Property" },
                    { new Guid("019c240d-8b86-7303-b114-c733077eb990"), "Header:Form:Update:EntitySchemaContext", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Update Schema Context" },
                    { new Guid("019c240d-8b86-764f-86ad-1e8c2203acee"), "Header:Form:Update:EntitySchemaPropertyBoolean", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Update Schema Boolean Property" },
                    { new Guid("019c240d-8b86-78a6-942c-7210fa5d6003"), "Header:Form:Update:EntitySchemaPropertyTable", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Update Schema Table Property" },
                    { new Guid("019c240d-8b86-794e-a648-3da4fd31dc9b"), "Header:Form:Update:EntitySchemaPropertyCollection", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Update Schema Collection Property" },
                    { new Guid("019c240d-8b86-79e9-af55-66544574de3a"), "Header:Form:Update:EntitySchemaPropertyInteger", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Update Schema Integer Property" },
                    { new Guid("019c240d-8b86-79f5-9787-1aeb9cde12b1"), "Header:Form:Update:EntitySchemaParameterSystem", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Update Schema System Parameter" },
                    { new Guid("019c240d-8b86-7a12-bcd4-5316fd6894b0"), "Header:Form:Update:EntitySchemaElement", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Update Schema Element" },
                    { new Guid("019c240d-8b86-7b31-98f5-60c0c15ecb49"), "Header:Form:Update:EntitySchemaPropertyDecimal", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Update Schema Decimal Property" },
                    { new Guid("019c240d-8b86-7b6f-8892-d4ddd7793234"), "Header:Form:Update:EntitySchemaParameterStyle", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Update Schema Style Parameter" },
                    { new Guid("019c240d-8b86-7b8a-9c71-0d9ae20a2a50"), "Header:Form:Update:EntitySchemaPropertyDateTime", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Update Schema DateTime Property" },
                    { new Guid("019c240d-8b86-7c30-8564-1242f5f9abaa"), "Header:Form:Update:EntitySchema", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Update Schema" },
                    { new Guid("019c240d-8b86-7d81-bfbc-9f8384ab8a2d"), "Header:Form:Update:EntitySchemaPropertyImage", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Update Schema Image Property" },
                    { new Guid("019c240d-8b86-7eb2-bf9e-fa45f0051326"), "Header:Form:Update:EntitySchemaParameterText", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Update Schema Text Parameter" }
                });

            migrationBuilder.InsertData(
                table: "TableSchema",
                columns: new[] { "ID", "IsActive", "Name", "Note", "Order", "TimeActivated", "TimeCreated", "TimeUpdated" },
                values: new object[] { new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), true, "Base schema", "This is a base schema intended to be cloned and extended.", 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null });

            migrationBuilder.InsertData(
                table: "TableSchemaParameterText",
                columns: new[] { "ID", "IsApprovalRequired", "Name", "Note", "Order", "SchemaID", "SchemaIdentifier", "TimeCreated", "TimeUpdated", "Type" },
                values: new object[,]
                {
                    { new Guid("019764ca-25c4-7785-bd02-daa168ae477d"), false, "ButtonActivate", "", 2, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "ButtonActivate", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019764ca-25c4-7785-bd02-de67a804e592"), false, "ButtonCreate", "", 3, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "ButtonCreate", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019764ca-25c4-7785-bd02-e26550fa3aa9"), false, "ButtonUpdate", "", 4, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "ButtonUpdate", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019764ca-25c4-7785-bd02-e5902e766443"), false, "ButtonSubmit", "", 5, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "ButtonSubmit", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("01977fb8-8d9e-7179-b098-d8800c456351"), false, "ButtonSignIn", "", 6, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "ButtonSignIn", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("01977fb8-8d9e-7179-b098-dde7fae42dc4"), false, "ButtonSignUp", "", 7, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "ButtonSignUp", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("01978309-3029-74e9-931c-3cf1322948fd"), false, "LinkNavigationSchema", "", 8, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "LinkNavigationSchema", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019789de-e449-71aa-ab1d-0992c66a9dae"), false, "LabelFormSchemaIdentifier", "", 9, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "LabelFormSchemaIdentifier", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019789de-e449-71aa-ab1d-0e7af6463e30"), false, "LabelFormInputID", "", 15, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "LabelFormInputID", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019789de-e449-71aa-ab1d-12e8b546b239"), false, "LabelFormName", "", 10, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "LabelFormName", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019789de-e449-71aa-ab1d-17209b94ded6"), false, "LabelFormNote", "", 11, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "LabelFormNote", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019789de-e449-71aa-ab1d-192c4a3c0eb6"), false, "LabelFormTitle", "", 12, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "LabelFormTitle", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019789de-e449-71aa-ab1d-1c1707f61f89"), false, "LabelFormDescription", "", 13, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "LabelFormDescription", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019789de-e449-71aa-ab1d-205e04219122"), false, "LabelFormOrder", "", 14, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "LabelFormOrder", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019789fc-3929-75a9-99e8-f7a6abf0c139"), false, "HelpFormSchemaIdentifier", "", 35, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "HelpFormSchemaIdentifier", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019789fc-3929-75a9-99e8-f819edf63934"), false, "HelpFormName", "", 36, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "HelpFormName", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019789fc-3929-75a9-99e8-ffd616e87b30"), false, "HelpFormNote", "", 37, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "HelpFormNote", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019789fc-3929-75a9-99e9-0229499f0ddd"), false, "HelpFormTitle", "", 38, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "HelpFormTitle", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019789fc-3929-75a9-99e9-046632ba51b7"), false, "HelpFormDescription", "", 39, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "HelpFormDescription", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019789fc-3929-75a9-99e9-0a5f8ca00604"), false, "HelpFormOrder", "", 40, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "HelpFormOrder", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019789fc-3929-75a9-99e9-0c9241c61162"), false, "HelpFormInputID", "", 41, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "HelpFormInputID", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019789fc-3929-75a9-99e9-11f9d8b81e8a"), false, "DefaultEmptyValue", "", 1, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "DefaultEmptyValue", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("01978a17-7901-7131-8b48-f882aa64e37a"), false, "LabelFormValue", "", 16, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "LabelFormValue", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("01978a17-7901-7131-8b48-ff6a41005bb3"), false, "LabelFormDefaultValue", "", 17, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "LabelFormDefaultValue", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("01978a17-7901-7131-8b49-005b042a1608"), false, "LabelFormIsApprovalRequired", "", 18, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "LabelFormIsApprovalRequired", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("01978a17-7901-7131-8b49-07a43e807dfd"), false, "LabelFormIsAsynchronous", "", 19, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "LabelFormIsAsynchronous", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("01978a17-b7b1-772f-a129-4ce642008489"), false, "HelpFormValue", "", 42, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "HelpFormValue", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("01978a17-b7b1-772f-a129-52ee6b9269cd"), false, "HelpFormDefaultValue", "", 43, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "HelpFormDefaultValue", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("01978a17-b7b1-772f-a129-543b087e1606"), false, "HelpFormIsApprovalRequired", "", 44, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "HelpFormIsApprovalRequired", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("01978a17-b7b1-772f-a129-5bbae750bee1"), false, "HelpFormIsAsynchronous", "", 45, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "HelpFormIsAsynchronous", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("01978a1f-f0a2-731f-b17d-0991eba49899"), false, "LabelFormIsRepeatable", "", 20, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "LabelFormIsRepeatable", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("01978a1f-f0a2-731f-b17d-0e2bbf4c0700"), false, "LabelFormAttributeType", "", 21, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "LabelFormAttributeType", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("01978a1f-f0a2-731f-b17d-12579bcc7198"), false, "LabelFormTextParameterType", "", 22, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "LabelFormTextParameterType", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("01978a1f-f0a2-731f-b17d-14f803055489"), false, "LabelFormIsList", "", 25, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "LabelFormIsList", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("01978a20-9692-72ff-be7d-9cb3d3d46359"), false, "HelpFormIsRepeatable", "", 46, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "HelpFormIsRepeatable", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("01978a20-9692-72ff-be7d-a3b72d338572"), false, "HelpFormAttributeType", "", 47, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "HelpFormAttributeType", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("01978a20-9692-72ff-be7d-a9bb3d99565a"), false, "HelpFormIsList", "", 51, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "HelpFormIsList", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019799a3-a72f-725a-b4f6-b7144f565f3f"), false, "LabelFormBoolean", "", 26, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "LabelFormBoolean", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019799a3-a72f-725a-b4f6-b8b9e65fa2e9"), false, "LabelFormDateTime", "", 27, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "LabelFormDateTime", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019799a3-a72f-725a-b4f6-be98a390cf1a"), false, "LabelFormDecimal", "", 28, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "LabelFormDecimal", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019799a3-a72f-725a-b4f6-c0efbd9b5dbb"), false, "LabelFormInteger", "", 29, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "LabelFormInteger", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019799a3-a72f-725a-b4f6-c449cc81422f"), false, "LabelFormString", "", 30, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "LabelFormString", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019799a3-a72f-725a-b4f6-c9599b28ac9c"), false, "LabelFormDynamicValueCode", "", 31, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "LabelFormDynamicValueCode", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019799a3-a72f-725a-b4f6-cd308b80c164"), false, "LabelFormDynamicValueStyleParameter", "", 32, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "LabelFormDynamicValueStyleParameter", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019799a3-a72f-725a-b4f6-d3344af0a274"), false, "LabelFormDynamicValueSystemParameter", "", 33, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "LabelFormDynamicValueSystemParameter", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019799a3-a72f-725a-b4f6-d6b06a4f6d1b"), false, "LabelFormDynamicValueTextParameter", "", 34, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "LabelFormDynamicValueTextParameter", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019799a6-8dc0-75af-8866-97a26b3415dc"), false, "HelpFormBoolean", "", 52, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "HelpFormBoolean", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019799a6-8dc0-75af-8866-986c4570a7bd"), false, "HelpFormDateTime", "", 53, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "HelpFormDateTime", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019799a6-8dc0-75af-8866-9f89357ebb7f"), false, "HelpFormDecimal", "", 54, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "HelpFormDecimal", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019799a6-8dc0-75af-8866-a390807971d2"), false, "HelpFormInteger", "", 55, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "HelpFormInteger", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019799a6-8dc0-75af-8866-a44ca62f2e18"), false, "HelpFormString", "", 56, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "HelpFormString", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019799a6-8dc0-75af-8866-a93e37c915a8"), false, "HelpFormDynamicValueCode", "", 57, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "HelpFormDynamicValueCode", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019799a6-8dc0-75af-8866-ad7a01af4eac"), false, "HelpFormDynamicValueStyleParameter", "", 58, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "HelpFormDynamicValueStyleParameter", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019799a6-8dc0-75af-8866-b0be19e7dcdd"), false, "HelpFormDynamicValueSystemParameter", "", 59, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "HelpFormDynamicValueSystemParameter", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019799a6-8dc0-75af-8866-b4f08d0df491"), false, "HelpFormDynamicValueTextParameter", "", 60, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "HelpFormDynamicValueTextParameter", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("01979d14-54ad-72f9-b5d8-a291a3ca06d4"), false, "LabelFormDynamicValueTypeList", "", 23, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "LabelFormDynamicValueTypeList", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("01979d14-54ad-72f9-b5d8-a5aa1089fe1b"), false, "LabelFormPropertyTypeList", "", 24, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "LabelFormPropertyTypeList", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("01979d14-54ad-72f9-b5d8-b33557c5a806"), false, "HelpFormTextParameterType", "", 48, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "HelpFormTextParameterType", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("01979d14-54ad-72f9-b5d8-c3a68639a28a"), false, "HelpFormDynamicValueTypeList", "", 49, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "HelpFormDynamicValueTypeList", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("01979d14-54ad-72f9-b5d8-c4050ea1f0fd"), false, "HelpFormPropertyTypeList", "", 50, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "HelpFormPropertyTypeList", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019c240c-791c-70b5-866b-f755d1c45382"), false, "Header:Form:Create:EntitySchemaParameterStyle", "", 64, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "Header:Form:Create:EntitySchemaParameterStyle", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019c240c-791c-728d-a32f-32740d710848"), false, "Header:Form:Create:EntitySchemaPropertyBoolean", "", 67, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "Header:Form:Create:EntitySchemaPropertyBoolean", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019c240c-791c-73eb-be79-a8f9fd6195bf"), false, "Header:Form:Create:EntitySchemaElement", "", 63, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "Header:Form:Create:EntitySchemaElement", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019c240c-791c-7579-8011-fd306ef3be7d"), false, "Header:Form:Create:EntitySchema", "", 61, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "Header:Form:Create:EntitySchema", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019c240c-791c-77c6-9d30-a90e02a17d97"), false, "Header:Form:Create:EntitySchemaPropertyCollection", "", 68, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "Header:Form:Create:EntitySchemaPropertyCollection", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019c240c-791c-77f9-97ee-ad37630c7fdf"), false, "Header:Form:Create:EntitySchemaPropertyTable", "", 74, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "Header:Form:Create:EntitySchemaPropertyTable", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019c240c-791c-780b-8287-8db5087a9579"), false, "Header:Form:Create:EntitySchemaPropertyDateTime", "", 69, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "Header:Form:Create:EntitySchemaPropertyDateTime", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019c240c-791c-79a9-ad25-bd0e7f197bfe"), false, "Header:Form:Create:EntitySchemaParameterSystem", "", 65, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "Header:Form:Create:EntitySchemaParameterSystem", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019c240c-791c-7a15-946f-d1cf59ddac12"), false, "Header:Form:Create:EntitySchemaPropertyDecimal", "", 70, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "Header:Form:Create:EntitySchemaPropertyDecimal", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019c240c-791c-7a50-8798-918e8726d667"), false, "Header:Form:Create:EntitySchemaPropertyImage", "", 71, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "Header:Form:Create:EntitySchemaPropertyImage", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019c240c-791c-7afa-a013-344f1600999c"), false, "Header:Form:Create:EntitySchemaPropertyInteger", "", 72, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "Header:Form:Create:EntitySchemaPropertyInteger", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019c240c-791c-7b41-ad27-573d62c60f54"), false, "Header:Form:Create:EntitySchemaParameterText", "", 66, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "Header:Form:Create:EntitySchemaParameterText", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019c240c-791c-7d7e-accd-c533af9a13f2"), false, "Header:Form:Create:EntitySchemaContext", "", 62, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "Header:Form:Create:EntitySchemaContext", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019c240c-791c-7ffb-8f29-38fb70a51934"), false, "Header:Form:Create:EntitySchemaPropertyString", "", 73, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "Header:Form:Create:EntitySchemaPropertyString", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019c240d-6e22-70f5-bb78-f1017feca103"), false, "Header:Form:Update:EntitySchemaContext", "", 76, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "Header:Form:Update:EntitySchemaContext", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019c240d-6e22-71fa-a972-a870f1167975"), false, "Header:Form:Update:EntitySchemaPropertyDateTime", "", 83, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "Header:Form:Update:EntitySchemaPropertyDateTime", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019c240d-6e22-7532-a326-85012b6af7be"), false, "Header:Form:Update:EntitySchemaPropertyInteger", "", 86, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "Header:Form:Update:EntitySchemaPropertyInteger", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019c240d-6e22-77a8-99d9-76c997bc7eff"), false, "Header:Form:Update:EntitySchema", "", 75, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "Header:Form:Update:EntitySchema", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019c240d-6e22-77d3-a36c-bbff9662a8e1"), false, "Header:Form:Update:EntitySchemaPropertyTable", "", 88, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "Header:Form:Update:EntitySchemaPropertyTable", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019c240d-6e22-78af-affd-196b637a0dfc"), false, "Header:Form:Update:EntitySchemaParameterStyle", "", 78, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "Header:Form:Update:EntitySchemaParameterStyle", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019c240d-6e22-7913-9fae-b135cf903053"), false, "Header:Form:Update:EntitySchemaPropertyBoolean", "", 81, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "Header:Form:Update:EntitySchemaPropertyBoolean", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019c240d-6e22-7942-adec-6a8773092b8b"), false, "Header:Form:Update:EntitySchemaPropertyCollection", "", 82, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "Header:Form:Update:EntitySchemaPropertyCollection", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019c240d-6e22-7b1e-b540-fe8a2aac155f"), false, "Header:Form:Update:EntitySchemaPropertyString", "", 87, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "Header:Form:Update:EntitySchemaPropertyString", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019c240d-6e22-7bf6-9778-7d42751cc301"), false, "Header:Form:Update:EntitySchemaElement", "", 77, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "Header:Form:Update:EntitySchemaElement", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019c240d-6e22-7df2-9ad3-1f4337f1f415"), false, "Header:Form:Update:EntitySchemaPropertyDecimal", "", 84, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "Header:Form:Update:EntitySchemaPropertyDecimal", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019c240d-6e22-7e3a-8edf-09814b4b8a57"), false, "Header:Form:Update:EntitySchemaPropertyImage", "", 85, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "Header:Form:Update:EntitySchemaPropertyImage", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019c240d-6e22-7f13-91f7-40b557df76e5"), false, "Header:Form:Update:EntitySchemaParameterSystem", "", 79, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "Header:Form:Update:EntitySchemaParameterSystem", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("019c240d-6e22-7fed-b8e1-87cca3ee930d"), false, "Header:Form:Update:EntitySchemaParameterText", "", 80, new Guid("01974e8c-ecb8-75ab-9070-ef902ff370a7"), "Header:Form:Update:EntitySchemaParameterText", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TableAuthentication_TimeCreated",
                table: "TableAuthentication",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableDataElement_TimeCreated",
                table: "TableDataElement",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableDataPropertyBoolean_ElementID",
                table: "TableDataPropertyBoolean",
                column: "ElementID");

            migrationBuilder.CreateIndex(
                name: "IX_TableDataPropertyBoolean_TimeCreated",
                table: "TableDataPropertyBoolean",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableDataPropertyDateTime_ElementID",
                table: "TableDataPropertyDateTime",
                column: "ElementID");

            migrationBuilder.CreateIndex(
                name: "IX_TableDataPropertyDateTime_TimeCreated",
                table: "TableDataPropertyDateTime",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableDataPropertyString_ElementID",
                table: "TableDataPropertyString",
                column: "ElementID");

            migrationBuilder.CreateIndex(
                name: "IX_TableDataPropertyString_TimeCreated",
                table: "TableDataPropertyString",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableDataStyleParameter_TimeCreated",
                table: "TableDataStyleParameter",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableDataSystemParameter_TimeCreated",
                table: "TableDataSystemParameter",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableDataTextParameter_TimeCreated",
                table: "TableDataTextParameter",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableIdentity_TimeCreated",
                table: "TableIdentity",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableIdentity_UserID",
                table: "TableIdentity",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_TableJunctionSchemaContextHasElement_EntityID_RelationID",
                table: "TableJunctionSchemaContextHasElement",
                columns: new[] { "EntityID", "RelationID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TableJunctionSchemaContextHasElement_RelationID",
                table: "TableJunctionSchemaContextHasElement",
                column: "RelationID");

            migrationBuilder.CreateIndex(
                name: "IX_TableJunctionSchemaContextHasElement_TimeCreated",
                table: "TableJunctionSchemaContextHasElement",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableJunctionSchemaElementHasElement_EntityID_RelationID",
                table: "TableJunctionSchemaElementHasElement",
                columns: new[] { "EntityID", "RelationID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TableJunctionSchemaElementHasElement_RelationID",
                table: "TableJunctionSchemaElementHasElement",
                column: "RelationID");

            migrationBuilder.CreateIndex(
                name: "IX_TableJunctionSchemaElementHasElement_TimeCreated",
                table: "TableJunctionSchemaElementHasElement",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableJunctionSchemaElementHasProperty_EntityID_RelationID",
                table: "TableJunctionSchemaElementHasProperty",
                columns: new[] { "EntityID", "RelationID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TableJunctionSchemaElementHasProperty_RelationID",
                table: "TableJunctionSchemaElementHasProperty",
                column: "RelationID");

            migrationBuilder.CreateIndex(
                name: "IX_TableJunctionSchemaElementHasProperty_TimeCreated",
                table: "TableJunctionSchemaElementHasProperty",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableJunctionSchemaPropertyCollectionHasProperty_EntityID_RelationID",
                table: "TableJunctionSchemaPropertyCollectionHasProperty",
                columns: new[] { "EntityID", "RelationID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TableJunctionSchemaPropertyCollectionHasProperty_RelationID",
                table: "TableJunctionSchemaPropertyCollectionHasProperty",
                column: "RelationID");

            migrationBuilder.CreateIndex(
                name: "IX_TableJunctionSchemaPropertyCollectionHasProperty_TimeCreated",
                table: "TableJunctionSchemaPropertyCollectionHasProperty",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableJunctionSchemaPropertyTableHasProperty_EntityID_RelationID",
                table: "TableJunctionSchemaPropertyTableHasProperty",
                columns: new[] { "EntityID", "RelationID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TableJunctionSchemaPropertyTableHasProperty_RelationID",
                table: "TableJunctionSchemaPropertyTableHasProperty",
                column: "RelationID");

            migrationBuilder.CreateIndex(
                name: "IX_TableJunctionSchemaPropertyTableHasProperty_TimeCreated",
                table: "TableJunctionSchemaPropertyTableHasProperty",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchema_TimeCreated",
                table: "TableSchema",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaContext_DescriptionTextParameterID",
                table: "TableSchemaContext",
                column: "DescriptionTextParameterID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaContext_SchemaID_SchemaIdentifier",
                table: "TableSchemaContext",
                columns: new[] { "SchemaID", "SchemaIdentifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaContext_TimeCreated",
                table: "TableSchemaContext",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaContext_TitleTextParameterID",
                table: "TableSchemaContext",
                column: "TitleTextParameterID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaElement_DescriptionTextParameterID",
                table: "TableSchemaElement",
                column: "DescriptionTextParameterID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaElement_SchemaID_SchemaIdentifier",
                table: "TableSchemaElement",
                columns: new[] { "SchemaID", "SchemaIdentifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaElement_TimeCreated",
                table: "TableSchemaElement",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaElement_TitleTextParameterID",
                table: "TableSchemaElement",
                column: "TitleTextParameterID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaParameterStyle_SchemaID",
                table: "TableSchemaParameterStyle",
                column: "SchemaID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaParameterStyle_SchemaID_SchemaIdentifier",
                table: "TableSchemaParameterStyle",
                columns: new[] { "SchemaID", "SchemaIdentifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaParameterStyle_TimeCreated",
                table: "TableSchemaParameterStyle",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaParameterSystem_SchemaID",
                table: "TableSchemaParameterSystem",
                column: "SchemaID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaParameterSystem_SchemaID_SchemaIdentifier",
                table: "TableSchemaParameterSystem",
                columns: new[] { "SchemaID", "SchemaIdentifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaParameterSystem_TimeCreated",
                table: "TableSchemaParameterSystem",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaParameterText_SchemaID",
                table: "TableSchemaParameterText",
                column: "SchemaID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaParameterText_SchemaID_SchemaIdentifier",
                table: "TableSchemaParameterText",
                columns: new[] { "SchemaID", "SchemaIdentifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaParameterText_TimeCreated",
                table: "TableSchemaParameterText",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyBoolean_DescriptionTextParameterID",
                table: "TableSchemaPropertyBoolean",
                column: "DescriptionTextParameterID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyBoolean_SchemaID",
                table: "TableSchemaPropertyBoolean",
                column: "SchemaID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyBoolean_SchemaID_SchemaIdentifier",
                table: "TableSchemaPropertyBoolean",
                columns: new[] { "SchemaID", "SchemaIdentifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyBoolean_TimeCreated",
                table: "TableSchemaPropertyBoolean",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyBoolean_TitleTextParameterID",
                table: "TableSchemaPropertyBoolean",
                column: "TitleTextParameterID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyCollection_DescriptionTextParameterID",
                table: "TableSchemaPropertyCollection",
                column: "DescriptionTextParameterID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyCollection_SchemaID",
                table: "TableSchemaPropertyCollection",
                column: "SchemaID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyCollection_SchemaID_SchemaIdentifier",
                table: "TableSchemaPropertyCollection",
                columns: new[] { "SchemaID", "SchemaIdentifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyCollection_TimeCreated",
                table: "TableSchemaPropertyCollection",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyCollection_TitleTextParameterID",
                table: "TableSchemaPropertyCollection",
                column: "TitleTextParameterID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyDateTime_DescriptionTextParameterID",
                table: "TableSchemaPropertyDateTime",
                column: "DescriptionTextParameterID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyDateTime_SchemaID",
                table: "TableSchemaPropertyDateTime",
                column: "SchemaID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyDateTime_SchemaID_SchemaIdentifier",
                table: "TableSchemaPropertyDateTime",
                columns: new[] { "SchemaID", "SchemaIdentifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyDateTime_TimeCreated",
                table: "TableSchemaPropertyDateTime",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyDateTime_TitleTextParameterID",
                table: "TableSchemaPropertyDateTime",
                column: "TitleTextParameterID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyString_DescriptionTextParameterID",
                table: "TableSchemaPropertyString",
                column: "DescriptionTextParameterID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyString_SchemaID",
                table: "TableSchemaPropertyString",
                column: "SchemaID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyString_SchemaID_SchemaIdentifier",
                table: "TableSchemaPropertyString",
                columns: new[] { "SchemaID", "SchemaIdentifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyString_TimeCreated",
                table: "TableSchemaPropertyString",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyString_TitleTextParameterID",
                table: "TableSchemaPropertyString",
                column: "TitleTextParameterID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyTable_DescriptionTextParameterID",
                table: "TableSchemaPropertyTable",
                column: "DescriptionTextParameterID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyTable_SchemaID",
                table: "TableSchemaPropertyTable",
                column: "SchemaID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyTable_SchemaID_SchemaIdentifier",
                table: "TableSchemaPropertyTable",
                columns: new[] { "SchemaID", "SchemaIdentifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyTable_TimeCreated",
                table: "TableSchemaPropertyTable",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyTable_TitleTextParameterID",
                table: "TableSchemaPropertyTable",
                column: "TitleTextParameterID");

            migrationBuilder.CreateIndex(
                name: "IX_TableUser_AuthenticationID",
                table: "TableUser",
                column: "AuthenticationID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TableUser_Email",
                table: "TableUser",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TableUser_TimeCreated",
                table: "TableUser",
                column: "TimeCreated");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TableDataPropertyBoolean");

            migrationBuilder.DropTable(
                name: "TableDataPropertyDateTime");

            migrationBuilder.DropTable(
                name: "TableDataPropertyString");

            migrationBuilder.DropTable(
                name: "TableDataStyleParameter");

            migrationBuilder.DropTable(
                name: "TableDataSystemParameter");

            migrationBuilder.DropTable(
                name: "TableDataTextParameter");

            migrationBuilder.DropTable(
                name: "TableIdentity");

            migrationBuilder.DropTable(
                name: "TableJunctionSchemaContextHasElement");

            migrationBuilder.DropTable(
                name: "TableJunctionSchemaElementHasElement");

            migrationBuilder.DropTable(
                name: "TableJunctionSchemaElementHasProperty");

            migrationBuilder.DropTable(
                name: "TableJunctionSchemaPropertyCollectionHasProperty");

            migrationBuilder.DropTable(
                name: "TableJunctionSchemaPropertyTableHasProperty");

            migrationBuilder.DropTable(
                name: "TableSchemaParameterStyle");

            migrationBuilder.DropTable(
                name: "TableSchemaParameterSystem");

            migrationBuilder.DropTable(
                name: "TableSchemaPropertyBoolean");

            migrationBuilder.DropTable(
                name: "TableSchemaPropertyDateTime");

            migrationBuilder.DropTable(
                name: "TableSchemaPropertyString");

            migrationBuilder.DropTable(
                name: "TableDataElement");

            migrationBuilder.DropTable(
                name: "TableUser");

            migrationBuilder.DropTable(
                name: "TableSchemaContext");

            migrationBuilder.DropTable(
                name: "TableSchemaElement");

            migrationBuilder.DropTable(
                name: "TableSchemaPropertyCollection");

            migrationBuilder.DropTable(
                name: "TableSchemaPropertyTable");

            migrationBuilder.DropTable(
                name: "TableAuthentication");

            migrationBuilder.DropTable(
                name: "TableSchemaParameterText");

            migrationBuilder.DropTable(
                name: "TableSchema");
        }
    }
}

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
                name: "TableDataParameterStyle",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    TimeApproved = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TimeEffective = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SchemaIdentifier = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableDataParameterStyle", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TableDataParameterSystem",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    TimeApproved = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TimeEffective = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SchemaIdentifier = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableDataParameterSystem", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TableDataParameterText",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    TimeApproved = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TimeEffective = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Culture = table.Column<string>(type: "varchar(5)", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SchemaIdentifier = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableDataParameterText", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TableSchema",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    TimeActivated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true)
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
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    IsSystemDefined = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsApprovalRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
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
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    IsSystemDefined = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsApprovalRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPublic = table.Column<bool>(type: "INTEGER", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
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
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    IsSystemDefined = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsApprovalRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
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
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SchemaIdentifier = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SchemaID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TitleParameterTextID = table.Column<Guid>(type: "TEXT", nullable: false),
                    DescriptionParameterTextID = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableSchemaContext", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TableSchemaContext_TableSchemaParameterText_DescriptionParameterTextID",
                        column: x => x.DescriptionParameterTextID,
                        principalTable: "TableSchemaParameterText",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TableSchemaContext_TableSchemaParameterText_TitleParameterTextID",
                        column: x => x.TitleParameterTextID,
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
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Weight = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SchemaIdentifier = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SchemaID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TitleParameterTextID = table.Column<Guid>(type: "TEXT", nullable: false),
                    DescriptionParameterTextID = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableSchemaElement", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TableSchemaElement_TableSchemaParameterText_DescriptionParameterTextID",
                        column: x => x.DescriptionParameterTextID,
                        principalTable: "TableSchemaParameterText",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TableSchemaElement_TableSchemaParameterText_TitleParameterTextID",
                        column: x => x.TitleParameterTextID,
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
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Weight = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SchemaIdentifier = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SchemaID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TitleParameterTextID = table.Column<Guid>(type: "TEXT", nullable: false),
                    DescriptionParameterTextID = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableSchemaPropertyBoolean", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyBoolean_TableSchemaParameterText_DescriptionParameterTextID",
                        column: x => x.DescriptionParameterTextID,
                        principalTable: "TableSchemaParameterText",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyBoolean_TableSchemaParameterText_TitleParameterTextID",
                        column: x => x.TitleParameterTextID,
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
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Weight = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SchemaIdentifier = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SchemaID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TitleParameterTextID = table.Column<Guid>(type: "TEXT", nullable: false),
                    DescriptionParameterTextID = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableSchemaPropertyCollection", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyCollection_TableSchemaParameterText_DescriptionParameterTextID",
                        column: x => x.DescriptionParameterTextID,
                        principalTable: "TableSchemaParameterText",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyCollection_TableSchemaParameterText_TitleParameterTextID",
                        column: x => x.TitleParameterTextID,
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
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Weight = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SchemaIdentifier = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SchemaID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TitleParameterTextID = table.Column<Guid>(type: "TEXT", nullable: false),
                    DescriptionParameterTextID = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableSchemaPropertyDateTime", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyDateTime_TableSchemaParameterText_DescriptionParameterTextID",
                        column: x => x.DescriptionParameterTextID,
                        principalTable: "TableSchemaParameterText",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyDateTime_TableSchemaParameterText_TitleParameterTextID",
                        column: x => x.TitleParameterTextID,
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
                name: "TableSchemaPropertyDecimal",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Weight = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SchemaIdentifier = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SchemaID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TitleParameterTextID = table.Column<Guid>(type: "TEXT", nullable: false),
                    DescriptionParameterTextID = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableSchemaPropertyDecimal", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyDecimal_TableSchemaParameterText_DescriptionParameterTextID",
                        column: x => x.DescriptionParameterTextID,
                        principalTable: "TableSchemaParameterText",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyDecimal_TableSchemaParameterText_TitleParameterTextID",
                        column: x => x.TitleParameterTextID,
                        principalTable: "TableSchemaParameterText",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyDecimal_TableSchema_SchemaID",
                        column: x => x.SchemaID,
                        principalTable: "TableSchema",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TableSchemaPropertyImage",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Weight = table.Column<int>(type: "INTEGER", nullable: false),
                    AllowedExtensions = table.Column<string>(type: "TEXT", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SchemaIdentifier = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SchemaID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TitleParameterTextID = table.Column<Guid>(type: "TEXT", nullable: false),
                    DescriptionParameterTextID = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableSchemaPropertyImage", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyImage_TableSchemaParameterText_DescriptionParameterTextID",
                        column: x => x.DescriptionParameterTextID,
                        principalTable: "TableSchemaParameterText",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyImage_TableSchemaParameterText_TitleParameterTextID",
                        column: x => x.TitleParameterTextID,
                        principalTable: "TableSchemaParameterText",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyImage_TableSchema_SchemaID",
                        column: x => x.SchemaID,
                        principalTable: "TableSchema",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TableSchemaPropertyInteger",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Weight = table.Column<int>(type: "INTEGER", nullable: false),
                    IsUnsigned = table.Column<bool>(type: "INTEGER", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SchemaIdentifier = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SchemaID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TitleParameterTextID = table.Column<Guid>(type: "TEXT", nullable: false),
                    DescriptionParameterTextID = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableSchemaPropertyInteger", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyInteger_TableSchemaParameterText_DescriptionParameterTextID",
                        column: x => x.DescriptionParameterTextID,
                        principalTable: "TableSchemaParameterText",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyInteger_TableSchemaParameterText_TitleParameterTextID",
                        column: x => x.TitleParameterTextID,
                        principalTable: "TableSchemaParameterText",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyInteger_TableSchema_SchemaID",
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
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Weight = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SchemaIdentifier = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SchemaID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TitleParameterTextID = table.Column<Guid>(type: "TEXT", nullable: false),
                    DescriptionParameterTextID = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableSchemaPropertyString", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyString_TableSchemaParameterText_DescriptionParameterTextID",
                        column: x => x.DescriptionParameterTextID,
                        principalTable: "TableSchemaParameterText",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyString_TableSchemaParameterText_TitleParameterTextID",
                        column: x => x.TitleParameterTextID,
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
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Weight = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SchemaIdentifier = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SchemaID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TitleParameterTextID = table.Column<Guid>(type: "TEXT", nullable: false),
                    DescriptionParameterTextID = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableSchemaPropertyTable", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyTable_TableSchemaParameterText_DescriptionParameterTextID",
                        column: x => x.DescriptionParameterTextID,
                        principalTable: "TableSchemaParameterText",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyTable_TableSchemaParameterText_TitleParameterTextID",
                        column: x => x.TitleParameterTextID,
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
                name: "TableSchemaContextHasElement",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EntityID = table.Column<Guid>(type: "TEXT", nullable: false),
                    RelationID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableSchemaContextHasElement", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TableSchemaContextHasElement_TableSchemaContext_EntityID",
                        column: x => x.EntityID,
                        principalTable: "TableSchemaContext",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TableSchemaContextHasElement_TableSchemaElement_RelationID",
                        column: x => x.RelationID,
                        principalTable: "TableSchemaElement",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TableSchemaElementHasProperty",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EntityID = table.Column<Guid>(type: "TEXT", nullable: false),
                    RelationID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableSchemaElementHasProperty", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TableSchemaElementHasProperty_TableSchemaElement_EntityID",
                        column: x => x.EntityID,
                        principalTable: "TableSchemaElement",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TableSchemaPropertyCollectionHasProperty",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EntityID = table.Column<Guid>(type: "TEXT", nullable: false),
                    RelationID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableSchemaPropertyCollectionHasProperty", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyCollectionHasProperty_TableSchemaPropertyCollection_EntityID",
                        column: x => x.EntityID,
                        principalTable: "TableSchemaPropertyCollection",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TableSchemaPropertyTableHasProperty",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EntityID = table.Column<Guid>(type: "TEXT", nullable: false),
                    RelationID = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableSchemaPropertyTableHasProperty", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TableSchemaPropertyTableHasProperty_TableSchemaPropertyTable_EntityID",
                        column: x => x.EntityID,
                        principalTable: "TableSchemaPropertyTable",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "TableDataParameterSystem",
                columns: new[] { "ID", "SchemaIdentifier", "TimeApproved", "TimeCreated", "TimeEffective", "TimeUpdated", "Value" },
                values: new object[,]
                {
                    { new Guid("057f56e9-586f-ae55-b31c-3f5ebf496d44"), "Schema-Deactivated-Property-Add", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "False" },
                    { new Guid("074b202f-7b8f-d85d-b79a-f62bc5fd7432"), "Schema-Deactivated-Element-Delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "False" },
                    { new Guid("0ba5a00f-4200-9a57-81a1-a872cdf2a374"), "Schema-Inactive-Context-Clone", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "True" },
                    { new Guid("14c484bc-46e0-1e5a-9f37-d290ef3e8580"), "Schema-Deactivated-ElementHasProperty-Add", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "False" },
                    { new Guid("14eb3f96-aa2e-7155-b4a2-86bb2e96f580"), "Schema-Inactive-Context-Edit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "True" },
                    { new Guid("1aa9964c-6fbf-8453-bd23-310bb613f9df"), "Schema-Deactivated-Element-Add", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "False" },
                    { new Guid("1cc3e7ae-082b-2958-bf6d-7d989b452a9e"), "Schema-Deactivated-Element-Edit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "False" },
                    { new Guid("26c31d23-fa92-c45f-a8ac-33ca2089cc05"), "Schema-Inactive-Property-Delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "True" },
                    { new Guid("2e7bb2f9-c596-7453-8b37-d8f04b29b188"), "Schema-Inactive-ElementHasProperty-Add", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "True" },
                    { new Guid("3261d79c-f725-1c54-b708-d8c029a40d52"), "Schema-Inactive-Parameter-Delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "True" },
                    { new Guid("3add094d-b659-bf5c-a995-6a86b9f3e9a8"), "Schema-Inactive-Element-Clone", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "True" },
                    { new Guid("3b3d0778-2bdc-2657-a74b-4135f9e11510"), "Schema-Deactivated-Entity-Delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "False" },
                    { new Guid("3b43778a-78d2-1854-b8d9-582c56412168"), "Schema-Deactivated-Parameter-Add", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "False" },
                    { new Guid("44af3993-3ed5-ac58-bb3e-b0783cec4a35"), "Schema-Deactivated-Context-Delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "False" },
                    { new Guid("47e43cfa-0de4-dc51-b349-e34d769c73e1"), "Schema-Deactivated-Property-Edit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "False" },
                    { new Guid("4b4230b6-e3f3-e559-87ee-59e841b1a9c4"), "Schema-Inactive-Element-Delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "True" },
                    { new Guid("4ef0d931-2990-c456-acb9-3f1804ecc02b"), "Schema-Inactive-Entity-Delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "True" },
                    { new Guid("4f5b700b-bd73-0350-8b53-8aba8851f609"), "Schema-Deactivated-Property-Delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "False" },
                    { new Guid("502722aa-69de-1356-be50-b61cb75b4fd6"), "Schema-Inactive-Entity-Edit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "True" },
                    { new Guid("5a34c53e-deee-fb5b-9914-cf775402aa0b"), "Schema-Deactivated-Parameter-Delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "False" },
                    { new Guid("5cd4c34a-5378-e053-87c4-f0742da20823"), "Schema-Inactive-Element-Add", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "True" },
                    { new Guid("6768dd5f-423e-0d57-8538-a89a3ca7e383"), "Schema-Inactive-Element-Edit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "True" },
                    { new Guid("6d35a961-683a-0a5e-85ed-5137239e64be"), "Schema-Deactivated-ContextHasElement-Add", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "False" },
                    { new Guid("762dc541-40f7-295f-a417-20cda5fe8035"), "Schema-Inactive-Property-Clone", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "True" },
                    { new Guid("8e236eb9-60f5-cd5f-9033-030cb87b7308"), "Schema-Inactive-Parameter-Clone", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "True" },
                    { new Guid("93f69a56-fe06-6a58-a8ad-bafee283667f"), "Schema-Inactive-Context-Delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "True" },
                    { new Guid("95c37e4c-ce08-b356-ae8a-0ede175d4b5e"), "Schema-Inactive-Property-Add", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "True" },
                    { new Guid("9a47a1a0-62f3-415a-8b55-30deb5317e8a"), "Schema-Inactive-ContextHasElement-Add", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "True" },
                    { new Guid("a8055218-6eb7-645c-adb7-f439c5ea2e41"), "Schema-Deactivated-Context-Clone", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "False" },
                    { new Guid("b8e55d28-8588-405c-a1fb-f51b6a0ac1d6"), "Schema-Deactivated-Entity-Edit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "False" },
                    { new Guid("bc94a975-7938-4f5b-9c63-44a3a996a808"), "Schema-Inactive-Parameter-Edit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "True" },
                    { new Guid("bd6e66c5-3ea9-b35c-93c4-592d17ae5d98"), "Schema-Deactivated-Parameter-Clone", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "False" },
                    { new Guid("c3513a83-1f98-6e59-9fa0-5e1dfc121c07"), "Schema-Inactive-Property-Edit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "True" },
                    { new Guid("db62813f-ec26-545e-b5d9-98fdcb1cf721"), "Schema-Deactivated-Property-Clone", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "False" },
                    { new Guid("e2f48b5d-82c2-4652-9171-ba862124359f"), "Schema-Inactive-Context-Add", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "True" },
                    { new Guid("e5ab2af6-b9b4-3454-b329-db76405e4dd8"), "Schema-Inactive-Parameter-Add", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "True" },
                    { new Guid("eb110116-5b80-2955-8192-4d224f3879a7"), "Schema-Deactivated-Parameter-Edit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "False" },
                    { new Guid("ed44c646-5c00-7d5a-8f44-b3c390a3dcfd"), "Schema-Deactivated-Context-Edit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "False" },
                    { new Guid("f3372b13-80ec-1e5c-812b-feb18048a8bf"), "Schema-Deactivated-Element-Clone", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "False" },
                    { new Guid("f3b3e688-49aa-5f5a-93be-55958103a1d2"), "Schema-Deactivated-Context-Add", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "False" }
                });

            migrationBuilder.InsertData(
                table: "TableDataParameterText",
                columns: new[] { "ID", "Culture", "SchemaIdentifier", "TimeApproved", "TimeCreated", "TimeEffective", "TimeUpdated", "Value" },
                values: new object[,]
                {
                    { new Guid("00a4f79b-24ec-c853-a6e4-91c6a0562123"), "en", "Label-PropertyDateTime", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "DateTime property" },
                    { new Guid("02f0b593-bb47-1d54-8f1d-70cd5d48000e"), "en", "Help-Form-Boolean", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Represents a boolean value." },
                    { new Guid("0674a780-84a2-a25c-b535-4e1222687acc"), "en", "Button-SignOut", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Sign out" },
                    { new Guid("082f2278-38e3-df58-8137-3831d3e07239"), "en", "Header-Form-Update-SchemaPropertyImage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Update Schema Image Property" },
                    { new Guid("0b12235d-379c-ad56-8bce-fb6820e54e51"), "en", "Label-PropertyDecimal", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Decimal property" },
                    { new Guid("0c3fd4b1-ab11-f552-9c62-5217309350a9"), "en", "Help-Form-Column", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "The column which the page is sorted by" },
                    { new Guid("0ef26fde-058c-9d50-a499-561c0b5f9924"), "en", "Label-ParameterText", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Text parameter" },
                    { new Guid("100c142b-0e52-ee59-b16d-4c89037b4b18"), "en", "Value-PropertyDateTimeType-Time", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Time" },
                    { new Guid("102f3f9c-7883-3857-96ab-233bd043454f"), "en", "Value-IsActivated-Yes", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Has been activated" },
                    { new Guid("117e6f92-56f3-9459-8a45-bec093985227"), "en", "Label-Weight", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Weight" },
                    { new Guid("1269ef9e-311d-be53-ba13-ef27837b06db"), "en", "Header-Form-Update-SchemaParameterStyle", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Update Schema Style Parameter" },
                    { new Guid("13b29916-24cb-545d-b1d0-e560b9e05a8e"), "en", "Label-Form-Name", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Name" },
                    { new Guid("149a0911-cddb-3656-a78c-51afa72de76c"), "en", "Header-Parameter-Style", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Style Parameters" },
                    { new Guid("14bbd35b-ae77-3256-9f8f-6c30163ebd65"), "en", "Header-Form-Update-SchemaPropertyBoolean", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Update Schema Boolean Property" },
                    { new Guid("153f51b1-59c3-8055-ab2d-acab250d91c9"), "en", "Header-Context", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Contexts" },
                    { new Guid("155adb9d-851e-6b5c-8f0e-d56cf17c9ad4"), "en", "Header-Form-Create-SchemaPropertyCollection", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Create Schema Collection Property" },
                    { new Guid("164cff52-270b-b757-b054-5e9b40468935"), "en", "Header-Form-Create-SchemaPropertyImage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Create Schema Image Property" },
                    { new Guid("194395e6-8803-0255-8ccc-0281ba88c068"), "en", "Help-Form-SchemaIdentifier", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "The unique, humanly readable identifier for this entity type." },
                    { new Guid("1a899ead-9102-c553-9018-f33f3496d0ef"), "en", "Header-Property-Decimal", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Decimal Properties" },
                    { new Guid("1ac21c39-aaf5-a657-a204-5721ae23ac96"), "en", "Label-Form-Email", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Email" },
                    { new Guid("1ae16087-b4c3-6c52-9afe-fec0cddcdc91"), "en", "Help-Form-DateTimeType", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "The type of DateTime property." },
                    { new Guid("1d9a8668-daa1-0151-adb7-1785e8f49e71"), "en", "Title", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Noxy.NET" },
                    { new Guid("1e354fc4-f331-d751-8ce3-aebd0816f58c"), "en", "Label-Name", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Name" },
                    { new Guid("2282419f-40a5-5552-abb3-8e99742031d3"), "en", "Help-Form-Note", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "A short note detailing how this entity type is used. Should only be visible in the configuration." },
                    { new Guid("24f6c10d-75dc-935b-8d04-2f4087e8fd26"), "en", "Help-Form-DateEffective", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "The date from which this entity should become active." },
                    { new Guid("262de63e-9073-fa50-8bdc-3aa7db6f0260"), "en", "Header-Form-Create-SchemaPropertyTable", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Create Schema Table Property" },
                    { new Guid("2b642532-13ba-1d51-afb7-d11ad430989d"), "en", "Help-Form-Value", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "The value to be used with this entity." },
                    { new Guid("2b7529bd-12f8-6c52-b697-39c60ada17c9"), "en", "Value-Descending", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Descending" },
                    { new Guid("2e8b8de8-e5da-fb55-8458-3da8483cce19"), "en", "Label-Form-DateTimeType", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "DateTime type" },
                    { new Guid("3008a38b-43ea-8950-ab93-ddb9ee90b273"), "en", "Help-Form-Integer", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Represents a integer value." },
                    { new Guid("3246bb26-08d3-055b-b9b0-9685f6b569fa"), "en", "Label-Form-Note", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Note" },
                    { new Guid("32818d87-4187-3655-bc64-5594a929037c"), "en", "Value-IsSystemDefined-Yes", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "System defined" },
                    { new Guid("32df9053-fea2-895e-b30c-20d4d716f632"), "en", "Label-Form-DateTime", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "DateTime" },
                    { new Guid("3598cac7-7ab3-7c56-9206-c85766248da3"), "en", "Button-Submit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Submit" },
                    { new Guid("3606f699-ca34-3952-99e0-4ad26a2b50ed"), "en", "Help-Form-IsActivated", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Determines if the entity is active or has been previously activated." },
                    { new Guid("36bc4024-6d87-5d53-b374-6ca7ffc5605a"), "en", "Header-Parameter-Text", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Text Parameters" },
                    { new Guid("3710670f-d554-ff5c-aee2-0128ed6ed719"), "en", "Label-Form-Title", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Title" },
                    { new Guid("38b4bcde-c649-c85b-ab61-d752e5b04d88"), "en", "Header-Form-Create-SchemaPropertyInteger", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Create Schema Integer Property" },
                    { new Guid("38debbe4-4c4b-745a-a38f-894ed597bb2e"), "en", "Label-Culture", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Culture" },
                    { new Guid("41d60b8a-19f8-3b5c-9b0c-78e9195b6c8f"), "en", "Label-Form-IsPublic", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Is public?" },
                    { new Guid("425be657-60c2-af5c-8025-2db8fb89358e"), "en", "Button-Update", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Update" },
                    { new Guid("42cb4999-991f-8e52-abc2-0690c6a1a583"), "en", "Label-Form-ParameterSystem", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "System parameter" },
                    { new Guid("47c45bfc-c6d9-b15b-afbe-899845d06e04"), "en", "Label-Note", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Note" },
                    { new Guid("48310dba-070a-2d5c-b88c-87eba79513db"), "en", "Label-Form-ConfirmPassword", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Confirm password" },
                    { new Guid("4889f291-8df9-255e-905a-f31dcdc9ad1a"), "en", "Default-EmptyValue", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "-" },
                    { new Guid("4894a34e-c226-9d57-b9f5-df4a4bbf0b74"), "en", "Header-Form-Update-SchemaPropertyCollection", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Update Schema Collection Property" },
                    { new Guid("490eb845-c0cf-0852-8bc7-b57ef69d78ae"), "en", "Header-Form-Update-SchemaPropertyTable", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Update Schema Table Property" },
                    { new Guid("4c756b59-e335-d853-b7cf-2543d196459f"), "en", "Header-Form-Update-SchemaParameterSystem", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Update Schema System Parameter" },
                    { new Guid("4cb04f7d-78e2-c05d-99e0-b6503557b7be"), "en", "Help-Form-ParameterSystemType", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "The type of system parameter." },
                    { new Guid("4e2086d9-e563-3458-939b-0b02f0059c8c"), "en", "Help-Form-IsSystemDefined", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Determines if the entity is system defined and therefore cannot be changed." },
                    { new Guid("4f4e759e-49da-5f52-8add-ec00fe7c6328"), "en", "Label-TimeCreated", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Created at" },
                    { new Guid("522194f4-621a-1a51-b3c3-f4b5d59edb8a"), "en", "Label-Form-Description", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Description" },
                    { new Guid("52b260d7-a65a-8f58-966d-3a9f18d8b426"), "en", "Label-TimeEffectiveTo", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Effective to" },
                    { new Guid("533d44b3-85e8-e757-944b-1fa74640daf1"), "en", "Button-Filter", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Filter" },
                    { new Guid("53751d0c-ffe9-2052-a00f-d6feac8252b3"), "en", "Label-IsSystemDefined", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Is system defined?" },
                    { new Guid("54389ce1-e9c1-2659-a5f6-77a3261bdf3b"), "en", "Button-Cancel", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Cancel" },
                    { new Guid("545f38d3-d2c5-c054-b78d-fd3b86978f74"), "en", "Value-ParameterTextType-Line", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Line" },
                    { new Guid("576f6d3b-224c-6158-b398-aa74cc69db94"), "en", "Header-Form-Update-SchemaPropertyDateTime", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Update Schema DateTime Property" },
                    { new Guid("57fcf980-ac14-9f53-b111-8d9854aff1ab"), "en", "Header-Form-Create-SchemaPropertyString", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Create Schema String Property" },
                    { new Guid("583538f3-ef60-3458-b608-b868b3f2408a"), "en", "Header-Property-String", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "String Properties" },
                    { new Guid("58384d89-b166-d358-b7bd-ab4a7f883705"), "en", "Header-Parameter", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Parameters" },
                    { new Guid("58a849ae-3232-555d-8546-f55f0f4cd961"), "en", "Help-Form-ParameterText", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Represents a dynamic text parameter value." },
                    { new Guid("5b2ba982-fb77-285b-bb43-ef9301e549ad"), "en", "Header-Schema", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Schemas" },
                    { new Guid("5b4d2734-efcf-625a-90ce-db276c3c8db9"), "en", "Button-Search", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Search" },
                    { new Guid("5bb89682-f6d8-cf56-824b-31f70b3696cf"), "en", "Label-Form-SortColumn", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Sorting column" },
                    { new Guid("5c37c656-8515-cf5d-9e21-4e531514d0d2"), "en", "Help-Form-ParameterSystem", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Represents a dynamic system parameter value." },
                    { new Guid("5e450513-5c07-bf55-92ab-b52c1c324e5d"), "en", "Header-Element", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Elements" },
                    { new Guid("5f47a8fa-beb0-1e59-b26f-cdd1a5168043"), "en", "Help-Form-Description", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "The description used when displaying an entity of this type." },
                    { new Guid("5f66a499-1d1f-3b58-816a-f58b12ce86db"), "en", "Header-Form-Update-SchemaPropertyString", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Update Schema String Property" },
                    { new Guid("5f78202b-b8e9-ef5c-aa71-a98a3bbed1e5"), "en", "Header-Form-Create-Schema", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Create Schema" },
                    { new Guid("60baf7a7-86fc-e75d-ba2b-99b85fd3c50a"), "en", "Button-Activate", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Activate" },
                    { new Guid("61b134cc-0391-3157-aa75-d552c9eb16a7"), "en", "Header-Property-Integer", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Integer Properties" },
                    { new Guid("63b05b76-729a-1a5b-8047-4b9be625ef52"), "en", "Header-Form-Update-SchemaPropertyInteger", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Update Schema Integer Property" },
                    { new Guid("68de3128-2419-815c-8b75-af88b1621f06"), "en", "Help-Form-Search", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Enter the search term you wish to use" },
                    { new Guid("6a692f6c-9ce2-d151-897b-ee21df321223"), "en", "Help-Form-IsApprovalRequired", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Determines if another user must approve a text parameter value before it becomes active." },
                    { new Guid("6ab2fba3-62aa-7154-8525-5ad461672f63"), "en", "Header-Form-SignIn", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "...Or sign in" },
                    { new Guid("6b0c28df-e22a-7a52-b444-7bd00aa51c75"), "en", "Label-TimeEffectiveFrom", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Effective from" },
                    { new Guid("6c2c1ff8-981c-7e5e-a611-eea008820861"), "en", "Label-Form-Weight", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Weight" },
                    { new Guid("6f90155a-4097-d754-bae0-7d2c82f8c40e"), "en", "Value-IsSystemDefined-No", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "User defined" },
                    { new Guid("70a581ea-7cad-095d-aa34-c6e06b1adaf8"), "en", "Label-ParameterSystem", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "System parameter" },
                    { new Guid("70e4a3f5-aee3-ea58-b507-a8ba90769a88"), "en", "Button-Reset", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Reset" },
                    { new Guid("71033a4e-c74a-5752-8f72-f6354163e6a3"), "en", "Value-PropertyDateTimeType-DateTime", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Date and time" },
                    { new Guid("711b234c-d827-8d56-9505-6351ed066c0e"), "en", "Value-No", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "No" },
                    { new Guid("720b8817-c1d1-fa57-a5a0-a382d36f89ae"), "en", "Help-Form-Name", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "The internal name of this entity type. Should only be visible in the configuration." },
                    { new Guid("7300afe9-5500-4750-a979-3094a9b5a58c"), "en", "Help-Form-Culture", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "The culture this entity is used together with." },
                    { new Guid("731c9d51-8441-c955-b541-3108eee4b484"), "en", "Label-TimeActivated", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Activated at" },
                    { new Guid("7381ca03-1c8a-525a-853a-4680de692737"), "en", "Label-Form-PageNumber", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Page" },
                    { new Guid("766687ab-2c7d-8458-b905-ff7814dd4650"), "en", "Help-Form-PageNumber", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "The current page being shown" },
                    { new Guid("76b4432a-d6f5-8257-afa1-ec5d35a2874d"), "en", "Label-PropertyInteger", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Integer property" },
                    { new Guid("76e77ee3-1d0e-615e-b77d-ebbcbca8257f"), "en", "Help-Form-Title", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "The title used when displaying an entity of this type." },
                    { new Guid("7741f4c2-0fdd-a150-948b-578dbda71297"), "en", "Help-Form-String", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Represents a string value." },
                    { new Guid("77ae8752-83d4-a555-8169-ed33835eac39"), "en", "Button-Create", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Create" },
                    { new Guid("77af5f7a-2e78-c153-9cdd-5d65cd199a50"), "en", "Help-Form-PageSize", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "The number of rows shown on each page" },
                    { new Guid("78ccc483-5841-835e-a389-b3e3cb3b09d8"), "en", "Label-Form-IsApprovalRequired", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Is approval required?" },
                    { new Guid("79aaf2f1-7d26-2952-9db3-031d5ace713e"), "en", "Label-IsApprovalRequired", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Is approval required?" },
                    { new Guid("7a50296f-8c13-385f-ace1-90ed92cb2bd0"), "en", "Header-Property-DateTime", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "DateTime Properties" },
                    { new Guid("7c8b4c27-8b82-af53-812b-59952f799285"), "en", "Label-Form-ParameterTextType", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Text parameter type" },
                    { new Guid("7cb40d8a-8a87-0b5c-b7a6-4ebf401699a7"), "en", "Help-Form-IsPublic", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Determines if the entity is public and can be accessed in the frontend." },
                    { new Guid("7f2819c7-2c9c-f958-be9e-6116ee4b59eb"), "en", "Value-IsActivated-Neutral", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Either" },
                    { new Guid("7f4ac09f-58f1-9a5b-952f-045cb6e2517d"), "en", "Label-ParameterTextType", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Text parameter type" },
                    { new Guid("80332dcd-cafb-1557-8490-cf8f49b8d5eb"), "en", "Header-Property-Collection", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Collection Properties" },
                    { new Guid("82986341-205b-055f-be0b-fd1f8a1f390c"), "en", "Label-Description", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Description" },
                    { new Guid("831a977a-3b2b-4b5b-ab8a-1fed259707d3"), "en", "Label-SchemaIdentifier", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Schema Identifier" },
                    { new Guid("83ed4112-37c9-2953-b93e-f300d530c26f"), "en", "Label-Form-Culture", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Culture" },
                    { new Guid("8516c014-3f4c-e259-858e-21c0e077fe8d"), "en", "Help-Form-ParameterStyle", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Represents a dynamic style parameter value." },
                    { new Guid("863f5c2d-a1f3-9b54-879a-ae7aa0839527"), "en", "Label-Form-PageSize", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Rows" },
                    { new Guid("874c09a4-bf67-c356-83e0-299db2542fdd"), "en", "Header-Form-Update-Schema", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Update Schema" },
                    { new Guid("87550a87-6c93-bd55-8bc2-d1227f7f537c"), "en", "Label-Title", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Title" },
                    { new Guid("89186e58-803c-795a-9b12-4b227f477828"), "en", "Label-IsPublic", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Is public?" },
                    { new Guid("8a25d1e6-9dec-a659-8a4f-e4e02ec7d16f"), "en", "Link-Navigation-Schema", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Schemas" },
                    { new Guid("8eab687c-9499-c15a-b54c-cd15bff4a398"), "en", "Label-Form-Boolean", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Boolean" },
                    { new Guid("8f2c1e2c-601b-a359-ba90-76893bde3926"), "en", "Label-PropertyBoolean", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Boolean property" },
                    { new Guid("915ee913-023c-835d-9692-b7744b1859df"), "en", "Header-Property", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Properties" },
                    { new Guid("916edb4a-d985-0258-a0da-0714c0756a59"), "en", "Header-Form-Create-SchemaContext", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Create Schema Context" },
                    { new Guid("91f289d7-37f7-8650-8991-317aea869045"), "en", "Help-Form-Password", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "A password consisting of at least 12 characters." },
                    { new Guid("94cf5069-32b8-9958-9365-9169d93bef5a"), "en", "Label-Form-SchemaIdentifier", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Schema identifier" },
                    { new Guid("96b1ed5b-6136-3a52-8626-57913232627d"), "en", "Label-Value", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Value" },
                    { new Guid("991c07a0-ea31-5a54-ad09-a6f8c65037b1"), "en", "Header-Parameter-System", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "System Parameters" },
                    { new Guid("9b9946b6-814c-8f5a-b448-573d6cd67d95"), "en", "Value-IsApprovalRequired-No", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Without approval required" },
                    { new Guid("9ba01eda-9757-145b-9ffd-d7722a273672"), "en", "Help-Form-ConfirmPassword", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Repeat your password to confirm it." },
                    { new Guid("9c7197d4-a19d-9c5a-ade3-6dae52d4430e"), "en", "Label-Form-DateEffective", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Effective date" },
                    { new Guid("9d7eb240-29cf-3751-9d76-d66a6ffbdd56"), "en", "Button-SignUp", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Sign up" },
                    { new Guid("9f7b9031-cd79-6e5d-bba1-3c931c4d83de"), "en", "Value-Yes", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Yes" },
                    { new Guid("a14ef23c-68e2-8656-8239-3c198d6af5e4"), "en", "Label-Form-ParameterStyle", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Style parameter" },
                    { new Guid("a5710070-300d-ee57-b89e-8c129f62bfe2"), "en", "Label-Form-SortDirection", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Sorting direction" },
                    { new Guid("aa7f1f73-9114-7450-b5b0-204423dab26f"), "en", "Label-Form-Password", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Password" },
                    { new Guid("ad7733eb-c7bf-cf52-800f-fcec78a87b92"), "en", "Header-Form-Create-SchemaParameterSystem", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Create Schema System Parameter" },
                    { new Guid("ae3a3d5f-be37-b956-9b30-9f4993d6c1e4"), "en", "Header-Form-Update-SchemaContext", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Update Schema Context" },
                    { new Guid("aeddf9de-a7d6-725a-b56d-494d87532536"), "en", "Header-Form-Update-SchemaElement", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Update Schema Element" },
                    { new Guid("b52c2908-d021-0a58-9c0e-fe999d66dd08"), "en", "Label-Form-IsActivated", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Is activated?" },
                    { new Guid("b783d5c0-b526-5457-8562-eb239410c7ee"), "en", "Value-IsActivated-No", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Never activated" },
                    { new Guid("b79ec6b6-f05a-e557-bdcb-299c7769f214"), "en", "Header-Form-Create-SchemaPropertyDecimal", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Create Schema Decimal Property" },
                    { new Guid("ba0e89c8-8af7-b158-93b3-a6559a6b7590"), "en", "Help-Form-ParameterType", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "The specific type of parameter to be used." },
                    { new Guid("bc3e9e6b-fbc5-c95e-9abf-7f31b2fcceb4"), "en", "Label-Form-Decimal", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Decimal" },
                    { new Guid("bc4a8035-4838-d85d-815c-2cd0c6e438a3"), "en", "Label-Form-Value", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Value" },
                    { new Guid("bc8d9a0b-2d8a-9a50-b79e-6b0766e7d6ad"), "en", "Label-TimeApproved", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Approved at" },
                    { new Guid("bd26110d-464b-f355-944c-6e8a6ea6c9ff"), "en", "Value-IsSystemDefined-Neutral", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Either" },
                    { new Guid("bf043def-4c30-5758-b65e-448b55252097"), "en", "Header-Form-Update-SchemaPropertyDecimal", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Update Schema Decimal Property" },
                    { new Guid("bf05580f-4b16-225a-be36-316d95445dae"), "en", "Header-Form-Create-SchemaPropertyDateTime", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Create Schema DateTime Property" },
                    { new Guid("c01e9da7-1f9e-1155-a911-b17bdab8c883"), "en", "Help-Form-Email", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "The email address you want to associate with the account." },
                    { new Guid("c2b664b9-a36f-b351-aa3d-b86e4cf2b26e"), "en", "Header-Property-Image", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Image Properties" },
                    { new Guid("c2b74fe3-6657-545e-9fa0-e36ded378383"), "en", "Button-SignIn", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Sign in" },
                    { new Guid("c4c9650e-b1c1-1d5f-96f9-c2de2ef66cbc"), "en", "Header-Property-Boolean", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Boolean Properties" },
                    { new Guid("c8fcbec0-f595-5150-9076-932e95a1cb8c"), "en", "Label-Form-IsSystemDefined", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Is system defined?" },
                    { new Guid("c9a7ba2b-3950-185d-9738-cefc2ee843e6"), "en", "Value-Ascending", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Ascending" },
                    { new Guid("ca277b63-8e35-9751-9138-ff4a60f86343"), "en", "Label-Form-String", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "String" },
                    { new Guid("cafd5eb6-acf6-b25f-a1dd-d46d6a3e318d"), "en", "Help-Form-ParameterTextType", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "The type of text parameter." },
                    { new Guid("cd1531a7-16e8-575b-96d3-5c724e42c690"), "en", "Header-Form-SignUp", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Create an account" },
                    { new Guid("cd423812-422a-3754-a268-2c6da4530f55"), "en", "Label-Form-ParameterSystemType", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "System parameter type" },
                    { new Guid("cfc93955-49e7-1a52-a75e-d4ca5b8ea774"), "en", "Label-PropertyImage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Image property" },
                    { new Guid("cfe751e2-9c65-7450-a51c-d6c0ae9a88d0"), "en", "Label-TimeEffective", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Effective at" },
                    { new Guid("d29e9af1-da61-e053-94aa-e3f209955a00"), "en", "Help-Form-DateTime", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Represents a datetime value." },
                    { new Guid("d3b5fe6e-2f93-1e5a-b63d-9a1ea7640cc0"), "en", "Header-Form-Create-SchemaPropertyBoolean", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Create Schema Boolean Property" },
                    { new Guid("d4749ad1-8058-9452-ab1a-47927a49837e"), "en", "Label-Form-Integer", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Integer" },
                    { new Guid("d49ef34c-0df3-7d59-a117-83ca1d669398"), "en", "Header-Form-Create-SchemaParameterStyle", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Create Schema Style Parameter" },
                    { new Guid("d5c7d129-9df3-8753-8146-e53ee23ed932"), "en", "Help-Form-Decimal", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Represents a decimal value." },
                    { new Guid("d5eabcb2-3add-d759-856b-d157ecf10bea"), "en", "Default-EmptyList", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Nothing here yet..." },
                    { new Guid("d76e3d61-5f23-df5c-9f53-fee8bc20634d"), "en", "Value-PropertyDateTimeType-Date", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Date" },
                    { new Guid("d8dd57b8-8b1a-1f5c-b7c0-e78a301f460f"), "en", "Value-ParameterTextType-Text", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Text" },
                    { new Guid("dac31e61-5ab8-cf5a-a821-23e8dd15f0bf"), "en", "Label-Form-Search", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Search" },
                    { new Guid("dba48fd6-2f03-aa5f-8fc8-658d3a4a8fe9"), "en", "Header-Form-Create-SchemaParameterText", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Create Schema Text Parameter" },
                    { new Guid("dbc8487f-a4d0-a654-a947-601b4ab10810"), "en", "Label-Form-ParameterText", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Text parameter" },
                    { new Guid("dc23ebb2-0a17-3c5f-aad3-5939e1b49ba6"), "en", "Label-Form-PropertyType", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Property type" },
                    { new Guid("de4b3b3a-153b-555d-87be-918e0bd0bf5e"), "en", "Label-PropertyDateTimeType", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "DateTime property type" },
                    { new Guid("de7033ae-894b-8458-b655-e14955010d3d"), "en", "Link-Navigation-Parameter", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Parameters" },
                    { new Guid("df1cb2b3-0586-8651-a85e-9ad1bfbf32d7"), "en", "Value-IsApprovalRequired-Yes", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "With approval required" },
                    { new Guid("e23c6ad9-408f-ec53-8eea-87e03bc5730d"), "en", "Header-Form-Create-SchemaElement", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Create Schema Element" },
                    { new Guid("e37acee3-b9e6-5b56-999e-78fa6aa645e8"), "en", "Value-IsApprovalRequired-Neutral", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Either" },
                    { new Guid("e3c84e6d-26ba-ef56-97d2-38f117a55b5d"), "en", "Label-ID", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "ID" },
                    { new Guid("e7db3545-82d2-cb59-9e9b-2c2f7993afe1"), "en", "Header-Form-Update-SchemaParameterText", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Update Schema Text Parameter" },
                    { new Guid("eb2181ee-2663-3b5b-892f-dbfc211c1e9c"), "en", "Help-Form-Direction", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "The direction which the page is sorted by" },
                    { new Guid("ede5a638-bd66-6c5e-9fda-ac7601dde571"), "en", "Label-ParameterSystemType", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "System parameter type" },
                    { new Guid("f6491027-3c92-9e50-b5d6-d18299e89efb"), "en", "Help-Form-PropertyType", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "The type of property." },
                    { new Guid("f6acff54-8ddf-5856-aa2e-941622e79e88"), "en", "Label-PropertyString", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "String property" },
                    { new Guid("f9818080-74d2-7650-a77b-8d11b2037dac"), "en", "Value-ParameterTextType-RichText", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Rich text" },
                    { new Guid("f9912a32-a019-5255-9769-7214da807ebc"), "en", "Label-Form-ParameterType", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Parameter type" },
                    { new Guid("faeadfa3-051b-345c-bb9f-f87a73dc24c4"), "en", "Label-ParameterStyle", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Style parameter" },
                    { new Guid("fb277ee9-d100-e25d-9d4b-97b86bdb563d"), "en", "Header-Property-Table", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Table Properties" },
                    { new Guid("ffd89259-2bd0-9a55-8609-148cb3f4cfe6"), "en", "Help-Form-Weight", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "The sorting weight of this entity type when it is being ordered." }
                });

            migrationBuilder.InsertData(
                table: "TableSchema",
                columns: new[] { "ID", "IsActive", "Name", "Note", "TimeActivated", "TimeCreated", "TimeUpdated" },
                values: new object[] { new Guid("12345678-1234-1234-1234-1234567890ab"), true, "Base schema", "This is a base schema intended to be cloned and extended.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null });

            migrationBuilder.InsertData(
                table: "TableSchemaParameterSystem",
                columns: new[] { "ID", "IsApprovalRequired", "IsPublic", "IsSystemDefined", "Name", "Note", "SchemaID", "SchemaIdentifier", "TimeCreated", "TimeUpdated", "Type" },
                values: new object[,]
                {
                    { new Guid("0604bbd3-5fd9-6654-9a89-05b21d16da76"), true, true, true, "Schema-Inactive-Element-Edit", "Can you edit a Element entity from a Schema that has not been activated yet?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Inactive-Element-Edit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("10d8395c-f8b1-095c-af66-f872c1b632f5"), true, true, true, "Schema-Inactive-Context-Add", "Can you add new Context entities to a Schema that has not been activated yet?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Inactive-Context-Add", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("12828032-0de9-ad59-bdc7-5a139e2c4f37"), true, true, true, "Schema-Deactivated-Parameter-Clone", "Can you clone a Parameter entity from a Schema that used to be active?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Deactivated-Parameter-Clone", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("1de1ac71-2af4-3653-b358-d0da6daa7374"), true, true, true, "Schema-Inactive-Parameter-Edit", "Can you edit a Property entity from a Schema that has not been activated yet?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Inactive-Parameter-Edit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("201a6f64-5ef6-ac51-801f-ccc70d12c46b"), true, true, true, "Schema-Deactivated-Context-Add", "Can you add new Context entities to a Schema that used to be active?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Deactivated-Context-Add", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("213a921b-38b4-4c59-80b8-dc8949e71768"), true, true, true, "Schema-Inactive-Element-Clone", "Can you clone an Element entity from a Schema that has not been activated yet?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Inactive-Element-Clone", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("2e64285e-6342-455c-a6c8-93057af67f96"), true, true, true, "Schema-Deactivated-Property-Edit", "Can you edit a Property entity from a Schema that used to be active?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Deactivated-Property-Edit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("2f27782e-16f8-5354-b0c9-614b804e1422"), true, true, true, "Schema-Deactivated-Property-Add", "Can you add new Property entities to a Schema that used to be active?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Deactivated-Property-Add", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("3531680f-e528-f855-a9eb-39f31ce3b9b2"), true, true, true, "Schema-Inactive-Context-Delete", "Can you remove a Context entity from a Schema that has not been activated yet?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Inactive-Context-Delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("3a119a0f-8276-125e-9420-7b26eeb34c30"), true, true, true, "Schema-Inactive-Parameter-Clone", "Can you clone a Parameter entity from a Schema that has not been activated yet?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Inactive-Parameter-Clone", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("41ad1a2e-c2a3-5750-95de-14990b31e4ef"), true, true, true, "Schema-Inactive-Context-Edit", "Can you edit a Context entity from a Schema that has not been activated yet?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Inactive-Context-Edit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("4270ffc2-7b75-5759-a13b-28bcca050774"), true, true, true, "Schema-Inactive-Property-Clone", "Can you clone a Property entity from a Schema that has not been activated yet?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Inactive-Property-Clone", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("4371e46b-ab38-b25a-8411-c1f889e0b3fe"), true, true, true, "Schema-Deactivated-Entity-Edit", "Can you edit a Schema entity that used to be active?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Deactivated-Entity-Edit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("4499661e-007d-ef54-b484-fa1cb5354cb6"), true, true, true, "Schema-Deactivated-Property-Clone", "Can you clone a Property entity from a Schema that used to be active?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Deactivated-Property-Clone", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("44b83b84-fff4-e459-a65b-7919fc1219a5"), true, true, true, "Schema-Inactive-ElementHasProperty-Add", "Can you add new ElementHasProperty entities to a Schema that has not been activated yet?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Inactive-ElementHasProperty-Add", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("48fc0232-8593-535d-bc11-70ae375dcd6a"), true, true, true, "Schema-Deactivated-ElementHasProperty-Add", "Can you add new ElementHasProperty entities to a Schema that used to be active?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Deactivated-ElementHasProperty-Add", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("4ea3f38d-c50e-ac53-b28c-701ff652e4cb"), true, true, true, "Schema-Inactive-Entity-Edit", "Can you edit a Schema entity that has not been activated yet?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Inactive-Entity-Edit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("565a7741-5e03-685a-bbcb-1eebb84a1c8b"), true, true, true, "Schema-Deactivated-Element-Delete", "Can you remove an Element entity from a Schema that used to be active?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Deactivated-Element-Delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("570ac253-f1bb-655a-926a-5d382053d5b6"), true, true, true, "Schema-Deactivated-Context-Delete", "Can you remove a Context entity from a Schema that used to be active?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Deactivated-Context-Delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("5b062cc7-ac63-cd5e-a351-37c433758738"), true, true, true, "Schema-Deactivated-Parameter-Edit", "Can you edit a Parameter entity from a Schema that used to be active?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Deactivated-Parameter-Edit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("5c268f8c-b7b9-e35b-ae16-1eba43b78a36"), true, true, true, "Schema-Deactivated-Parameter-Delete", "Can you remove a Parameter entity from a Schema that used to be active?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Deactivated-Parameter-Delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("6b8b0153-c050-0c55-abc4-9943e2dad6ab"), true, true, true, "Schema-Inactive-Entity-Delete", "Can you remove a Schema entity that has not been activated yet?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Inactive-Entity-Delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("6cd1d1d7-f7f8-2a5c-8a94-f35100e801d8"), true, true, true, "Schema-Deactivated-ContextHasElement-Add", "Can you add new ContextHasElement entities to a Schema that used to be active?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Deactivated-ContextHasElement-Add", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("8135b9ff-c66d-ae59-9c92-6309db358946"), true, true, true, "Schema-Inactive-Element-Add", "Can you add new Element entities to a Schema that has not been activated yet?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Inactive-Element-Add", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("852ac549-b744-f256-abda-fbf63e082012"), true, true, true, "Schema-Inactive-Property-Edit", "Can you edit a Parameter entity from a Schema that has not been activated yet?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Inactive-Property-Edit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("8f82ec83-5778-d65e-b6b2-e2f357f0f051"), true, true, true, "Schema-Inactive-Context-Clone", "Can you clone a Context entity from a Schema that has not been activated yet?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Inactive-Context-Clone", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("9386df1c-3652-c153-9b65-13ca7450ff9d"), true, true, true, "Schema-Inactive-Property-Delete", "Can you remove a Parameter entity from a Schema that has not been activated yet?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Inactive-Property-Delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("938efbde-22de-405c-9b35-f6f91846f128"), true, true, true, "Schema-Deactivated-Entity-Delete", "Can you remove a Schema entity that used to be active?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Deactivated-Entity-Delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("9480a2c9-ed16-575e-a691-5461cbae7801"), true, true, true, "Schema-Deactivated-Element-Add", "Can you add new Element entities to a Schema that used to be active?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Deactivated-Element-Add", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("97ba2bdf-0a01-7358-9d9f-b5bff98ae8fb"), true, true, true, "Schema-Inactive-ContextHasElement-Add", "Can you add new ContextHasElement entities to a Schema that has not been activated yet?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Inactive-ContextHasElement-Add", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("a3450dae-74a5-1255-9984-04c3d440cb49"), true, true, true, "Schema-Deactivated-Context-Clone", "Can you clone a Context entity from a Schema that used to be active?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Deactivated-Context-Clone", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("a3be2cd5-57b7-6c54-a8a9-7fa77c469e2d"), true, true, true, "Schema-Deactivated-Element-Edit", "Can you edit an Element entity from a Schema that used to be active?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Deactivated-Element-Edit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("b0937fbb-4d21-7656-a294-7c6b6deef7bd"), true, true, true, "Schema-Deactivated-Element-Clone", "Can you clone an Element entity from a Schema that used to be active?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Deactivated-Element-Clone", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("b2f1e7c4-21fb-a25f-8d4c-479b9820b9ac"), true, true, true, "Schema-Deactivated-Context-Edit", "Can you edit a Context entity from a Schema that used to be active?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Deactivated-Context-Edit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("b9618dee-39b4-d75d-bba8-a83036ccaac1"), true, true, true, "Schema-Inactive-Parameter-Delete", "Can you remove a Property entity from a Schema that has not been activated yet?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Inactive-Parameter-Delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("ca63470d-840e-9b59-ac61-99ad3242d3d1"), true, true, true, "Schema-Inactive-Parameter-Add", "Can you add new Property entities to a Schema that has not been activated yet?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Inactive-Parameter-Add", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("d0fc8cc6-98fa-e153-98f5-52e593acb87d"), true, true, true, "Schema-Deactivated-Parameter-Add", "Can you add new Parameter entities to a Schema that used to be active?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Deactivated-Parameter-Add", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("dc8aad8d-1dbc-b651-a0fb-48d4770e93e2"), true, true, true, "Schema-Inactive-Property-Add", "Can you add new Parameter entities to a Schema that has not been activated yet?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Inactive-Property-Add", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("ef52bb6b-aec6-a856-b49d-6a61437e7d5b"), true, true, true, "Schema-Deactivated-Property-Delete", "Can you remove a Property entity from a Schema that used to be active?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Deactivated-Property-Delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("f32679d3-f5dc-e659-b0c0-fb9aa5a6ab6d"), true, true, true, "Schema-Inactive-Element-Delete", "Can you remove an Element entity from a Schema that has not been activated yet?", new Guid("12345678-1234-1234-1234-1234567890ab"), "Schema-Inactive-Element-Delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 }
                });

            migrationBuilder.InsertData(
                table: "TableSchemaParameterText",
                columns: new[] { "ID", "IsApprovalRequired", "IsSystemDefined", "Name", "Note", "SchemaID", "SchemaIdentifier", "TimeCreated", "TimeUpdated", "Type" },
                values: new object[,]
                {
                    { new Guid("03d8bee9-de6d-595e-9017-048d135dac24"), false, true, "Label-ID", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-ID", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("04359e0e-61b9-1151-b449-c60fd088ce03"), false, true, "Help-Form-Email", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Help-Form-Email", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("06dbaf41-de9d-6050-b7d9-002296e91e3a"), false, true, "Link-Navigation-Parameter", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Link-Navigation-Parameter", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("08f9d6cc-a236-fa51-95f7-f80b068d507b"), false, true, "Value-No", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Value-No", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("097a818a-40f0-885d-a5ce-87d485927e75"), false, true, "Help-Form-Title", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Help-Form-Title", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("0c614ab4-c187-5a5e-9fdb-b6815e491c3d"), false, true, "Label-IsPublic", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-IsPublic", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("0c7174ae-e7d0-525a-b3ff-6cf2a1f31450"), false, true, "Help-Form-ParameterText", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Help-Form-ParameterText", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("0e5950d1-8a47-1f5c-8312-5161ce11457e"), false, true, "Button-Cancel", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Button-Cancel", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("0e813e3b-5afa-965c-807e-ebebd97fd62a"), false, true, "Label-PropertyInteger", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-PropertyInteger", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("0fb77e0f-7907-025e-98a8-afe0e70bc26f"), false, true, "Header-Form-Create-SchemaPropertyCollection", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Form-Create-SchemaPropertyCollection", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("0fbba327-1ae7-1f55-9035-d7813e57ddd4"), false, true, "Help-Form-ParameterStyle", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Help-Form-ParameterStyle", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("10ac4aee-be26-1455-8991-1aab70c19320"), false, true, "Label-Form-Note", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Form-Note", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("116eaf30-790a-d75d-9679-21314bee8d46"), false, true, "Button-Submit", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Button-Submit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("11b6fb86-28cb-d850-8dd1-41e4bbecae32"), false, true, "Header-Parameter", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Parameter", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("11b7d226-5653-945a-a90a-b792d8b3d187"), false, true, "Header-Parameter-Style", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Parameter-Style", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("132c107a-603d-c55c-943b-9c1c5ef28ac8"), false, true, "Header-Form-Create-SchemaParameterStyle", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Form-Create-SchemaParameterStyle", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("135901e3-dbbe-b552-8bf4-a49f4ceee05d"), false, true, "Label-Form-Password", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Form-Password", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("142a15eb-bba0-155c-8a03-a4211140da55"), false, true, "Header-Form-Create-SchemaPropertyDecimal", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Form-Create-SchemaPropertyDecimal", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("158e4685-6eef-3d55-b435-255af1075e58"), false, true, "Header-Form-Create-SchemaPropertyDateTime", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Form-Create-SchemaPropertyDateTime", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("16ed80f8-9d3d-af5c-a807-954bd2f6a3d7"), false, true, "Label-Form-String", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Form-String", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("17cba7cd-915d-ee5f-a9e5-66ae166f6686"), false, true, "Label-Form-IsActivated", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Form-IsActivated", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("18c7194f-fd7b-625a-8a4f-e5daa0b505a8"), false, true, "Label-IsApprovalRequired", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-IsApprovalRequired", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("1974c186-97ab-8652-a3d9-59e4f760c097"), false, true, "Label-Form-Culture", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Form-Culture", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("1af65777-7376-435d-a407-2bb58eb03b86"), false, true, "Header-Form-Create-SchemaElement", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Form-Create-SchemaElement", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("1b114808-25db-fc55-a5e8-482cd7a8d6a0"), false, true, "Label-Form-IsApprovalRequired", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Form-IsApprovalRequired", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("1b52f3b9-7112-1050-9b46-dba2db1c3621"), false, true, "Label-Form-ParameterStyle", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Form-ParameterStyle", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("1d2c4bf5-a82f-4e57-b009-2720bf7da03c"), false, true, "Label-Form-Value", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Form-Value", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("22edeee7-fb43-9a50-8c73-4df85395f24e"), false, true, "Header-Property-Collection", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Property-Collection", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("25b1cefd-493c-0252-9fd6-4250413a6056"), false, true, "Header-Form-Update-SchemaPropertyCollection", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Form-Update-SchemaPropertyCollection", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("260aab2b-ebd9-8b5f-89f6-d05615bca770"), false, true, "Header-Property-DateTime", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Property-DateTime", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("265a999a-83e3-c35b-bb21-3380d600e21a"), false, true, "Button-Create", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Button-Create", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("2aa1a894-48d9-b35e-9c0a-5dd3ab759462"), false, true, "Value-IsApprovalRequired-Neutral", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Value-IsApprovalRequired-Neutral", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("2c2ad8eb-b8f1-fe57-9b85-f11242f8b2bb"), false, true, "Label-PropertyDateTime", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-PropertyDateTime", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("2c7ba339-3a46-655c-a578-e7c2db4efca8"), false, true, "Header-Property-String", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Property-String", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("2c88ed52-9e2b-0854-be2e-4ec48333d883"), false, true, "Help-Form-Column", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Help-Form-Column", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("2cd4da75-47b6-fe55-94e4-b8f6c77764e5"), false, true, "Header-Form-Update-SchemaPropertyDecimal", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Form-Update-SchemaPropertyDecimal", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("2cfa78c3-7571-ce5d-86cc-810e7583592d"), false, true, "Label-Form-SortColumn", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Form-SortColumn", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("2dea77bb-2780-9651-9625-6651214fe95e"), false, true, "Help-Form-ParameterType", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Help-Form-ParameterType", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("2e0e2197-5388-8358-b1c5-02d3f1143f16"), false, true, "Help-Form-Weight", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Help-Form-Weight", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("31037c21-56c3-725f-817b-468d2f25261b"), false, true, "Help-Form-PageSize", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Help-Form-PageSize", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("31884903-9ace-2159-8660-5f2875d9429e"), false, true, "Header-Form-Create-SchemaPropertyBoolean", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Form-Create-SchemaPropertyBoolean", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("34e516ee-ee7d-b15c-80cf-f680abfb909c"), false, true, "Value-IsActivated-Neutral", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Value-IsActivated-Neutral", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("350a9593-9e6d-0f53-b755-a0488f7a6cf7"), false, true, "Label-Description", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Description", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("3a0c553c-7825-2b53-9491-813230975088"), false, true, "Label-PropertyImage", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-PropertyImage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("3b71208b-3bbf-7156-8c78-2196e39f9505"), false, true, "Label-Form-ParameterTextType", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Form-ParameterTextType", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("3cccf20e-fbd3-5959-ba15-f6aaa77bcaf2"), false, true, "Label-TimeEffective", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-TimeEffective", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("3d95a0b5-5809-515f-9244-433e48c57881"), false, true, "Header-Form-Update-SchemaPropertyDateTime", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Form-Update-SchemaPropertyDateTime", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("3ff4dd78-4e0e-b353-ac18-e14090837680"), false, true, "Help-Form-PropertyType", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Help-Form-PropertyType", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("410afb63-11f8-5f59-ac4c-fc9436003881"), false, true, "Value-IsActivated-No", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Value-IsActivated-No", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("414555c9-4804-df52-8afd-118251ff3c2d"), false, true, "Label-PropertyDateTimeType", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-PropertyDateTimeType", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("43428440-c843-315a-9807-6e213d81bc69"), false, true, "Label-IsSystemDefined", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-IsSystemDefined", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("434e4d4f-77af-0456-ab29-1527f6030aa4"), false, true, "Header-Context", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Context", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("43510c88-6b18-dd5e-b7f2-4056c99cee4f"), false, true, "Header-Form-Create-SchemaContext", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Form-Create-SchemaContext", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("44f307b7-929d-0d58-9ee5-737b00077a23"), false, true, "Label-PropertyDecimal", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-PropertyDecimal", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("490085d6-33e6-ef5a-ab60-e3752a45ec5b"), false, true, "Header-Property-Image", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Property-Image", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("49bd95b8-b4fd-1a54-8a0e-856abb764fe4"), false, true, "Label-TimeApproved", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-TimeApproved", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("4ad72c5d-c675-7a55-9e6c-2bf456e0c6ba"), false, true, "Help-Form-ParameterSystem", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Help-Form-ParameterSystem", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("4c3a7f71-54f4-c852-bdda-c8c6f4f0d053"), false, true, "Help-Form-Value", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Help-Form-Value", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("4c3c8785-c675-0953-83f1-9a5a5bbf0e2c"), false, true, "Label-TimeCreated", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-TimeCreated", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("4dbbdf71-7f2a-2850-bfd6-0ea2d65d69cb"), false, true, "Label-Form-Description", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Form-Description", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("526c02db-81a6-9a58-a057-374238f2fcbc"), false, true, "Value-IsSystemDefined-Neutral", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Value-IsSystemDefined-Neutral", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("528ac6f8-74f8-405e-b429-7612e6843383"), false, true, "Label-ParameterText", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-ParameterText", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("5392b793-af8c-1555-8f6c-6fa798e6704e"), false, true, "Help-Form-Note", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Help-Form-Note", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("57126d79-5d0e-8954-bab4-ad4a46f59211"), false, true, "Help-Form-Culture", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Help-Form-Culture", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("5834c937-fbd7-ea57-b4c4-9398c1a50a86"), false, true, "Header-Form-Update-SchemaPropertyInteger", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Form-Update-SchemaPropertyInteger", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("58759138-5fe8-0155-976d-22971965a21a"), false, true, "Help-Form-DateTimeType", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Help-Form-DateTimeType", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("590d1532-9dfa-665f-afd1-1127984bd94c"), false, true, "Default-EmptyValue", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Default-EmptyValue", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("5b931132-afa8-205f-81c4-7ac61d11c83a"), false, true, "Label-Form-Decimal", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Form-Decimal", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("5d8b4114-d390-6a5a-bebc-6db15bcb3b17"), false, true, "Header-Parameter-Text", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Parameter-Text", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("5f8c5539-54cf-c952-8d7b-de5307991ef6"), false, true, "Header-Form-SignUp", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Form-SignUp", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("5ff52e07-d728-a25f-9029-e7b2a347951f"), false, true, "Label-Form-Name", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Form-Name", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("6061f50d-bdfd-1a50-bf9c-111a6699896e"), false, true, "Help-Form-SchemaIdentifier", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Help-Form-SchemaIdentifier", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("61403e18-f73e-cc55-9add-5db1fe96aa8d"), false, true, "Value-IsApprovalRequired-Yes", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Value-IsApprovalRequired-Yes", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("61fd28ed-2bc0-3858-9af6-3d9f7d888683"), false, true, "Label-ParameterSystem", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-ParameterSystem", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("62f8d79d-0705-1059-9232-fb12cbb11d6d"), false, true, "Header-Property-Table", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Property-Table", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("649eaecd-b634-ae55-9b0e-86a3af92993c"), false, true, "Help-Form-String", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Help-Form-String", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("64c3ff58-6184-245a-a227-fec2c2fc007b"), false, true, "Value-Descending", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Value-Descending", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("655e1ef4-26ab-dd52-a2e1-b57cdb994a1f"), false, true, "Value-PropertyDateTimeType-DateTime", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Value-PropertyDateTimeType-DateTime", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("67e7b2eb-b4cd-ac59-974d-e61b7e4e2996"), false, true, "Label-Form-Weight", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Form-Weight", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("688da123-8483-c455-bd0b-39aa9cae7309"), false, true, "Help-Form-IsPublic", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Help-Form-IsPublic", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("6903216f-e7fd-ca5d-8f8e-e91a0d8e1e21"), false, true, "Help-Form-ParameterTextType", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Help-Form-ParameterTextType", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("6adba4e4-cd25-5c5a-b941-b155e49c2a42"), false, true, "Help-Form-PageNumber", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Help-Form-PageNumber", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("6afac287-144e-d05a-a47c-9fc2effc1e6a"), false, true, "Header-Element", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Element", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("6c707423-7e96-f458-ad9b-928d2c593069"), false, true, "Label-TimeEffectiveTo", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-TimeEffectiveTo", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("6c749d02-8cf5-f15d-bd1f-1db740daa5d5"), false, true, "Label-Form-SortDirection", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Form-SortDirection", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("6cfca215-5e97-ed53-b19b-fda959aa4e59"), false, true, "Help-Form-DateTime", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Help-Form-DateTime", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("72e87d47-d740-1e5a-9f3a-9ae2b036f40d"), false, true, "Value-IsApprovalRequired-No", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Value-IsApprovalRequired-No", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("737feb08-2fec-c35b-86af-6546b3ce2e23"), false, true, "Header-Form-Update-SchemaPropertyTable", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Form-Update-SchemaPropertyTable", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("73cbd243-dddb-4e55-bc54-8301cd123ed2"), false, true, "Help-Form-Direction", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Help-Form-Direction", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("75021139-599e-6653-8d95-5d6d212ba344"), false, true, "Label-Form-Integer", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Form-Integer", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("77a9d53c-4228-b551-96c2-61faab351094"), false, true, "Label-ParameterSystemType", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-ParameterSystemType", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("7976e65a-db3d-b05e-8e82-e4675f348c08"), false, true, "Button-Activate", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Button-Activate", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("79f2c3bb-6ab5-2b54-95bf-cb4aa3102c14"), false, true, "Value-ParameterTextType-RichText", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Value-ParameterTextType-RichText", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("7a2c0c12-3776-995c-9d8e-3424a335ed62"), false, true, "Label-TimeEffectiveFrom", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-TimeEffectiveFrom", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("7ab5e90a-62df-8a55-874f-ec7fd38387cd"), false, true, "Help-Form-IsApprovalRequired", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Help-Form-IsApprovalRequired", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("7b6799b5-3215-4b53-98e4-665f0f74c07f"), false, true, "Header-Schema", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Schema", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("7db2900f-b15f-8d5c-893b-16f46e3afecb"), false, true, "Help-Form-IsSystemDefined", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Help-Form-IsSystemDefined", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("81e81a72-369a-4458-b96c-4463b7a359e8"), false, true, "Header-Form-Create-SchemaParameterSystem", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Form-Create-SchemaParameterSystem", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("824e48bb-141e-c85d-9669-3d29d8b147ee"), false, true, "Header-Form-Create-Schema", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Form-Create-Schema", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("82fbca23-9805-3456-9989-55026045c7e4"), false, true, "Help-Form-ConfirmPassword", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Help-Form-ConfirmPassword", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("84618cb1-4aca-165a-a1ca-01d4d0b94131"), false, true, "Value-PropertyDateTimeType-Date", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Value-PropertyDateTimeType-Date", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("85b74e80-0856-ec58-9180-7117b8573aa0"), false, true, "Label-PropertyBoolean", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-PropertyBoolean", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("8812be01-6af6-e75c-8b76-fb84791c6942"), false, true, "Help-Form-Search", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Help-Form-Search", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("89bd5a45-ebb4-f856-bc5d-6934d9dfc996"), false, true, "Label-Form-SchemaIdentifier", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Form-SchemaIdentifier", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("8d455300-50f7-e05e-9370-4c28405be08d"), false, true, "Label-Form-ParameterType", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Form-ParameterType", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("8eed3f11-62ba-ed56-a70d-b57161c18534"), false, true, "Header-Property-Boolean", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Property-Boolean", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("9396fe23-7cf6-f05a-8f3c-7084cec384a2"), false, true, "Value-ParameterTextType-Text", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Value-ParameterTextType-Text", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("95f523c0-f7a9-1658-b288-568343981990"), false, true, "Header-Form-Update-SchemaPropertyString", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Form-Update-SchemaPropertyString", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("98096c4b-8ac5-5f51-b02a-e22ec4c3b6dd"), false, true, "Label-Form-IsPublic", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Form-IsPublic", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("98ea9f4b-4edd-d955-b142-5087d6158593"), false, true, "Header-Form-SignIn", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Form-SignIn", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("99af574b-78d4-8a56-81d5-ebe130937d13"), false, true, "Label-Form-ParameterText", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Form-ParameterText", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("9abdb591-bb00-5e56-9c03-93e3efa65a7c"), false, true, "Label-TimeActivated", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-TimeActivated", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("9c756ae5-5301-735f-9328-02d428a3e518"), false, true, "Label-ParameterStyle", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-ParameterStyle", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("9df06249-549e-105f-91d1-f9c44af2a9f8"), false, true, "Value-IsActivated-Yes", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Value-IsActivated-Yes", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("9f010acb-a96d-5e56-a3ae-1efc4c5bee96"), false, true, "Header-Property", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Property", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("9fc93efc-5b0c-4759-a767-628ffca796a1"), false, true, "Help-Form-Integer", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Help-Form-Integer", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("a137875f-4a36-a451-84b0-6a546f321f17"), false, true, "Value-Yes", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Value-Yes", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("a223f441-89e5-b151-9ea2-89e38a1d265e"), false, true, "Header-Form-Update-Schema", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Form-Update-Schema", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("a30f6b5c-f2c9-7158-ae87-f924e7dd2c8e"), false, true, "Label-Name", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Name", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("a36e4484-c980-5354-94cb-f6c5e1931952"), false, true, "Help-Form-IsActivated", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Help-Form-IsActivated", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("a4b58161-5f43-325e-a8a2-2d6cefc20654"), false, true, "Label-Form-DateEffective", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Form-DateEffective", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("a7a61bdd-c8ae-e75c-8abc-409322eec1e7"), false, true, "Header-Property-Integer", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Property-Integer", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("a8374dfa-342b-f256-b60f-137c7bf781be"), false, true, "Label-Form-ParameterSystemType", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Form-ParameterSystemType", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("a942ca51-793a-0c5a-8b20-41e4a16457e6"), false, true, "Header-Form-Update-SchemaParameterSystem", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Form-Update-SchemaParameterSystem", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("ab8d6e94-2500-e05b-8ffb-2d0e1034a90d"), false, true, "Help-Form-Boolean", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Help-Form-Boolean", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("adadd9a6-2822-d551-97db-34083cb11d6b"), false, true, "Value-ParameterTextType-Line", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Value-ParameterTextType-Line", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("ae013268-60b6-4b50-9f8e-24feb54e22c5"), false, true, "Help-Form-DateEffective", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Help-Form-DateEffective", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("aeefbadf-f76c-2f5e-af37-056196ff0578"), false, true, "Header-Form-Create-SchemaParameterText", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Form-Create-SchemaParameterText", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("af5c35af-3daf-2453-9ffe-996232694848"), false, true, "Label-Form-PageNumber", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Form-PageNumber", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("afcbef10-0ff6-5657-9f03-447de69704d2"), false, true, "Header-Form-Create-SchemaPropertyImage", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Form-Create-SchemaPropertyImage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("b0246f51-a60f-535d-aac1-2d52d8ba3aba"), false, true, "Label-Form-ParameterSystem", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Form-ParameterSystem", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("b07f82dd-4cbf-045e-b675-026b9b6c5a18"), false, true, "Help-Form-Decimal", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Help-Form-Decimal", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("b268cf24-1929-9d51-97bc-6cf98b05bccf"), false, true, "Button-Update", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Button-Update", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("b4ce93b5-f1a4-5d55-ae1a-107358d06c82"), false, true, "Label-Form-Title", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Form-Title", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("b4d98083-3d31-8057-aca6-3d57ccfdee7b"), false, true, "Button-Search", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Button-Search", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("b6ae1d3d-4f65-2b5d-9709-64bf1147365b"), false, true, "Header-Form-Create-SchemaPropertyInteger", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Form-Create-SchemaPropertyInteger", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("b6e7d329-0ddc-3f5f-9f32-aadfcd5dc6cf"), false, true, "Button-SignOut", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Button-SignOut", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("b9f82b92-4d03-7b55-b2ea-2c9ab6ef4cd8"), false, true, "Button-SignUp", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Button-SignUp", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("ba479c57-f7b1-605d-b572-883fa57b396d"), false, true, "Label-PropertyString", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-PropertyString", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("bbb2b273-b2b9-a65e-992d-f259151342b1"), false, true, "Value-IsSystemDefined-Yes", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Value-IsSystemDefined-Yes", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("bd9bfb5d-a86f-c054-bfbf-87775021662c"), false, true, "Default-EmptyList", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Default-EmptyList", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("bf5d90bd-d8b3-b152-89ee-38aea61c9cbf"), false, true, "Header-Form-Update-SchemaElement", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Form-Update-SchemaElement", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("c001b2a4-d427-fa58-93fa-318a23538a77"), false, true, "Label-Form-DateTimeType", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Form-DateTimeType", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("c485f4f2-1053-2651-8325-8fb6ca28d19e"), false, true, "Title", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Title", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("c516e17d-30fa-1a52-8ee6-e123b6e1b68e"), false, true, "Header-Form-Update-SchemaContext", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Form-Update-SchemaContext", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("c6c6600a-067c-7051-a16e-f9cc3d8c2338"), false, true, "Button-SignIn", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Button-SignIn", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("c7335760-315c-145c-83ab-3b8c7cb92954"), false, true, "Header-Form-Update-SchemaParameterStyle", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Form-Update-SchemaParameterStyle", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("c8da0a7f-b792-a458-8c7b-23189960e3bf"), false, true, "Button-Filter", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Button-Filter", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("ccc99c01-750f-6455-a305-dfcb2ee668f3"), false, true, "Value-Ascending", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Value-Ascending", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("cde8f645-e0c1-d55b-87c1-d3db8a0753f6"), false, true, "Label-Form-DateTime", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Form-DateTime", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("d248ba44-9166-a157-8c47-a623f949dc29"), false, true, "Label-Culture", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Culture", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("d2d7c565-d7e3-e35d-9b55-a3c2aa89a83a"), false, true, "Header-Form-Update-SchemaPropertyBoolean", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Form-Update-SchemaPropertyBoolean", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("d30281f2-a915-8d53-ba86-6aa227a11ffa"), false, true, "Help-Form-Description", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Help-Form-Description", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("d4206167-9361-fc5e-b4eb-7d4c2a293bea"), false, true, "Header-Form-Create-SchemaPropertyTable", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Form-Create-SchemaPropertyTable", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("d4cef582-c811-ed53-9444-6fe472d248fa"), false, true, "Label-Value", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Value", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("dafe5862-c4b6-5b58-83d7-4f1d5ed47aea"), false, true, "Header-Form-Update-SchemaPropertyImage", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Form-Update-SchemaPropertyImage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("db99ccc4-2684-c65a-8daf-2033fbf5e6fa"), false, true, "Label-SchemaIdentifier", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-SchemaIdentifier", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("dba60e04-bdd3-3b5c-91f6-5ee82179057d"), false, true, "Link-Navigation-Schema", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Link-Navigation-Schema", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("dbc1cc4c-cec8-9c52-aaaa-60dd5c3235a7"), false, true, "Label-Form-IsSystemDefined", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Form-IsSystemDefined", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("dc87d648-43b6-cf53-8c60-32bf36b2c069"), false, true, "Label-Weight", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Weight", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("de92902e-b535-e451-9915-60c7ead8aacc"), false, true, "Label-Form-Email", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Form-Email", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("e14f25f5-43ee-ac5f-9620-c1941fe1883a"), false, true, "Value-PropertyDateTimeType-Time", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Value-PropertyDateTimeType-Time", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("e1c538e0-3e75-2756-8836-225df7c0c2b1"), false, true, "Label-Title", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Title", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("e2d4764b-3be7-6752-96a2-77c2cc13b3e9"), false, true, "Header-Form-Update-SchemaParameterText", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Form-Update-SchemaParameterText", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("e3ac5cd6-9703-c453-aa45-34a1e7117cbd"), false, true, "Label-Form-Boolean", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Form-Boolean", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("e4d5b426-c59e-8852-9e24-a004d38f55db"), false, true, "Label-Form-ConfirmPassword", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Form-ConfirmPassword", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("e6cf537a-1a9b-2852-b3ee-5add43c8fbb6"), false, true, "Header-Property-Decimal", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Property-Decimal", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("e8d8dfaa-d3c0-ba5c-92a1-aa90e38b9186"), false, true, "Button-Reset", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Button-Reset", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("e9138287-d188-5d5d-a1c1-0996da4c7cec"), false, true, "Help-Form-Password", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Help-Form-Password", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("eac273c9-2f0a-6655-8b45-3a9e161dd8ac"), false, true, "Label-Form-PageSize", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Form-PageSize", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("eb4967c8-f769-e05f-8904-a67eed24b7f7"), false, true, "Header-Parameter-System", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Parameter-System", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("eb9993f5-cef6-505a-9777-d8b627e50bdf"), false, true, "Help-Form-Name", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Help-Form-Name", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("f193a26d-fe31-3e56-aa77-79984753e30c"), false, true, "Help-Form-ParameterSystemType", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Help-Form-ParameterSystemType", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("f4654062-2cd7-725d-9592-185b834934f6"), false, true, "Label-Form-PropertyType", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Form-PropertyType", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("f4a4e591-84af-b65e-ae66-bbadc5089150"), false, true, "Label-ParameterTextType", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-ParameterTextType", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("f67a9ee0-6d32-8e5a-b580-e90ba7d4b045"), false, true, "Header-Form-Create-SchemaPropertyString", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Header-Form-Create-SchemaPropertyString", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("f6ffe4c5-c66b-a651-b34c-5943e6edc35c"), false, true, "Label-Form-Search", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Form-Search", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("fadaf512-6878-7d5d-b9fb-f7aaa050431a"), false, true, "Value-IsSystemDefined-No", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Value-IsSystemDefined-No", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 },
                    { new Guid("fdf35cba-fc03-2152-99e6-22e523445c59"), false, true, "Label-Note", "", new Guid("12345678-1234-1234-1234-1234567890ab"), "Label-Note", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TableAuthentication_TimeCreated",
                table: "TableAuthentication",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableDataElement_SchemaIdentifier",
                table: "TableDataElement",
                column: "SchemaIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_TableDataElement_TimeCreated",
                table: "TableDataElement",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableDataParameterStyle_SchemaIdentifier",
                table: "TableDataParameterStyle",
                column: "SchemaIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_TableDataParameterStyle_TimeApproved",
                table: "TableDataParameterStyle",
                column: "TimeApproved");

            migrationBuilder.CreateIndex(
                name: "IX_TableDataParameterStyle_TimeCreated",
                table: "TableDataParameterStyle",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableDataParameterSystem_SchemaIdentifier",
                table: "TableDataParameterSystem",
                column: "SchemaIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_TableDataParameterSystem_TimeApproved",
                table: "TableDataParameterSystem",
                column: "TimeApproved");

            migrationBuilder.CreateIndex(
                name: "IX_TableDataParameterSystem_TimeCreated",
                table: "TableDataParameterSystem",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableDataParameterText_SchemaIdentifier",
                table: "TableDataParameterText",
                column: "SchemaIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_TableDataParameterText_TimeApproved",
                table: "TableDataParameterText",
                column: "TimeApproved");

            migrationBuilder.CreateIndex(
                name: "IX_TableDataParameterText_TimeCreated",
                table: "TableDataParameterText",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableDataPropertyBoolean_ElementID",
                table: "TableDataPropertyBoolean",
                column: "ElementID");

            migrationBuilder.CreateIndex(
                name: "IX_TableDataPropertyBoolean_SchemaIdentifier",
                table: "TableDataPropertyBoolean",
                column: "SchemaIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_TableDataPropertyBoolean_TimeCreated",
                table: "TableDataPropertyBoolean",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableDataPropertyDateTime_ElementID",
                table: "TableDataPropertyDateTime",
                column: "ElementID");

            migrationBuilder.CreateIndex(
                name: "IX_TableDataPropertyDateTime_SchemaIdentifier",
                table: "TableDataPropertyDateTime",
                column: "SchemaIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_TableDataPropertyDateTime_TimeCreated",
                table: "TableDataPropertyDateTime",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableDataPropertyString_ElementID",
                table: "TableDataPropertyString",
                column: "ElementID");

            migrationBuilder.CreateIndex(
                name: "IX_TableDataPropertyString_SchemaIdentifier",
                table: "TableDataPropertyString",
                column: "SchemaIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_TableDataPropertyString_TimeCreated",
                table: "TableDataPropertyString",
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
                name: "IX_TableSchema_Name",
                table: "TableSchema",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchema_TimeActivated",
                table: "TableSchema",
                column: "TimeActivated");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchema_TimeCreated",
                table: "TableSchema",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaContext_DescriptionParameterTextID",
                table: "TableSchemaContext",
                column: "DescriptionParameterTextID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaContext_Name",
                table: "TableSchemaContext",
                column: "Name");

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
                name: "IX_TableSchemaContext_TitleParameterTextID",
                table: "TableSchemaContext",
                column: "TitleParameterTextID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaContextHasElement_EntityID_RelationID",
                table: "TableSchemaContextHasElement",
                columns: new[] { "EntityID", "RelationID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaContextHasElement_RelationID",
                table: "TableSchemaContextHasElement",
                column: "RelationID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaContextHasElement_TimeCreated",
                table: "TableSchemaContextHasElement",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaElement_DescriptionParameterTextID",
                table: "TableSchemaElement",
                column: "DescriptionParameterTextID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaElement_Name",
                table: "TableSchemaElement",
                column: "Name");

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
                name: "IX_TableSchemaElement_TitleParameterTextID",
                table: "TableSchemaElement",
                column: "TitleParameterTextID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaElement_Weight",
                table: "TableSchemaElement",
                column: "Weight");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaElementHasProperty_EntityID_RelationID",
                table: "TableSchemaElementHasProperty",
                columns: new[] { "EntityID", "RelationID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaElementHasProperty_RelationID",
                table: "TableSchemaElementHasProperty",
                column: "RelationID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaElementHasProperty_TimeCreated",
                table: "TableSchemaElementHasProperty",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaParameterStyle_Name",
                table: "TableSchemaParameterStyle",
                column: "Name");

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
                name: "IX_TableSchemaParameterSystem_Name",
                table: "TableSchemaParameterSystem",
                column: "Name");

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
                name: "IX_TableSchemaParameterText_Name",
                table: "TableSchemaParameterText",
                column: "Name");

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
                name: "IX_TableSchemaPropertyBoolean_DescriptionParameterTextID",
                table: "TableSchemaPropertyBoolean",
                column: "DescriptionParameterTextID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyBoolean_Name",
                table: "TableSchemaPropertyBoolean",
                column: "Name");

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
                name: "IX_TableSchemaPropertyBoolean_TitleParameterTextID",
                table: "TableSchemaPropertyBoolean",
                column: "TitleParameterTextID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyBoolean_Weight",
                table: "TableSchemaPropertyBoolean",
                column: "Weight");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyCollection_DescriptionParameterTextID",
                table: "TableSchemaPropertyCollection",
                column: "DescriptionParameterTextID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyCollection_Name",
                table: "TableSchemaPropertyCollection",
                column: "Name");

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
                name: "IX_TableSchemaPropertyCollection_TitleParameterTextID",
                table: "TableSchemaPropertyCollection",
                column: "TitleParameterTextID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyCollection_Weight",
                table: "TableSchemaPropertyCollection",
                column: "Weight");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyCollectionHasProperty_EntityID_RelationID",
                table: "TableSchemaPropertyCollectionHasProperty",
                columns: new[] { "EntityID", "RelationID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyCollectionHasProperty_RelationID",
                table: "TableSchemaPropertyCollectionHasProperty",
                column: "RelationID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyCollectionHasProperty_TimeCreated",
                table: "TableSchemaPropertyCollectionHasProperty",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyDateTime_DescriptionParameterTextID",
                table: "TableSchemaPropertyDateTime",
                column: "DescriptionParameterTextID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyDateTime_Name",
                table: "TableSchemaPropertyDateTime",
                column: "Name");

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
                name: "IX_TableSchemaPropertyDateTime_TitleParameterTextID",
                table: "TableSchemaPropertyDateTime",
                column: "TitleParameterTextID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyDateTime_Weight",
                table: "TableSchemaPropertyDateTime",
                column: "Weight");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyDecimal_DescriptionParameterTextID",
                table: "TableSchemaPropertyDecimal",
                column: "DescriptionParameterTextID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyDecimal_Name",
                table: "TableSchemaPropertyDecimal",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyDecimal_SchemaID_SchemaIdentifier",
                table: "TableSchemaPropertyDecimal",
                columns: new[] { "SchemaID", "SchemaIdentifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyDecimal_TimeCreated",
                table: "TableSchemaPropertyDecimal",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyDecimal_TitleParameterTextID",
                table: "TableSchemaPropertyDecimal",
                column: "TitleParameterTextID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyDecimal_Weight",
                table: "TableSchemaPropertyDecimal",
                column: "Weight");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyImage_DescriptionParameterTextID",
                table: "TableSchemaPropertyImage",
                column: "DescriptionParameterTextID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyImage_Name",
                table: "TableSchemaPropertyImage",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyImage_SchemaID_SchemaIdentifier",
                table: "TableSchemaPropertyImage",
                columns: new[] { "SchemaID", "SchemaIdentifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyImage_TimeCreated",
                table: "TableSchemaPropertyImage",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyImage_TitleParameterTextID",
                table: "TableSchemaPropertyImage",
                column: "TitleParameterTextID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyImage_Weight",
                table: "TableSchemaPropertyImage",
                column: "Weight");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyInteger_DescriptionParameterTextID",
                table: "TableSchemaPropertyInteger",
                column: "DescriptionParameterTextID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyInteger_Name",
                table: "TableSchemaPropertyInteger",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyInteger_SchemaID_SchemaIdentifier",
                table: "TableSchemaPropertyInteger",
                columns: new[] { "SchemaID", "SchemaIdentifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyInteger_TimeCreated",
                table: "TableSchemaPropertyInteger",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyInteger_TitleParameterTextID",
                table: "TableSchemaPropertyInteger",
                column: "TitleParameterTextID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyInteger_Weight",
                table: "TableSchemaPropertyInteger",
                column: "Weight");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyString_DescriptionParameterTextID",
                table: "TableSchemaPropertyString",
                column: "DescriptionParameterTextID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyString_Name",
                table: "TableSchemaPropertyString",
                column: "Name");

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
                name: "IX_TableSchemaPropertyString_TitleParameterTextID",
                table: "TableSchemaPropertyString",
                column: "TitleParameterTextID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyString_Weight",
                table: "TableSchemaPropertyString",
                column: "Weight");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyTable_DescriptionParameterTextID",
                table: "TableSchemaPropertyTable",
                column: "DescriptionParameterTextID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyTable_Name",
                table: "TableSchemaPropertyTable",
                column: "Name");

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
                name: "IX_TableSchemaPropertyTable_TitleParameterTextID",
                table: "TableSchemaPropertyTable",
                column: "TitleParameterTextID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyTable_Weight",
                table: "TableSchemaPropertyTable",
                column: "Weight");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyTableHasProperty_EntityID_RelationID",
                table: "TableSchemaPropertyTableHasProperty",
                columns: new[] { "EntityID", "RelationID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyTableHasProperty_RelationID",
                table: "TableSchemaPropertyTableHasProperty",
                column: "RelationID");

            migrationBuilder.CreateIndex(
                name: "IX_TableSchemaPropertyTableHasProperty_TimeCreated",
                table: "TableSchemaPropertyTableHasProperty",
                column: "TimeCreated");

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
                name: "TableDataParameterStyle");

            migrationBuilder.DropTable(
                name: "TableDataParameterSystem");

            migrationBuilder.DropTable(
                name: "TableDataParameterText");

            migrationBuilder.DropTable(
                name: "TableDataPropertyBoolean");

            migrationBuilder.DropTable(
                name: "TableDataPropertyDateTime");

            migrationBuilder.DropTable(
                name: "TableDataPropertyString");

            migrationBuilder.DropTable(
                name: "TableIdentity");

            migrationBuilder.DropTable(
                name: "TableSchemaContextHasElement");

            migrationBuilder.DropTable(
                name: "TableSchemaElementHasProperty");

            migrationBuilder.DropTable(
                name: "TableSchemaParameterStyle");

            migrationBuilder.DropTable(
                name: "TableSchemaParameterSystem");

            migrationBuilder.DropTable(
                name: "TableSchemaPropertyBoolean");

            migrationBuilder.DropTable(
                name: "TableSchemaPropertyCollectionHasProperty");

            migrationBuilder.DropTable(
                name: "TableSchemaPropertyDateTime");

            migrationBuilder.DropTable(
                name: "TableSchemaPropertyDecimal");

            migrationBuilder.DropTable(
                name: "TableSchemaPropertyImage");

            migrationBuilder.DropTable(
                name: "TableSchemaPropertyInteger");

            migrationBuilder.DropTable(
                name: "TableSchemaPropertyString");

            migrationBuilder.DropTable(
                name: "TableSchemaPropertyTableHasProperty");

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

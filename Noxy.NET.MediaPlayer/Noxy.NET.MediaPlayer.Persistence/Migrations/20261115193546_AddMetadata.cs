using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Noxy.NET.MediaPlayer.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "TitleWasSent",
                table: "MediaUploadJob",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "AlbumID",
                table: "MediaFile",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Album",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    TitleKey = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    IdentityKey = table.Column<string>(type: "TEXT", maxLength: 4096, nullable: false),
                    TimeModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Album", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Album_User_UserID",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Artist",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    NameKey = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    TimeModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artist", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Artist_User_UserID",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Genre",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    NameKey = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    TimeModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genre", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Genre_User_UserID",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSetting",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    TimeModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSetting", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UserSetting_User_UserID",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlbumArtist",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    AlbumID = table.Column<Guid>(type: "TEXT", nullable: false),
                    ArtistID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumArtist", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AlbumArtist_Album_AlbumID",
                        column: x => x.AlbumID,
                        principalTable: "Album",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumArtist_Artist_ArtistID",
                        column: x => x.ArtistID,
                        principalTable: "Artist",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MediaFileArtist",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    MediaFileID = table.Column<Guid>(type: "TEXT", nullable: false),
                    ArtistID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaFileArtist", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MediaFileArtist_Artist_ArtistID",
                        column: x => x.ArtistID,
                        principalTable: "Artist",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MediaFileArtist_MediaFile_MediaFileID",
                        column: x => x.MediaFileID,
                        principalTable: "MediaFile",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MediaFileGenre",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    MediaFileID = table.Column<Guid>(type: "TEXT", nullable: false),
                    GenreID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaFileGenre", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MediaFileGenre_Genre_GenreID",
                        column: x => x.GenreID,
                        principalTable: "Genre",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MediaFileGenre_MediaFile_MediaFileID",
                        column: x => x.MediaFileID,
                        principalTable: "MediaFile",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSettingBoolean",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Value = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettingBoolean", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UserSettingBoolean_UserSetting_ID",
                        column: x => x.ID,
                        principalTable: "UserSetting",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSettingEnum",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettingEnum", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UserSettingEnum_UserSetting_ID",
                        column: x => x.ID,
                        principalTable: "UserSetting",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSettingInteger",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Value = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettingInteger", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UserSettingInteger_UserSetting_ID",
                        column: x => x.ID,
                        principalTable: "UserSetting",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MediaFile_AlbumID",
                table: "MediaFile",
                column: "AlbumID");

            migrationBuilder.CreateIndex(
                name: "IX_Album_TimeCreated",
                table: "Album",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_Album_UserID_IdentityKey",
                table: "Album",
                columns: new[] { "UserID", "IdentityKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlbumArtist_AlbumID_Position",
                table: "AlbumArtist",
                columns: new[] { "AlbumID", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_AlbumArtist_ArtistID",
                table: "AlbumArtist",
                column: "ArtistID");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumArtist_TimeCreated",
                table: "AlbumArtist",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_Artist_TimeCreated",
                table: "Artist",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_Artist_UserID_NameKey",
                table: "Artist",
                columns: new[] { "UserID", "NameKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Genre_TimeCreated",
                table: "Genre",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_Genre_UserID_NameKey",
                table: "Genre",
                columns: new[] { "UserID", "NameKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MediaFileArtist_ArtistID",
                table: "MediaFileArtist",
                column: "ArtistID");

            migrationBuilder.CreateIndex(
                name: "IX_MediaFileArtist_MediaFileID_Position",
                table: "MediaFileArtist",
                columns: new[] { "MediaFileID", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_MediaFileArtist_TimeCreated",
                table: "MediaFileArtist",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_MediaFileGenre_GenreID",
                table: "MediaFileGenre",
                column: "GenreID");

            migrationBuilder.CreateIndex(
                name: "IX_MediaFileGenre_MediaFileID_Position",
                table: "MediaFileGenre",
                columns: new[] { "MediaFileID", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_MediaFileGenre_TimeCreated",
                table: "MediaFileGenre",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_UserSetting_TimeCreated",
                table: "UserSetting",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_UserSetting_UserID_Key",
                table: "UserSetting",
                columns: new[] { "UserID", "Key" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MediaFile_Album_AlbumID",
                table: "MediaFile",
                column: "AlbumID",
                principalTable: "Album",
                principalColumn: "ID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MediaFile_Album_AlbumID",
                table: "MediaFile");

            migrationBuilder.DropTable(
                name: "AlbumArtist");

            migrationBuilder.DropTable(
                name: "MediaFileArtist");

            migrationBuilder.DropTable(
                name: "MediaFileGenre");

            migrationBuilder.DropTable(
                name: "UserSettingBoolean");

            migrationBuilder.DropTable(
                name: "UserSettingEnum");

            migrationBuilder.DropTable(
                name: "UserSettingInteger");

            migrationBuilder.DropTable(
                name: "Album");

            migrationBuilder.DropTable(
                name: "Artist");

            migrationBuilder.DropTable(
                name: "Genre");

            migrationBuilder.DropTable(
                name: "UserSetting");

            migrationBuilder.DropIndex(
                name: "IX_MediaFile_AlbumID",
                table: "MediaFile");

            migrationBuilder.DropColumn(
                name: "TitleWasSent",
                table: "MediaUploadJob");

            migrationBuilder.DropColumn(
                name: "AlbumID",
                table: "MediaFile");
        }
    }
}

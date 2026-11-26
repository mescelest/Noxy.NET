using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Noxy.NET.MediaPlayer.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    IsEmailVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.ID);
                });

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
                    TimeArtModified = table.Column<DateTime>(type: "TEXT", nullable: true),
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
                    Weight = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 100),
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
                name: "ExternalLogin",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Provider = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ProviderKey = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    UserID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalLogin", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ExternalLogin_User_UserID",
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
                    Weight = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 100),
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
                name: "MediaUploadJob",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserID = table.Column<Guid>(type: "TEXT", nullable: false),
                    StagedFileName = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    TitleWasSent = table.Column<bool>(type: "INTEGER", nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 127, nullable: false),
                    SizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    ContentHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaUploadJob", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MediaUploadJob_User_UserID",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Playlist",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    AllowDuplicates = table.Column<bool>(type: "INTEGER", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeLastPlayed = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ArtHue = table.Column<int>(type: "INTEGER", nullable: true),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playlist", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Playlist_User_UserID",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshToken",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TokenHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    TimeExpires = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeRevoked = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UserID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshToken", x => x.ID);
                    table.ForeignKey(
                        name: "FK_RefreshToken_User_UserID",
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
                name: "MediaFile",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 127, nullable: false),
                    SizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    ContentHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    TimeModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserID = table.Column<Guid>(type: "TEXT", nullable: false),
                    AlbumID = table.Column<Guid>(type: "TEXT", nullable: true),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaFile", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MediaFile_Album_AlbumID",
                        column: x => x.AlbumID,
                        principalTable: "Album",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MediaFile_User_UserID",
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
                name: "MediaFileMetadata",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Subtitle = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Comment = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true),
                    TrackNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    DiscNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    Year = table.Column<int>(type: "INTEGER", nullable: true),
                    Duration = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    MediaFileID = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaFileMetadata", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MediaFileMetadata_MediaFile_MediaFileID",
                        column: x => x.MediaFileID,
                        principalTable: "MediaFile",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlaylistEntry",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlaylistID = table.Column<Guid>(type: "TEXT", nullable: false),
                    MediaFileID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaylistEntry", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PlaylistEntry_MediaFile_MediaFileID",
                        column: x => x.MediaFileID,
                        principalTable: "MediaFile",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaylistEntry_Playlist_PlaylistID",
                        column: x => x.PlaylistID,
                        principalTable: "Playlist",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_ExternalLogin_Provider_ProviderKey",
                table: "ExternalLogin",
                columns: new[] { "Provider", "ProviderKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalLogin_TimeCreated",
                table: "ExternalLogin",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalLogin_UserID",
                table: "ExternalLogin",
                column: "UserID");

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
                name: "IX_MediaFile_AlbumID",
                table: "MediaFile",
                column: "AlbumID");

            migrationBuilder.CreateIndex(
                name: "IX_MediaFile_FilePath",
                table: "MediaFile",
                column: "FilePath",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MediaFile_TimeCreated",
                table: "MediaFile",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_MediaFile_UserID",
                table: "MediaFile",
                column: "UserID");

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
                name: "IX_MediaFileMetadata_MediaFileID",
                table: "MediaFileMetadata",
                column: "MediaFileID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MediaFileMetadata_TimeCreated",
                table: "MediaFileMetadata",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_MediaUploadJob_Status_TimeCreated",
                table: "MediaUploadJob",
                columns: new[] { "Status", "TimeCreated" });

            migrationBuilder.CreateIndex(
                name: "IX_MediaUploadJob_TimeCreated",
                table: "MediaUploadJob",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_MediaUploadJob_UserID",
                table: "MediaUploadJob",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Playlist_TimeCreated",
                table: "Playlist",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_Playlist_UserID",
                table: "Playlist",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistEntry_MediaFileID",
                table: "PlaylistEntry",
                column: "MediaFileID");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistEntry_PlaylistID_Position",
                table: "PlaylistEntry",
                columns: new[] { "PlaylistID", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistEntry_TimeCreated",
                table: "PlaylistEntry",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_TimeCreated",
                table: "RefreshToken",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_TokenHash",
                table: "RefreshToken",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_UserID",
                table: "RefreshToken",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_TimeCreated",
                table: "User",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlbumArtist");

            migrationBuilder.DropTable(
                name: "ExternalLogin");

            migrationBuilder.DropTable(
                name: "MediaFileArtist");

            migrationBuilder.DropTable(
                name: "MediaFileGenre");

            migrationBuilder.DropTable(
                name: "MediaFileMetadata");

            migrationBuilder.DropTable(
                name: "MediaUploadJob");

            migrationBuilder.DropTable(
                name: "PlaylistEntry");

            migrationBuilder.DropTable(
                name: "RefreshToken");

            migrationBuilder.DropTable(
                name: "UserSettingBoolean");

            migrationBuilder.DropTable(
                name: "UserSettingEnum");

            migrationBuilder.DropTable(
                name: "UserSettingInteger");

            migrationBuilder.DropTable(
                name: "Artist");

            migrationBuilder.DropTable(
                name: "Genre");

            migrationBuilder.DropTable(
                name: "MediaFile");

            migrationBuilder.DropTable(
                name: "Playlist");

            migrationBuilder.DropTable(
                name: "UserSetting");

            migrationBuilder.DropTable(
                name: "Album");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}

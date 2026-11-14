using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Noxy.NET.MediaPlayer.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaFile", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MediaFile_User_UserID",
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
                name: "MediaFileMetadata",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Artist = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Album = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    AlbumArtist = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Genre = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MediaFileMetadata");

            migrationBuilder.DropTable(
                name: "MediaUploadJob");

            migrationBuilder.DropTable(
                name: "MediaFile");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Noxy.NET.MediaPlayer.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaybackSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlaybackSession",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserID = table.Column<Guid>(type: "TEXT", nullable: false),
                    QueueMediaFileIDList = table.Column<string>(type: "TEXT", nullable: false),
                    CurrentIndex = table.Column<int>(type: "INTEGER", nullable: true),
                    PositionSeconds = table.Column<double>(type: "REAL", nullable: false),
                    PlaylistID = table.Column<Guid>(type: "TEXT", nullable: true),
                    TimeModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaybackSession", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PlaybackSession_User_UserID",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlaybackSession_TimeCreated",
                table: "PlaybackSession",
                column: "TimeCreated");

            migrationBuilder.CreateIndex(
                name: "IX_PlaybackSession_UserID",
                table: "PlaybackSession",
                column: "UserID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlaybackSession");
        }
    }
}

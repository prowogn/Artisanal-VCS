using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtisanalVCS.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingOwnerMemberships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO ProjectMembers (ProjectId, UserId, AddedAt)
                SELECT p.Id, p.OwnerId, datetime('now')
                FROM Projects p
                WHERE NOT EXISTS (
                    SELECT 1 FROM ProjectMembers pm
                    WHERE pm.ProjectId = p.Id AND pm.UserId = p.OwnerId
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

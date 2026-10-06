using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebHocTap_SaaS_.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmissionFileStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "Submissions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "FileData",
                table: "Submissions",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Submissions",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "FileData",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Submissions");
        }
    }
}

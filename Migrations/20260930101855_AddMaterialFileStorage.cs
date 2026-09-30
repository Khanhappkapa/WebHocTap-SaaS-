using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebHocTap_SaaS_.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialFileStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "Materials",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "FileData",
                table: "Materials",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Materials",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "FileData",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Materials");
        }
    }
}

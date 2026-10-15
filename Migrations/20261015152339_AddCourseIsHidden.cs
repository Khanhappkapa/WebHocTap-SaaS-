using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebHocTap_SaaS_.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseIsHidden : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "Courses",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHidden",
                table: "Courses");
        }
    }
}

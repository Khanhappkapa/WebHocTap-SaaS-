// ============================================================
// File: Models/Material.cs
// Mô tả: Model tài liệu học tập
// CHECKPOINT: FK tới Course, sử dụng enum FileType
// PHẦN 5: Thêm FileName, ContentType, FileData lưu file vào DB (bytea) thay vì wwwroot
// ============================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebHocTap_SaaS_.Models
{
    /// <summary>
    /// Tài liệu học tập thuộc một khóa học
    /// Có thể là file PDF, DOCX, Video, hoặc Link bên ngoài
    /// </summary>
    public class Material
    {
        // --------------------------------------------------
        // CHECKPOINT: Thuộc tính cơ bản
        // --------------------------------------------------

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// ID khóa học (Foreign Key)
        /// </summary>
        [Required]
        [ForeignKey("Course")]
        public int CourseId { get; set; }

        /// <summary>
        /// Tiêu đề tài liệu
        /// </summary>
        [Required(ErrorMessage = "Tiêu đề tài liệu là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề tối đa 200 ký tự")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Đường dẫn tệp tin (URL lưu trữ trên cloud/server)
        /// </summary>
        [StringLength(500, ErrorMessage = "URL tối đa 500 ký tự")]
        [Display(Name = "Đường dẫn tệp")]
        public string? FileUrl { get; set; }

        /// <summary>
        /// Loại tệp tin: Pdf, Docx, Video, Link
        /// CHECKPOINT: Sử dụng enum FileType để type-safe
        /// </summary>
        [Display(Name = "Loại tệp")]
        public FileType? FileType { get; set; }

        // --------------------------------------------------
        // PHẦN 5: Lưu file trực tiếp vào DB (bytea) — Render ephemeral disk safe
        // --------------------------------------------------

        /// <summary>
        /// Tên file gốc khi tải xuống (Guid + extension gốc)
        /// </summary>
        [StringLength(300)]
        [Display(Name = "Tên file")]
        public string? FileName { get; set; }

        /// <summary>
        /// MIME type (application/pdf, ...)
        /// </summary>
        [StringLength(100)]
        public string? ContentType { get; set; }

        /// <summary>
        /// Nội dung file (bytea trong PostgreSQL)
        /// Null khi FileType == Link
        /// </summary>
        public byte[]? FileData { get; set; }

        /// <summary>
        /// Thời điểm tải lên
        /// </summary>
        [Display(Name = "Ngày tải lên")]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // --------------------------------------------------
        // CHECKPOINT: Navigation Properties
        // --------------------------------------------------

        /// <summary>
        /// Khóa học chứa tài liệu này
        /// </summary>
        public virtual Course Course { get; set; } = null!;
    }
}

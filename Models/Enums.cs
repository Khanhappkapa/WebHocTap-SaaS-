// ============================================================
// File: Models/Enums.cs
// Mô tả: Chứa các enum dùng chung trong toàn bộ hệ thống
// CHECKPOINT: Định nghĩa trạng thái khóa học và loại tệp tin
// ============================================================

namespace WebHocTap_SaaS_.Models
{
    /// <summary>
    /// Trạng thái của khóa học (Course)
    /// - Draft:     Bản nháp, chưa công khai
    /// - Published: Đã công khai, học sinh có thể đăng ký
    /// - Closed:    Đã kết thúc, không nhận đăng ký mới
    /// </summary>
    public enum CourseStatus
    {
        Draft = 0,
        Published = 1,
        Closed = 2
    }

    /// <summary>
    /// Loại tệp tin tài liệu (Material)
    /// - Pdf:   Tệp PDF
    /// - Docx:  Tệp Word
    /// - Video: Tệp Video
    /// - Link:  Đường dẫn bên ngoài
    /// </summary>
    public enum FileType
    {
        Pdf = 0,
        Docx = 1,
        Video = 2,
        Link = 3
    }
}

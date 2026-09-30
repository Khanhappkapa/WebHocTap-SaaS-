// ============================================================
// File: Utils/FileValidation.cs
// Mô tả: Utility kiểm tra file upload — whitelist extension, giới hạn dung lượng
// PHẦN 5: Dùng chung cho Material (Phần 5) và Submission (Phần 6)
// ============================================================

namespace WebHocTap_SaaS_.Utils
{
    public static class FileValidation
    {
        /// <summary>
        /// Danh sách phần mở rộng cho phép (whitelist)
        /// </summary>
        public static readonly string[] AllowedExtensions = { ".pdf", ".docx", ".pptx", ".xlsx" };

        /// <summary>
        /// Giới hạn dung lượng: 10MB
        /// </summary>
        public const long MaxBytes = 10 * 1024 * 1024; // 10MB

        /// <summary>
        /// Validate file upload:
        /// - Không null, không rỗng
        /// - Dung lượng ≤ 10MB
        /// - Extension trong whitelist
        /// - FileName không chứa path traversal ('/' hoặc '\')
        /// </summary>
        public static (bool Ok, string Error) Validate(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return (false, "Vui lòng chọn file để tải lên.");

            if (file.Length > MaxBytes)
                return (false, "File vượt quá 10MB. Vui lòng chọn file nhỏ hơn.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                return (false, $"Định dạng '{ext}' không được hỗ trợ. Chỉ cho phép: {string.Join(", ", AllowedExtensions)}");

            // Chống path traversal
            if (file.FileName.Contains('/') || file.FileName.Contains('\\'))
                return (false, "Tên file không hợp lệ (chứa ký tự đường dẫn).");

            return (true, string.Empty);
        }
    }
}

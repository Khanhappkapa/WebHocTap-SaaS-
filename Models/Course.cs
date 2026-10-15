// ============================================================
// File: Models/Course.cs
// MÃ´ táº£: Model khÃ³a há»c (lá»›p há»c trá»±c tuyáº¿n)
// CHECKPOINT: Quan há»‡ 1-nhiá»u vá»›i Teacher(AppUser), Enrollments, Sessions, Materials, Assignments
// ============================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebHocTap_SaaS_.Models
{
    /// <summary>
    /// KhÃ³a há»c / Lá»›p há»c trá»±c tuyáº¿n
    /// Má»—i khÃ³a há»c thuá»™c vá» má»™t Teacher (AppUser)
    /// </summary>
    public class Course
    {
        // --------------------------------------------------
        // CHECKPOINT: Thuá»™c tÃ­nh cÆ¡ báº£n
        // --------------------------------------------------

        /// <summary>
        /// KhÃ³a chÃ­nh, tá»± tÄƒng
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// TÃªn khÃ³a há»c
        /// </summary>
        [Required(ErrorMessage = "TÃªn khÃ³a há»c lÃ  báº¯t buá»™c")]
        [StringLength(200, ErrorMessage = "TÃªn khÃ³a há»c tá»‘i Ä‘a 200 kÃ½ tá»±")]
        [Display(Name = "TÃªn khÃ³a há»c")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// MÃ´ táº£ khÃ³a há»c (khÃ´ng báº¯t buá»™c)
        /// </summary>
        [StringLength(2000, ErrorMessage = "MÃ´ táº£ tá»‘i Ä‘a 2000 kÃ½ tá»±")]
        [Display(Name = "MÃ´ táº£")]
        public string? Description { get; set; }

        /// <summary>
        /// ID cá»§a giÃ¡o viÃªn (Foreign Key tá»›i AppUser)
        /// Kiá»ƒu string vÃ¬ IdentityUser.Id lÃ  string (GUID)
        /// </summary>
        [Required(ErrorMessage = "GiÃ¡o viÃªn lÃ  báº¯t buá»™c")]
        [ForeignKey("Teacher")]
        public string TeacherId { get; set; } = string.Empty;

        /// <summary>
        /// Tráº¡ng thÃ¡i khÃ³a há»c: Draft / Published / Closed
        /// CHECKPOINT: Sá»­ dá»¥ng enum CourseStatus thay vÃ¬ string Ä‘á»ƒ type-safe
        /// </summary>
        [Required]
        [Display(Name = "Tráº¡ng thÃ¡i")]
        public CourseStatus Status { get; set; } = CourseStatus.Draft;

        /// <summary>
        /// NgÃ y táº¡o khÃ³a há»c
        /// </summary>
        [Display(Name = "NgÃ y táº¡o")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// NgÃ y cáº­p nháº­t gáº§n nháº¥t (null náº¿u chÆ°a cáº­p nháº­t)
        /// </summary>
        [Display(Name = "NgÃ y cáº­p nháº­t")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Ẩn khóa học khỏi danh sách công khai (soft-hide bởi Admin)
        /// </summary>
        [Display(Name = "Ẩn khóa học")]
        public bool IsHidden { get; set; } = false;

        // --------------------------------------------------
        // CHECKPOINT: Navigation Properties
        // --------------------------------------------------

        /// <summary>
        /// GiÃ¡o viÃªn phá»¥ trÃ¡ch khÃ³a há»c
        /// Quan há»‡: Nhiá»u Course â†’ 1 Teacher (AppUser)
        /// </summary>
        public virtual AppUser Teacher { get; set; } = null!;

        /// <summary>
        /// Danh sÃ¡ch há»c sinh Ä‘Äƒng kÃ½ khÃ³a há»c
        /// </summary>
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

        /// <summary>
        /// Danh sÃ¡ch buá»•i há»c trá»±c tuyáº¿n
        /// </summary>
        public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();

        /// <summary>
        /// Danh sÃ¡ch tÃ i liá»‡u há»c táº­p
        /// </summary>
        public virtual ICollection<Material> Materials { get; set; } = new List<Material>();

        /// <summary>
        /// Danh sÃ¡ch bÃ i táº­p
        /// </summary>
        public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}


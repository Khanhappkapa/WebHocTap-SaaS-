namespace WebHocTap_SaaS_.Models.ViewModels
{
    /// <summary>
    /// ViewModel tổng hợp cho Course Details (Phần 5)
    /// Chứa course + sessions + materials + trạng thái quyền truy cập
    /// </summary>
    public class CourseDetailsViewModel
    {
        public Course Course { get; set; } = null!;
        public List<Session> Sessions { get; set; } = new();
        public List<Material> Materials { get; set; } = new();
        public bool IsEnrolled { get; set; }
        public bool IsOwner { get; set; }
        public int StudentCount { get; set; }
    }
}

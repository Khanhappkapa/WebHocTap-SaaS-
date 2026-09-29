namespace WebHocTap_SaaS_.Models.ViewModels
{
    public class CourseListViewModel
    {
        public List<CourseCardViewModel> Items { get; set; } = new List<CourseCardViewModel>();
        public string Search { get; set; } = string.Empty;
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious { get; set; }
        public bool HasNext { get; set; }
    }
}

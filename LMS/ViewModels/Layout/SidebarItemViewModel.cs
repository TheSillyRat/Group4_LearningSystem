namespace LMS.ViewModels.Layout
{
    public class SidebarItemViewModel
    {
        public string Title { get; set; } = string.Empty;

        public string? Icon { get; set; }

        public string? Url { get; set; }

        public bool IsActive { get; set; }

        public bool IsExpanded { get; set; }

        public string? Badge { get; set; }

        public List<SidebarItemViewModel> Children { get; set; } = new();
    }
}
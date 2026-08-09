using LMS.ViewModels.Layout;

namespace LMS.ViewModels.Layout
{
    public class SidebarViewModel
    {
        public string DisplayName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? RoleName { get; set; }

        public string? AvatarUrl { get; set; }

        public List<SidebarItemViewModel> Items { get; set; } = new();
    }
}
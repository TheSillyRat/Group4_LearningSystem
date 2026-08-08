namespace LMS.ViewModels.Layout
{
    public class NavbarViewModel
    {
        public string DisplayName { get; set; } = string.Empty;

        public string? AvatarUrl { get; set; }

        public string? RoleName { get; set; }

        public int NotificationCount { get; set; }
    }
}
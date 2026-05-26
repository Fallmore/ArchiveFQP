using ArchiveFqp.Models.Database;

namespace ArchiveFqp.Models.Settings.Profile
{
    public class SettingsProfile : BaseSettings
    {
        public Профиль Profile { get; set; } = new();
    }
}

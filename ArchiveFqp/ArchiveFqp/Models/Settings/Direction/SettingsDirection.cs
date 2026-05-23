using ArchiveFqp.Models.Settings.Profile;

namespace ArchiveFqp.Models.Settings.Direction
{
    public class SettingsDirection : BaseSettings
    {
        public List<SettingsProfile> SettingsProfiles { get; set; } = new();
    }
}

using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.Settings.Profile;

namespace ArchiveFqp.Models.Settings.Direction
{
    public class SettingsDirection : BaseSettings
    {
        public Направление Direction { get; set; } = new();
        public List<SettingsProfile> SettingsProfiles { get; set; } = new();
    }
}

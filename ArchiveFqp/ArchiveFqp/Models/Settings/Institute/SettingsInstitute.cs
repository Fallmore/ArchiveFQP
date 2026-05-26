using ArchiveFqp.Models.Settings.Applications;

namespace ArchiveFqp.Models.Settings.Institute
{
    public class SettingsInstitute : BaseSettings
    {
        public SettingsApplications SettingsApplications { get; set; } = new();
    }
}

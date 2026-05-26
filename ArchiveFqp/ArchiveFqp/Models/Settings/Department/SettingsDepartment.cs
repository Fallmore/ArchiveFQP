using ArchiveFqp.Models.Settings.Applications;
using ArchiveFqp.Models.Settings.Department;
using ArchiveFqp.Models.Settings.Direction;

namespace ArchiveFqp.Models.Settings.Department
{
    public class SettingsDepartment : BaseSettings
    {
        public SettingsApplications SettingsApplications { get; set; } = new();

        public List<SettingsDirection> SettingsDirections { get; set; } = new();

    }
}

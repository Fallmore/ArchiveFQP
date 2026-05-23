using ArchiveFqp.Models.Settings.Direction;

namespace ArchiveFqp.Models.Settings.Department
{
    public class SettingsDepartment : BaseSettings
    {
        public List<SettingsDirection> SettingsDirections { get; set; } = new();
    }
}

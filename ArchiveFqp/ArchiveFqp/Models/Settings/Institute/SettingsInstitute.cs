using ArchiveFqp.Models.Settings.Department;

namespace ArchiveFqp.Models.Settings.Institute
{
    public class SettingsInstitute : BaseSettings
    {
        public List<SettingsDepartment> SettingsDepartments { get; set; } = new();
    }
}

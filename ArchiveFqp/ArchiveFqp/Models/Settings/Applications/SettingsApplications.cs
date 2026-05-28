using ArchiveFqp.Models.DTO.Structure;

namespace ArchiveFqp.Models.Settings.Applications
{
    public class SettingsApplications
    {
        public Dictionary<StructureDto, List<StructureDto>> AutoAgreeWorkApplications { get; set; } = [];
        public bool AutoAgreeWorkApplicationsEnabled { get; set; } = false;

        public Dictionary<StructureDto, List<StructureDto>> AutoDenyWorkApplications { get; set; } = [];
        public bool AutoDenyWorkApplicationsEnabled { get; set; } = false;

    }
}

using Newtonsoft.Json;

namespace ArchiveFqp.Models.Database;

public partial class Профиль
{
    public int IdПрофиля { get; set; }

    [JsonIgnore]
    public string Название { get; set; } = null!;

    public int IdНаправления { get; set; }

    [JsonIgnore]
    public virtual Направление IdНаправленияNavigation { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<АтрибутПрофиля> АтрибутПрофиляs { get; set; } = new List<АтрибутПрофиля>();

    [JsonIgnore]
    public virtual ICollection<ЗаявлениеАтрибута> ЗаявлениеАтрибутаs { get; set; } = new List<ЗаявлениеАтрибута>();

    [JsonIgnore]
    public virtual ICollection<Студент> Студентs { get; set; } = new List<Студент>();

}

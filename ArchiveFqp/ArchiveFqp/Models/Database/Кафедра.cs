using Newtonsoft.Json;

namespace ArchiveFqp.Models.Database;

public partial class Кафедра
{
    public int IdКафедры { get; set; }

    [JsonIgnore]
    public string Название { get; set; } = null!;

    public int IdИнститута { get; set; }

    [JsonIgnore]
    public virtual Институт IdИнститутаNavigation { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Студент> Студентs { get; set; } = new List<Студент>();

    [JsonIgnore]
    public virtual ICollection<АтрибутКафедры> АтрибутКафедрыs { get; set; } = new List<АтрибутКафедры>();

    [JsonIgnore]
    public virtual ICollection<ЗаявлениеАтрибута> ЗаявлениеАтрибутаs { get; set; } = new List<ЗаявлениеАтрибута>();

    [JsonIgnore]
    public virtual НастройкиКафедры? НастройкиКафедры { get; set; }

    [JsonIgnore]
    public virtual ICollection<Направление> Направлениеs { get; set; } = new List<Направление>();

    [JsonIgnore]
    public virtual ICollection<Преподаватель> Преподавательs { get; set; } = new List<Преподаватель>();

}

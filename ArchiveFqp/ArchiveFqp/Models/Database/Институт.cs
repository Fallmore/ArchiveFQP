using Newtonsoft.Json;

namespace ArchiveFqp.Models.Database;

public partial class Институт
{
    public int IdИнститута { get; set; }

    [JsonIgnore]
    public string Название { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<АтрибутИнститута> АтрибутИнститутаs { get; set; } = new List<АтрибутИнститута>();

    [JsonIgnore]
    public virtual ICollection<ЗаявлениеАтрибута> ЗаявлениеАтрибутаs { get; set; } = new List<ЗаявлениеАтрибута>();

    [JsonIgnore]
    public virtual ICollection<Кафедра> Кафедраs { get; set; } = new List<Кафедра>();

    [JsonIgnore]
    public virtual НастройкиИнститута? НастройкиИнститута { get; set; }

    [JsonIgnore]
    public virtual ICollection<Преподаватель> Преподавательs { get; set; } = new List<Преподаватель>();

    [JsonIgnore]
    public virtual ICollection<Студент> Студентs { get; set; } = new List<Студент>();

}

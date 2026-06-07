using Newtonsoft.Json;

namespace ArchiveFqp.Models.Database;

public partial class Направление
{
    public int IdНаправления { get; set; }

    [JsonIgnore]
    public string Название { get; set; } = null!;

    public int IdКафедры { get; set; }

    public int IdУгсн { get; set; }

    [JsonIgnore]
    public virtual Угсн? IdУгснNavigation { get; set; }

    [JsonIgnore]
    public virtual Кафедра IdКафедрыNavigation { get; set; } = null!;

    [JsonIgnore]
    public virtual НастройкиНаправления? НастройкиНаправления { get; set; }

    [JsonIgnore]
    public virtual ICollection<АтрибутНаправления> АтрибутНаправленияs { get; set; } = new List<АтрибутНаправления>();

    [JsonIgnore]
    public virtual ICollection<ЗаявлениеАтрибута> ЗаявлениеАтрибутаs { get; set; } = new List<ЗаявлениеАтрибута>();

    [JsonIgnore]
    public virtual ICollection<Профиль> Профильs { get; set; } = new List<Профиль>();

    [JsonIgnore]
    public virtual ICollection<Студент> Студентs { get; set; } = new List<Студент>();

}

using Newtonsoft.Json;

namespace ArchiveFqp.Models.Database;

/// <summary>
/// угсн
/// </summary>
public partial class Угсн
{
    public int IdУгсн { get; set; }

    [JsonIgnore]
    public string Название { get; set; } = null!;

    public int IdУгснСтандарта { get; set; }

    [JsonIgnore]
    public virtual УгснСтандарт IdУгснСтандартаNavigation { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Направление> Направлениеs { get; set; } = new List<Направление>();
}

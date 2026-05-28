using Newtonsoft.Json;

namespace ArchiveFqp.Models.Database;

/// <summary>
/// Стандарт угсн
/// </summary>
public partial class УгснСтандарт
{
    public int IdУгснСтандарта { get; set; }

    [JsonIgnore]
    public string Название { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Угсн> Угснs { get; set; } = new List<Угсн>();
}

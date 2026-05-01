namespace ArchiveFqp.Models.Database;

/// <summary>
/// угсн
/// </summary>
public partial class Угсн
{
    public int IdУгсн { get; set; }

    public string Название { get; set; } = null!;

    public int IdУгснСтандарта { get; set; }

    public virtual УгснСтандарт IdУгснСтандартаNavigation { get; set; } = null!;

    public virtual ICollection<Направление> Направлениеs { get; set; } = new List<Направление>();
}

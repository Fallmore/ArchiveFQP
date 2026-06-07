namespace ArchiveFqp.Models.Database;

public partial class НастройкиНаправления
{
    public int IdНастройки { get; set; }

    public int IdНаправления { get; set; }

    public string? Настройки { get; set; }

    public virtual Направление IdНаправленияNavigation { get; set; } = null!;
}

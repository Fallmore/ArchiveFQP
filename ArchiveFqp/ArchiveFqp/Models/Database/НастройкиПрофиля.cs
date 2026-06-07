namespace ArchiveFqp.Models.Database;

public partial class НастройкиПрофиля
{
    public int IdНастройки { get; set; }

    public int IdПрофиля { get; set; }

    public string? Настройки { get; set; }

    public virtual Профиль IdПрофиляNavigation { get; set; } = null!;
}

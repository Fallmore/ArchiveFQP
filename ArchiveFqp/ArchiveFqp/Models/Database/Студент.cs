using System.ComponentModel.DataAnnotations;

namespace ArchiveFqp.Models.Database;

public partial class Студент
{
    public int IdСтудента { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Выберите пользователя")]
    public int IdПользователя { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Выберите институт")]
    public int IdИнститута { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Выберите кафедру")]
    public int IdКафедры { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Выберите направление")]
    public int IdНаправления { get; set; }

    public int? IdПрофиля { get; set; }

    public bool Активно { get; set; } = false;

    [Range(1, int.MaxValue, ErrorMessage = "Выберите форму обучения")]
    public int IdФормыОбучения { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Выберите уровень образования")]
    public int IdУровняОбразования { get; set; }

    [Range(1900, int.MaxValue, ErrorMessage = "Выберите год окончания")]
    public int ГодОкончания { get; set; }

    public virtual Институт IdИнститутаNavigation { get; set; } = null!;

    public virtual Кафедра IdКафедрыNavigation { get; set; } = null!;

    public virtual Направление IdНаправленияNavigation { get; set; } = null!;

    public virtual Пользователь IdПользователяNavigation { get; set; } = null!;

    public virtual Профиль? IdПрофиляNavigation { get; set; }

    public virtual УровеньОбразования IdУровняОбразованияNavigation { get; set; } = null!;

    public virtual ФормаОбучения IdФормыОбученияNavigation { get; set; } = null!;

    public virtual ICollection<Работа> Работаs { get; set; } = new List<Работа>();
}

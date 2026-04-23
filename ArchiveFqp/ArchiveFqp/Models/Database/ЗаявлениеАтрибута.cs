using System.ComponentModel.DataAnnotations;

namespace ArchiveFqp.Models.Database;

public partial class ЗаявлениеАтрибута
{
    public int IdЗаявления { get; set; }

    public int IdПользователя { get; set; }

    public int? IdАтрибута { get; set; }

    [MinLength(3, ErrorMessage = "Минимальная длина названия 3 символа")]
    public string Название { get; set; } = null!;

    public bool Новый { get; set; } = false;

    [Required(ErrorMessage = "Введите описание")]
    [MinLength(3, ErrorMessage = "Минимальная длина названия 3 символа")]
    public string? Описание { get; set; }
    
    public string[]? Примеры { get; set; }

    public int? IdИнститута { get; set; }

    public int? IdКафедры { get; set; }

    public int? IdНаправления { get; set; }

    public int? IdПрофиля { get; set; }

    public int IdСтатуса { get; set; }

    public DateTime ДатаПоступления { get; set; }

    public string? Ответ { get; set; }

    public DateTime? ДатаОтвета { get; set; }

    public virtual Пользователь IdПользователяNavigation { get; set; } = null!;

    public virtual Атрибут IdАтрибутаNavigation { get; set; } = null!;

    // TODO: докончить ссылки, когда будет инет
    public virtual Институт? IdИнститутаNavigation { get; set; }

    public virtual Кафедра? IdКафедрыNavigation { get; set; }

    public virtual Направление? IdНаправленияNavigation { get; set; }

    public virtual Профиль? IdПрофиляNavigation { get; set; }

    public virtual СтатусЗаявления IdСтатусЗаявленияNavigation { get; set; } = null!;
}
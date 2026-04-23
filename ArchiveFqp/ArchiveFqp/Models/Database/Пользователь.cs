using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ArchiveFqp.Models.Database;

public partial class Пользователь
{
    public int IdПользователя { get; set; }

    [MinLength(1, ErrorMessage = "Введите фамилию")]
    public string Фамилия { get; set; } = null!;

    [MinLength(1, ErrorMessage = "Введите имя")]
    public string Имя { get; set; } = null!;

    public string? Отчество { get; set; }

    [MinLength(1, ErrorMessage = "Введите почту")]
    public string? Email { get; set; }

    public virtual ICollection<ЗаявлениеРаботы> ЗаявлениеРаботыs { get; set; } = new List<ЗаявлениеРаботы>();

    public virtual ICollection<ЗаявлениеАтрибута> ЗаявлениеАтрибутаs { get; set; } = new List<ЗаявлениеАтрибута>();

    public virtual ICollection<Преподаватель> Преподавательs { get; set; } = new List<Преподаватель>();

    public virtual ICollection<Студент> Студентs { get; set; } = new List<Студент>();
}

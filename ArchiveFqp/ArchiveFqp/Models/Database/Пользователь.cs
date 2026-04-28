using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ArchiveFqp.Models.Database;

public partial class Пользователь
{
    public int IdПользователя { get; set; }

    public string Фамилия { get; set; } = null!;

    public string Имя { get; set; } = null!;

    public string? Отчество { get; set; }

    public string? Email { get; set; }

    public virtual АккаунтПользователя? АккаунтПользователя { get; set; }

    public virtual ICollection<НастройкиПользователя> НастройкиПользователяs { get; set; } = new List<НастройкиПользователя>();

    public virtual ICollection<СеансПользователя> СеансПользователяs { get; set; } = new List<СеансПользователя>();

    public virtual ICollection<ЗаявлениеРаботы> ЗаявлениеРаботыs { get; set; } = new List<ЗаявлениеРаботы>();

    public virtual ICollection<ЗаявлениеАтрибута> ЗаявлениеАтрибутаs { get; set; } = new List<ЗаявлениеАтрибута>();

    public virtual ICollection<Преподаватель> Преподавательs { get; set; } = new List<Преподаватель>();

    public virtual ICollection<Студент> Студентs { get; set; } = new List<Студент>();
}

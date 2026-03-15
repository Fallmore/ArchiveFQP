using System;
using System.Collections.Generic;

namespace ArchiveFqp.Models.Database;

public partial class Пользователь
{
    public int IdПользователя { get; set; }

    public string Фамилия { get; set; } = null!;

    public string Имя { get; set; } = null!;

    public string? Отчество { get; set; }

    public string? Email { get; set; }

    public virtual ICollection<ВыдачаРаботы> ВыдачаРаботыs { get; set; } = new List<ВыдачаРаботы>();

    public virtual ICollection<Преподаватель> Преподавательs { get; set; } = new List<Преподаватель>();

    public virtual ICollection<Студент> Студентs { get; set; } = new List<Студент>();
}

using System;
using System.Collections.Generic;

namespace ArchiveFqp.Models.Database;

public partial class Профиль
{
    public int IdПрофиля { get; set; }

    public string Название { get; set; } = null!;

    public int IdНаправления { get; set; }

    public virtual Направление IdНаправленияNavigation { get; set; } = null!;

    public virtual ICollection<АтрибутПрофиля> АтрибутПрофиляs { get; set; } = new List<АтрибутПрофиля>();

    public virtual ICollection<ЗаявлениеАтрибута> ЗаявлениеАтрибутаs { get; set; } = new List<ЗаявлениеАтрибута>();

    public virtual ICollection<Студент> Студентs { get; set; } = new List<Студент>();

}

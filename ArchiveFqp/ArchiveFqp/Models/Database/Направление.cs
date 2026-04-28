using System;
using System.Collections.Generic;

namespace ArchiveFqp.Models.Database;

public partial class Направление
{
    public int IdНаправления { get; set; }

    public string Название { get; set; } = null!;

    public int IdКафедры { get; set; }

    public virtual Кафедра IdКафедрыNavigation { get; set; } = null!;

    public virtual ICollection<АтрибутНаправления> АтрибутНаправленияs { get; set; } = new List<АтрибутНаправления>();

    public virtual ICollection<ЗаявлениеАтрибута> ЗаявлениеАтрибутаs { get; set; } = new List<ЗаявлениеАтрибута>();

    public virtual ICollection<Профиль> Профильs { get; set; } = new List<Профиль>();

    public virtual ICollection<Студент> Студентs { get; set; } = new List<Студент>();

}

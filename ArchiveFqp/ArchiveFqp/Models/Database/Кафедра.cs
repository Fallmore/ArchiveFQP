using System;
using System.Collections.Generic;

namespace ArchiveFqp.Models.Database;

public partial class Кафедра
{
    public int IdКафедры { get; set; }

    public string Название { get; set; } = null!;

    public int IdИнститута { get; set; }

    public int? IdУгсн { get; set; }

    public virtual Институт IdИнститутаNavigation { get; set; } = null!;

    public virtual Угсн? IdУгснNavigation { get; set; }

    public virtual ICollection<Направление> Направлениеs { get; set; } = new List<Направление>();

    public virtual ICollection<Преподаватель> Преподавательs { get; set; } = new List<Преподаватель>();

    public virtual ICollection<ЗаявлениеАтрибута> ЗаявлениеАтрибутаs { get; set; } = new List<ЗаявлениеАтрибута>();

}

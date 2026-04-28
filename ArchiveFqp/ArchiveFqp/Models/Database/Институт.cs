using System;
using System.Collections.Generic;

namespace ArchiveFqp.Models.Database;

public partial class Институт
{
    public int IdИнститута { get; set; }

    public string Название { get; set; } = null!;

    public virtual ICollection<АтрибутИнститута> АтрибутИнститутаs { get; set; } = new List<АтрибутИнститута>();

    public virtual ICollection<ЗаявлениеАтрибута> ЗаявлениеАтрибутаs { get; set; } = new List<ЗаявлениеАтрибута>();

    public virtual ICollection<Кафедра> Кафедраs { get; set; } = new List<Кафедра>();

    public virtual ICollection<НастройкиИнститута> НастройкиИнститутаs { get; set; } = new List<НастройкиИнститута>();

    public virtual ICollection<Преподаватель> Преподавательs { get; set; } = new List<Преподаватель>();

    public virtual ICollection<Студент> Студентs { get; set; } = new List<Студент>();

}

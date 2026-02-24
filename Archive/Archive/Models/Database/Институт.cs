using System;
using System.Collections.Generic;

namespace Archive.Models.Database;

public partial class Институт
{
    public int IdИнститута { get; set; }

    public string Название { get; set; } = null!;

    public virtual ICollection<Кафедра> Кафедраs { get; set; } = new List<Кафедра>();

    public virtual ICollection<Преподаватель> Преподавательs { get; set; } = new List<Преподаватель>();

    public virtual ICollection<Студент> Студентs { get; set; } = new List<Студент>();
}

using System;
using System.Collections.Generic;

namespace ArchiveFqp.Models.Database;

public partial class Должность
{
    public int IdДолжности { get; set; }

    public string Название { get; set; } = null!;

    public virtual ICollection<Консультант> Консультантs { get; set; } = new List<Консультант>();

    public virtual ICollection<Рецензент> Рецензентs { get; set; } = new List<Рецензент>();

    public virtual ICollection<Преподаватель> Преподавательs { get; set; } = new List<Преподаватель>();

    public virtual ICollection<Работа> Работаs { get; set; } = new List<Работа>();
}

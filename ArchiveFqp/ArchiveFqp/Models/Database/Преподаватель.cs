using System;
using System.Collections.Generic;

namespace ArchiveFqp.Models.Database;

public partial class Преподаватель
{
    public int IdПреподавателя { get; set; }

    public int IdПользователя { get; set; }

    public int IdДолжности { get; set; }

    public int IdИнститута { get; set; }

    public int IdКафедры { get; set; }

    public virtual Должность IdДолжностиNavigation { get; set; } = null!;

    public virtual Институт IdИнститутаNavigation { get; set; } = null!;

    public virtual Кафедра IdКафедрыNavigation { get; set; } = null!;

    public virtual Пользователь IdПользователяNavigation { get; set; } = null!;

    public virtual ICollection<Консультант> Консультантs { get; set; } = new List<Консультант>();

    public virtual ICollection<Рецензент> Рецензентs { get; set; } = new List<Рецензент>();

    public virtual ICollection<ОценкаПреподавателя> ОценкаПреподавателяs { get; set; } = new List<ОценкаПреподавателя>();

    public virtual ICollection<Работа> Работаs { get; set; } = new List<Работа>();
}

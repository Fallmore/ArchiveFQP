using System;
using System.Collections.Generic;

namespace ArchiveFqp.Models.Database;

public partial class ОценкаПреподавателя
{
    public int IdОценки { get; set; }

    public int IdПреподавателя { get; set; }

    public int IdРаботы { get; set; }

    public int Оценка { get; set; }

    public string? Отзыв { get; set; }

    public virtual Преподаватель IdПреподавателяNavigation { get; set; } = null!;

    public virtual Работа IdРаботыNavigation { get; set; } = null!;
}

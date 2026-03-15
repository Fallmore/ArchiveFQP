using System;
using System.Collections.Generic;

namespace ArchiveFqp.Models.Database;

public partial class Консультант
{
    public int Id { get; set; }

    public int IdРаботы { get; set; }

    public int IdПреподавателя { get; set; }

    public int IdДолжности { get; set; }

    public virtual Должность IdДолжностиNavigation { get; set; } = null!;

    public virtual Преподаватель IdПреподавателяNavigation { get; set; } = null!;

    public virtual Работа IdРаботыNavigation { get; set; } = null!;
}

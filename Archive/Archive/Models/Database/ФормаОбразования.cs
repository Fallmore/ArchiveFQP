using System;
using System.Collections.Generic;

namespace Archive.Models.Database;

public partial class ФормаОбразования
{
    public int IdФормыОбразования { get; set; }

    public string Название { get; set; } = null!;

    public virtual ICollection<Студент> Студентs { get; set; } = new List<Студент>();
}

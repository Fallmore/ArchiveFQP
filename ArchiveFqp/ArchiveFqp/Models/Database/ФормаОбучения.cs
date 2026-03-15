using System;
using System.Collections.Generic;

namespace ArchiveFqp.Models.Database;

public partial class ФормаОбучения
{
    public int IdФормыОбучения { get; set; }

    public string Название { get; set; } = null!;

    public virtual ICollection<Студент> Студентs { get; set; } = new List<Студент>();
}

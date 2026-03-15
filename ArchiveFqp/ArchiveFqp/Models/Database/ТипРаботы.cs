using System;
using System.Collections.Generic;

namespace ArchiveFqp.Models.Database;

public partial class ТипРаботы
{
    public int IdТипаРаботы { get; set; }

    public string Название { get; set; } = null!;

    public virtual ICollection<АтрибутУчреждения> АтрибутУчрежденияs { get; set; } = new List<АтрибутУчреждения>();

    public virtual ICollection<Работа> Работаs { get; set; } = new List<Работа>();
}

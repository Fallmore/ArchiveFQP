using System;
using System.Collections.Generic;

namespace Archive.Models.Database;

public partial class Атрибут
{
    public int IdАтрибута { get; set; }

    public string Название { get; set; } = null!;

    public virtual ICollection<АтрибутУчреждения> АтрибутУчрежденияs { get; set; } = new List<АтрибутУчреждения>();
}

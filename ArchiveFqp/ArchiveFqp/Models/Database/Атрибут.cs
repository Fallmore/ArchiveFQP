using System;
using System.Collections.Generic;

namespace ArchiveFqp.Models.Database;

public partial class Атрибут
{
    public int IdАтрибута { get; set; }

    public string Название { get; set; } = null!;

    public virtual ICollection<АтрибутУчреждения> АтрибутУчрежденияs { get; set; } = new List<АтрибутУчреждения>();
    
    public virtual ICollection<ЗаявлениеАтрибута> ЗаявлениеАтрибутаs { get; set; } = new List<ЗаявлениеАтрибута>();
}

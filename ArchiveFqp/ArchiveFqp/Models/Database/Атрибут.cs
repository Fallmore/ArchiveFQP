using System;
using System.Collections.Generic;

namespace ArchiveFqp.Models.Database;

public partial class Атрибут
{
    public int IdАтрибута { get; set; }

    public string Название { get; set; } = null!;

    public virtual ICollection<АтрибутИнститута> АтрибутИнститутаs { get; set; } = new List<АтрибутИнститута>();

    public virtual ICollection<АтрибутКафедры> АтрибутКафедрыs { get; set; } = new List<АтрибутКафедры>();

    public virtual ICollection<АтрибутНаправления> АтрибутНаправленияs { get; set; } = new List<АтрибутНаправления>();

    public virtual ICollection<АтрибутПрофиля> АтрибутПрофиляs { get; set; } = new List<АтрибутПрофиля>();

    public virtual ICollection<АтрибутУчреждения> АтрибутУчрежденияs { get; set; } = new List<АтрибутУчреждения>();

    public virtual ICollection<ЗаявлениеАтрибута> ЗаявлениеАтрибутаs { get; set; } = new List<ЗаявлениеАтрибута>();
}

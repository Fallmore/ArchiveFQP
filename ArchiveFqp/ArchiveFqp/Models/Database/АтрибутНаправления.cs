using System;
using System.Collections.Generic;

namespace ArchiveFqp.Models.Database;

public partial class АтрибутНаправления
{
    public int IdСтруктуры { get; set; }

    public int IdАтрибута { get; set; }

    public int? IdСтатусаРаботы { get; set; }

    public int? IdТипаРаботы { get; set; }

    public int IdНаправления { get; set; }

    public virtual Атрибут IdАтрибутаNavigation { get; set; } = null!;

    public virtual Направление IdНаправленияNavigation { get; set; } = null!;

    public virtual СтатусРаботы? IdСтатусаРаботыNavigation { get; set; }

    public virtual ТипРаботы? IdТипаРаботыNavigation { get; set; }
}

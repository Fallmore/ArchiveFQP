using System;
using System.Collections.Generic;

namespace ArchiveFqp.Models.Database;

public partial class АтрибутИнститута
{
    public int IdСтруктуры { get; set; }

    public int IdАтрибута { get; set; }

    public int? IdСтатусаРаботы { get; set; }

    public int? IdТипаРаботы { get; set; }

    public int IdИнститута { get; set; }

    public virtual Атрибут IdАтрибутаNavigation { get; set; } = null!;

    public virtual Институт IdИнститутаNavigation { get; set; } = null!;

    public virtual СтатусРаботы? IdСтатусаРаботыNavigation { get; set; }

    public virtual ТипРаботы? IdТипаРаботыNavigation { get; set; }
}

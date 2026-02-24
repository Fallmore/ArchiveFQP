using System;
using System.Collections.Generic;

namespace Archive.Models.Database;

public partial class АтрибутИнститута
{
    public int IdСтруктуры { get; set; }

    public int IdАтрибута { get; set; }

    public int? IdСтатусаРаботы { get; set; }

    public int? IdТипаРаботы { get; set; }

    public int IdИнститута { get; set; }

    public virtual Институт IdИнститутаNavigation { get; set; } = null!;
}

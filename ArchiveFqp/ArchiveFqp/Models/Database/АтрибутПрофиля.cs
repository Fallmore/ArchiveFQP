using System;
using System.Collections.Generic;

namespace ArchiveFqp.Models.Database;

public partial class АтрибутПрофиля
{
    public int IdСтруктуры { get; set; }

    public int IdАтрибута { get; set; }

    public int? IdСтатусаРаботы { get; set; }

    public int? IdТипаРаботы { get; set; }

    public int IdПрофиля { get; set; }

    public virtual Профиль IdПрофиляNavigation { get; set; } = null!;
}

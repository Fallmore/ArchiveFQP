using System;
using System.Collections.Generic;

namespace ArchiveFqp.Models.Database;

public partial class НастройкиИнститута
{
    public int IdНастройки { get; set; }

    public int IdИнститута { get; set; }

    public string? Настройки { get; set; }

    public virtual Институт IdИнститутаNavigation { get; set; } = null!;
}

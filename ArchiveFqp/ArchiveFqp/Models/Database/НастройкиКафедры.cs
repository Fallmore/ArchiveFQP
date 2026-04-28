using System;
using System.Collections.Generic;

namespace ArchiveFqp.Models.Database;

public partial class НастройкиКафедры
{
    public int IdНастройки { get; set; }

    public int IdКафедры { get; set; }

    public string? Настройки { get; set; }

    public virtual Кафедра IdКафедрыNavigation { get; set; } = null!;
}

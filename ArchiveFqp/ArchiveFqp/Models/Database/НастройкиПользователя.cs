using System;
using System.Collections.Generic;

namespace ArchiveFqp.Models.Database;

public partial class НастройкиПользователя
{
    public int IdНастройки { get; set; }

    public int IdПользователя { get; set; }

    public string? Настройки { get; set; }

    public virtual Пользователь IdПользователяNavigation { get; set; } = null!;
}

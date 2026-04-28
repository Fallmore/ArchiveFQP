using System;
using System.Collections.Generic;

namespace ArchiveFqp.Models.Database;

public partial class АккаунтПользователя
{
    public int IdАккаунта { get; set; }

    public int IdПользователя { get; set; }

    public string Логин { get; set; } = null!;

    public string Пароль { get; set; } = null!;

    public List<int> Роли { get; set; } = null!;

    public virtual Пользователь IdПользователяNavigation { get; set; } = null!;
}

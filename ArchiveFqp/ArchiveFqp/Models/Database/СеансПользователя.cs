using System;
using System.Collections.Generic;

namespace ArchiveFqp.Models.Database;

public partial class СеансПользователя
{
    public int IdСеанса { get; set; }

    public int IdПользователя { get; set; }

    public string Ip { get; set; } = null!;

    public string Браузер { get; set; } = null!;

    public string Ос { get; set; } = null!;

    public DateTime Вход { get; set; }

    public string ЧасовойПояс { get; set; } = null!;

    public virtual Пользователь IdПользователяNavigation { get; set; } = null!;
}

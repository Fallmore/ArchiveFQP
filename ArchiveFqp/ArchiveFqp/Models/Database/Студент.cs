using System;
using System.Collections.Generic;

namespace ArchiveFqp.Models.Database;

public partial class Студент
{
    public int IdСтудента { get; set; }

    public int IdПользователя { get; set; }

    public int IdИнститута { get; set; }

    public int IdНаправления { get; set; }

    public int? IdПрофиля { get; set; }

    public int IdФормыОбучения { get; set; }

    public int IdУровняОбразования { get; set; }

    public int ГодОкончания { get; set; }

    public virtual Институт IdИнститутаNavigation { get; set; } = null!;

    public virtual Направление IdНаправленияNavigation { get; set; } = null!;

    public virtual Пользователь IdПользователяNavigation { get; set; } = null!;

    public virtual Профиль? IdПрофиляNavigation { get; set; }

    public virtual УровеньОбразования IdУровняОбразованияNavigation { get; set; } = null!;

    public virtual ФормаОбучения IdФормыОбученияNavigation { get; set; } = null!;

    public virtual ICollection<Работа> Работаs { get; set; } = new List<Работа>();
}

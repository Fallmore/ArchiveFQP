using System;
using System.Collections.Generic;

namespace ArchiveFqp.Models.Database;

public partial class ВыдачаРаботы
{
    public int IdВыдачи { get; set; }

    public int IdРаботы { get; set; }

    public int IdПользователя { get; set; }

    public string Цель { get; set; } = null!;

    public DateTime ДатаПоступления { get; set; }

    public DateTime ДатаВозврПоЗаявл { get; set; }

    public string? Ответ { get; set; }

    public DateTime? ДатаОтвета { get; set; }

    public DateTime? ДатаВозврПоФакту { get; set; }

    public virtual Пользователь IdПользователяNavigation { get; set; } = null!;

    public virtual Работа IdРаботыNavigation { get; set; } = null!;
}

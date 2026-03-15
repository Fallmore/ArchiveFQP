using System;
using System.Collections.Generic;

namespace ArchiveFqp.Models.Database;

public partial class ДанныеПоАтриб
{
    public int IdСтруктуры { get; set; }

    public int IdРаботы { get; set; }

    public string Данные { get; set; } = null!;

    public virtual Работа IdРаботыNavigation { get; set; } = null!;
}

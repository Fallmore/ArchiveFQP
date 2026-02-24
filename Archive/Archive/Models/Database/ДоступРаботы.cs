using System;
using System.Collections.Generic;

namespace Archive.Models.Database;

public partial class ДоступРаботы
{
    public int IdДоступаРаботы { get; set; }

    public string Название { get; set; } = null!;

    public virtual ICollection<Работа> Работаs { get; set; } = new List<Работа>();
}

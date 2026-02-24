using System;
using System.Collections.Generic;

namespace Archive.Models.Database;

public partial class РольПользователя
{
    public int IdРоли { get; set; }

    public string Название { get; set; } = null!;
}

using System;
using System.Collections.Generic;

namespace Archive.Models.Database;

public partial class ЖурналДействий
{
    public DateTime? Время { get; set; }

    public string Схема { get; set; } = null!;

    public string Таблица { get; set; } = null!;

    public string Операция { get; set; } = null!;

    public string? СтарыеДанные { get; set; }

    public string? НовыеДанные { get; set; }

    public string? Запрос { get; set; }
}

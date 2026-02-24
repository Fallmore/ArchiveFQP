using System;
using System.Collections.Generic;

namespace Archive.Models.Database;

/// <summary>
/// Стандарт угсн
/// </summary>
public partial class УгснСтандарт
{
    public int IdУгснСтандарта { get; set; }

    public string Название { get; set; } = null!;

    public virtual ICollection<Угсн> Угснs { get; set; } = new List<Угсн>();
}

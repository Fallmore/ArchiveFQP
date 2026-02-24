using System;
using System.Collections.Generic;

namespace Archive.Models.Database;

/// <summary>
/// Укрупнённые группы специальностей и направлений подготовки
/// </summary>
public partial class Угсн
{
    public int IdУгсн { get; set; }

    public string Название { get; set; } = null!;

    public int IdУгснСтандарта { get; set; }

    public virtual УгснСтандарт IdУгснСтандартаNavigation { get; set; } = null!;

    public virtual ICollection<Кафедра> Кафедраs { get; set; } = new List<Кафедра>();
}

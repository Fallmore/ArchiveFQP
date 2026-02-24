using System;
using System.Collections.Generic;

namespace Archive.Models.Database;

public partial class Направление
{
    public int IdНаправления { get; set; }

    public string Название { get; set; } = null!;

    public int IdКафедры { get; set; }

    public virtual Кафедра IdКафедрыNavigation { get; set; } = null!;

    public virtual ICollection<Профиль> Профильs { get; set; } = new List<Профиль>();

    public virtual ICollection<Студент> Студентs { get; set; } = new List<Студент>();
}

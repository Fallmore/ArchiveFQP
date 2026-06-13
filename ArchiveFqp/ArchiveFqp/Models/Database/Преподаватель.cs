using System.ComponentModel.DataAnnotations;

namespace ArchiveFqp.Models.Database;

public partial class Преподаватель
{
    public int IdПреподавателя { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Выберите пользователя")]
    public int IdПользователя { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Выберите должность")]
    public int IdДолжности { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Выберите институт")]
    public int IdИнститута { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Выберите кафедру")]
    public int IdКафедры { get; set; }
    
    public List<int> Роли { get; set; } = null!;

    public bool Активно { get; set; } = false;

    public virtual Должность IdДолжностиNavigation { get; set; } = null!;

    public virtual Институт IdИнститутаNavigation { get; set; } = null!;

    public virtual Кафедра IdКафедрыNavigation { get; set; } = null!;

    public virtual Пользователь IdПользователяNavigation { get; set; } = null!;

    public virtual ICollection<Консультант> Консультантs { get; set; } = new List<Консультант>();

    public virtual ICollection<Рецензент> Рецензентs { get; set; } = new List<Рецензент>();

    public virtual ICollection<ОценкаПреподавателя> ОценкаПреподавателяs { get; set; } = new List<ОценкаПреподавателя>();

    public virtual ICollection<Работа> Работаs { get; set; } = new List<Работа>();
}

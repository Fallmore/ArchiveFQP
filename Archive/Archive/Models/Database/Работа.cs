using System;
using System.Collections.Generic;

namespace Archive.Models.Database;

public partial class Работа
{
	public int IdРаботы { get; set; }

	public int IdСтудента { get; set; }

	public int IdПреподавателя { get; set; }

	public int IdДолжности { get; set; }

	public string Тема { get; set; } = null!;

	public int? КоличСтраниц { get; set; }

	public string? Аннотация { get; set; }

	public string? Эцп { get; set; }

	public int IdТипаРаботы { get; set; }

	public int IdСтатусаРаботы { get; set; }

	public DateTime ДатаДобавления { get; set; }

	public DateTime? ДатаИзменения { get; set; }

	public string? Местоположение { get; set; }

	public int IdДоступаРаботы { get; set; }

	public virtual Должность IdДолжностиNavigation { get; set; } = null!;

	public virtual ДоступРаботы IdДоступаРаботыNavigation { get; set; } = null!;

	public virtual Преподаватель IdПреподавателяNavigation { get; set; } = null!;

	public virtual СтатусРаботы IdСтатусаРаботыNavigation { get; set; } = null!;

	public virtual Студент IdСтудентаNavigation { get; set; } = null!;

	public virtual ТипРаботы IdТипаРаботыNavigation { get; set; } = null!;

	public virtual ICollection<ВыдачаРаботы> ВыдачаРаботыs { get; set; } = new List<ВыдачаРаботы>();

	public virtual ICollection<Консультант> Консультантs { get; set; } = new List<Консультант>();

	public virtual ICollection<Рецензент> Рецензентs { get; set; } = new List<Рецензент>();

	public virtual ICollection<ОценкаПреподавателя> ОценкаПреподавателяs { get; set; } = new List<ОценкаПреподавателя>();
}

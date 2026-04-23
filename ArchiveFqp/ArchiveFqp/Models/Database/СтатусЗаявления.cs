namespace ArchiveFqp.Models.Database;

public partial class СтатусЗаявления
{
    public int IdСтатуса { get; set; }

    public string Название { get; set; } = null!;

    public virtual ICollection<ЗаявлениеРаботы> ЗаявлениеРаботыs { get; set; } = new List<ЗаявлениеРаботы>();
    
    public virtual ICollection<ЗаявлениеАтрибута> ЗаявлениеАтрибутаs { get; set; } = new List<ЗаявлениеАтрибута>();
}

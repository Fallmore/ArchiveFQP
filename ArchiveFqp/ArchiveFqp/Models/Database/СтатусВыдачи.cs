namespace ArchiveFqp.Models.Database;

public partial class СтатусВыдачи
{
    public int IdСтатусаВыдачи { get; set; }

    public string Название { get; set; } = null!;

    public virtual ICollection<ВыдачаРаботы> ВыдачаРаботыs { get; set; } = new List<ВыдачаРаботы>();
}

using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Attribute;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ArchiveFqp.Models.DTO.Work
{
    public class WorkCreateDto
    {
        public int IdРаботы { get; set; } = 0;

        [Required(ErrorMessage = "Тема обязательна")]
        public string Тема { get; set; } = "";

        [Required(ErrorMessage = "Выберите студента")]
        public int? IdСтудента { get; set; }

        [Required(ErrorMessage = "Выберите руководителя")]
        public int? IdПреподавателя { get; set; }

        public List<int> IdКонсультантов { get; set; } = [-1];
        public List<int> IdРецензентов { get; set; } = [-1];

        [Required(ErrorMessage = "Выберите УГСН")]
        public int? IdУгсн { get; set; }

        [Required(ErrorMessage = "Выберите стандарт")]
        public int? IdУгснСтандарта { get; set; }

        [Required(ErrorMessage = "Выберите направление")]
        public int? IdНаправления { get; set; }

        public int? IdПрофиля { get; set; }

        [Required(ErrorMessage = "Выберите уровень образования")]
        public int? IdУровняОбразования { get; set; }

        [Required(ErrorMessage = "Выберите форму обучения")]
        public int? IdФормыОбучения { get; set; }

        [Required(ErrorMessage = "Выберите институт")]
        public int? IdИнститута { get; set; }

        [Required(ErrorMessage = "Выберите кафедру")]
        public int? IdКафедры { get; set; }

        public int? ГодВыпуска { get; set; }

        public string? Аннотация { get; set; }

        [Required(ErrorMessage = "Укажите количество страниц")]
        [Range(1, 300, ErrorMessage = "Количество страниц должно быть от 1 до 300")]
        public int? КоличСтраниц { get; set; }

        [Required(ErrorMessage = "Выберите тип работы")]
        public int? IdТипаРаботы { get; set; }

        [Required(ErrorMessage = "Выберите доступ работы")]
        public int? IdДоступаРаботы { get; set; }

        public int IdСтатусаРаботы { get; set; } = -1;

        public bool AttributesValuesAdded { get; set; } = false;

        private List<AttributeDto>? attributes;
        public List<AttributeDto>? Attributes { get => attributes; set => SetAttibutes(value); }

        private void SetAttibutes(List<AttributeDto>? value)
        {
            if (attributes == default || value == default)
            {
                attributes = value;
                return;
            }
            
            attributes.RemoveAll(f => !value.Contains(f));
            attributes = attributes.Union(value).ToList();
        }

        
    }
}

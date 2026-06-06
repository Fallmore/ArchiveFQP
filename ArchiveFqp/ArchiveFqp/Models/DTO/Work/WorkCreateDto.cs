using ArchiveFqp.Models.DTO.Attribute;
using ArchiveFqp.Models.FileUpload;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ArchiveFqp.Models.DTO.Work
{
    public class WorkCreateDto
    {
        public int IdРаботы { get; set; } = 0;

        [Required(ErrorMessage = "Тема обязательна")]
        public string Тема { get; set; } = "";

        public int? IdСтудента { get; set; }

        #region Новый пользователь
        public bool IsNewUser { get; set; } = false;
        public string Surname { get; set; } = "";
        public string Name { get; set; } = "";
        public string Patronymic { get; set; } = "";
        #endregion

        [Required(ErrorMessage = "Выберите руководителя")]
        public int? IdПреподавателя { get; set; }
        public int? IdДолжности { get; set; }

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

        [Required(ErrorMessage = "Выберите год выпуска")]
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

        public DateTime ДатаДобавления { get; set; }
        public DateTime? ДатаИзменения { get; set; }
        public string? Эцп { get; set; }
        public string? Местоположение { get; set; }


        public TempFileInfo? TempFileWithTitle { get; set; }
        public bool WithoutFileWithTitle { get; set; } = false;


        public bool AttributesValuesAdded { get; set; } = false;
        private List<AttributeDto>? attributes;
        public List<AttributeDto>? Attributes { get => attributes; set => SetAttibutes(value); }
        public TypeAddAttributes TypeAddAttributes { get; set; } = TypeAddAttributes.Handle;
        public bool AttributesSearched { get; set; } = false;

        public WorkCreateDto()
        {

        }

        public WorkCreateDto(WorkCreateDto work)
        {
            Copy(work);
        }

        public WorkCreateDto(WorkDisplayDto work)
        {
            Copy(work);
        }


        /// <summary>
        /// Обновляет список атрибутов, добавляя новые, оставляя старые
        /// и удаляя те, которых нет в новом списке.
        /// </summary>
        /// <param name="value"></param>
        private void SetAttibutes(List<AttributeDto>? value)
        {
            if (attributes == default || value == default)
            {
                attributes = value;
                return;
            }

            attributes.RemoveAll(f => !value.Contains(f));
            attributes = attributes.Union(value).ToList();
            foreach (AttributeDto item in value)
            {
                if (!string.IsNullOrWhiteSpace(item.Данные))
                {
                    attributes.First(x => x.Название == item.Название).Данные = item.Данные;
                }
            }
        }


        public void Copy(WorkCreateDto work)
        {
            IdРаботы = work.IdРаботы;
            Тема = work.Тема;
            IdСтудента = work.IdСтудента;
            IsNewUser = work.IsNewUser;
            Surname = work.Surname;
            Name = work.Name;
            Patronymic = work.Patronymic;
            IdПреподавателя = work.IdПреподавателя;
            IdДолжности = work.IdДолжности;
            IdКонсультантов = work.IdКонсультантов;
            IdРецензентов = work.IdРецензентов;
            IdУгсн = work.IdУгсн;
            IdУгснСтандарта = work.IdУгснСтандарта;
            IdНаправления = work.IdНаправления;
            IdПрофиля = work.IdПрофиля;
            IdУровняОбразования = work.IdУровняОбразования;
            IdФормыОбучения = work.IdФормыОбучения;
            IdИнститута = work.IdИнститута;
            IdКафедры = work.IdКафедры;
            ГодВыпуска = work.ГодВыпуска;
            Аннотация = work.Аннотация;
            КоличСтраниц = work.КоличСтраниц;
            IdТипаРаботы = work.IdТипаРаботы;
            IdДоступаРаботы = work.IdДоступаРаботы;
            IdСтатусаРаботы = work.IdСтатусаРаботы;
            ДатаДобавления = work.ДатаДобавления;
            ДатаИзменения = work.ДатаИзменения;
            Местоположение = work.Местоположение;
            Эцп = work.Эцп;
            AttributesValuesAdded = work.AttributesValuesAdded;
            this.attributes = work.attributes;
        }

        public void Copy(WorkDisplayDto work)
        {
            IdРаботы = work.IdРаботы;
            Тема = work.Тема;
            IdСтудента = work.Студент.IdСтудента;
            IdПреподавателя = work.Руководитель.IdПреподавателя;
            IdДолжности = work.Руководитель.Должность.IdДолжности;
            IdКонсультантов = work.Консультанты?.Select(c => c.IdПреподавателя).ToList() ?? [];
            IdРецензентов = work.Рецензенты?.Select(r => r.IdПреподавателя).ToList() ?? [];
            IdУгсн = work.Студент.Структура.Угсн.IdУгсн;
            IdУгснСтандарта = work.Студент.Структура.УгснСтандарт.IdУгснСтандарта;
            IdНаправления = work.Студент.Структура.Направление.IdНаправления;
            IdПрофиля = work.Студент.Структура.Профиль?.IdПрофиля;
            IdУровняОбразования = work.Студент.УровеньОбразования.IdУровняОбразования;
            IdФормыОбучения = work.Студент.ФормаОбучения.IdФормыОбучения;
            IdИнститута = work.Студент.Структура.Институт.IdИнститута;
            IdКафедры = work.Студент.Структура.Кафедра.IdКафедры;
            ГодВыпуска = work.Студент.ГодОкончания;
            Аннотация = work.Аннотация;
            ДатаДобавления = work.ДатаДобавления;
            ДатаИзменения = work.ДатаИзменения;
            Местоположение = work.Местоположение;
            Эцп = work.Эцп;
            КоличСтраниц = work.КоличСтраниц;
            IdТипаРаботы = work.ТипРаботы.IdТипаРаботы;
            IdДоступаРаботы = work.ДоступРаботы.IdДоступаРаботы;
            IdСтатусаРаботы = work.СтатусРаботы.IdСтатусаРаботы;
        }
    }

    public enum TypeAddAttributes
    {
        Handle,
        AI,
        Template
    };
}

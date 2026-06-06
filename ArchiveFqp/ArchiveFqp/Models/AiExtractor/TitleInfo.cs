using System.Text.Json.Serialization;

namespace ArchiveFqp.Models.AiExtractor
{
    public class TitleInfo
    {
        public string Институт { get; set; } = string.Empty;
        public string Кафедра { get; set; } = string.Empty;
        public string Направление { get; set; } = string.Empty;
        public string Профиль { get; set; } = string.Empty;
        [JsonPropertyName("тип работы")]
        public string ТипРаботы { get; set; } = string.Empty;
        public string Тема { get; set; } = string.Empty;
        public string Группа { get; set; } = string.Empty;
        public string Автор { get; set; } = string.Empty;
        public string Руководитель { get; set; } = string.Empty;
        [JsonPropertyName("должность руководителя")]
        public string ДолжностьРуководителя { get; set; } = string.Empty;
        public string Год { get; set; } = string.Empty;
    }
}

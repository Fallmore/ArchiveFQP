using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.WorkApplication;

namespace ArchiveFqp.Interfaces.Applications
{
    public interface IApplicationsService
    {

        /// <summary>
        /// Получение справочника статусов заявлений
        /// </summary>
        /// <returns></returns>
        Task<List<СтатусЗаявления>> GetApplicationsStatusesAsync();

        /// <summary>
        /// Получение списка заявлений выдачи работ
        /// </summary>
        /// <returns></returns>
        Task<List<ЗаявлениеАтрибута>> GetAttributeApplicationsAsync();

        /// <summary>
        /// Получение списка заявлений выдачи работ
        /// </summary>
        /// <returns></returns>
        Task<List<ЗаявлениеРаботы>> GetWorkApplicationsAsync();

        /// <summary>
        /// Получение заявлений выдачи работы как <see cref="WorkApplicationDto"/> для отображение данных в виде строк
        /// </summary>
        /// <returns></returns>
        Task<WorkApplicationDto> GetWorkApplicationAsync(ЗаявлениеРаботы app, List<Консультант>? consultants = null, List<Рецензент>? reviewers = null);

        /// <summary>
        /// Добавление заявления на просмотр работы
        /// </summary>
        /// <param name="workApplication"></param>
        /// <returns></returns>
        Task<bool> AddWorkApplication(WorkApplicationDto workApplication);
        /// <summary>
        /// Добавление ответа на заявление на просмотр работы
        /// </summary>
        /// <param name="workApplication"></param>
        /// <returns></returns>
        Task<bool> AddAnswerWorkApplication(WorkApplicationDto workApplication);

        /// <summary>
        /// Добавление заявления на добавление атрибута
        /// </summary>
        /// <param name="attributeApplication"></param>
        /// <returns></returns>
        Task<bool> AddAttributeApplication(ЗаявлениеАтрибута attributeApplication);
        /// <summary>
        /// Добавление ответа на заявление на добавление атрибута
        /// </summary>
        /// <param name="attributeApplication"></param>
        /// <returns></returns>
        Task<bool> AddAnswerAttributeApplication(ЗаявлениеАтрибута attributeApplication);
    }
}

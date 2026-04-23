using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.WorkApplication;

namespace ArchiveFqp.Services.Applications
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

    }
}

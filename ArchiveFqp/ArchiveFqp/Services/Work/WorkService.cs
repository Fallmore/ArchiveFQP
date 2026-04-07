using ArchiveFqp.Factories.DisplayDto.Work;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Attribute;
using ArchiveFqp.Models.DTO.Work;
using ArchiveFqp.Models.Search;
using ArchiveFqp.Services.ReferenceData;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ArchiveFqp.Services.Work
{

    /// <summary>
    /// Сервис взаимодействия с работами
    /// </summary>
    public class WorkService : IWorkService
    {
        private readonly IDbContextFactory<ArchiveFqpContext> _dbFactory;
        private readonly IReferenceDataService _refDataService;

        // Настройка сериалайзера для кириллицы
        private readonly JsonSerializerOptions _options = new()
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };

        public WorkService(IDbContextFactory<ArchiveFqpContext> dbFactory, IReferenceDataService referenceDataService)
        {
            _dbFactory = dbFactory;
            _refDataService = referenceDataService;
        }

        public async Task<PaginatedResult<Работа>> SearchWorksAsync(WorkSearchModel searchModel)
        {
            using ArchiveFqpContext context = _dbFactory.CreateDbContext();

            // Преобразуем атрибуты в JSON
            string? attributesJson = null;

            if (searchModel.SelectedAttributes.Any(a => !string.IsNullOrEmpty(a.Value)))
            {
                Dictionary<string, string> dict = searchModel.SelectedAttributes
                    .Where(a => !string.IsNullOrEmpty(a.Value))
                    .ToDictionary(a => a.Key.ToString(), a => a.Value);

                attributesJson = JsonSerializer.Serialize(dict, _options);
            }

            // Вызов функции PostgreSQL
            List<Работа>? works = await context.Работаs
                .FromSqlRaw(@"SELECT * FROM поиск_работы({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, 
				{8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}::timestamp, {18}::timestamp,
				{19}::timestamp, {20}::timestamp, {21}::json)",
                    searchModel.SearchText ?? "",
                    searchModel.InstituteId,
                    searchModel.DepartmentId,
                    searchModel.DirectionId,
                    searchModel.ProfileId,
                    searchModel.WorkId,
                    searchModel.StudentId == 0 ? -1 : searchModel.StudentId,
                    searchModel.TeacherId == 0 ? -1 : searchModel.TeacherId,
                    searchModel.PostId,
                    searchModel.ConsultantsId.Count == 0 ? [-1] : searchModel.ConsultantsId,
                    searchModel.ReviewersId.Count == 0 ? [-1] : searchModel.ReviewersId,
                    searchModel.MinPages ?? -1,
                    searchModel.MaxPages ?? -1,
                    searchModel.WorkTypeId,
                    searchModel.WorkStatusId,
                    searchModel.MinYearDefense ?? -1,
                    searchModel.MaxYearDefense ?? -1,
                    searchModel.MinDateAdded,
                    searchModel.MaxDateAdded,
                    searchModel.MinDateChanged,
                    searchModel.MaxDateChanged,
                    attributesJson)
                .AsNoTracking()
                //.Include("IdПреподавателяNavigation.IdПользователяNavigation")
                //.Include("IdПреподавателяNavigation.IdДолжностиNavigation")
                //.Include("IdСтудентаNavigation.IdПользователяNavigation")
                //.Include("IdСтудентаNavigation.IdИнститутаNavigation")
                //.Include("IdСтудентаNavigation.IdУровняОбразованияNavigation")
                //.Include("IdСтудентаNavigation.IdФормыОбученияNavigation")
                //.Include("IdСтудентаNavigation.IdНаправленияNavigation.IdКафедрыNavigation.IdУгснNavigation.IdУгснСтандартаNavigation")
                //.Include(x => x.IdТипаРаботыNavigation)
                //.Include(x => x.IdСтатусаРаботыNavigation)
                //            .Include(w => w.ВыдачаРаботыs)
                //            .Include(w => w.ОценкаПреподавателяs)
                //.AsSingleQuery()
                .OrderByDescending(x => x.ДатаДобавления)
                .ToListAsync();

            return new PaginatedResult<Работа>
            {
                Items = works,
                TotalCount = works.Count(),
                Page = searchModel.Page,
                PageSize = searchModel.PageSize
            };
        }

        public async Task<List<ТипРаботы>> GetWorkTypesAsync()
        {
            return await _refDataService.GetAsync<ТипРаботы>();
        }

        public async Task<List<СтатусРаботы>> GetWorkStatusesAsync()
        {
            return await _refDataService.GetAsync<СтатусРаботы>();
        }

        public async Task<List<Студент>> GetStudentsAsync()
        {
            return [..(await _refDataService.GetAsync<Студент>())
                .DistinctBy(s => s.IdСтудента)];
        }

        public async Task<List<Преподаватель>> GetTeachersAsync()
        {
            return [..(await _refDataService.GetAsync<Преподаватель>())
                .DistinctBy(s => s.IdПреподавателя)];
        }

        public async Task<List<Консультант>> GetConsultantsAsync()
        {
            return await _refDataService.GetAsync<Консультант>();
        }

        public async Task<List<Рецензент>> GetReviewersAsync()
        {
            return await _refDataService.GetAsync<Рецензент>();
        }

        public async Task<List<Консультант>> GetConsultantsAsync(Работа work)
        {
            return (await GetConsultantsAsync())
                .Where(r => r.IdРаботы == work.IdРаботы)
                .ToList();
        }

        public async Task<List<Рецензент>> GetReviewersAsync(Работа work)
        {
            return (await GetReviewersAsync())
                .Where(r => r.IdРаботы == work.IdРаботы)
                .ToList();
        }

        public async Task<List<Атрибут>> GetAllAttributesAsync()
        {
            return await _refDataService.GetAsync<Атрибут>();
        }

        public async Task<List<AttributeValuesDto>> GetAttributeValuesAsync(List<Атрибут>? attrs = null, List<string>? abandonedValues = null)
        {
            using ArchiveFqpContext? context = _dbFactory.CreateDbContext();

            attrs ??= await _refDataService.GetAsync<Атрибут>();
            abandonedValues ??= IWorkService.AbandonedValues;

            List<AttributeValuesDto> avDto = await _refDataService.GetAsync<AttributeValuesDto>();
            avDto.ForEach(av => av.Данные.RemoveAll(x => abandonedValues.Contains(x)));

            if (attrs != null && attrs.Count != 0)
            {
                List<int> attrIds = attrs.Select(a => a.IdАтрибута).ToList();
                avDto = [.. avDto.Where(x => attrIds.Contains(x.IdАтрибута))];
            }

            return avDto;
        }

        public async Task<List<AttributeDto>?> GetWorkAttributesAsync(int idWork, List<string>? abandonedValues)
        {
            return (await GetWorksAttributesAsync([new() { IdРаботы = idWork }], abandonedValues)).FirstOrDefault().Value;
        }

        public async Task<Dictionary<int, List<AttributeDto>>> GetWorksAttributesAsync(List<Работа> works, List<string>? abandonedValues)
        {
            Dictionary<int, List<AttributeDto>> result = [];

            if (works.Count == 0) return result;
            if (abandonedValues == default) abandonedValues = IWorkService.AbandonedValues;

            using ArchiveFqpContext? context = _dbFactory.CreateDbContext();

            List<int> workIds = works.Select(w => w.IdРаботы).ToList();
            List<Атрибут> attributes = await _refDataService.GetAsync<Атрибут>();

            List<AttributeDto> data = (await _refDataService.GetAsync<АтрибутУчреждения>())
                .Join(context.ДанныеПоАтрибs, a => a.IdСтруктуры, d => d.IdСтруктуры,
                    (a, d) => new AttributeDto(
                        a.IdАтрибута, d.IdДанных,
                        d.IdСтруктуры, d.IdРаботы,
                        attributes.First(atr => atr.IdАтрибута == a.IdАтрибута).Название, d.Данные))
                .Where(ad => workIds.Contains(ad.IdРаботы)
                    && !abandonedValues.Contains(ad.Данные))
                .ToList();

            if (data?.Count == 0 || data is null) return result;

            data.Sort((x, y) => x.IdДанных.CompareTo(y.IdДанных));

            result = data
                .GroupBy(ad => ad.IdРаботы)
                .ToDictionary(g => g.Key,
                d => d.ToList()
                );

            return result;
        }

        public async Task<WorkDisplayDto> GetWorkDisplayAsync(Работа work, List<Консультант>? consultants = null, List<Рецензент>? reviewers = null)
        {
            consultants ??= await GetConsultantsAsync(work);
            reviewers ??= await GetReviewersAsync(work);
            WorkDtoFactory factory = new(_dbFactory, _refDataService);
            return await factory.CreateDisplayDtoAsync(work, consultants, reviewers);
        }

        public bool SetStudent(WorkCreateDto work, int? idStudent)
        {
            bool nulify()
            {
                work.IdУровняОбразования = work.IdФормыОбучения = work.IdПрофиля = work.ГодВыпуска = null;
                // Обнуляем остальные связанные данные вверх по иерархии учреждения
                // студент -> направление (профиль пропускается, т.к. он может быть null
                // и это норма)
                return (SetDirection(work, -1));
            }

            if (!idStudent.HasValue)
            {
                return nulify();
            }

            Студент? student = _refDataService.GetAsync<Студент>().Result
                .FirstOrDefault(s => s.IdСтудента == idStudent.Value);
            if (student != null)
            {
                work.IdУровняОбразования = student.IdУровняОбразования;
                work.IdФормыОбучения = student.IdФормыОбучения;
                work.IdПрофиля = student.IdПрофиля;
                work.ГодВыпуска = student.ГодОкончания;

                bool isOk = SetDirection(work, student.IdНаправления);

                if (!isOk)
                {
                    work.IdУровняОбразования = work.IdФормыОбучения = work.IdПрофиля = work.ГодВыпуска = null;
                }

                return isOk;
            }

            return nulify();
        }

        public bool SetProfile(WorkCreateDto work, int? idProfile)
        {
            bool nulify()
            {
                work.IdПрофиля = null;
                // Не идём вверх по иерархии, т.к. профиль
                // может быть null и это норма
                return false;
            }

            if (!idProfile.HasValue)
            {
                return nulify();
            }

            Профиль? profile = _refDataService.GetAsync<Профиль>().Result
                .FirstOrDefault(s => s.IdПрофиля == idProfile.Value);
            if (profile != null)
            {
                work.IdПрофиля = profile.IdПрофиля;

                bool isOk = SetDirection(work, profile.IdНаправления);
                if (!isOk)
                {
                    work.IdПрофиля = null;
                }

                return isOk;
            }

            return nulify();
        }

        public bool SetDirection(WorkCreateDto work, int? idDirection)
        {
            int? idDepartment = null;
            bool nulify()
            {
                work.IdНаправления = null;
                // Обнуляем остальные связанные данные вверх по иерархии учреждения
                // Направление -> кафедра
                return (SetDepartment(work, idDepartment));
            }

            if (!idDirection.HasValue)
            {
                return nulify();
            }

            Направление? directions = _refDataService.GetAsync<Направление>().Result
                .FirstOrDefault(s => s.IdНаправления == idDirection);
            if (directions != null)
            {
                work.IdНаправления = directions.IdНаправления;
                idDepartment = _refDataService.GetAsync<Кафедра>().Result
                    .FirstOrDefault(d => d.IdКафедры == (_refDataService.GetAsync<Направление>().Result
                        .FirstOrDefault(d => d.IdНаправления == work.IdНаправления)
                        ?.IdКафедры ?? -1))
                    ?.IdКафедры;

                bool isOk = SetDepartment(work, idDepartment);
                if (!isOk)
                {
                    work.IdНаправления = null;
                }

                return isOk;
            }

            return nulify();
        }

        public bool SetDepartment(WorkCreateDto work, int? idDepartment)
        {
            bool nulify()
            {
                work.IdКафедры = work.IdУгсн = work.IdИнститута = work.IdУгснСтандарта = null;
                // Конец иерархии
                return false;
            }

            if (!idDepartment.HasValue) return nulify();

            Кафедра? department = _refDataService.GetAsync<Кафедра>().Result
                .FirstOrDefault(s => s.IdКафедры == idDepartment);
            if (department != null)
            {
                work.IdКафедры = idDepartment;
                Угсн? ugsn = _refDataService.GetAsync<Угсн>().Result
                    .FirstOrDefault(d => d.IdУгсн == (_refDataService.GetAsync<Кафедра>().Result
                        .FirstOrDefault(d => d.IdКафедры == work.IdКафедры)?.IdУгсн ?? -1));
                if (ugsn != null)
                {
                    work.IdУгсн = ugsn.IdУгсн;
                    work.IdУгснСтандарта = ugsn.IdУгснСтандарта;
                }
                work.IdИнститута = _refDataService.GetAsync<Институт>().Result
                    .FirstOrDefault(d => d.IdИнститута == (_refDataService.GetAsync<Кафедра>().Result
                        .FirstOrDefault(d => d.IdКафедры == work.IdКафедры)
                        ?.IdИнститута ?? -1))
                    ?.IdИнститута;

                bool isOk = work.IdИнститута.HasValue;
                if (!isOk)
                {
                    work.IdКафедры = work.IdУгсн = work.IdИнститута = work.IdУгснСтандарта = null;
                }

                return isOk;
            }

            return nulify();
        }

        public int PickDateWork(Работа work)
        {
            string workType = GetWorkTypesAsync().Result
                .First(t => t.IdТипаРаботы == work.IdТипаРаботы).Название;

            if (IWorkService.FqpWorks.Contains(workType))
            {
                int dateGraduation = GetStudentsAsync().Result
                .First(s => s.IdСтудента == work.IdСтудента).ГодОкончания;
                return dateGraduation;
            }

            return work.ДатаДобавления.Year;
        }

        public int PickDateWork(WorkDisplayDto work)
        {
            if (IWorkService.FqpWorks.Contains(work.ТипРаботы))
                return work.Студент.ГодОкончания;

            return work.ДатаДобавления.Year;
        }
    }

}

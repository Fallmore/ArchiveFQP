using ArchiveFqp.Factories.DisplayDto.Work;
using ArchiveFqp.Interfaces.ReferenceData;
using ArchiveFqp.Interfaces.Work;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Attribute;
using ArchiveFqp.Models.DTO.Work;
using ArchiveFqp.Models.Search;
using ArchiveFqp.Models.Settings.SettingsArchive;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
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
        private SettingsArchive _settings;

        // Настройка сериалайзера для кириллицы
        private readonly JsonSerializerOptions _options = new()
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };

        public WorkService(IDbContextFactory<ArchiveFqpContext> dbFactory,
            IReferenceDataService referenceDataService, SettingsArchive settings)
        {
            _dbFactory = dbFactory;
            _refDataService = referenceDataService;
            _settings = settings;
            _settings.SettingsChanged += SettingsChanged;
        }

        private void SettingsChanged(object? sender, SettingsArchive e)
        {
            _settings = e;
        }

        public async Task<PaginatedResult<Работа>> FindWorksAsync(WorkSearchModel searchModel)
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
				{8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19},
				{20}, {21}, {22}::json)",
                    searchModel.SearchText ?? "",
                    searchModel.IdInstitute ?? -1,
                    searchModel.IdDepartment ?? -1,
                    searchModel.IdDirection ?? -1,
                    searchModel.IdProfile ?? -1,
                    searchModel.IdWork ?? -1,
                    searchModel.IdStudent ?? -1,
                    searchModel.IdTeacher ?? -1,
                    searchModel.IdPost ?? -1,
                    searchModel.IdConsultants.Count == 0 ? [-1] : searchModel.IdConsultants,
                    searchModel.IdReviewers.Count == 0 ? [-1] : searchModel.IdReviewers,
                    searchModel.MinPages ?? -1,
                    searchModel.MaxPages ?? -1,
                    searchModel.IdWorkType ?? -1,
                    searchModel.IdWorkStatus ?? -1,
                    searchModel.IdWorkAccess ?? -1,
                    searchModel.MinYearDefense ?? -1,
                    searchModel.MaxYearDefense ?? -1,
                    searchModel.MinDateAdded == null ? null : new NpgsqlParameter("p18", NpgsqlDbType.Timestamp) { Value = searchModel.MinDateAdded },
                    searchModel.MaxDateAdded == null ? null : new NpgsqlParameter("p19", NpgsqlDbType.Timestamp) { Value = searchModel.MaxDateAdded },
                    searchModel.MinDateChanged == null ? null : new NpgsqlParameter("p20", NpgsqlDbType.Timestamp) { Value = searchModel.MinDateChanged },
                    searchModel.MaxDateChanged == null ? null : new NpgsqlParameter("p21", NpgsqlDbType.Timestamp) { Value = searchModel.MaxDateChanged },
                    attributesJson)
                .AsNoTracking()
                .OrderByDescending(x => x.ДатаДобавления)
                .ToListAsync();

            return new PaginatedResult<Работа>
            {
                Items = works,
                CurrentPage = searchModel.Page,
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
            abandonedValues ??= _settings.AttributesAbandonedValues;

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
            abandonedValues ??= _settings.AttributesAbandonedValues;

            using ArchiveFqpContext context = _dbFactory.CreateDbContext();

            List<int> workIds = works.Select(w => w.IdРаботы).ToList();
            List<Атрибут> attributes = await _refDataService.GetAsync<Атрибут>();

            List<AttributeDto> data = (await _refDataService.GetAsync<АтрибутУчреждения>())
                .Join(await _refDataService.GetAsync<ДанныеПоАтриб>(), a => a.IdСтруктуры, d => d.IdСтруктуры,
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

        public async Task<List<WorkDisplayDto>> GetWorkDisplayAsync(List<Работа> works, List<string>? abandonedValues = null)
        {
            abandonedValues ??= _settings.AttributesAbandonedValues;
            Task<Dictionary<int, List<AttributeDto>>> attributes = Task.Run(() => GetWorksAttributesAsync(works, abandonedValues));
            
            WorkDtoFactory factory = new(_dbFactory, _refDataService);
            List<WorkDisplayDto> res = await factory.CreateDisplayDtoAsync(works);

            Dictionary<int, List<AttributeDto>>? dict = await attributes;
            foreach (WorkDisplayDto item in res)
            {
                item.Атрибуты = dict[item.IdРаботы];
            }

            return res;
        }

        public async Task<WorkDisplayDto> GetWorkDisplayAsync(Работа work,
            List<Консультант>? consultants = null, List<Рецензент>? reviewers = null,
            List<string>? abandonedValues = null)
        {
            Task<List<AttributeDto>?> attributes = Task.Run(() => GetWorkAttributesAsync(work.IdРаботы, abandonedValues));
            
            consultants ??= await GetConsultantsAsync(work);
            reviewers ??= await GetReviewersAsync(work);
            WorkDtoFactory factory = new(_dbFactory, _refDataService);
            WorkDisplayDto res = await factory.CreateDisplayDtoAsync(work, consultants, reviewers);

            res.Атрибуты = await attributes;
            return res;
        }

        public async Task<WorkDisplayDto> GetWorkDisplayAsync(int idWork, 
            List<Консультант>? consultants = null, List<Рецензент>? reviewers = null,
            List<string>? abandonedValues = null)
        {
            using ArchiveFqpContext context = _dbFactory.CreateDbContext();
            Работа? work = (await _refDataService.GetAsync<Работа>()).FirstOrDefault(w => w.IdРаботы == idWork);
            if (work == null) return new();
            return await GetWorkDisplayAsync(work, consultants, reviewers, abandonedValues);
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

                Угсн? ugsn = _refDataService.GetAsync<Угсн>().Result
                    .FirstOrDefault(d => d.IdУгсн == (_refDataService.GetAsync<Направление>().Result
                        .FirstOrDefault(d => d.IdНаправления == work.IdНаправления)?.IdУгсн ?? -1));
                if (ugsn != null)
                {
                    work.IdУгсн = ugsn.IdУгсн;
                    work.IdУгснСтандарта = ugsn.IdУгснСтандарта;
                }

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

            if (_settings.FqpWorks.Contains(workType))
            {
                int dateGraduation = GetStudentsAsync().Result
                .First(s => s.IdСтудента == work.IdСтудента).ГодОкончания;
                return dateGraduation;
            }

            return work.ДатаДобавления.Year;
        }

        public int PickDateWork(WorkDisplayDto work)
        {
            if (_settings.FqpWorks.Contains(work.ТипРаботы))
                return work.Студент.ГодОкончания;

            return work.ДатаДобавления.Year;
        }

        public async Task<bool> UpdateStatusAsync(int idWork, int idStatus)
        {
            using ArchiveFqpContext context = _dbFactory.CreateDbContext();
            Работа? work = context.Работаs.Find(idWork);
            if (work == null) return false;

            work.IdСтатусаРаботы = idStatus;
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Работа>> GetWorksAsync()
        {
            return await _refDataService.GetAsync<Работа>();
        }

        public async Task<Работа?> GetWorkAsync(int idWork)
        {
            return (await GetWorksAsync()).FirstOrDefault(w => w.IdРаботы == idWork);
        }
    }

}

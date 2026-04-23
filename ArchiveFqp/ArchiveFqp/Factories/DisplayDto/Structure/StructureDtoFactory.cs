using ArchiveFqp.Factories.DisplayDto.User;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Structure;
using ArchiveFqp.Models.DTO.Student;
using ArchiveFqp.Services.ReferenceData;

namespace ArchiveFqp.Factories.DisplayDto.Structure
{
    public class StructureDtoFactory : IDisplayDtoFactory<StructureDto, Профиль>
    {
        private readonly IReferenceDataService _refDataService;
        private readonly UserDtoFactory _userDisplayFactory;

        private List<Институт> _institutes = [];
        private List<Кафедра> _departments = [];
        private List<Угсн> _ugsns = [];
        private List<УгснСтандарт> _ugsnsStandarts = [];
        private List<Направление> _directions = [];
        private List<Профиль> _profiles = [];

        private Task _init;

        public StructureDtoFactory(IReferenceDataService refDataService)
        {
            _refDataService = refDataService;
            _userDisplayFactory = new(_refDataService);
            _init = Task.Run(InitializeLists);
        }

        private async Task InitializeLists()
        {
            _institutes = await _refDataService.GetAsync<Институт>();
            _departments = await _refDataService.GetAsync<Кафедра>();
            _ugsns = await _refDataService.GetAsync<Угсн>();
            _ugsnsStandarts = await _refDataService.GetAsync<УгснСтандарт>();
            _directions = await _refDataService.GetAsync<Направление>();
            _profiles = await _refDataService.GetAsync<Профиль>();
        }

        public async Task<StructureDto?> CreateDisplayDtoAsync(int idProfile)
        {
            _init.Wait();
            Профиль? profile = _profiles.FirstOrDefault(o => o.IdПрофиля == idProfile);
            if (profile == null) return null;

            return await CreateDisplayDtoAsync(profile);
        }

        /// <summary>
        /// <inheritdoc cref="CreateDisplayDtoAsync(int)"/> <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">1 из 3 классов: <see cref="Профиль"/>, 
        /// <see cref="Направление"/> или <see cref="Кафедра"/></typeparam>
        /// <param name="id"></param>
        /// <returns>Вовзращает DTO <see cref="StructureDto"/> или <stron>null</value> в случае,
        /// если был не найден объект типа <typeparamref name="T"/> или указанный 
        /// тип не соотвествует ни одному из указанных типов параметра T</returns>
        public async Task<StructureDto?> CreateDisplayDtoAsync<T>(int id) where T : class
        {
            _init.Wait();
            switch (typeof(T).Name)
            {
                case nameof(Профиль):
                    Профиль? profile = _profiles.FirstOrDefault(o => o.IdПрофиля == id);
                    if (profile == null) return null;

                    return await CreateDisplayDtoAsync(profile);
                case nameof(Направление):
                    Направление? direction = _directions.FirstOrDefault(o => o.IdНаправления == id);
                    if (direction == null) return null;

                    return await CreateDisplayDtoAsync(direction);
                case nameof(Кафедра):
                    Кафедра? department = _departments.FirstOrDefault(o => o.IdКафедры == id);
                    if (department == null) return null;

                    return await CreateDisplayDtoAsync(department);
                default:
                    return null;
            }
        }

        public async Task<StructureDto> CreateDisplayDtoAsync(Профиль obj)
        {
            _init.Wait();
            Направление направление = _directions.FirstOrDefault(x => x.IdНаправления == obj.IdНаправления) ?? new();
            if (направление == null) return new() { Профиль = obj };

            StructureDto f = await CreateDisplayDtoAsync(направление);
            f.Профиль = obj;
            return f;
        }

        public async Task<StructureDto> CreateDisplayDtoAsync(Направление obj)
        {
            _init.Wait();
            Кафедра? кафедра = _departments.FirstOrDefault(o => o.IdКафедры == obj.IdКафедры);
            if (кафедра == null) return new() { Направление = obj };

            StructureDto f = await CreateDisplayDtoAsync(кафедра);
            f.Направление = obj;
            return f;
        }

        public async Task<StructureDto> CreateDisplayDtoAsync(Кафедра obj)
        {
            _init.Wait();
            Институт институт = _institutes.FirstOrDefault(o => o.IdИнститута == obj.IdИнститута) ?? new();
            Угсн угсн = _ugsns.FirstOrDefault(o => o.IdУгсн == obj.IdУгсн) ?? new();

            return new()
            {
                Институт = институт,
                Кафедра = obj,
                Угсн = угсн,
                УгснСтандарт = _ugsnsStandarts.FirstOrDefault(o => o.IdУгснСтандарта == угсн.IdУгснСтандарта) ?? new()
            };
        }

        public async Task<List<StructureDto>> CreateDisplayDtoListAsync(IEnumerable<Профиль> obj)
        {
            IEnumerable<Task<StructureDto>> tasks = obj.Select(CreateDisplayDtoAsync);
            StructureDto[] results = await Task.WhenAll(tasks);
            return results.ToList();
        }

        public async Task<List<StructureDto>> CreateDisplayDtoListAsync(IEnumerable<Направление> obj)
        {
            IEnumerable<Task<StructureDto>> tasks = obj.Select(CreateDisplayDtoAsync);
            StructureDto[] results = await Task.WhenAll(tasks);
            return results.ToList();
        }

        public async Task<List<StructureDto>> CreateDisplayDtoListAsync(IEnumerable<Кафедра> obj)
        {
            IEnumerable<Task<StructureDto>> tasks = obj.Select(CreateDisplayDtoAsync);
            StructureDto[] results = await Task.WhenAll(tasks);
            return results.ToList();
        }
    }
}

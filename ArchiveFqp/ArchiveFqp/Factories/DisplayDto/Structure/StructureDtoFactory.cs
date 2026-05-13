using ArchiveFqp.Factories.DisplayDto.User;
using ArchiveFqp.Interfaces.ReferenceData;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Structure;
using ArchiveFqp.Models.DTO.User;
using ArchiveFqp.Models.ReferenceData;

namespace ArchiveFqp.Factories.DisplayDto.Structure
{
    public class StructureDtoFactory : IDisplayDtoFactory<StructureDto, Профиль>
    {
        private readonly IReferenceDataService _refDataService;
        private readonly UserDtoFactory _userDisplayFactory;

        private ReferenceDataSnapshot _snapshot = null!;

        private Task _init;

        public StructureDtoFactory(IReferenceDataService refDataService)
        {
            _refDataService = refDataService;
            _userDisplayFactory = new(_refDataService);
            _init = Task.Run(InitializeLists);
        }

        private async Task InitializeLists()
        {
            _snapshot = await _refDataService.GetAllAsync();
        }

        public async Task<StructureDto?> CreateDisplayDtoAsync(int idProfile)
        {
            _init.Wait();
            Профиль? profile = _snapshot.Profiles.FirstOrDefault(o => o.IdПрофиля == idProfile);
            if (profile == null) return null;

            return await CreateDisplayDtoAsync(profile);
        }

        /// <summary>
        /// <inheritdoc cref="CreateDisplayDtoAsync(int)"/> <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">1 из 3 классов: <see cref="Профиль"/>, 
        /// <see cref="Направление"/> или <see cref="Кафедра"/></typeparam>
        /// <param name="id"></param>
        /// <returns>Вовзращает DTO <see cref="StructureDto"/> или <strong>null</strong> в случае,
        /// если был не найден объект типа <typeparamref name="T"/> или указанный 
        /// тип не соотвествует ни одному из указанных типов параметра T</returns>
        public async Task<StructureDto?> CreateDisplayDtoAsync<T>(int id) where T : class
        {
            _init.Wait();
            switch (typeof(T).Name)
            {
                case nameof(Профиль):
                    Профиль? profile = _snapshot.Profiles.FirstOrDefault(o => o.IdПрофиля == id);
                    if (profile == null) return null;

                    return await CreateDisplayDtoAsync(profile);
                case nameof(Направление):
                    Направление? direction = _snapshot.Directions.FirstOrDefault(o => o.IdНаправления == id);
                    if (direction == null) return null;

                    return await CreateDisplayDtoAsync(direction);
                case nameof(Кафедра):
                    Кафедра? department = _snapshot.Departments.FirstOrDefault(o => o.IdКафедры == id);
                    if (department == null) return null;

                    return await CreateDisplayDtoAsync(department);
                default:
                    return null;
            }
        }

        /// <inheritdoc cref="IDisplayDtoFactory{T, V}.CreateDisplayDtoAsync(V)"/>
        public async Task<StructureDto> CreateDisplayDtoAsync(Работа obj)
        {
            _init.Wait();
            Студент? студент = (await _refDataService.GetAsync<Студент>()).FirstOrDefault(x => x.IdСтудента == obj.IdСтудента);
            if (студент == null) return new();

            return await CreateDisplayDtoAsync(студент);
        }

        /// <inheritdoc cref="CreateDisplayDtoAsync(Профиль)"/>
        public async Task<StructureDto> CreateDisplayDtoAsync(Студент obj)
        {
            _init.Wait();
            Направление? направление = _snapshot.Directions.FirstOrDefault(x => x.IdНаправления == obj.IdНаправления);

            if (obj.IdПрофиля != null)
                return await CreateDisplayDtoAsync(obj.IdПрофиля.Value) ?? new();
            else if (направление != null)
                return await CreateDisplayDtoAsync(направление);

            return new();
        }

        public async Task<StructureDto> CreateDisplayDtoAsync(Профиль obj)
        {
            _init.Wait();
            Направление? направление = _snapshot.Directions.FirstOrDefault(x => x.IdНаправления == obj.IdНаправления);
            if (направление == null) return new() { Профиль = obj };

            StructureDto res = await CreateDisplayDtoAsync(направление);
            res.Профиль = obj;
            return res;
        }

        /// <inheritdoc cref="CreateDisplayDtoAsync(Профиль)"/>
        public async Task<StructureDto> CreateDisplayDtoAsync(Направление obj)
        {
            _init.Wait();
            Кафедра? кафедра = _snapshot.Departments.FirstOrDefault(o => o.IdКафедры == obj.IdКафедры);
            if (кафедра == null) return new() { Направление = obj };
            Угсн угсн = _snapshot.Ugsns.FirstOrDefault(o => o.IdУгсн == obj.IdУгсн) ?? new();

            StructureDto res = await CreateDisplayDtoAsync(кафедра);
            res.Направление = obj;
            res.Угсн = угсн;
            res.УгснСтандарт = _snapshot.UgsnStandarts.FirstOrDefault(o => o.IdУгснСтандарта == угсн.IdУгснСтандарта) ?? new();
            return res;
        }

        /// <inheritdoc cref="CreateDisplayDtoAsync(Профиль)"/>
        public async Task<StructureDto> CreateDisplayDtoAsync(Кафедра obj)
        {
            _init.Wait();
            Институт институт = _snapshot.Institutes.FirstOrDefault(o => o.IdИнститута == obj.IdИнститута) ?? new();

            return new()
            {
                Институт = институт,
                Кафедра = obj,
            };
        }

        public async Task<List<StructureDto>> CreateDisplayDtoAsync(IEnumerable<Профиль> obj)
        {
            IEnumerable<Task<StructureDto>> tasks = obj.Select(CreateDisplayDtoAsync);
            StructureDto[] results = await Task.WhenAll(tasks);
            return results.ToList();
        }

        /// <inheritdoc cref="CreateDisplayDtoAsync(IEnumerable{Профиль})"/>
        public async Task<List<StructureDto>> CreateDisplayDtoAsync(IEnumerable<Направление> obj)
        {
            IEnumerable<Task<StructureDto>> tasks = obj.Select(CreateDisplayDtoAsync);
            StructureDto[] results = await Task.WhenAll(tasks);
            return results.ToList();
        }

        /// <inheritdoc cref="CreateDisplayDtoAsync(IEnumerable{Профиль})"/>
        public async Task<List<StructureDto>> CreateDisplayDtoAsync(IEnumerable<Кафедра> obj)
        {
            IEnumerable<Task<StructureDto>> tasks = obj.Select(CreateDisplayDtoAsync);
            StructureDto[] results = await Task.WhenAll(tasks);
            return results.ToList();
        }
    }
}

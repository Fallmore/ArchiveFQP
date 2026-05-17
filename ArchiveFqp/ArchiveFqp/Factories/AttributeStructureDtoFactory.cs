using ArchiveFqp.Factories.DisplayDto.Structure;
using ArchiveFqp.Interfaces.ReferenceData;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Attribute;
using ArchiveFqp.Models.ReferenceData;

namespace ArchiveFqp.Factories
{
    public class AttributeStructureDtoFactory
    {
        private readonly IReferenceDataService _refDataService;
        private readonly StructureDtoFactory _structureDtoFactory;
        private ReferenceDataSnapshot _snapshot = null!;

        private Task _init;

        public AttributeStructureDtoFactory(IReferenceDataService refDataService)
        {
            _refDataService = refDataService;
            _structureDtoFactory = new(_refDataService);
            _init = Task.Run(InitializeLists);
        }

        private async Task InitializeLists()
        {
            _snapshot = await _refDataService.GetSnapshotAsync();
        }

        /// <summary>
        /// Создает DTO <see cref="AttributeStructureDto"/> по id структуры от типа <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>T может быть одним из следующих типов: <see cref="АтрибутУчреждения"/>, <see cref="АтрибутИнститута"/>, <see cref="АтрибутКафедры"/>, <see cref="АтрибутНаправления"/>, <see cref="АтрибутПрофиля"/></remarks>
        /// <typeparam name="T">1 из <see cref="АтрибутУчреждения"/>, <see cref="АтрибутИнститута"/>, <see cref="АтрибутКафедры"/>, <see cref="АтрибутНаправления"/>, <see cref="АтрибутПрофиля"/></typeparam>
        /// <param name="idStructure"></param>
        /// <returns></returns>
        public async Task<AttributeStructureDto?> CreateAsync<T>(int idStructure) where T : class
        {
            _init.Wait();
            List<T> structures = await _refDataService.GetAsync<T>();
            return typeof(T).Name switch
            {
                nameof(АтрибутУчреждения) => await CreateAsync((structures as List<АтрибутУчреждения>)!.First(x => x.IdСтруктуры == idStructure)),
                nameof(АтрибутИнститута) => await CreateAsync((structures as List<АтрибутИнститута>)!.First(x => x.IdСтруктуры == idStructure)),
                nameof(АтрибутКафедры) => await CreateAsync((structures as List<АтрибутКафедры>)!.First(x => x.IdСтруктуры == idStructure)),
                nameof(АтрибутНаправления) => await CreateAsync((structures as List<АтрибутНаправления>)!.First(x => x.IdСтруктуры == idStructure)),
                nameof(АтрибутПрофиля) => await CreateAsync((structures as List<АтрибутПрофиля>)!.First(x => x.IdСтруктуры == idStructure)),
                _ => null
            };
        }

        /// <summary>
        /// Создает DTO <see cref="AttributeStructureDto"/> по атрибуту структуры от типа <see cref="АтрибутУчреждения"/>.
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public async Task<AttributeStructureDto> CreateAsync(АтрибутУчреждения attribute)
        {
            _init.Wait();
            return new()
            {
                IdСтруктуры = attribute.IdСтруктуры,
                Атрибут = _snapshot.Attributes.FirstOrDefault(x => x.IdАтрибута == attribute.IdАтрибута) ?? new(),
                СтатусРаботы = _snapshot.WorkStatuses.FirstOrDefault(x => x.IdСтатусаРаботы == attribute.IdСтатусаРаботы),
                ТипРаботы = _snapshot.WorkTypes.FirstOrDefault(x => x.IdТипаРаботы == attribute.IdТипаРаботы),
                ТипСтруктуры = StructureType.Учреждение
            };
        }

        /// <summary>
        /// Создает DTO <see cref="AttributeStructureDto"/> по атрибуту структуры от типа <see cref="АтрибутИнститута"/>.
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public async Task<AttributeStructureDto> CreateAsync(АтрибутИнститута attribute)
        {
            _init.Wait();
            return new()
            {
                IdСтруктуры = attribute.IdСтруктуры,
                Атрибут = _snapshot.Attributes.FirstOrDefault(x => x.IdАтрибута == attribute.IdАтрибута) ?? new(),
                СтатусРаботы = _snapshot.WorkStatuses.FirstOrDefault(x => x.IdСтатусаРаботы == attribute.IdСтатусаРаботы),
                ТипРаботы = _snapshot.WorkTypes.FirstOrDefault(x => x.IdТипаРаботы == attribute.IdТипаРаботы),
                Структура = await _structureDtoFactory.CreateDisplayDtoAsync<Институт>(attribute.IdИнститута),
                ТипСтруктуры = StructureType.Институт
            };
        }

        /// <summary>
        /// Создает DTO <see cref="AttributeStructureDto"/> по атрибуту структуры от типа <see cref="АтрибутКафедры"/>.
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public async Task<AttributeStructureDto> CreateAsync(АтрибутКафедры attribute)
        {
            _init.Wait();
            return new()
            {
                IdСтруктуры = attribute.IdСтруктуры,
                Атрибут = _snapshot.Attributes.FirstOrDefault(x => x.IdАтрибута == attribute.IdАтрибута) ?? new(),
                СтатусРаботы = _snapshot.WorkStatuses.FirstOrDefault(x => x.IdСтатусаРаботы == attribute.IdСтатусаРаботы),
                ТипРаботы = _snapshot.WorkTypes.FirstOrDefault(x => x.IdТипаРаботы == attribute.IdТипаРаботы),
                Структура = await _structureDtoFactory.CreateDisplayDtoAsync<Кафедра>(attribute.IdКафедры),
                ТипСтруктуры = StructureType.Кафедра
            };
        }

        /// <summary>
        /// Создает DTO <see cref="AttributeStructureDto"/> по атрибуту структуры от типа <see cref="АтрибутНаправления"/>.
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public async Task<AttributeStructureDto> CreateAsync(АтрибутНаправления attribute)
        {
            _init.Wait();
            return new()
            {
                IdСтруктуры = attribute.IdСтруктуры,
                Атрибут = _snapshot.Attributes.FirstOrDefault(x => x.IdАтрибута == attribute.IdАтрибута) ?? new(),
                СтатусРаботы = _snapshot.WorkStatuses.FirstOrDefault(x => x.IdСтатусаРаботы == attribute.IdСтатусаРаботы),
                ТипРаботы = _snapshot.WorkTypes.FirstOrDefault(x => x.IdТипаРаботы == attribute.IdТипаРаботы),
                Структура = await _structureDtoFactory.CreateDisplayDtoAsync<Направление>(attribute.IdНаправления),
                ТипСтруктуры = StructureType.Направление
            };
        }

        /// <summary>
        /// Создает DTO <see cref="AttributeStructureDto"/> по атрибуту структуры от типа <see cref="АтрибутПрофиля"/>.
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public async Task<AttributeStructureDto> CreateAsync(АтрибутПрофиля attribute)
        {
            _init.Wait();
            return new()
            {
                IdСтруктуры = attribute.IdСтруктуры,
                Атрибут = _snapshot.Attributes.FirstOrDefault(x => x.IdАтрибута == attribute.IdАтрибута) ?? new(),
                СтатусРаботы = _snapshot.WorkStatuses.FirstOrDefault(x => x.IdСтатусаРаботы == attribute.IdСтатусаРаботы),
                ТипРаботы = _snapshot.WorkTypes.FirstOrDefault(x => x.IdТипаРаботы == attribute.IdТипаРаботы),
                Структура = await _structureDtoFactory.CreateDisplayDtoAsync<Профиль>(attribute.IdПрофиля),
                ТипСтруктуры = StructureType.Профиль
            };
        }





        /// <summary>
        /// Создает список DTO <see cref="AttributeStructureDto"/> по id структуры от типа <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>T может быть одним из следующих типов: <see cref="АтрибутУчреждения"/>, <see cref="АтрибутИнститута"/>, <see cref="АтрибутКафедры"/>, <see cref="АтрибутНаправления"/>, <see cref="АтрибутПрофиля"/></remarks>
        /// <typeparam name="T">1 из <see cref="АтрибутУчреждения"/>, <see cref="АтрибутИнститута"/>, <see cref="АтрибутКафедры"/>, <see cref="АтрибутНаправления"/>, <see cref="АтрибутПрофиля"/></typeparam>
        /// <param name="idStructure"></param>
        /// <returns></returns>
        public async Task<List<AttributeStructureDto>?> CreateAsync<T>(List<int> idStructures) where T : class
        {
            _init.Wait();
            List<T> structures = await _refDataService.GetAsync<T>();
            return typeof(T).Name switch
            {
                nameof(АтрибутУчреждения) => await CreateAsync((structures as List<АтрибутУчреждения>)!.Where(x => idStructures.Contains(x.IdСтруктуры)).ToList()),
                nameof(АтрибутИнститута) => await CreateAsync((structures as List<АтрибутИнститута>)!.Where(x => idStructures.Contains(x.IdСтруктуры)).ToList()),
                nameof(АтрибутКафедры) => await CreateAsync((structures as List<АтрибутКафедры>)!.Where(x => idStructures.Contains(x.IdСтруктуры)).ToList()),
                nameof(АтрибутНаправления) => await CreateAsync((structures as List<АтрибутНаправления>)!.Where(x => idStructures.Contains(x.IdСтруктуры)).ToList()),
                nameof(АтрибутПрофиля) => await CreateAsync((structures as List<АтрибутПрофиля>)!.Where(x => idStructures.Contains(x.IdСтруктуры)).ToList()),
                _ => null
            };
        }

        /// <summary>
        /// Создает список DTO <see cref="AttributeStructureDto"/> по атрибуту структуры от типа <see cref="АтрибутУчреждения"/>.
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public async Task<List<AttributeStructureDto>> CreateAsync(List<АтрибутУчреждения> attributes)
        {
            IEnumerable<Task<AttributeStructureDto>> tasks = attributes.Select(CreateAsync);
            AttributeStructureDto?[] results = await Task.WhenAll(tasks);
            return results.ToList()!;
        }

        /// <summary>
        /// Создает список DTO <see cref="AttributeStructureDto"/> по атрибуту структуры от типа <see cref="АтрибутИнститута"/>.
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public async Task<List<AttributeStructureDto>> CreateAsync(List<АтрибутИнститута> attributes)
        {
            IEnumerable<Task<AttributeStructureDto>> tasks = attributes.Select(CreateAsync);
            AttributeStructureDto?[] results = await Task.WhenAll(tasks);
            return results.ToList()!;
        }

        /// <summary>
        /// Создает список DTO <see cref="AttributeStructureDto"/> по атрибуту структуры от типа <see cref="АтрибутКафедры"/>.
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public async Task<List<AttributeStructureDto>> CreateAsync(List<АтрибутКафедры> attributes)
        {
            IEnumerable<Task<AttributeStructureDto>> tasks = attributes.Select(CreateAsync);
            AttributeStructureDto?[] results = await Task.WhenAll(tasks);
            return results.ToList()!;
        }

        /// <summary>
        /// Создает список DTO <see cref="AttributeStructureDto"/> по атрибуту структуры от типа <see cref="АтрибутНаправления"/>.
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public async Task<List<AttributeStructureDto>> CreateAsync(List<АтрибутНаправления> attributes)
        {
            IEnumerable<Task<AttributeStructureDto>> tasks = attributes.Select(CreateAsync);
            AttributeStructureDto?[] results = await Task.WhenAll(tasks);
            return results.ToList()!;
        }

        /// <summary>
        /// Создает список DTO <see cref="AttributeStructureDto"/> по атрибуту структуры от типа <see cref="АтрибутПрофиля"/>.
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public async Task<List<AttributeStructureDto>> CreateAsync(List<АтрибутПрофиля> attributes)
        {
            IEnumerable<Task<AttributeStructureDto>> tasks = attributes.Select(CreateAsync);
            AttributeStructureDto?[] results = await Task.WhenAll(tasks);
            return results.ToList()!;
        }
    }
}

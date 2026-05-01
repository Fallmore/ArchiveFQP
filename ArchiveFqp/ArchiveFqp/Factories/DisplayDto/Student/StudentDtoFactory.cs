using ArchiveFqp.Factories.DisplayDto.Structure;
using ArchiveFqp.Factories.DisplayDto.User;
using ArchiveFqp.Interfaces.ReferenceData;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Student;
using ArchiveFqp.Models.ReferenceData;

namespace ArchiveFqp.Factories.DisplayDto.Student
{

    public class StudentDtoFactory : IDisplayDtoFactory<StudentDisplayDto, Студент>
    {
        private readonly IReferenceDataService _refDataService;
        private readonly UserDtoFactory _userDisplayFactory;
        private readonly StructureDtoFactory _structureDisplayFactory;

        private ReferenceDataSnapshot _snapshot = null!;

        private Task _init;

        public StudentDtoFactory(IReferenceDataService refDataService,
            UserDtoFactory? userDisplayFactory = null, StructureDtoFactory? structureDisplayFactory = null)
        {
            _refDataService = refDataService;
            _userDisplayFactory = userDisplayFactory ?? new(_refDataService);
            _structureDisplayFactory = structureDisplayFactory ?? new(_refDataService);
            _init = Task.Run(InitializeLists);
        }

        private async Task InitializeLists()
        {
            _snapshot = await _refDataService.GetAllAsync();
        }

        public async Task<StudentDisplayDto> CreateDisplayDtoAsync(Студент student)
        {
            _init.Wait();

            return new()
            {
                Пользователь = await _userDisplayFactory.CreateDisplayDtoAsync(student.IdПользователя) ?? new(),
                УровеньОбразования = _snapshot.EducationLevels.FirstOrDefault(o => o.IdУровняОбразования == student.IdУровняОбразования)?.Название ?? "",
                ФормаОбучения = _snapshot.EducationForms.FirstOrDefault(o => o.IdФормыОбучения == student.IdФормыОбучения)?.Название ?? "",
                ГодОкончания = student.ГодОкончания,
                Структура = student.IdПрофиля.HasValue
                    ? await _structureDisplayFactory.CreateDisplayDtoAsync<Профиль>(student.IdПрофиля.Value) ?? new()
                    : await _structureDisplayFactory.CreateDisplayDtoAsync<Направление>(student.IdНаправления) ?? new()
            };
        }

        public async Task<StudentDisplayDto?> CreateDisplayDtoAsync(int id)
        {
            _init.Wait();
            Студент? student = _snapshot.Students.FirstOrDefault(o => o.IdСтудента == id);
            if (student == null) return null;

            return await CreateDisplayDtoAsync(student);
        }

        public async Task<List<StudentDisplayDto>> CreateDisplayDtoListAsync(IEnumerable<Студент> students)
        {
            IEnumerable<Task<StudentDisplayDto>> tasks = students.Select(CreateDisplayDtoAsync);
            StudentDisplayDto[] results = await Task.WhenAll(tasks);
            return results.ToList();
        }
    }
}

using ArchiveFqp.Factories.DisplayDto.Structure;
using ArchiveFqp.Factories.DisplayDto.User;
using ArchiveFqp.Interfaces.ReferenceData;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Student;
using ArchiveFqp.Models.DTO.User;
using ArchiveFqp.Models.ReferenceData;

namespace ArchiveFqp.Factories.DisplayDto.Student
{

    public class StudentDtoFactory : IDisplayDtoFactory<StudentDisplayDto, Студент>
    {
        private readonly IReferenceDataService _refDataService;
        private readonly StructureDtoFactory _structureDisplayFactory;

        private ReferenceDataSnapshot _snapshot = null!;

        private Task _init;

        public StudentDtoFactory(IReferenceDataService refDataService, StructureDtoFactory? structureDisplayFactory = null)
        {
            _refDataService = refDataService;
            _structureDisplayFactory = structureDisplayFactory ?? new(_refDataService);
            _init = Task.Run(InitializeLists);
        }

        private async Task InitializeLists()
        {
            _snapshot = await _refDataService.GetSnapshotAsync();
        }

        public async Task<StudentDisplayDto> CreateDisplayDtoAsync(Студент student)
        {
            _init.Wait();

            return new()
            {
                IdСтудента = student.IdСтудента,
                Пользователь = (await _refDataService.GetAsync<UserDisplayDto>()).First(x => x.Пользователь.IdПользователя == student.IdПользователя) ?? new(),
                УровеньОбразования = _snapshot.EducationLevels.FirstOrDefault(o => o.IdУровняОбразования == student.IdУровняОбразования) ?? new(),
                ФормаОбучения = _snapshot.EducationForms.FirstOrDefault(o => o.IdФормыОбучения == student.IdФормыОбучения) ?? new(),
                ГодОкончания = student.ГодОкончания,
                Активно = student.Активно,
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

        public async Task<List<StudentDisplayDto>> CreateDisplayDtoAsync(IEnumerable<Студент> students)
        {
            IEnumerable<Task<StudentDisplayDto>> tasks = students.Select(CreateDisplayDtoAsync);
            StudentDisplayDto[] results = await Task.WhenAll(tasks);
            return results.ToList();
        }
    }
}

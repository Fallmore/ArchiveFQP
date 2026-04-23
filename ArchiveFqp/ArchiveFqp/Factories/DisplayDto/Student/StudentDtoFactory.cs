using ArchiveFqp.Factories.DisplayDto;
using ArchiveFqp.Factories.DisplayDto.Structure;
using ArchiveFqp.Factories.DisplayDto.User;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Student;
using ArchiveFqp.Services.ReferenceData;
using Microsoft.EntityFrameworkCore;

namespace ArchiveFqp.Factories.DisplayDto.Student
{

    public class StudentDtoFactory : IDisplayDtoFactory<StudentDisplayDto, Студент>
    {
        private readonly IReferenceDataService _refDataService;
        private readonly UserDtoFactory _userDisplayFactory;
        private readonly StructureDtoFactory _structureDisplayFactory;

        private List<Студент> _students = [];
        private List<УровеньОбразования> _levelEducations = [];
        private List<ФормаОбучения> _formEducations = [];

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
            _levelEducations = await _refDataService.GetAsync<УровеньОбразования>();
            _formEducations = await _refDataService.GetAsync<ФормаОбучения>();
        }

        public async Task<StudentDisplayDto> CreateDisplayDtoAsync(Студент student)
        {
            _init.Wait();

            return new()
            {
                Пользователь = await _userDisplayFactory.CreateDisplayDtoAsync(student.IdПользователя) ?? new(),
                УровеньОбразования = _levelEducations.FirstOrDefault(o => o.IdУровняОбразования == student.IdУровняОбразования)?.Название ?? "",
                ФормаОбучения = _formEducations.FirstOrDefault(o => o.IdФормыОбучения == student.IdФормыОбучения)?.Название ?? "",
                ГодОкончания = student.ГодОкончания,
                Структура = student.IdПрофиля.HasValue
                    ? await _structureDisplayFactory.CreateDisplayDtoAsync<Профиль>(student.IdПрофиля.Value) ?? new()
                    : await _structureDisplayFactory.CreateDisplayDtoAsync<Направление>(student.IdНаправления) ?? new()
            };
        }

        public async Task<StudentDisplayDto?> CreateDisplayDtoAsync(int id)
        {
            _init.Wait();
            if (_students.Count == 0) _students = await _refDataService.GetAsync<Студент>();
            Студент? student = _students.FirstOrDefault(o => o.IdСтудента == id);
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

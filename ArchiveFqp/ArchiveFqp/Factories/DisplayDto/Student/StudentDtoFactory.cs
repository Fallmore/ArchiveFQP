using ArchiveFqp.Factories.DisplayDto;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Student;
using ArchiveFqp.Services.ReferenceData;
using Microsoft.EntityFrameworkCore;

namespace ArchiveFqp.Factories.DisplayDto.Student
{

    public class StudentDtoFactory : IDisplayDtoFactory<StudentDisplayDto, Студент>
    {
        private readonly IReferenceDataService _refDataService;

        private List<Пользователь> _users = [];
        private List<Студент> _students = [];
        private List<Институт> _institutes = [];
        private List<Кафедра> _departments = [];
        private List<Угсн> _ugsns = [];
        private List<УгснСтандарт> _ugsnsStandarts = [];
        private List<Направление> _directions = [];
        private List<Профиль> _profiles = [];
        private List<УровеньОбразования> _levelEducations = [];
        private List<ФормаОбучения> _formEducations = [];

        private Task _init;

        public StudentDtoFactory(IReferenceDataService refDataService)
        {
            _refDataService = refDataService;
            _init = Task.Run(InitializeLists);
        }

        private async Task InitializeLists()
        {
            _users = await _refDataService.GetAsync<Пользователь>();
            _institutes = await _refDataService.GetAsync<Институт>();
            _departments = await _refDataService.GetAsync<Кафедра>();
            _ugsns = await _refDataService.GetAsync<Угсн>();
            _ugsnsStandarts = await _refDataService.GetAsync<УгснСтандарт>();
            _directions = await _refDataService.GetAsync<Направление>();
            _profiles = await _refDataService.GetAsync<Профиль>();
            _levelEducations = await _refDataService.GetAsync<УровеньОбразования>();
            _formEducations = await _refDataService.GetAsync<ФормаОбучения>();
        }

        public async Task<StudentDisplayDto> CreateDisplayDtoAsync(Студент student)
        {
            _init.Wait();
            Кафедра кафедра = _departments.First(o => o.IdИнститута == student.IdИнститута);
            Угсн угсн = _ugsns.First(o => o.IdУгсн == кафедра.IdУгсн);

            return new()
            {
                Пользователь = _users.FirstOrDefault(o => o.IdПользователя == student.IdПользователя) ?? new(),
                Институт = _institutes.FirstOrDefault(o => o.IdИнститута == student.IdИнститута)?.Название ?? "",
                Кафедра = кафедра.Название,
                Угсн = угсн.Название,
                УгснСтандарт = _ugsnsStandarts.FirstOrDefault(o => o.IdУгснСтандарта == угсн.IdУгснСтандарта)?.Название ?? "",
                Направление = _directions.FirstOrDefault(o => o.IdНаправления == student.IdНаправления)?.Название ?? "",
                Профиль = _profiles.FirstOrDefault(o => o.IdПрофиля == student.IdПрофиля)?.Название ?? "",
                УровеньОбразования = _levelEducations.FirstOrDefault(o => o.IdУровняОбразования == student.IdУровняОбразования)?.Название ?? "",
                ФормаОбучения = _formEducations.FirstOrDefault(o => o.IdФормыОбучения == student.IdФормыОбучения)?.Название ?? "",
                ГодОкончания = student.ГодОкончания
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

using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.Teacher;
using ArchiveFqp.Models.DTO.User;
using ArchiveFqp.Services.ReferenceData;
using Microsoft.EntityFrameworkCore;

namespace ArchiveFqp.Factories.DisplayDto.User
{
    public class UserDtoFactory : IDisplayDtoFactory<UserDisplayDto, Пользователь>
    {
        private readonly IReferenceDataService _refDataService;

        private List<Пользователь> _users = [];
        private List<РольПользователя> _roles = [];
        private List<АккаунтПользователя> _accounts = [];

        private Task _init;

        public UserDtoFactory(IReferenceDataService refDataService)
        {
            _refDataService = refDataService;
            _init = Task.Run(InitializeLists);
        }

        private async Task InitializeLists()
        {
            _roles = await _refDataService.GetAsync<РольПользователя>();
        }

        public async Task<UserDisplayDto> CreateDisplayDtoAsync(Пользователь user)
        {
            _init.Wait();
            List<string>? roles;
            if (_accounts.Count == 0)
            {
                roles = (await _refDataService.GetAsync<АккаунтПользователя>())
                    .Where(o => o.IdПользователя == user.IdПользователя)
                    .Select(o => o.Роли.Select(x => _roles.Find(y => y.IdРоли == x)!.Название).ToList())
                    .ToList().FirstOrDefault();
            }
            else
            {
                var f = _accounts
                    .Where(o => o.IdПользователя == user.IdПользователя);
                var d = f.Select(o => o.Роли).ToList();
                var s = d.Select(o => o.Select(x => _roles.Find(y => y.IdРоли == x)!.Название).ToList());
                roles = _accounts
                    .Where(o => o.IdПользователя == user.IdПользователя)
                    .Select(o => o.Роли.Select(x => _roles.Find(y => y.IdРоли == x)!.Название).ToList())
                    .ToList().FirstOrDefault();
            }
            return new()
            {
                Пользователь = user,
                Роли = roles ?? []
            };
        }

        public async Task<UserDisplayDto?> CreateDisplayDtoAsync(int id)
        {
            _init.Wait();
            if (_users.Count == 0) _users = await _refDataService.GetAsync<Пользователь>();
            Пользователь? user = _users.FirstOrDefault(o => o.IdПользователя == id);
            if (user == null) return null;

            return await CreateDisplayDtoAsync(user);
        }

        public async Task<List<UserDisplayDto>> CreateDisplayDtoListAsync(IEnumerable<Пользователь> users)
        {
            _accounts = await _refDataService.GetAsync<АккаунтПользователя>();
            IEnumerable<Task<UserDisplayDto>> tasks = users.Select(CreateDisplayDtoAsync);
            UserDisplayDto[] results = await Task.WhenAll(tasks);
            _accounts = [];
            return [.. results];
        }
    }
}

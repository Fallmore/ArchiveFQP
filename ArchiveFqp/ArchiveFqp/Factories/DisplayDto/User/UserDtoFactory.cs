using ArchiveFqp.Interfaces.ReferenceData;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.DTO.User;
using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;

namespace ArchiveFqp.Factories.DisplayDto.User
{
    public class UserDtoFactory : IDisplayDtoFactory<UserDisplayDto, Пользователь>
    {
        private readonly IReferenceDataService _refDataService;
        private readonly IDbContextFactory<ArchiveFqpContext> _dbFactory;

        private List<Пользователь> _users = [];
        private List<РольПользователя> _roles = [];
        private List<АккаунтПользователя> _accounts = [];

        private Task _init;

        public UserDtoFactory(IReferenceDataService refDataService, IDbContextFactory<ArchiveFqpContext> dbFactory)
        {
            _refDataService = refDataService;
            _dbFactory = dbFactory;
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
                using ArchiveFqpContext context = _dbFactory.CreateDbContext();
                roles = context.АккаунтПользователяs.AsNoTracking()
                    .Where(o => o.IdПользователя == user.IdПользователя)
                    .Select(o => o.Роли.Select(x => _roles.Find(y => y.IdРоли == x)!.Название).ToList())
                    .ToList().FirstOrDefault();
            }
            else
            {
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

        public async Task<List<UserDisplayDto>> CreateDisplayDtoAsync(IEnumerable<Пользователь> users)
        {
            using ArchiveFqpContext context = _dbFactory.CreateDbContext();
            _accounts = await context.АккаунтПользователяs.AsNoTracking().ToListAsync();
            IEnumerable<Task<UserDisplayDto>> tasks = users.Select(CreateDisplayDtoAsync);
            UserDisplayDto[] results = await Task.WhenAll(tasks);
            _accounts = [];
            return [.. results];
        }
    }
}

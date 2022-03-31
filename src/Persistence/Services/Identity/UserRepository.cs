using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Persistence.Contexts;

namespace Persistence.Services.Identity
{
    public class UserRepository : GenericRepository<User>
    {
        public UserRepository(DefaultContext context, ILoggerFactory logger, IConfiguration configuration) : base(context, logger, configuration)
        {
        }

        private async Task Validate(params User[] entities)
        {
            var news = entities.Where(m => m.Id == 0).ToArray();
            if (news.Any())
            {
                var emails = news.Select(m => m.NormalizedEmail).ToArray();
                var invalidEmail = await _dbContext.Set<User>().Select(m => new { m.Id, m.NormalizedEmail }).FirstOrDefaultAsync(m => emails.Contains(m.NormalizedEmail));
                if (invalidEmail != null)
                    throw new OperationCanceledException($"The email '{invalidEmail.NormalizedEmail}' is already in use.");
            }

            var olds = entities.Where(m => m.Id != 0).ToArray();
            if (olds.Any())
            {
                var users = await _dbContext.Set<User>().Select(m => new { m.Id, m.NormalizedEmail }).ToArrayAsync();

                var joint = from item in users
                    join old in olds on item.NormalizedEmail equals old.NormalizedEmail
                    where item.Id != old.Id
                    select item;

                var jointIds = joint.Select(m => m.Id).ToArray();
                var invalidId = await _dbContext.Set<User>().Select(m => new { m.Id, m.NormalizedEmail }).FirstOrDefaultAsync(m => jointIds.Contains(m.Id));
                if (invalidId != null)
                    throw new OperationCanceledException($"The email '{invalidId.NormalizedEmail}' is already in use.");
            }
        }

        public override async Task CreateAsync(params User[] entities)
        {
            await Validate(entities);
            await base.CreateAsync(entities);
        }

        public override async Task UpdateAsync(params User[] entities)
        {
            await Validate(entities);
            await base.UpdateAsync(entities);
        }

        public override async Task DeleteAsync(params object[] keys)
        {
            if ((await _dbContext.Set<User>().CountAsync() == 1))
                throw new InvalidOperationException("Cannot delete all users");

            if (keys.Contains(1))
                throw new InvalidOperationException("NCannot delete the first user");

            await base.DeleteAsync(keys);
        }
    }
}

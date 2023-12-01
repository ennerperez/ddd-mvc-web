using System;
using System.Linq;
using System.Threading;
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

        private async Task Validate(User[] entities, CancellationToken cancellationToken = default)
        {
            var news = entities.Where(m => m.Id == 0).ToArray();
            if (news.Any())
            {
                var emails = news.Select(m => m.NormalizedEmail).ToArray();
                var invalidEmail = await _dbContext.Set<User>().Select(m => new { m.Id, m.NormalizedEmail }).FirstOrDefaultAsync(m => emails.Contains(m.NormalizedEmail), cancellationToken);
                if (invalidEmail != null)
                {
                    throw new OperationCanceledException($"The email '{invalidEmail.NormalizedEmail}' is already in use.");
                }
            }

            var olds = entities.Where(m => m.Id != 0).ToArray();
            if (olds.Any())
            {
                var users = await _dbContext.Set<User>().Select(m => new { m.Id, m.NormalizedEmail }).ToArrayAsync(cancellationToken);

                var joint = from item in users
                            join old in olds on item.NormalizedEmail equals old.NormalizedEmail
                            where item.Id != old.Id
                            select item;

                var jointIds = joint.Select(m => m.Id).ToArray();
                var invalidId = await _dbContext.Set<User>().Select(m => new { m.Id, m.NormalizedEmail }).FirstOrDefaultAsync(m => jointIds.Contains(m.Id), cancellationToken);
                if (invalidId != null)
                {
                    throw new OperationCanceledException($"The email '{invalidId.NormalizedEmail}' is already in use.");
                }
            }
        }

        public override async Task CreateAsync(User[] entities, CancellationToken cancellationToken = default)
        {
            await Validate(entities, cancellationToken);
            await base.CreateAsync(entities, cancellationToken);
        }

        public override async Task UpdateAsync(User[] entities, CancellationToken cancellationToken = default)
        {
            await Validate(entities, cancellationToken);
            await base.UpdateAsync(entities, cancellationToken);
        }

        public override async Task DeleteAsync<T>(T[] keys, CancellationToken cancellationToken = default)
        {
            if ((await _dbContext.Set<User>().CountAsync(cancellationToken) == 1))
            {
                throw new InvalidOperationException("Cannot delete all users");
            }

            // if (keys.Cast<int>().Contains(1))
            // {
            //     throw new InvalidOperationException("Cannot delete the first user");
            // }

            await base.DeleteAsync(keys, cancellationToken);
        }
    }
}

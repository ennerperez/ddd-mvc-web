using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Business.Requests;
using Domain.Entities;
using MediatR;
using Tests.IntegrationTests.Interfaces;
#if USING_REQNROLL
using Reqnroll;

#else
using Test.Framework.Extended;
#endif

namespace Tests.IntegrationTests.Services
{
    public class ClientTestService : GenericTestService<Client>, ITestService<Client>
    {
        private const string Type = nameof(Client);

        public ClientTestService(ISender mediator) : base(mediator)
        {
        }

        public override async Task CreateAsync(Table table) => await ExecuteAsync<CreateClientRequest>(Type, table);

        public override Task<Client> ReadAsync(Table table)
            => throw new NotImplementedException();

        public override async Task UpdateAsync(Table table)
        {
            var customAction = new Action<UpdateClientRequest>(entity =>
            {
                if (entity.Id != 0)
                {
                    return;
                }

                var idField = table.GetValue<string>("Id");
                var lstRegEx = new Regex("\\{Last\\:(.*)\\}", RegexOptions.Compiled);
                var lstMatch = lstRegEx.Match(idField);
                if (!lstMatch.Success)
                {
                    return;
                }

                var prop = lstMatch.Groups[1].Value;
                var propValue = AutomationContext.GetAttribute($"{this.ScenarioCode}_{Type}_{prop}".ToLower(), false);
                entity.Id = int.Parse(propValue.ToString() ?? string.Empty);
            });
            await ExecuteAsync(Type, table, customAction);
        }

        public override async Task PartialUpdateAsync(Table table)
        {
            var customAction = new Action<PartialUpdateClientRequest>(entity =>
            {
                if (entity.Id != 0)
                {
                    return;
                }

                var idField = table.GetValue<string>("Id");
                var lstRegEx = new Regex("\\{Last\\:(.*)\\}", RegexOptions.Compiled);
                var lstMatch = lstRegEx.Match(idField);
                if (!lstMatch.Success)
                {
                    return;
                }

                var prop = lstMatch.Groups[1].Value;
                var propValue = AutomationContext.GetAttribute($"{this.ScenarioCode}_{Type}_{prop}".ToLower(), false);
                entity.Id = int.Parse(propValue.ToString() ?? string.Empty);
            });
            await ExecuteAsync(Type, table, customAction);
        }

        public override async Task DeleteAsync(Table table)
        {
            var customAction = new Action<DeleteClientRequest>(entity =>
            {
                if (entity.Id != 0)
                {
                    return;
                }

                var idField = table.GetValue<string>("Id");
                var lstRegEx = new Regex("\\{Last\\:(.*)\\}", RegexOptions.Compiled);
                var lstMatch = lstRegEx.Match(idField);
                if (!lstMatch.Success)
                {
                    return;
                }

                var prop = lstMatch.Groups[1].Value;
                var propValue = AutomationContext.GetAttribute($"{this.ScenarioCode}_{Type}_{prop}".ToLower(), false);
                entity.Id = int.Parse(propValue.ToString() ?? string.Empty);
            });
            await ExecuteAsync(Type, table, customAction);
        }
    }
}

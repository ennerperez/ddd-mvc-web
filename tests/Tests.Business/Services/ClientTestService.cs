using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Business.Requests;
using Domain.Entities;
using MediatR;
#if USING_SPECFLOW
using TechTalk.SpecFlow;
#else
using Test.Framework.Extended;

#endif
using Tests.Business.Interfaces;

namespace Tests.Business.Services
{
    public class ClientTestService : GenericTestService<Client>, ITestService<Client>
    {

        private const string Type = nameof(Client);

        public ClientTestService(ISender mediator) : base(mediator)
        {
        }
        public override async Task CreateAsync(Table table)
        {
            await ExecuteAsync<CreateClientRequest>(Type, table);
        }

        public override Task<Client> ReadAsync(Table table)
            => throw new NotImplementedException();
        public override async Task UpdateAsync(Table table)
        {
            var customAction = new Action<UpdateClientRequest>(entity =>
            {
                if (entity.Id == 0)
                {
                    var idField = table.GetValue<string>("Id");
                    var lstRegEx = new Regex("\\{Last\\:(.*)\\}", RegexOptions.Compiled);
                    var lstMatch = lstRegEx.Match(idField);
                    if (lstMatch.Success)
                    {
                        var prop = lstMatch.Groups[1].Value;
                        var propValue = _automationContext.GetAttribute($"{ScenarioCode}_{Type}_{prop}".ToLower(), throwException: false);
                        entity.Id = int.Parse(propValue.ToString() ?? string.Empty);
                    }
                }
            });
            await ExecuteAsync(Type, table, customProps: customAction);
        }

        public override async Task PartialUpdateAsync(Table table)
        {
            var customAction = new Action<PartialUpdateClientRequest>(entity =>
            {
                if (entity.Id == 0)
                {
                    var idField = table.GetValue<string>("Id");
                    var lstRegEx = new Regex("\\{Last\\:(.*)\\}", RegexOptions.Compiled);
                    var lstMatch = lstRegEx.Match(idField);
                    if (lstMatch.Success)
                    {
                        var prop = lstMatch.Groups[1].Value;
                        var propValue = _automationContext.GetAttribute($"{ScenarioCode}_{Type}_{prop}".ToLower(), throwException: false);
                        entity.Id = int.Parse(propValue.ToString() ?? string.Empty);
                    }
                }
            });
            await ExecuteAsync(Type, table, customProps: customAction);
        }
        public override async Task DeleteAsync(Table table)
        {
            var customAction = new Action<DeleteClientRequest>(entity =>
            {
                if (entity.Id == 0)
                {
                    var idField = table.GetValue<string>("Id");
                    var lstRegEx = new Regex("\\{Last\\:(.*)\\}", RegexOptions.Compiled);
                    var lstMatch = lstRegEx.Match(idField);
                    if (lstMatch.Success)
                    {
                        var prop = lstMatch.Groups[1].Value;
                        var propValue = _automationContext.GetAttribute($"{ScenarioCode}_{Type}_{prop}".ToLower(), throwException: false);
                        entity.Id = int.Parse(propValue.ToString() ?? string.Empty);
                    }
                }
            });
            await ExecuteAsync(Type, table, customProps: customAction);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Tests.Abstractions.Interfaces;
using Tests.Business.Interfaces;
#if USING_SPECFLOW
using TechTalk.SpecFlow;

#else
using Test.Framework.Extended;
#endif

namespace Tests.Business.Services
{
    public abstract class GenericTestService<TEntity> : ITestService<TEntity>
    {
        public IAutomationContext AutomationContext
        {
            get => _automationContext;
            set => _automationContext = value;
        }

        protected IAutomationContext _automationContext;
        protected readonly ISender Mediator;

        protected GenericTestService(ISender mediator)
        {
            Mediator = mediator;
        }

#if USING_SPECFLOW
        protected string ScenarioCode => _automationContext.ScenarioContext.ScenarioInfo.GetHashCode().ToString();
#else
        private string _scenarioCode = Guid.NewGuid().ToString("N");
        protected string ScenarioCode => _scenarioCode;
#endif

        public abstract Task CreateAsync(Table table);
        public abstract Task<TEntity> ReadAsync(Table table);
        public abstract Task UpdateAsync(Table table);
        public abstract Task PartialUpdateAsync(Table table);
        public abstract Task DeleteAsync(Table table);

        protected async Task ExecuteAsync<T>(string type, Table table, Action<T> customProps = null)
        {
            var entities = new List<T>();
            var entity = table.CastTo<T>();
            customProps?.Invoke(entity);

            try
            {
                var response = await Mediator.Send(entity);
                var ex = new InvalidOperationException("Unable to complete the operation");
                // if (response != null && (response is int) && ((int)response) == 0) throw ex;
                // else if (response != null && (response is long) && ((long)response) == 0) throw ex;
                // else if (response != null && (response is decimal) && ((decimal)response) == 0) throw ex;
                // else if (response != null && (response is Guid) && ((Guid)response) == Guid.Empty) throw ex;
                // else if (response != null && (response is DateTime) && ((DateTime)response) == DateTime.MinValue) throw ex;
                // else if (response != null && (response is string) && ((string)response) == string.Empty) throw ex;
                // else if (response == null) throw ex;
                if (response == null)
                {
                    throw ex;
                }

                if (response.GetType() != typeof(Unit))
                {
                    _automationContext.SetAttribute($"{ScenarioCode}_{type}_id".ToLower(), response);
                }
            }
            catch (NotImplementedException)
            {
                throw;
            }
            catch (Exception e)
            {
                _automationContext.AddException(e);
            }

            entities.Add(entity);

            if (entities.Count >= 1)
            {
                _automationContext.SetAttribute($"{ScenarioCode}_{type}".ToLower(), entities);
            }
        }
    }
}

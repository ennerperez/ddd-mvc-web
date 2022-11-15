using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using TechTalk.SpecFlow;
using Tests.Abstractions.Interfaces;
using Tests.Business.Interfaces;

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

		public GenericTestService(ISender mediator)
		{
			Mediator = mediator;
		}

		protected string _scenarioCode => _automationContext.ScenarioContext.ScenarioInfo.GetHashCode().ToString();

		public abstract Task CreateAsync(Table table);
		public abstract Task<TEntity> ReadAsync(Table table);
		public abstract Task UpdateAsync(Table table);
		public abstract Task PartialUpdateAsync(Table table);
		public abstract Task DeleteAsync(Table table);
		protected async Task ExecuteAsync<T>(string type, Table table, Action<T> customProps = null)
		{
			var entities = new List<T>();
			var entity = table.CastTo<T>();
			if (customProps != null) customProps.Invoke(entity);

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
				if (response == null) throw ex;
				if (response.GetType() != typeof(Unit))
					_automationContext.SetAttributeInAttributeLibrary($"{_scenarioCode}_{type}_id".ToLower(), response);
			}
			catch (NotImplementedException)
			{
				throw;
			}
			catch (Exception e)
			{
				_automationContext.Exceptions.Add(e);
			}

			entities.Add(entity);

			if (entities.Count >= 1)
			{
				_automationContext.SetAttributeInAttributeLibrary($"{_scenarioCode}_{type}".ToLower(), entities);
			}
		}
	}
}

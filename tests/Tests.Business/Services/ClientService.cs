using System.Threading.Tasks;
using Business.Requests;
using MediatR;
using TechTalk.SpecFlow;
using Tests.Business.Interfaces;

namespace Tests.Business.Services
{
	public class ClientService : ITestService
	{

		private readonly ISender _mediator;
		public ClientService(ISender mediator)
		{
			_mediator = mediator;
		}
		public async Task CreateAsync(Table table)
		{
			var request = new CreateClientRequest();
			request = table.CastTo<CreateClientRequest>();
			await _mediator.Send(request);
		}
		public async Task UpdateAsync(Table table)
		{
			var request = new UpdateClientRequest();
			request = table.CastTo<UpdateClientRequest>();
			await _mediator.Send(request);
		}
		public async Task DeleteAsync(Table table)
		{
			var request = new DeleteClientRequest();
			request = table.CastTo<DeleteClientRequest>();
			await _mediator.Send(request);
		}
	}
}

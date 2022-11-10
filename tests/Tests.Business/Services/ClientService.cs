using System.Threading.Tasks;
using Business.Requests;
using Domain.Entities;
using MediatR;
using TechTalk.SpecFlow;
using Tests.Business.Interfaces;

namespace Tests.Business.Services
{
	public class ClientService : GenericTestService, ITestService
	{

		//private string type = nameof(Client);

		private readonly ISender _mediator;
		public ClientService(ISender mediator) : base(mediator)
		{
			_mediator = mediator;
		}
		public override async Task CreateAsync(Table table)
		{
			var request = new CreateClientRequest();
			request = table.CastTo<CreateClientRequest>();
			await _mediator.Send(request);
		}
		public override async Task UpdateAsync(Table table)
		{
			var request = new UpdateClientRequest();
			request = table.CastTo<UpdateClientRequest>();
			await _mediator.Send(request);
		}

		public override async Task PartialUpdateAsync(Table table)
		{
			var request = new PartialUpdateClientRequest();
			request = table.CastTo<PartialUpdateClientRequest>();
			await _mediator.Send(request);
		}
		public override async Task DeleteAsync(Table table)
		{
			var request = new DeleteClientRequest();
			request = table.CastTo<DeleteClientRequest>();
			await _mediator.Send(request);
		}
	}
}

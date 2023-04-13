using System;
using System.Threading.Tasks;
using Domain;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Areas.Admin.Models;
using Web.Controllers;

namespace Web.Areas.Admin.Controllers
{

	[Authorize(Roles = Roles.Admin)]
	[Area("Admin")]
	public class DashboardController : MvcControllerBase
	{
		public async Task<ActionResult> Index()
		{
			var clients = await Repository<Client>().CountAsync();
			var budgets = await Repository<Budget, Guid>().CountAsync();

			// var clients = await Mediator.SendWithRepository<Client>(s => new Client() {Id = s.Id});
			// var budgets = await Mediator.SendWithRepository<Budget, Guid>(s => new Budget() {Id = s.Id});

			var clientCounter = new DashboardCounterViewModel() {DisplayName = "Clients", Counter = clients};
			var budgetCounter = new DashboardCounterViewModel() {DisplayName = "Budgets", Counter = budgets};

			var model = new DashboardViewModel() {Counters = new[] {clientCounter, budgetCounter}};

			return View(model);
		}


	}
}

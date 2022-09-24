﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Areas.Admin.Controllers
{
	[Authorize(Roles = "Admin")]
	[Area("Admin")]
	public class BudgetsController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}

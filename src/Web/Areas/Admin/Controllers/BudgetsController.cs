using System;
using System.Threading.Tasks;
using Domain.Entities;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Interfaces;
using Web.Areas.Admin.Reports;

namespace Web.Areas.Admin.Controllers
{
	[Authorize(Roles = "Admin")]
	[Area("Admin")]
	public class BudgetsController : Controller
	{
		private readonly IDocumentService<IDocument> _documentService;
		private readonly IGenericRepository<Budget, Guid> _budgetRepository;

		public BudgetsController(IDocumentService<IDocument> documentService, IGenericRepository<Budget, Guid> budgetRepository)
		{
			_documentService = documentService;
			_budgetRepository = budgetRepository;
		}
		public IActionResult Index()
		{
			return View();
		}

		public async Task<IActionResult> Export(Guid? id = null, string format = "pdf", bool download = true)
		{
			object model;
			string title;
			if (id != null)
			{
				model = await _budgetRepository.FirstOrDefaultAsync(s => new
				{
					s.Code,
					Client = s.Client.FullName,
					s.Status,
					s.Subtotal,
					s.Taxes,
					s.Total,
					s.CreatedAt,
					s.ExpireAt
				}, m => m.Id == id);
				title = $"Budget: {id}";
			}
			else
			{
				model = await _budgetRepository.ReadAsync(s => new
				{
					s.Code,
					Client = s.Client.FullName,
					s.Status,
					s.Subtotal,
					s.Taxes,
					s.Total,
					s.CreatedAt,
					s.ExpireAt
				});
				title = "Budgets";
			}

			var definition = _documentService.Compose<BudgetDocument>(model);
			definition.Title = title;
			definition.FileName = $"{definition.Title}.{format}";
			var report = _documentService.Generate(definition, format);

			var contentType = string.Empty;
			switch (format)
			{
				case "pdf":
					contentType = System.Net.Mime.MediaTypeNames.Application.Pdf;
					break;
			}

			if (download)
				return File(report, contentType, definition.FileName);
			else
				return File(report, contentType);
		}
	}
}

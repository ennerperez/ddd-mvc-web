using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Domain;
using Domain.Entities;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Interfaces;
using Web.Areas.Admin.Reports;
using Web.Controllers;

namespace Web.Areas.Admin.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    [Area("Admin")]
    public class BudgetsController : MvcControllerBase
    {
        private readonly IGenericRepository<Budget, Guid> _budgetRepository;
        private readonly IDocumentService _documentService;

        public BudgetsController(IDocumentService documentService, IGenericRepository<Budget, Guid> budgetRepository)
        {
            _documentService = documentService;
            _budgetRepository = budgetRepository;
        }

        public IActionResult Index() => View();

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
                    contentType = MediaTypeNames.Application.Pdf;
                    break;
            }

            if (download)
            {
                return File(report, contentType, definition.FileName);
            }

            return File(report, contentType);
        }
    }
}

using System;
using System.Collections;
using System.Threading.Tasks;
using Domain.Entities;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Interfaces;
using IDocument = Infrastructure.Interfaces.IDocument;
#if USING_QUESTPDF
using QuestPDF.Drawing;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
#endif

namespace Web.Areas.Admin.Controllers
{
	public class BudgetDocument
#if USING_QUESTPDF
		: IPdfDocument
#else
		: IDocument
#endif
	{
		public string Title { get; set; }
		public string FileName { get; set; }
		public object Model { get; private set; }

		public BudgetDocument(object model)
		{
			Model = model;
		}

#if USING_QUESTPDF
		public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
		public void Compose(IDocumentContainer container)
		{
			container
				.Page(page =>
				{
					page.Margin(25);
					page.Size(PageSizes.Letter.Landscape());

					page.Header().Element(ComposeHeader);
					page.Content().Element(ComposeContent);
				});
		}

		private void ComposeHeader(IContainer container)
		{
			container.Column(column =>
			{
				column.Item().Row(row =>
				{
					row.RelativeItem().Text(text =>
					{
						text.Span(" ");
					});
					row.RelativeItem().AlignCenter().Text(Title)
						.FontSize(20).SemiBold().FontColor(Colors.Green.Darken2);
				});
			});
		}
		private void ComposeContent(IContainer container)
		{
			if (typeof(IEnumerable).IsAssignableFrom(Model.GetType()))
			{
				var props = Model.GetType().GetGenericArguments()[0].GetProperties();
				var headerStyle = TextStyle.Default.SemiBold();
				float fontSizeHeader = 8;
				uint columnSpan = 13;
				
				container.Table(table =>
				{
					table.ColumnsDefinition(columns =>
					{
						columns.ConstantColumn(25);
						foreach (var unused in props)
							columns.RelativeColumn();
					});
					table.Header(header =>
					{
						header.Cell().Text("#");
						foreach (var prop in props)
							header.Cell().AlignRight().Text(text =>
							{
								text.Span(prop.Name).FontSize(fontSizeHeader).Style(headerStyle);
							});
						header.Cell().ColumnSpan(columnSpan).PaddingTop(5).BorderBottom(1).BorderColor(Colors.Black);
					});

					var index = 1;
					float fontSizeContent = 8;
					static IContainer CellStyle(IContainer container) => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);

					foreach (var item in ((IEnumerable)Model))
					{
						table.Cell().Element(CellStyle).Text(index).FontSize(fontSizeContent);
						foreach (var prop in props)
						{
							var value = prop.GetValue(item)?.ToString();
							table.Cell().Element(CellStyle).AlignRight().Text(text =>
							{
								text.Span(value).FontSize(fontSizeContent);
							});
						}
						index++;
					}
				});

			}
		}
#endif

	}

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

		public async Task<IActionResult> Export(Guid id, string format = "pdf")
		{
			var model = await _budgetRepository.FirstOrDefaultAsync(s=> s, m => m.Id == id);
			var definition = _documentService.Compose<BudgetDocument>(model);
			definition.Title = DateTime.Now.Ticks.ToString();
			definition.FileName = $"{definition.Title}.{format}";
			var report = _documentService.Generate(definition, format);

			var contentType = string.Empty;
			switch (format)
			{
				case "pdf":
					contentType = System.Net.Mime.MediaTypeNames.Application.Pdf;
					break;
			}

			return File(report, contentType, definition.FileName);
		}
	}
}

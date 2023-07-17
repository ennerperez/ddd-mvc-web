using System.Collections;
using Infrastructure.Interfaces;

#if USING_QUESTPDF
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
#endif

namespace Web.Areas.Admin.Reports
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
            if (typeof(IEnumerable).IsAssignableFrom(Model.GetType()))
            {
                container
                    .Page(page => BuildMultipleReport(page));
            }
            else
            {
                container
                    .Page(page => BuildSingleReport(page));
            }
        }

        private readonly TextStyle _titleStyle = TextStyle.Default.FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
        private void BuildSingleReport(PageDescriptor page)
        {
            page.Margin(25);
            page.Size(PageSizes.Letter.Portrait());

            var record = (dynamic)Model;
            page.Header().Element(container =>
            {

                container.Row(row =>
                {
                    row.RelativeItem().Column(column =>
                    {
                        column.Item().Text($"Budget #{record.Code}").Style(_titleStyle);

                        column.Item().Text(text =>
                        {
                            text.Span("Created date: ").SemiBold();
                            text.Span($"{record.CreatedAt:d}");
                        });

                        column.Item().Text(text =>
                        {
                            text.Span("Due date: ").SemiBold();
                            text.Span($"{record.ExpireAt:d}");
                        });
                    });
                    row.ConstantItem(100).Height(50).Placeholder();
                });
            });
            page.Content().Element(container =>
            {
                container.PaddingTop(4)
                    .BorderTop(2).BorderColor(Colors.Black)
                    .Row(row =>
                    {
                        row.RelativeItem().PaddingTop(8).Column(colum =>
                        {
                            colum.Item().Text($"Client: {record.Client}");
                            colum.Item().Text($"Status: {record.Status}");
                        });
                    });
            });
        }
        private void BuildMultipleReport(PageDescriptor page)
        {
            page.Margin(25);
            page.Size(PageSizes.Letter.Landscape());

            page.Header().Element(container =>
                container.Row(row =>
                {
                    row.RelativeItem().PaddingBottom(4).Column(column =>
                    {
                        column.Item().Text("Budgets").Style(_titleStyle);
                    });
                })
            );

            page.Content().Element(container =>
            {
                var props = Model.GetType().GetGenericArguments()[0].GetProperties();
                var headerStyle = TextStyle.Default.SemiBold();
                float fontSizeHeader = 8;

                container.Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(25);
                        foreach (var unused in props)
                        {
                            columns.RelativeColumn();
                        }
                    });
                    table.Header(header =>
                    {
                        header.Cell().Text("#");
                        foreach (var prop in props)
                        {
                            header.Cell().AlignRight().Text(text =>
                            {
                                text.Span(prop.Name).FontSize(fontSizeHeader).Style(headerStyle);
                            });
                        }

                        header.Cell().ColumnSpan((uint)(props.Length + 1)).PaddingTop(5).BorderBottom(1).BorderColor(Colors.Black);
                    });

                    var index = 1;
                    float fontSizeContent = 8;
                    static IContainer CellStyle(IContainer container) => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);

                    foreach (var item in ((IEnumerable)Model))
                    {
                        table.Cell().Element(CellStyle).Text(index.ToString()).FontSize(fontSizeContent);
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
            });
        }

#endif

    }
}

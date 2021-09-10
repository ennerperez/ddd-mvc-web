using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web.TagHelpers
{
    [HtmlTargetElement("alert")]
    public class AlertTagHelper : TagHelper
    {
        [HtmlAttributeName("title")]
        public string Title { get; set; }

        [HtmlAttributeName("content")]
        public string Content { get; set; }

        [HtmlAttributeName("footer")]
        public string Footer { get; set; }

        [HtmlAttributeName("color")]
        public string Color { get; set; }

        [HtmlAttributeName("dismissible")]
        public bool Dismissible { get; set; }

        // public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        // {
        //     await Task.Run(() => Process(context, output));
        // }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (output == null)
                throw new ArgumentNullException(nameof(output));

            if (string.IsNullOrWhiteSpace(Color))
                Color = "primary";

            output.TagName = "div";
            output.Attributes.SetAttribute("class", $"w-100 alert alert-{Color} {(Dismissible ? "alert-dismissible fade show" : "")}");
            output.Attributes.SetAttribute("role", "alert");

            var sb = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(Title))
                sb.Append($"<h4 class='alert-heading'>{Title}</h4>");
            if (!string.IsNullOrWhiteSpace(Content))
                sb.Append(Content);
            if (!string.IsNullOrWhiteSpace(Footer))
                sb.Append("<hr>" + Environment.NewLine + $"<p class='mb-0'>{Footer}</h4>");
            if (Dismissible)
            {
                sb.Append($"<button type='button' class='close' data-dismiss='alert' aria-label='Close'>" + Environment.NewLine
                                                                                                          + "<span aria-hidden='true'>&times;</span>"
                                                                                                          + Environment.NewLine + "</button>");
            }
            var content = await output.GetChildContentAsync();
            output.Content.SetHtmlContent($"{content.GetContent()}{sb.ToString()}");
        }
    }
}

using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Spider_QAMS.Utilities.TagHelperExtended
{
    [HtmlTargetElement("input", Attributes = "asp-for")]
    public class DescriptionTooltipTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-for")]
        public ModelExpression For { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var description = For.Metadata.Description;

            if (!string.IsNullOrEmpty(description))
            {
                output.Attributes.SetAttribute("title", description);
            }
            else
            {
                output.Attributes.SetAttribute("title", "No description available");
            }
        }
    }
}

using System.Text.Json.Serialization;

namespace Spider_QAMS.Models.ViewModels
{
    public class PageSiteVM
    {
        [JsonPropertyName("pageId")]
        public int PageId { get; set; }
        [JsonPropertyName("pageUrl")]
        public string PageUrl { get; set; }
        [JsonPropertyName("pageDesc")]
        public string PageDesc { get; set; }
        [JsonPropertyName("isSelected")]
        public bool IsSelected { get; set; }
    }
}

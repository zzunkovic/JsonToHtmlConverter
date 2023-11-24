using JsonToHtmlConverter.Models.Tags;

namespace JsonToHtmlConverter.Models
{
    public class Html
    {

        public string Doctype { get; set; } = "html";
        public string Language { get; set; } = "en";
        public List<Tag>? Head { get; set; } = null;
        public PairedTag? Body { get; set; } = null;
    }
}

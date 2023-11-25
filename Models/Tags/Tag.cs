namespace JsonToHtmlConverter.Models.Tags
{
    public abstract class Tag
    {
        public string TagName { get; set; } = string.Empty;
        public List<TagAttribute>? Attributes { get; set; } = new List<TagAttribute>();

        public List<Style> Styles = new List<Style>();

    }
}

namespace Arcmage.Layout.InputConvertor
{
    public class LayoutInputConvertor
    {
        public static string ToXml(string markdownLayout)
        {
            return MarkdownRender.MarkdownRender.ToXml(markdownLayout);
        }

        public static string ToMarkdown(string xmlLayout)
        {
            return XmlRender.XmlRender.ToMarkdown(xmlLayout);
        }
    }
}

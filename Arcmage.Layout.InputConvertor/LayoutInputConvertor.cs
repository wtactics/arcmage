namespace Arcmage.Layout.InputConvertor
{
    public class LayoutInputConvertor
    {
        public static string ToXml(string markdownLayout, bool removeRoot)
        {
            return MarkdownRender.MarkdownRender.ToXml(markdownLayout, removeRoot);
        }

        public static string ToMarkdown(string xmlLayout, bool hasXmlRoot)
        {
            return XmlRender.XmlRender.ToMarkdown(xmlLayout, hasXmlRoot);
        }
    }
}

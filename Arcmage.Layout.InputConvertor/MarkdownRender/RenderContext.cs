using Microsoft.Toolkit.Parsers.Markdown.Render;

namespace Arcmage.Layout.InputConvertor.MarkdownRender
{
    class RenderContext: IRenderContext
    {

        public IRenderContext Clone()
        {
            return new RenderContext() { Parent = Parent, SetBoldRun = SetBoldRun, SetItalicRun = SetItalicRun };
        }

        public bool TrimLeadingWhitespace { get; set; }

        public bool SetBoldRun { get; set; }

        public bool SetItalicRun { get; set; }

        public object Parent { get; set; }
    }
}

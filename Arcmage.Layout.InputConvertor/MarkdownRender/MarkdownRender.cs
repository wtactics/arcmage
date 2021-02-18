using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Toolkit.Parsers.Markdown;
using Microsoft.Toolkit.Parsers.Markdown.Blocks;
using Microsoft.Toolkit.Parsers.Markdown.Inlines;
using Microsoft.Toolkit.Parsers.Markdown.Render;

namespace Arcmage.Layout.InputConvertor.MarkdownRender
{
    public class MarkdownRender: MarkdownRendererBase
    {

        static MarkdownRender()
        {
            // Overwrite emoji codes with our own.
            EmojiInline.SetCodesDictionary(new Dictionary<string, int>()
            {
                { "g0", 0 },
                { "g1", 1 },
                { "g2", 2 },
                { "g3", 3 },
                { "g4", 4 },
                { "g5", 5 },
                { "g6", 6 },
                { "g7", 7 },
                { "g8", 8 },
                { "g9", 9 },
                { "gx", 10 },
                { "gt", 11 },

                { "m0", 20 },
                { "m1", 21 },
                { "m2", 22 },
                { "m3", 23 },
                { "m4", 24 },
                { "m5", 25 },
                { "m6", 26 },
                { "m7", 27 },
                { "m8", 28 },
                { "m9", 29 },
                { "mx", 20 },
                { "mt", 21 },
                { "ma", 22 },

                { "m",   30 },
                { "mi",  31 },
                { "mc",  32 },
                { "mic", 33 },
                { "mis", 34 },

                { "br", 50 },
                { "pa", 51 },

                {"A", 65},
                {"B", 66},
                {"C", 67},
                {"D", 68},
                {"E", 69},
                {"F", 70},
                {"G", 71},
                {"H", 72},
                {"I", 73},
                {"J", 74},
                {"K", 75},
                {"L", 76},
                {"M", 77},
                {"N", 78},
                {"O", 79},
                {"P", 80},
                {"Q", 81},
                {"R", 82},
                {"S", 83},
                {"T", 84},
                {"U", 85},
                {"V", 86},
                {"W", 87},
                {"X", 88},
                {"Y", 89},
                {"Z", 90},
            });
        }

        public static string ToXml(string markdownLayout)
        {
            // Normalizing input to newline endings only
            markdownLayout = markdownLayout.Replace("\r\n", "\n");
            
            // Normalize markdown ends.
            if (!markdownLayout.EndsWith("\n\n"))
            {
                markdownLayout = markdownLayout + "\n\n";
            }
        
            // Adding support for a empty paragraphs
            markdownLayout = SupportEmptyParagraphs(markdownLayout);

            // The markdown parser doesn't support spaces at the start or end of bold,
            // italic or bold-italic textrun, nor at the beginning of a line.
            // - We'll replace it by a placeholder before parsing, and change it back afterwards.

            // Adding support for space ends in enclosed bold, italic and bold-italic
            markdownLayout = markdownLayout.Replace(" *", "@@*");
            // Adding support for space starts in enclosed bold, italic and bold-italic
            markdownLayout = markdownLayout.Replace("* ", "*@@");
            // Adding support for a space at start of a line
            markdownLayout = markdownLayout.Replace("\n ", "\n@@");

            // Added support for backslash line breaking
            markdownLayout = markdownLayout.Replace("\\", ":br:");
          
            // Parse the markdown document to structured information
            var document = new MarkdownDocument();
            document.Parse(markdownLayout);

            // Create the output xml document
            var layoutDocument = new XDocument();
            layoutDocument.Add(new XElement("layout") { Value = string.Empty });

            // Create a new layout render
            var layoutRender = new MarkdownRender(document);
            // Setup the render context with the output xml document
            var renderContext = new RenderContext {Parent = layoutDocument};

            // Render the markdown in xml
            layoutRender.Render(renderContext);

            // Pretty print the xml document
            var xml = layoutDocument.ToString();

            // Replace the pre-render space support placeholder with a space
            return xml.Replace("@@", " ");
        }

        private static string SupportEmptyParagraphs(string markdownLayout)
        {
            // To support empty paragraphs at the start, we'll add a dummy empty line
            markdownLayout = "\n" + markdownLayout;

            // Replace all empty paragraph markup, backslash + enter (+ blank line) with empty paragraph emoji code.
            int pos = markdownLayout.IndexOf("\n\\\n\n"); 
            while (pos >= 0)
            {
                markdownLayout = markdownLayout.Substring(0, pos) + "\n:pa:\n\n" + markdownLayout.Substring(pos + "\n\\\n\n".Length);
                pos = markdownLayout.IndexOf("\n\\\n\n");
            }
            return markdownLayout;
        }

        public MarkdownRender(MarkdownDocument document) : base(document)
        {
        }

        protected override void RenderParagraph(ParagraphBlock element, IRenderContext context)
        {
            // Adding new paragraphs to the root of the xml document
            var doc = context.Parent as XDocument;
            var paragraph = new XElement("p");
            doc.Root.Add(paragraph);

            // Render the paragraph's children
            RenderInlineChildren(element.Inlines, new RenderContext(){ Parent = paragraph });
        }

        protected override void RenderYamlHeader(YamlHeaderBlock element, IRenderContext context)
        {
        }

        protected override void RenderHeader(HeaderBlock element, IRenderContext context)
        {
        }

        protected override void RenderListElement(ListBlock element, IRenderContext context)
        {
        }

        protected override void RenderHorizontalRule(IRenderContext context)
        {
        }

        protected override void RenderQuote(QuoteBlock element, IRenderContext context)
        {
        }

        protected override void RenderCode(CodeBlock element, IRenderContext context)
        {
        }

        protected override void RenderTable(TableBlock element, IRenderContext context)
        {
        }

        protected override void RenderEmoji(EmojiInline element, IRenderContext context)
        {
            // Fetch the paragraph 
            var paragraph = context.Parent as XElement;

            // Custom tags support
            if (0 <= element.Code && element.Code < 50)
            {
                paragraph.Add(new XElement(element.Text));
                return;
            }

            // Break line support support
            if (element.Code == 50)
            {
                paragraph.Add(new XElement(element.Text));
                context.TrimLeadingWhitespace = true;
                return;
            }

            // Empty paragraph support
            if (element.Code == 51)
            {
                paragraph.Value = String.Empty;
                return;
            }

            // Drop cap support
            if (65 <= element.Code && element.Code < 91)
            {
                paragraph.Add(new XElement("c", element.Text));
                return;
            }

        }

        protected override void RenderTextRun(TextRunInline element, IRenderContext context)
        {
            // Fetch the paragraph to add the text content to
            var renderContext = context as RenderContext;
            var paragraph = context.Parent as XElement;

            // Fetch the content of the inline element
            var text = element.Text;
            
            // Check if we need to trim leading white space
            if (renderContext.TrimLeadingWhitespace)
            {
                text = text.TrimStart();
                renderContext.TrimLeadingWhitespace = false;
            }

            // No content, nothing to do
            if (string.IsNullOrWhiteSpace(text)) return;

            // Add a bold italic xml tag
            if (renderContext.SetBoldRun && renderContext.SetItalicRun)
            {
                paragraph.Add(new XElement("bi", text));
                return;
            }
            // Add a bold xml tag
            if (renderContext.SetBoldRun)
            {
                paragraph.Add(new XElement("b", text));
                return;
            }
            // Add a italic xml tag
            if (renderContext.SetItalicRun)
            {
                paragraph.Add(new XElement("i", text));
                return;
            }
            // Add a normal xml tag
            paragraph.Add(new XElement("n", text));
        }

       

        protected override void RenderBoldRun(BoldTextInline element, IRenderContext context)
        {
            // We've detected a bold content element, mark it as such
            var renderContext = context.Clone() as RenderContext;
            renderContext.SetBoldRun = true;
            RenderInlineChildren(element.Inlines, renderContext);
        }

        protected override void RenderMarkdownLink(MarkdownLinkInline element, IRenderContext context)
        {
        }

        protected override void RenderImage(ImageInline element, IRenderContext context)
        {
        }

        protected override void RenderHyperlink(HyperlinkInline element, IRenderContext context)
        {
        }

        protected override void RenderItalicRun(ItalicTextInline element, IRenderContext context)
        {
            // We've detected an italic content element, mark it as such
            var renderContext = context.Clone() as RenderContext;
            renderContext.SetItalicRun = true;
            RenderInlineChildren(element.Inlines, renderContext);
        }

        protected override void RenderStrikethroughRun(StrikethroughTextInline element, IRenderContext context)
        {
        }

        protected override void RenderSuperscriptRun(SuperscriptTextInline element, IRenderContext context)
        {
        }

        protected override void RenderSubscriptRun(SubscriptTextInline element, IRenderContext context)
        {
        }

        protected override void RenderCodeRun(CodeInline element, IRenderContext context)
        {
        }
    }
}

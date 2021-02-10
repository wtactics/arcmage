using System;
using System.Collections.Generic;
using System.Linq;
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




        public static string ToXml(string markdownLayout, bool removeRoot = false)
        {
            // Fixing space endings in enclosed bold, italic and bold-italic
            markdownLayout = markdownLayout.Replace(" *", " @@*");
            // Normalizing input to newline endings only
            markdownLayout = markdownLayout.Replace("\r\n", "\n");
            // Added support for backslash line breaking
            markdownLayout = markdownLayout.Replace("\\", ":br:");
            

            var document = new MarkdownDocument();
            document.Parse(markdownLayout);
            var layoutRender = new MarkdownRender(document);
            var renderContext = new RenderContext();
            var layoutDocument = new XDocument();
            layoutDocument.Add(new XElement("layout"));
            renderContext.Parent = layoutDocument;
            layoutRender.Render(renderContext);
            if (removeRoot)
            {
                var paragraphs = layoutDocument.Root.Elements().ToList().Select(x => x.ToString());
                return string.Join(Environment.NewLine, paragraphs).Replace(" @@", " ");
            }
            return layoutDocument.ToString().Replace(" @@", " "); ;
        }

        public MarkdownRender(MarkdownDocument document) : base(document)
        {
        }

        protected override void RenderParagraph(ParagraphBlock element, IRenderContext context)
        {
            var doc = context.Parent as XDocument;
            var paragraph = new XElement("p");
            doc.Root.Add(paragraph);

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
            var paragraph = context.Parent as XElement;
            if (0 <= element.Code && element.Code < 65)
            {
                paragraph.Add(new XElement(element.Text));
                if (element.Code == 50)
                {
                    context.TrimLeadingWhitespace = true;
                }
                return;
            }

            if (65 <= element.Code && element.Code < 91)
            {
                paragraph.Add(new XElement("c", element.Text));
                return;
            }

        }

        protected override void RenderTextRun(TextRunInline element, IRenderContext context)
        {
            var renderContext = context as RenderContext;
            var paragraph = context.Parent as XElement;
            var text = element.Text;
            if (renderContext.TrimLeadingWhitespace)
            {
                text = text.TrimStart();
                renderContext.TrimLeadingWhitespace = false;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            if (renderContext.SetBoldRun && renderContext.SetItalicRun)
            {
                paragraph.Add(new XElement("bi", text));
                return;
            }
            if (renderContext.SetBoldRun)
            {
                paragraph.Add(new XElement("b", text));
                return;
            }
            if (renderContext.SetItalicRun)
            {
                paragraph.Add(new XElement("i", text));

                return;
            }
            paragraph.Add(new XElement("n", text));
        }

       

        protected override void RenderBoldRun(BoldTextInline element, IRenderContext context)
        {
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

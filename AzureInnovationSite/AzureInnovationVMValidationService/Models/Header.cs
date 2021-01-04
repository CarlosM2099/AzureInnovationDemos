using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureInnovationVMValidationService.Models
{
    public class Header : IRenderable
    {
        public int Level { get; set; }
        public string Text { get; set; }

        public Header(int level, string text)
        {
            this.Level = level;
            this.Text = text;
        }

        public Header(string text)
            : this(2, text)
        { }

        public string ToHtml() => $"<H{Level}>{Utils.HtmlEscape(Text)}</H{Level}>";
    }
}

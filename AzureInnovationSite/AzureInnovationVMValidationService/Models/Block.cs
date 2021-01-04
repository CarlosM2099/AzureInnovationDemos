using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureInnovationVMValidationService.Models
{
    public class Block : Collection<object>, IRenderable
    {
        public static Block OfType(string name, params object[] children)
        {
            return new Block(children) { TagName = name };
        }

        public string TagName { get; set; }

        public Block(params object[] children)
        {
            if (children != null)
            {
                foreach (var child in children)
                {
                    this.Add(child);
                }
            }
        }

        public string ToHtml()
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(TagName))
            {
                sb.AppendLine("<" + TagName + ">");
            }

            foreach (var o in this)
            {
                if (o is IRenderable r)
                {
                    sb.AppendLine(r.ToHtml());
                }
                else
                {
                    sb.AppendLine(Utils.HtmlEscape(o?.ToString()));
                }
            }

            if (!string.IsNullOrEmpty(TagName))
            {
                sb.AppendLine("</" + TagName + ">");
            }

            return sb?.ToString()?.Trim();
        }
    }
}

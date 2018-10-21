using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedForms.Models
{
    public class EditorPart
    {
        public EditorPart(string html)
        {
            Html = html;
        }

        public string Html { get; set; }
    }
}

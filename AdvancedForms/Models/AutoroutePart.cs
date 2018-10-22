using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedForms.Models
{
    public class AutoroutePart
    {
        public AutoroutePart(string path)
        {
            Path = path;
        }

        public string Path { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Camera.Image.Renamer
{
    public class Options
    {
        public Options()
        {
            this.Filters = new List<string>();
        }

        public bool RecurseSubdirectories { get; set; }
        public List<string> Filters { get; set; }
        public bool CopyInsteadOfRename { get; set; }
        public bool UseRegExFilters { get; set; }
        public bool Verbose { get; set; }
        public string SourcePath { get; set; }
        public bool TestOnly { get; set; }
    }
}

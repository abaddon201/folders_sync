using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace folders_sync
{
    public class FoldersPair
    {
        public string Source { get; private set; }
        public string Target { get; private set; }

        public FoldersPair(string source, string target)
        {
            Source = source;
            Target = target;
        }
    }
}

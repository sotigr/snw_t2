using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace snw.core.system.text
{
    public class article
    {
        public string Title { set; get; }
        public string Name { set; get; }
        public string Content { set; get; }
        public string Group { set; get; }
        public snw.core.system.user Publisher {set;get;}
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace BobDono.Attributes
{
    public class ModuleAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Authorize { get; set; }
        public bool Hidden { get; set; }

    }
}

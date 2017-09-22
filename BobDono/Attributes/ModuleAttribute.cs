using System;
using System.Collections.Generic;
using System.Text;
using BobDono.Interfaces;

namespace BobDono.Attributes
{
    public class ModuleAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Authorize { get; set; }
        public bool Hidden { get; set; }
        public bool IsChannelContextual { get; set; }

        public List<IModule> Contexts { get; } = new List<IModule>();
    }
}

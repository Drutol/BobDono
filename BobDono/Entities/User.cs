using System;
using System.Collections.Generic;
using System.Text;

namespace BobDono.Entities
{
    public class User
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        

        public ICollection<Election> Elections { get; set; }
        public ICollection<Waifu> Waifus { get; set; }
    }
}

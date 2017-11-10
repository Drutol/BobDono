using System;
using System.Collections.Generic;
using System.Text;

namespace BobDono.Models.Entities
{
    public class ExceptionReport
    {
        public long Id { get; set; }

        public string Type { get; set; }
        public string Content { get; set; }
        public string AffectedUser { get; set; }
        public string Command { get; set; }
        public string Channel { get; set; }
        public DateTime DateTime { get; set; }
    }
}

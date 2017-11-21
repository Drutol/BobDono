using System;
using System.Collections.Generic;
using System.Text;

namespace BobDono.Models.Entities
{
    public class HallOfFameMember
    {
        public long Id { get; set; }

        public WaifuContender Contender { get; set; }
        public DateTime WinDate { get; set; }
    }
}

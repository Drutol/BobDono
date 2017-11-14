using System.Collections.Generic;

namespace BobDono.Models.Entities
{
    public class Waifu
    {
        public long Id { get; set; }
        
        public virtual ICollection<UserWaifu> Users { get; set; }
       
        public string Name { get; set; }
        public string MalId { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }

        public string[] Animeography { get; set; } = new string[0];
        public string[] Mangaography { get; set; } = new string[0];
        public string[] Voiceactors { get; set; } = new string[0];
    }
}

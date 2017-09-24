using System.Collections.Generic;

namespace BobDono.Models.MalHell
{
    public class CharacterDetailsData
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public string ImgUrl { get; set; }
        public string Content { get; set; }
        public string SpoilerContent { get; set; }
        public string TotalFavs { get; set; }
        public List<AnimeLightEntry> Animeography { get; } = new List<AnimeLightEntry>();
        public List<AnimeLightEntry> Mangaography { get; }  = new List<AnimeLightEntry>();
        public List<AnimeStaffPerson> VoiceActors { get; } = new List<AnimeStaffPerson>();
    }
}

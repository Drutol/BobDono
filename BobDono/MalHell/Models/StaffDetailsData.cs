using System.Collections.Generic;

namespace BobDono.MalHell.Models
{
    public class ShowCharacterPair
    {
        public AnimeLightEntry AnimeLightEntry { get; set; }
        public AnimeCharacter AnimeCharacter { get; set; }
    }

    public class StaffDetailsData
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public string ImgUrl { get; set; }
        public List<string> Details { get; set; } = new List<string>();
        public List<ShowCharacterPair> ShowCharacterPairs { get; set; } = new List<ShowCharacterPair>();
        public List<AnimeLightEntry> StaffPositions { get; set; } = new List<AnimeLightEntry>();
    }
}

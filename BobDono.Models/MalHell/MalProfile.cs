using System.Collections.Generic;

namespace BobDono.Models.MalHell
{
    

    public class MalProfile
    {
        

        public class ProfileData
        {
            public MalUser User { get; } = new MalUser();

            //Fav Anime
            public List<int> FavouriteAnime { get; } = new List<int>();
            //Fav Manga
            public List<int> FavouriteManga { get; } = new List<int>();
            //Fav Characters
            public List<AnimeCharacter> FavouriteCharacters { get; } = new List<AnimeCharacter>();
            //Fav Ppl
            public List<AnimeStaffPerson> FavouritePeople { get; } = new List<AnimeStaffPerson>();
            //Recent Anime
            public List<int> RecentAnime { get; } = new List<int>();
            //Recent Manga 
            public List<int> RecentManga { get; } = new List<int>();

            public bool IsFriend { get; set; }
            public bool CanAddFriend { get; set; }
        }
    }
}

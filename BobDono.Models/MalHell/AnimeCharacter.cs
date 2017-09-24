using BobDono.Models.MalHell.Enums;

namespace BobDono.Models.MalHell
{
    public class AnimeCharacter : FavouriteBase
    {
        public string ShowId { get; set; }
        public bool FromAnime { get; set; }
        public string ShowName { get; set; }

        public override FavouriteType Type { get; } = FavouriteType.Character;
    }
}
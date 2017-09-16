using BobDono.MalHell.Models.Enums;

namespace BobDono.MalHell.Models
{
    public class AnimeCharacter : FavouriteBase
    {
        public string ShowId { get; set; }
        public bool FromAnime { get; set; }
        public string ShowName { get; set; }

        public override FavouriteType Type { get; } = FavouriteType.Character;
    }
}
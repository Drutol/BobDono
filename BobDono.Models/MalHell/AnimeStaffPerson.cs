using BobDono.Models.MalHell.Enums;

namespace BobDono.Models.MalHell
{
    public class AnimeStaffPerson : FavouriteBase
    {
        public override FavouriteType Type { get; } = FavouriteType.Person;
        public bool IsUnknown { get; set; }
    }
}
using BobDono.MalHell.Models.Enums;

namespace BobDono.MalHell.Models
{
    public class AnimeStaffPerson : FavouriteBase
    {
        public override FavouriteType Type { get; } = FavouriteType.Person;
        public bool IsUnknown { get; set; }
    }
}
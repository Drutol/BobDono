namespace BobDono.Models.Entities
{
    public class Vote
    {
        public long Id { get; set; }

        public Bracket Bracket { get; set; }
        public WaifuContender Contender { get; set; }
        public User User { get; set; }
    }
}

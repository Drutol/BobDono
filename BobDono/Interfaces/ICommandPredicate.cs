using BobDono.Attributes;

namespace BobDono.Interfaces
{
    public interface ICommandPredicate
    {
        bool MeetsCriteria(CommandHandlerAttribute attr,params object[] args);
    }
}

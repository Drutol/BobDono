using BobDono.Core.Attributes;

namespace BobDono.Core.BL
{
    public interface ICommandPredicate
    {
        bool MeetsCriteria(CommandHandlerAttribute attr,params object[] args);
    }
}

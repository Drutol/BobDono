using System.Threading.Tasks;
using BobDono.Models.MalHell;

namespace BobDono.Interfaces.Queries
{
    public interface IProfileQuery
    {
        Task<MalProfile.ProfileData> GetProfileData(string userName);
    }
}
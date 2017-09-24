using System.Threading.Tasks;
using BobDono.Models.MalHell;

namespace BobDono.Interfaces
{
    public interface IStaffDetailsQuery
    {
        Task<StaffDetailsData> GetStaffDetails(int id);
    }
}
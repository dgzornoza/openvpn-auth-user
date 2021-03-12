
using System.Threading.Tasks;

namespace OpenVpnAuthUser.Services
{
    public interface IAuthUserService
    {
        Task Validate();
    }
}

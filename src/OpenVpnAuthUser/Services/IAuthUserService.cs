
using System.Threading.Tasks;

namespace OpenVpnAuthUser.Services
{
    public interface IAuthUserService
    {
        /// <summary>
        /// method for validate user in openvpn
        /// </summary>
        /// <param name="args">program arguments established by openvpn</param>
        Task Validate(string[] args);
    }
}

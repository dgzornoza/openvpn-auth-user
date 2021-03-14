

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenVpnAuthUser.Models;

namespace OpenVpnAuthUser.Services
{
    /// <summary>
    /// Service for authenticate in openvpn with Environment vars, in server.ovpn config file should be configured with:
    /// 
    /// script-security 3    
    /// auth-user-pass-verify "auth\\OpenVpnAuthUser.exe" via-env
    /// 
    /// in client.ovpn file should be configured with
    /// 
    /// auth-user-pass
    /// 
    /// </summary>
    public class AuthUserPasswordEnvironmentService : IAuthUserService
    {
        private readonly ILogger<AuthUserPasswordEnvironmentService> _logger;
        private readonly SettingsModel _settings;

        public AuthUserPasswordEnvironmentService(ILogger<AuthUserPasswordEnvironmentService> logger, IOptions<SettingsModel> options)
        {
            _logger = logger;
            _settings = options.Value;
        }

        public async Task Validate(string[] args)
        {
            // openvpn environment vars should be configured with : auth-user-pass-verify OpenVpnAuthUser.exe via-env
            string userName = Environment.GetEnvironmentVariable("username");
            string password = Environment.GetEnvironmentVariable("password");

            string[] userPasswords = await File.ReadAllLinesAsync(_settings.UsersPasswordsPath);

            // validate user and password with file
            bool result = userPasswords.Contains($"{userName};{password}");

            if (result)
            {
                _logger.LogInformation($"Successful authentication: {userName}");
            }
            else
            {
                throw new UnauthorizedAccessException($"Failed authentication: {userName},{password}");
            }
        }
    }
}

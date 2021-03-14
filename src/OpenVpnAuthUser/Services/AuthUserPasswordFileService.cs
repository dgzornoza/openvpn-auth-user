

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
    /// script-security 2    
    /// auth-user-pass-verify "auth\\OpenVpnAuthUser.exe" via-file
    /// 
    /// in client.ovpn file should be configured with
    /// 
    /// auth-user-pass
    /// 
    /// </summary>
    public class AuthUserPasswordFileService : IAuthUserService
    {
        private readonly ILogger<AuthUserPasswordFileService> _logger;
        private readonly SettingsModel _settings;

        public AuthUserPasswordFileService(ILogger<AuthUserPasswordFileService> logger, IOptions<SettingsModel> options)
        {
            _logger = logger;
            _settings = options.Value;
        }

        public async Task Validate(string[] args)
        {
            // get openvpn temp file
            string[] lines = await File.ReadAllLinesAsync(args[0]);

            string userName = lines[0];
            string password = lines[1];

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

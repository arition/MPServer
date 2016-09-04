using System;
using Microsoft.AspNetCore.DataProtection;

namespace MPServer
{
    /// <summary>
    /// DataProtection for signing key
    /// </summary>
    public class SigningKeyProtector
    {
        private readonly IDataProtector _protector;

        // the 'provider' parameter is provided by DI
        public SigningKeyProtector(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("SigningKey");
        }

        public string ProtectKey(byte[] key)
        {
            return Convert.ToBase64String(_protector.Protect(key));
        }

        public byte[] UnprotectKey(string payload)
        {
            return _protector.Unprotect(Convert.FromBase64String(payload));
        }
    }
}

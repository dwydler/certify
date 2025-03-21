﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Certify.Providers;

namespace Certify.Management
{
    public static class CredentialsUtil
    {
        public static async Task<bool> IsCredentialInUse(IManagedItemStore itemStore, string storageKey)
        {
            if (itemStore == null)
            {
                return false;
            }

            // TODO: inject item manager or move this check out to certify manager
            var managedCertificates = await itemStore.Find(new Models.ManagedCertificateFilter { StoredCredentialKey = storageKey });
            if (managedCertificates.Any())
            {
                // credential is in use
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Get protected version of a secret 
        /// </summary>
        /// <param name="clearText"></param>
        /// <param name="optionalEntropy"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        public static string Protect(
                string clearText,
                string optionalEntropy = null,
                DataProtectionScope? scope = null)
        {
            // https://www.thomaslevesque.com/2013/05/21/an-easy-and-secure-way-to-store-a-password-using-data-protection-api/

            if (clearText == null)
            {
                return null;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (scope == null)
                {
                    scope = DataProtectionScope.CurrentUser;
                }

                var clearBytes = Encoding.UTF8.GetBytes(clearText);
                var entropyBytes = string.IsNullOrEmpty(optionalEntropy)
                    ? null
                    : Encoding.UTF8.GetBytes(optionalEntropy);
                var encryptedBytes = ProtectedData.Protect(clearBytes, entropyBytes, (DataProtectionScope)scope);
                return Convert.ToBase64String(encryptedBytes);
            }
            else
            {
#if RELEASE
                Trace.Assert(true, "Using dummy encryption, not suitable for production use.");
#endif
                Trace.WriteLine("Using dummy encryption, not suitable for production use.");

                // TODO: dummy implementation, require alternative implementation for non-windows
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(clearText).Reverse().ToArray());
            }
        }

        /// <summary>
        /// Get unprotected version of a secret 
        /// </summary>
        /// <param name="encryptedText"></param>
        /// <param name="optionalEntropy"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        public static string Unprotect(
            string encryptedText,
            string optionalEntropy = null,
            DataProtectionScope? scope = null)
        {
            // https://www.thomaslevesque.com/2013/05/21/an-easy-and-secure-way-to-store-a-password-using-data-protection-api/

            if (encryptedText == null)
            {
                throw new ArgumentNullException("encryptedText");
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (scope == null)
                {
                    scope = DataProtectionScope.CurrentUser;
                }

                var encryptedBytes = Convert.FromBase64String(encryptedText);
                var entropyBytes = string.IsNullOrEmpty(optionalEntropy)
                    ? null
                    : Encoding.UTF8.GetBytes(optionalEntropy);
                var clearBytes = ProtectedData.Unprotect(encryptedBytes, entropyBytes, (DataProtectionScope)scope);
                return Encoding.UTF8.GetString(clearBytes);
            }
            else
            {
                Debug.WriteLine("Using dummy encryption, not suitable for production use.");
                // TODO: dummy implementation, implement alternative implementation for non-windows
                var bytes = Convert.FromBase64String(encryptedText);
                return Encoding.UTF8.GetString(bytes.Reverse().ToArray());
            }
        }
    }
}

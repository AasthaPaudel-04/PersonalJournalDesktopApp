using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using PersonalJournalDesktopApp.Data;

namespace PersonalJournalDesktopApp.Services
{
    public class SecurityService
    {
        private readonly DatabaseService _database;
        private const string PIN_KEY = "AppPIN";
        private const string PASSWORD_KEY = "AppPassword";
        private const string SECURITY_ENABLED_KEY = "SecurityEnabled";

        public SecurityService(DatabaseService database)
        {
            _database = database;
        }

        public async Task<bool> IsSecurityEnabledAsync()
        {
            var setting = await _database.GetSettingAsync(SECURITY_ENABLED_KEY);
            return setting == "True";
        }

        public async Task EnableSecurityAsync(string pin)
        {
            var hashedPin = HashPassword(pin);
            await _database.SaveSettingAsync(PIN_KEY, hashedPin);
            await _database.SaveSettingAsync(SECURITY_ENABLED_KEY, "True");
        }

        public async Task DisableSecurityAsync()
        {
            await _database.SaveSettingAsync(SECURITY_ENABLED_KEY, "False");
        }

        public async Task<bool> ValidatePinAsync(string pin)
        {
            var storedHash = await _database.GetSettingAsync(PIN_KEY);
            if (string.IsNullOrEmpty(storedHash))
                return false;

            var inputHash = HashPassword(pin);
            return storedHash == inputHash;
        }

        public async Task ChangePinAsync(string newPin)
        {
            var hashedPin = HashPassword(newPin);
            await _database.SaveSettingAsync(PIN_KEY, hashedPin);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;

namespace AmazonReloader
{
    public class CreditCard
    {
        public static string GetMonthYearKeyForPurchases(DateTime date)
        {
            return $"{date.ToString("MMMM", CultureInfo.InvariantCulture)}_{date.Year}";
        }
        public string GetThisCardFileLocation()
        {
            return $"{ConfigurationManager.AppSettings["encrypted_info_folder"]}\\{Bank}.json";
        }

        private readonly string SecretKeyPath;
        private byte[] Key => File.ReadAllBytes(SecretKeyPath);

        public CreditCard()
        {
            SecretKeyPath = ConfigurationManager.AppSettings["secret_key_location"];
        }

        public Dictionary<string, int> NumberOfPurchasesForEachMonth { get; set; }
        public string EncryptedNumber { get; set; }
        public string EncryptedName { get; set; }
        public string EncryptedExpires { get; set; }
        public string Bank { get; set; }
        public int NumberOfNeededPurchasesPerMonth { get; set; }

        public string GetCcNumber()
        {
            return AESGCM.SimpleDecrypt(EncryptedNumber, Key);
        }
        public string GetNameOnCc()
        {
            return AESGCM.SimpleDecrypt(EncryptedName, Key);
        }
        public string GetExpiration()
        {
            return AESGCM.SimpleDecrypt(EncryptedExpires, Key);
        }
    }
}
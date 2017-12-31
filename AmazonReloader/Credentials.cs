using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace AmazonReloader
{
    public class Credentials
    {
        private readonly string KeyPath;
        private byte[] Key => File.ReadAllBytes(KeyPath);

        public Credentials()
        {
            KeyPath = ConfigurationManager.AppSettings["secret_key_location"];
        }

        public string EncryptedEmail;
        public string EncryptedPassword;

        public string GetEmail()
        {
            var decryptedEmail = AESGCM.SimpleDecrypt(EncryptedEmail, Key);
            return decryptedEmail;
        }
        public string GetPassword()
        {
            var decryptedPassword = AESGCM.SimpleDecrypt(EncryptedPassword, Key);
            return decryptedPassword;
        }
    }

    public class CreditCards
    {
        public static List<CreditCard> Cards;
    }
}
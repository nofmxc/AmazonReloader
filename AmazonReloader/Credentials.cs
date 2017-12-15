using System.Configuration;
using System.IO;

namespace AmazonReloader
{
    public static class Credentials
    {
        private const string KeyPath = "C:\\AmazonReloader\\key";
        private static byte[] Key => File.ReadAllBytes(KeyPath);

        public static string Email
        {
            get
            {
                var decryptedEmail = AESGCM.SimpleDecrypt(ConfigurationManager.AppSettings["email"], Key);
                return decryptedEmail;
            }
        }

        public static string Password
        {
            get
            {
                var decryptedEmail = AESGCM.SimpleDecrypt(ConfigurationManager.AppSettings["pass"], Key);
                return decryptedEmail;
            }
        }

        public static class CreditCards
        {
            public static CreditCard ConsumersCU => new CreditCard
            {
                Bank = "ConsumersCU",
                Name = AESGCM.SimpleDecrypt(ConfigurationManager.AppSettings["cc_name"], Key),
                Number = AESGCM.SimpleDecrypt(ConfigurationManager.AppSettings["ccu_number"], Key),
                Expires = AESGCM.SimpleDecrypt(ConfigurationManager.AppSettings["ccu_expires"], Key)
            };

            public static CreditCard NorthPointeBank => new CreditCard
            {
                Bank = "NorthPointeBank",
                Name = AESGCM.SimpleDecrypt(ConfigurationManager.AppSettings["cc_name"], Key),
                Number = AESGCM.SimpleDecrypt(ConfigurationManager.AppSettings["nb_number"], Key),
                Expires = AESGCM.SimpleDecrypt(ConfigurationManager.AppSettings["nb_expires"], Key)
            };
        }
    }
}
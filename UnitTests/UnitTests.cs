using System;
using System.IO;
using AmazonReloader;
using NUnit.Framework;
using System.Configuration;

namespace ConsoleApp2
{
    class UnitTests
    {
        [Test]
        public void CanOpenBrowser()
        {
            var clicker = new Clicker();
            clicker.OpenAmazon();
        }

        [Test]
        public void CanReload()
        {
            //var program = new Program();
            // TODO: Fix this test?
            var creditCard = new CreditCard();
            Program.Reload(creditCard);
        }

        [Test]
        public void CanEncryptAndDecrypt()
        {
//            var keyToSave = AESGCM.NewKey();
//            File.WriteAllBytes("C:\\folder\\key", keyToSave);
//
            var keyLoaded = File.ReadAllBytes("C:\\folder\\key");
            var encryptedEmail = AESGCM.SimpleEncrypt("slav.bukhal@gmail.com", keyLoaded);
            Console.WriteLine($"ENCRYPTED email: {encryptedEmail}");

            var ccname = AESGCM.SimpleEncrypt("Vyacheslav Bukhal", keyLoaded);
            Console.WriteLine($"ENCRYPTED cc_name: {ccname}");

            var ccuNumber = AESGCM.SimpleEncrypt("4112508210750823", keyLoaded);
            Console.WriteLine($"ENCRYPTED ccuNumber: {ccuNumber}");

            var ccuExpires = AESGCM.SimpleEncrypt("06/2019", keyLoaded);
            Console.WriteLine($"ENCRYPTED ccuExpires: {ccuExpires}");

            var nbNumber = AESGCM.SimpleEncrypt("4373005000287086", keyLoaded);
            Console.WriteLine($"ENCRYPTED nbNumber: {nbNumber}");

            var nbExpires = AESGCM.SimpleEncrypt("07/2020", keyLoaded);
            Console.WriteLine($"ENCRYPTED nbExpires: {nbExpires}");


            var email = ConfigurationManager.AppSettings["email"];
            var pass = ConfigurationManager.AppSettings["pass"];
            var decryptedEmail = AESGCM.SimpleDecrypt(email, keyLoaded);
            Console.WriteLine($"Email: {decryptedEmail}");
        }

        [Test]
        public void CanGenerateRandomPrice()
        {
//            var program = new Program();
            var v = Program.GenerateRandomSum();
            Console.WriteLine(v);
                
        }
    }
}

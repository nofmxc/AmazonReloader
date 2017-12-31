using AmazonReloader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace AccountEncryptor
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Choose what to do:");
                Console.WriteLine("0. Exit.");
                Console.WriteLine("1. Create Encrypted Credit Card Information.");
                Console.WriteLine("2. Create Encrypted Amazon Account Information.");

                var input = Console.ReadLine();
                switch (input)
                {
                    case "0":
                        return;
                    case "1":
                        Console.Clear();
                        CreateEncryptedCreditCardInfo();
                        break;
                    case "2":
                        Console.Clear();
                        CreateEncryptedAmazonAccountInfo();
                        break;
                    default:
                        Console.Clear();
                        break;
                }
            }
        }

        private static void CreateEncryptedAmazonAccountInfo()
        {
            var secretKey = GetSecretKey();

            Console.WriteLine($"Enter the following information to generate encrypted versions for the program to use.");
            Console.WriteLine("Amazon email address (example name@gmail.com):");
            var email = Console.ReadLine();
            Console.WriteLine("Amazon password:");
            var password = GetPassword();
           
            var encryptedEmail = AESGCM.SimpleEncrypt(email, secretKey);
            var encryptedPassword = AESGCM.SimpleEncrypt(password, secretKey);
            
            string encryptedInfoPath = 
                $"{ConfigurationManager.AppSettings["encrypted_info_folder"]}\\{ConfigurationManager.AppSettings["encrypted_amazon_account_into_file_name"]}";

            Console.WriteLine($"Encrypted Output Below. Saving to account info json file at {encryptedInfoPath}:");
            Console.WriteLine("");
            Console.WriteLine($"Encrypted Amazon Email        :{encryptedEmail}");
            Console.WriteLine($"Encrypted Amazon Password     :{encryptedPassword}");

            var newEncyptedCredentials = new Credentials
            {
                EncryptedPassword = encryptedPassword,
                EncryptedEmail = encryptedEmail
            };
            var serializedEncyptedCredentials = JsonConvert.SerializeObject(newEncyptedCredentials);

            FileInfo fileInfo = new FileInfo(encryptedInfoPath);
            if (!fileInfo.Exists)
            {
                Directory.CreateDirectory(fileInfo.Directory.FullName);
            }
            File.WriteAllText(encryptedInfoPath, serializedEncyptedCredentials);

            Console.WriteLine($"Success! Saved new account info.");
            Console.WriteLine("");
        }

        private static void CreateEncryptedCreditCardInfo()
        {
            var secretKey = GetSecretKey();

            Console.WriteLine($"Enter the following information to generate encrypted versions for the program to use.");
            Console.WriteLine("Bank info (your own description):");
            var bankName = Console.ReadLine();
            Console.WriteLine("Your name on the credit card:");
            var nameOnCc = Console.ReadLine();
            Console.WriteLine("Credit card number:");
            var ccNumber = Console.ReadLine();
            Console.WriteLine("Credit card expiration (example 06/2019):");
            var expiration = Console.ReadLine();
            Console.WriteLine("Number of needed purchases a month:");
            var numberOfPurchases = Int32.Parse(Console.ReadLine());
            
            var encryptedNameOnCc = AESGCM.SimpleEncrypt(nameOnCc, secretKey);
            var encryptedCcNumber = AESGCM.SimpleEncrypt(ccNumber, secretKey);
            var encryptedExpiration = AESGCM.SimpleEncrypt(expiration, secretKey);

            string encryptedInfoPath = ConfigurationManager.AppSettings["encrypted_info_folder"];

            Console.WriteLine($"Encrypted Output Below. Saving to credit card json file at {encryptedInfoPath}:");
            Console.WriteLine("");
            Console.WriteLine($"Bank Name (Not encrypted)           :{bankName}");
            Console.WriteLine($"Encrypted Name on Credit Card       :{encryptedNameOnCc}");
            Console.WriteLine($"Encrypted Credit Card Number        :{encryptedCcNumber}");
            Console.WriteLine($"Encrypted Expiration Date           :{encryptedExpiration}");
            Console.WriteLine($"Number of Purchases (Not encrypted) :{numberOfPurchases}");

            var newEncyptedCreditCardInfo = new CreditCard
            {
                Bank = bankName,
                EncryptedName = encryptedNameOnCc,
                EncryptedExpires = encryptedExpiration,
                EncryptedNumber = encryptedCcNumber,
                NumberOfNeededPurchasesPerMonth = numberOfPurchases,
                NumberOfPurchasesForEachMonth = new Dictionary<string, int>()
            };
            var serializedCcInfo = JsonConvert.SerializeObject(newEncyptedCreditCardInfo);

            var fullPath = newEncyptedCreditCardInfo.GetThisCardFileLocation();
            FileInfo fileInfo = new FileInfo(fullPath);
            if (!fileInfo.Exists)
            {
                Directory.CreateDirectory(fileInfo.Directory.FullName);
            }
            File.WriteAllText(fullPath, serializedCcInfo);
        }

        private static byte[] GetSecretKey()
        {
            string keyPath = ConfigurationManager.AppSettings["secret_key_location"];
            byte[] currentKey = File.ReadAllBytes(keyPath);

            if (currentKey.Length == 0)
            {
                Console.WriteLine("Current key does not exist. Need to create one.");
                currentKey = CreateNewKey();
            }
            return currentKey;
        }

        private static byte[] CreateNewKey()
        {
            string keyPath = ConfigurationManager.AppSettings["secret_key_location"];

            byte[] currentKey = File.ReadAllBytes(keyPath);

            if (currentKey.Length != 0)
            {
                throw new Exception("Trying to create new key, but key already exists.");
            }

            var newKey = AESGCM.NewKey();
            File.WriteAllBytes(keyPath, newKey);
            Console.WriteLine($"Success. Wrote new key to {keyPath}.");
            return newKey;
        }

        private static string GetPassword()
        {
            string password = "";
            ConsoleKeyInfo info = Console.ReadKey(true);
            while (info.Key != ConsoleKey.Enter)
            {
                if (info.Key != ConsoleKey.Backspace)
                {
                    Console.Write("*");
                    password += info.KeyChar;
                }
                else if (info.Key == ConsoleKey.Backspace)
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        // remove one character from the list of password characters
                        password = password.Substring(0, password.Length - 1);
                        // get the location of the cursor
                        int pos = Console.CursorLeft;
                        // move the cursor to the left by one character
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                        // replace it with space
                        Console.Write(" ");
                        // move the cursor to the left by one character again
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                    }
                }
                info = Console.ReadKey(true);
            }
            // add a new line because user pressed enter at the end of their password
            Console.WriteLine();
            return password;
        }
    }
}

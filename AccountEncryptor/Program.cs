using AmazonReloader;
using System;
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
                Console.WriteLine("1. Create Encrypted info.");

                var input = Console.ReadLine();
                switch (input)
                {
                    case "0":
                        return;
                    case "1":
                        Console.Clear();
                        CreateEncryptedInfo();
                        break;
                    default:
                        Console.Clear();
                        break;
                }
            }
        }

        private static void CreateEncryptedInfo()
        {
            string keyPath = ConfigurationManager.AppSettings["secret_key_location"];
            byte[] currentKey = File.ReadAllBytes(keyPath);

            if (currentKey.Length == 0)
            {
                Console.WriteLine("Current key does not exist. Need to create one.");
                CreateNewKey();
            }

            Console.WriteLine($"Enter the following information to generate encrypted versions which can be entered into config. (encrypted using key at {keyPath}):");
            Console.WriteLine("Amazon email address (example name@gmail.com):");
            var email = Console.ReadLine();
            Console.WriteLine("Amazon password:");
            var password = GetPassword();
            Console.WriteLine("Your name on the credit card:");
            var nameOnCc = Console.ReadLine();
            Console.WriteLine("Credit card number:");
            var ccNumber = Console.ReadLine();
            Console.WriteLine("Credit card expiration (example 06/2019):");
            var expiration = Console.ReadLine();

            Console.WriteLine("Encrypted Output Below. Copy to app.config:");
            Console.WriteLine("");
            Console.WriteLine($"Encrypted Amazon Email        :{AESGCM.SimpleEncrypt(email, currentKey)}");
            Console.WriteLine($"Encrypted Amazon Password     :{AESGCM.SimpleEncrypt(password, currentKey)}");
            Console.WriteLine($"Encrypted Name on Credit Card :{AESGCM.SimpleEncrypt(nameOnCc, currentKey)}");
            Console.WriteLine($"Encrypted Credit Card Number  :{AESGCM.SimpleEncrypt(ccNumber, currentKey)}");
            Console.WriteLine($"Encrypted Expiration Date     :{AESGCM.SimpleEncrypt(expiration, currentKey)}");
        }

        private static void CreateNewKey()
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

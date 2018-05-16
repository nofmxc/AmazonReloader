using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using log4net;
using Newtonsoft.Json;

namespace AmazonReloader
{
    public class Program
    {
        private static List<CreditCard> CreditCards = new List<CreditCard>();
        private static Credentials AmazonCredentials;
        
        
        static void Main(string[] args)
        {
            Configurelogger();
            Logger.Info("Starting...");

            // Set AmazonCredentials
            var accountInfoPath =
                $"{ConfigurationManager.AppSettings["encrypted_info_folder"]}\\{ConfigurationManager.AppSettings["encrypted_amazon_account_into_file_name"]}";
            AmazonCredentials = JsonConvert.DeserializeObject<Credentials>(File.ReadAllText(accountInfoPath));

            // Set CreditCards
            var ccInfoFolder = $"{ConfigurationManager.AppSettings["encrypted_info_folder"]}";
            var infoFiles = Directory.GetFiles(ccInfoFolder);
            foreach(var file in infoFiles)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.Name.ToLower().Equals(ConfigurationManager.AppSettings["encrypted_amazon_account_into_file_name"].ToLower()))
                {
                    // Don't process the account info as a Credit Card
                    continue;
                }

                var creditCard = JsonConvert.DeserializeObject<CreditCard>(File.ReadAllText(file));
                CreditCards.Add(creditCard);                
            }

            var currentMonthYear = CreditCard.GetMonthYearKeyForPurchases(DateTime.Now);
            foreach (var creditCard in CreditCards)
            {
                if (!creditCard.NumberOfPurchasesForEachMonth.ContainsKey(currentMonthYear))
                {
                    creditCard.NumberOfPurchasesForEachMonth.Add(currentMonthYear, 0);
                }
                if (creditCard.NumberOfPurchasesForEachMonth[currentMonthYear] < creditCard.NumberOfNeededPurchasesPerMonth)
                {
                    DoTheDeed(creditCard, 1);
                }
            }
        }

        private static bool DoTheDeed(CreditCard cc, int maxAttempts)
        {
            int attempts = 1;
            bool success = false;
            while (attempts <= maxAttempts)
            {
                success = Reload(cc);
                if (success) break;
                attempts++;
            }
            var successMessage = success ? "Success" : "Failure";
            Logger.Info($"{cc.Bank}: {successMessage} after {attempts} attempt(s)");
            if (success)
            {
                IncrementPurchaseCount(cc);
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void IncrementPurchaseCount(CreditCard cc)
        {
            var currentMonthYear = CreditCard.GetMonthYearKeyForPurchases(DateTime.Now);

            if (!cc.NumberOfPurchasesForEachMonth.ContainsKey(currentMonthYear))
            {
                cc.NumberOfPurchasesForEachMonth.Add(currentMonthYear, 0);
            }

            cc.NumberOfPurchasesForEachMonth[currentMonthYear]++;
            var updatedCardSerialized = JsonConvert.SerializeObject(cc);
            File.WriteAllText(cc.GetThisCardFileLocation(), updatedCardSerialized);        
        }

        public static bool Reload(CreditCard cc)
        {
            var clicker = new Clicker();
            var success = false;

            try
            {
                clicker.OpenAmazon();
                clicker.Login(AmazonCredentials);
                clicker.OpenReloadPage();
                clicker.SelectCard(cc);
                var money = GenerateRandomSum();
                clicker.AddBalance(money);
                success = clicker.ConfirmSuccess(money);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            finally
            {
                clicker.CloseBrowser();
            }

            return success;
        }

        public static decimal GenerateRandomSum()
        {
            var rand = new Random();
            var money = Math.Round((decimal)rand.Next(60, 140) / 100, 1) - 0.01m;
            return money;
        }

        private static void Configurelogger()
        {
            log4net.Config.XmlConfigurator.Configure();
        }
    }
}

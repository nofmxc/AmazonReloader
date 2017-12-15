using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using log4net;

namespace AmazonReloader
{
    internal class Program
    {
        private static readonly List<CreditCard> CreditCards =
            new List<CreditCard> { Credentials.CreditCards.ConsumersCU, Credentials.CreditCards.NorthPointeBank };

        private static ILog _log;
        private static int _ccuCount;
        private static int _npbCount;
        private static int _reloadsThisRun = 0;

        static void Main(string[] args)
        {
            Configurelogger();
            _log.Info("Starting...");
            GetCounters();

            if (_ccuCount < 12)
            {
                DoTheDeed(Credentials.CreditCards.ConsumersCU, 5);
            }

            if (_npbCount < 15)
            {
                DoTheDeed(Credentials.CreditCards.NorthPointeBank, 5);
            }

            if (_reloadsThisRun > 0)
            {
                File.Copy("counters.config", "C:\\AmazonReloader\\counters.config", true);
            }
        }

        private static void GetCounters()
        {
            var currentMonth = $"{DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture)}_{DateTime.Now.Year}";
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (config.AppSettings.Settings.AllKeys.Any(key => key.Contains(currentMonth)))
            {
                var counts = config.AppSettings.Settings[currentMonth].Value;
                _ccuCount = int.Parse(counts.Split(new []{"; "}, StringSplitOptions.None)[0].Substring(4));
                _npbCount = int.Parse(counts.Split(new[] {"; "}, StringSplitOptions.None)[1].Substring(4));
            }
            else
            {
                config.AppSettings.Settings.Add(currentMonth, "ccu: 0; npb: 0");
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
                _ccuCount = 0;
                _npbCount = 0;
            }
        }

        private static void DoTheDeed(CreditCard cc, int maxAttempts)
        {
            int attempts = 1;
            bool success = false;
            while (attempts < maxAttempts)
            {
                success = Reload(cc);
                if (success) break;
                attempts++;
            }
            var successMessage = success ? "Success" : "Failure";
            if (success)
            {
                IncrementCount(cc.Bank);
                _reloadsThisRun++;
            }
            _log.Info($"{cc.Bank}: {successMessage} after {attempts} attempt(s)");
        }

        private static void IncrementCount(string ccBank)
        {
            var currentMonth = $"{DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture)}_{DateTime.Now.Year}";
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
             
            if (ccBank == "ConsumersCU")
            {
                _ccuCount++;
            }
            else if (ccBank == "NorthPointeBank")
            {
                _npbCount++;
            }
            config.AppSettings.Settings[currentMonth].Value = $"ccu: {_ccuCount}; npb: {_npbCount}";
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
        }

        public static bool Reload(CreditCard cc)
        {
            var clicker = new Clicker();
            var success = false;

            try
            {
                clicker.OpenAmazon();
                clicker.Login();
                clicker.OpenReloadPage();
                clicker.SelectCard(cc);
                var money = GenerateRandomSum();
                clicker.AddBalance(money);
                clicker.ConfirmSuccess(money);
                success = true;
            }
            catch (Exception e)
            {
                _log.Error(e);
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
            _log = LogManager.GetLogger(typeof(Program));
        }
    }
}

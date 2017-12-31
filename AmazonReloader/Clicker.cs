using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace AmazonReloader
{
    public class Clicker
    {
        private readonly ChromeDriver _driver;
        private string _encryptedEmail;
        private string _encryptedPassword;


        public Clicker()
        {
            _driver = new ChromeDriver();
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        }

        public void OpenAmazon()
        {
            _driver.Navigate().GoToUrl("http://www.amazon.com");

//           Screenshot ss = ((ITakesScreenshot)_driver).GetScreenshot();
//            ss.SaveAsFile($"C:\\AmazonReloader\\Screenshots\\{DateTime.Now.ToString()}", ScreenshotImageFormat.Jpeg);
        }

        public void Login(Credentials creds)
        {
            _driver.FindElementById("nav-link-accountList").Click();
            _driver.FindElementById("ap_email").SendKeys(creds.GetEmail());
            _driver.FindElementById("ap_password").SendKeys(creds.GetPassword());
            _driver.FindElementById("signInSubmit").Click();
        }

        public void OpenReloadPage()
        {
            var element = _driver.FindElementByLinkText("Reload Your Balance");

            FindOnPageThenClick(element);

            _driver.FindElementByName("GC-Reload-Button").Click();
        }

        public void SelectCard(CreditCard cc)
        {
            var lastFour = cc.GetCcNumber().Substring(12);
            Thread.Sleep(1500);
            _driver.FindElementById("asv-payment-edit-link").Click();
            _driver.FindElementsByClassName("pmts-instrument-acct-number-tail").Single(el => el.Text == lastFour).Click();
            try
            {
                _driver.FindElementsByClassName("a-input-text").Single(el => el.GetAttribute("placeholder") == $"ending in {lastFour}")
                    .SendKeys(cc.GetCcNumber());
                _driver.FindElementsByClassName("a-button-text").Single(el => el.Text == "Confirm Card").Click();
                Thread.Sleep(1000);
            }
            catch (NoSuchElementException)
            {}

            var element = _driver.FindElementById("asv-form-submit");
            FindOnPageThenClick(element);
        }

        public void FindOnPageThenClick(IWebElement element)
        {
            IJavaScriptExecutor jse = (IJavaScriptExecutor)_driver;
            jse.ExecuteScript("arguments[0].scrollIntoView()", element);
            element.Click();
        }

        public void AddBalance(decimal money)
        {
            var moneyString = money.ToString(CultureInfo.InvariantCulture);
            var amountField = _driver.FindElementById("asv-manual-reload-amount");
            amountField.Clear();
            amountField.SendKeys(moneyString);
            try
            {
                _driver.FindElementById("asv-reward-box-reload-amount").Click();
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e);
            }
            Thread.Sleep(1300);
            _driver.FindElementsById("form-submit-button").Single(el => el.Text == $"Reload ${moneyString}").Click();
        }

        public bool ConfirmSuccess(decimal money)
        {
            var moneyString = $"${ money.ToString(CultureInfo.InvariantCulture)}";
            Thread.Sleep(1300);
            var successMsg = _driver.FindElementsByClassName("a-alert-heading").SingleOrDefault(el => el.Text == $"{moneyString} will be added to your account");
            if (successMsg == null)
            {
                Console.WriteLine($"Failed to reload {moneyString}!!!");
                return false;
            }
            else
            {
                Console.WriteLine($"Successfully reloaded {moneyString}!");
                return true;
            }
        }

        public void CloseBrowser()
        {
            _driver.Quit();
        }
    }
}

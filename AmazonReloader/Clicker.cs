using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;

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
            var a = _driver.FindElementById("nav-link-accountList");
            FindOnPageThenClick(a);
            FindOnPageThenSendKeys(_driver.FindElementById("ap_email"), creds.GetEmail());
            //FindOnPageThenClick(_driver.FindElementById("continue"));
            Thread.Sleep(500);
            FindOnPageThenSendKeys(_driver.FindElementById("ap_password"), creds.GetPassword());
            FindOnPageThenClick(_driver.FindElementById("signInSubmit"));
        }

        public void OpenReloadPage()
        {
            FindOnPageThenClick(_driver.FindElementByLinkText("Reload Your Balance"));

            var reloadButton = _driver.FindElementByName("GC-Reload-Button");
            
            FindOnPageThenHitIt(reloadButton);
        }

        public void SelectCard(CreditCard cc)
        {
            var lastFour = cc.GetCcNumber().Substring(12);
            Thread.Sleep(1500);
            FindOnPageThenClick(_driver.FindElementById("asv-payment-edit-link"));
            FindOnPageThenClick(_driver.FindElementsByClassName("pmts-cc-number").Single(x => x.Text.Contains(lastFour)));
            try
            {
                FindOnPageThenSendKeys(_driver.FindElementsByClassName("a-input-text").Single(el => el.GetAttribute("placeholder") == $"ending in {lastFour}"), 
                    cc.GetCcNumber());
                FindOnPageThenClick(_driver.FindElementsByClassName("a-button-text").Single(el => el.Text == "Confirm Card"));
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

        public void FindOnPageThenHitIt(IWebElement element)
        {
            IJavaScriptExecutor jse = (IJavaScriptExecutor)_driver;
            jse.ExecuteScript("arguments[0].scrollIntoView()", element);

            Actions builder = new Actions(_driver);
            builder.MoveToElement(element, 1, 1).Click().Build().Perform();
        }

        public void FindOnPageThenSendKeys(IWebElement element, string text)
        {
            IJavaScriptExecutor jse = (IJavaScriptExecutor)_driver;
            jse.ExecuteScript("arguments[0].scrollIntoView()", element);
            element.SendKeys(text);
        }

        public void AddBalance(decimal money)
        {
            var moneyString = money.ToString(CultureInfo.InvariantCulture);
            var amountField = _driver.FindElementById("asv-manual-reload-amount");
            amountField.Clear();
            amountField.SendKeys(moneyString);
            try
            {
                FindOnPageThenClick(_driver.FindElementById("asv-reward-box-reload-amount"));
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e);
            }
            Thread.Sleep(1300);
            FindOnPageThenClick(_driver.FindElementsById("form-submit-button").Single(el => el.Text == $"Reload ${moneyString}"));
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

using System;
using System.Diagnostics.Contracts;
using System.Threading;
using MaintainableSelenium.MvcPages.WebPages;
using MaintainableSelenium.MvcPages.WebPages.WebForms;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;

namespace MaintainableSelenium.MvcPages.SeleniumUtils
{
    public static class SeleniumExtensions
    {
        private const int SearchElementDefaultTimeout = 30;

        /// <summary>
        /// Get rid of focus from currently focused element
        /// </summary>
        /// <param name="driver">Selenium webdriver</param>
        public static void Blur(this RemoteWebDriver driver)
        {
            if(IsThereElementWithFocus(driver))
            {
                Thread.Sleep(500);
                driver.ExecuteScript("var f= document.querySelector(':focus'); if(f!=undefined){f.blur()}");
                Thread.Sleep(500);
            }
        }

        /// <summary>
        /// Tyoe text into field
        /// </summary>
        /// <param name="input">Field</param>
        /// <param name="text">Text to tyoe</param>
        /// <param name="speed">Speed of typing (chars per minute). 0 means default selenium speed</param>
        public static void Type(this IWebElement input, string text, int speed = 0)
        {
            input.Focus();

            if (speed == 0)
            {
                input.SendKeys(text);
            }
            else
            {
                var delay = (1000*60)/speed;
                foreach (var charToType in text)
                {
                    input.SendKeys(charToType.ToString());
                    Thread.Sleep(delay);
                }
            }
        }

        public static void Focus(this IWebElement input)
        {
            input.SendKeys("");
        }

        public static int GetVerticalScrollWidth(this RemoteWebDriver driver)
        {
            //INFO: It's hard to get scrollbar width using JS. 17 its default size of scrollbar on Ms Windows platform
            return 17;
        }

        public static int GetWindowHeight(this RemoteWebDriver driver)
        {
            return (int)(long)driver.ExecuteScript("return window.innerHeight");
        } 
        
        internal static PageFragmentWatcher WatchForContentChanges(this RemoteWebDriver driver, string containerId)
        {
            var watcher = new PageFragmentWatcher(driver, containerId);
            watcher.StartWatching();
            return watcher;
        }


        internal static bool IsElementClickable(this RemoteWebDriver driver, IWebElement element)
        {
            return (bool)driver.ExecuteScript(@"
                    window.__selenium__isElementClickable = window.__selenium__isElementClickable || function(element)
                    {
                        var rec = element.getBoundingClientRect();
                        var elementAtPosition = document.elementFromPoint(rec.left, rec.top);
                        return element == elementAtPosition;
                    };
                    return window.__selenium__isElementClickable(arguments[0]);
            ", element);
        }

        public static int GetPageHeight(this RemoteWebDriver driver)
        {
            var scriptResult = driver.ExecuteScript("return Math.max(document.body.scrollHeight, document.body.offsetHeight, document.documentElement.clientHeight, document.documentElement.scrollHeight, document.documentElement.offsetHeight);");
            return (int)(long)scriptResult;
        }

        public static void ScrollTo(this RemoteWebDriver driver, int y)
        {
            driver.ExecuteScript(string.Format("window.scrollTo(0,{0})", y));
            Thread.Sleep(100);
        }


        /// <summary>
        /// Check if any element has currently focus
        /// </summary>
        /// <param name="driver">Selenium webdriver</param>
        [Pure]
        private static bool IsThereElementWithFocus(RemoteWebDriver driver)
        {
            try
            {
                driver.FindElementByCssSelector(":focus");
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Search for element with given id
        /// </summary>
        /// <param name="driver">Selenium driver</param>
        /// <param name="elementId">Id of expected element</param>
        /// <param name="timeout">Timout for element serch</param>
        public static IWebElement GetElementById(this RemoteWebDriver driver, string elementId, int timeout = SearchElementDefaultTimeout)
        {
            try
            {
                return driver.GetElementBy(By.Id(elementId), timeout);
            }
            catch (WebDriverTimeoutException ex)
            {
                if (ex.InnerException is NoSuchElementException)
                {
                    var message = string.Format("Cannot find element with id='{0}'", elementId);
                    throw new WebElementNotFoundException(message, ex);
                }
                throw;
            }
        }

        /// <summary>
        /// Search for element using <see cref="By"/> criterion
        /// </summary>
        /// <param name="driver">Selenium driver</param>
        /// <param name="by"><see cref="By"/> criterion for given element</param>
        /// <param name="timeout">Timout for element serch</param>
        private static IWebElement GetElementBy(this RemoteWebDriver driver, By by, int timeout = SearchElementDefaultTimeout)
        {
            return driver.WaitUntil(timeout, (a) =>
            {
                try
                {
                    var element = driver.FindElement(by);
                    if (element != null && element.Displayed && element.Enabled)
                    {
                        return element;
                    }
                    return null;
                }
                catch (StaleElementReferenceException)
                {
                    return null;
                }
            });
        }

        private static IWebElement GetElementByInScope(RemoteWebDriver driver, By @by, IWebElement scope, int timeout = SearchElementDefaultTimeout)
        {
            return driver.WaitUntil(timeout, (a) =>
            {
                try
                {
                    var element = scope.FindElement(@by);
                    if (element != null && element.Displayed && element.Enabled)
                    {
                        return element;
                    }
                    return null;
                }
                catch (StaleElementReferenceException)
                {
                    return null;
                }
            });
        }

        internal static TResult WaitUntil<TResult>(this RemoteWebDriver driver, int timeout,
            Func<IWebDriver, TResult> waitPredictor)
        {
            var waiter = new WebDriverWait(driver, TimeSpan.FromSeconds(timeout));
            return waiter.Until(waitPredictor);
        }

        /// <summary>
        /// Return parent of given web element
        /// </summary>
        /// <param name="node">Child element</param>
        public static IWebElement GetParent(this IWebElement node)
        {
            return node.FindElement(By.XPath(".."));
        }

        /// <summary>
        /// Return type of input represented by the given web element
        /// </summary>
        /// <param name="inputElement">Web element</param>
        public static string GetInputType(this IWebElement inputElement)
        {
            var inputType = inputElement.GetAttribute("type") ?? string.Empty;
            return inputType.ToLower();
        }

        /// <summary>
        /// Click on any element with given text
        /// </summary>
        /// <param name="driver">Selenium driver</param>
        /// <param name="scope">Scope element to narrow link search</param>
        /// <param name="linkText">Element tekst</param>
        public static  void ClickOnElementWithText(this RemoteWebDriver driver, IWebElement scope, string linkText)
        {
            var expectedElement = GetElementWithText(driver, scope, linkText);
            ClickOn(driver, expectedElement);
        }

        public static void ClickOn(this RemoteWebDriver driver, IWebElement expectedElement)
        {
            try
            {
                expectedElement.Click();
            }
            catch (InvalidOperationException)
            {
                if (expectedElement.Location.Y > driver.GetWindowHeight())
                {
                    driver.ScrollTo(expectedElement.Location.Y + expectedElement.Size.Height);
                    Thread.Sleep(500);
                }
                driver.WaitUntil(SearchElementDefaultTimeout, (d) => driver.IsElementClickable(expectedElement));
                expectedElement.Click();
            }
        }

        public static IWebElement GetElementWithText(this RemoteWebDriver driver, IWebElement scope, string linkText)
        {
            try
            {
                var by = By.XPath(string.Format(".//*[contains(text(), '{0}') or (@type='submit' and @value='{0}')]", linkText));
                return GetElementByInScope(driver, @by, scope);
            }
            catch (WebDriverTimeoutException ex)
            {
                if (ex.InnerException is NoSuchElementException)
                {
                    var message = string.Format("Cannot find element with text='{0}'", linkText);
                    throw new WebElementNotFoundException(message, ex);
                }
                throw;
            }
        }
    }
}
using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Internal;
using OpenQA.Selenium.Remote;
using Tellurium.MvcPages.SeleniumUtils;

namespace Tellurium.MvcPages.WebPages
{
    public class PageFragment : IPageFragment
    {
        protected readonly RemoteWebDriver Driver;
        protected readonly IWebElement WebElement;

        public PageFragment(RemoteWebDriver driver, IWebElement webElement)
        {
            this.Driver = driver;
            this.WebElement = webElement;
        }

        public void Click()
        {
            this.Driver.ClickOn(this.WebElement);
        }

        public void ClickOnElementWithText(string text)
        {
            Driver.ClickOnElementWithText(WebElement, text, false);
        }

        public void ClickOnElementWithPartialText(string text)
        {
            Driver.ClickOnElementWithText(WebElement, text, true);
        }

        public void Hover()
        {
            this.Driver.HoverOn(this.WebElement);
        }

        public void HoverOnElementWithText(string text)
        {
            Driver.HoverOnElementWithText(WebElement, text, false);
        }

        public void HoverOnElementWithPartialText(string text)
        {
            Driver.HoverOnElementWithText(WebElement, text, true);
        }

        public WebList GetListWithId(string id)
        {
            return Driver.GetListWithId(id);
        }

        public WebList ToWebList()
        {
            return new WebList(Driver, WebElement);
        }

        public WebTree GetTreeWithId(string id, bool isSelfItemsContainer = true, By itemsContainerLocator = null)
        {
            return Driver.GetTreeWithId(id, isSelfItemsContainer, itemsContainerLocator);
        }

        public WebTree ToWebTree(bool isSelfItemsContainer = true, By itemsContainerLocator = null)
        {
            return new WebTree(Driver, WebElement,isSelfItemsContainer, itemsContainerLocator);
        }

        public WebTable GetTableWithId(string id)
        {
            return Driver.GetTableWithId(id);
        }

        public WebTable ToWebTable()
        {
            return new WebTable(Driver, WebElement);
        }

        public string Text => WebElement.Text;

        public void AffectWith(Action action, bool watchSubtree=true)
        {
            var watcher = new PageFragmentWatcher(Driver, WebElement);
            watcher.StartWatching(watchSubtree);
            action();
            watcher.WaitForChange();
        }

        public IPageFragment GetParent()
        {
            var parent = this.Driver.GetStableElementByInScope(this.WebElement, SeleniumExtensions.ParentSelector);
            return new PageFragment(this.Driver, parent);
        }

        public IPageFragment GetElementWithText(string text)
        {
            return FindElementWithText(text, false);
        }

        public IPageFragment GetElementWithPartialText(string text)
        {
            return FindElementWithText(text, true);
        }

        private IPageFragment FindElementWithText(string text, bool isPartialText)
        {
            var element = this.Driver.GetStableElementWithText(this.WebElement, text, isPartialText);
            return new PageFragment(this.Driver, element);
        }

        public IWebElement WrappedElement => WebElement;
    }

    public interface IPageFragment: IWrapsElement
    {
        /// <summary>
        /// Perform click action
        /// </summary> 
        void Click();

        /// <summary>
        /// Find element with given text and perform click action
        /// </summary>
        /// <param name="text">Text inside element</param>
        void ClickOnElementWithText(string text);

        /// <summary>
        /// Find element with given partial text and perform click action
        /// </summary>
        /// <param name="text">Partial text inside element</param>
        void ClickOnElementWithPartialText(string text);

        /// <summary>
        /// Perform hover action
        /// </summary>
        void Hover();
       
        /// <summary>
        /// Find element with given text and perform hover action
        /// </summary>
        /// <param name="tex">Text inside element</param>
        void HoverOnElementWithText(string tex);
        
        /// <summary>
        /// Find element with given partial text and perform hover action
        /// </summary>
        /// <param name="text">Partial text inside element</param>
        void HoverOnElementWithPartialText(string text);
        
        /// <summary>
        /// Find list like structure with given Id
        /// </summary>
        /// <param name="id">Value of id attribute</param>
        WebList GetListWithId(string id);

        /// <summary>
        /// Convert current element to <see cref="WebList"/> wrapper
        /// </summary>
        WebList ToWebList();

        /// <summary>
        /// Find tree like structure with given Id
        /// </summary>
        /// <param name="id">Value of id attribute</param>
        /// <param name="isSelfItemsContainer">Set to true if given element is a direct container for tree nodes, otherwise set <paramref name="itemsContainerLocator"/></param>
        /// <param name="itemsContainerLocator">Locator for direct tree (as also subtree) nodes container</param>
        /// <returns></returns>
        WebTree GetTreeWithId(string id, bool isSelfItemsContainer = true, By itemsContainerLocator = null);
        
        /// <summary>
        /// Convert current element to <see cref="WebTree"/> wrapper
        /// </summary>
        /// <param name="isSelfItemsContainer">Set to true if current element is a direct container for tree nodes, otherwise set <paramref name="itemsContainerLocator"/></param>
        /// <param name="itemsContainerLocator">Locator for direct tree (as also subtree) nodes container</param>
        WebTree ToWebTree(bool isSelfItemsContainer = true, By itemsContainerLocator = null);

        /// <summary>
        /// Find table like structure with given Id
        /// </summary>
        /// <param name="id">Value of id attribute</param>
        WebTable GetTableWithId(string id);

        /// <summary>
        /// Convert current element to <see cref="WebTable"/> wrapper
        /// </summary>
        WebTable ToWebTable();
        
        /// <summary>
        /// Returns value of <see cref="IWebElement.Text"/> of underlying element
        /// </summary>
        string Text { get; }
        
        /// <summary>
        /// Perform given action and wait until current element changes
        /// </summary>
        /// <param name="action">Action to perform</param>
        /// <param name="watchSubtree">Set true if changes in subtree shuld also be observed</param>
        void AffectWith(Action action, bool watchSubtree=true);

        /// <summary>
        /// Get parent of current element
        /// </summary>
        IPageFragment GetParent();

        /// <summary>
        /// Find element with given text
        /// </summary>
        /// <param name="text">Text inside element</param>
        IPageFragment GetElementWithText(string text);
        
        /// <summary>
        /// Find element with given partial text
        /// </summary>
        /// <param name="text">Partial text inside element</param>
        IPageFragment GetElementWithPartialText(string text);
    }
}
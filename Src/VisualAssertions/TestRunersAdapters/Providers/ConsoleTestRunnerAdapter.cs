﻿using System;
using Tellurium.MvcPages.Utils;
using Tellurium.VisualAssertions.Screenshots.Domain;

namespace Tellurium.VisualAssertions.TestRunersAdapters.Providers
{
    public class ConsoleTestRunnerAdapter:ITestRunnerAdapter
    {
        public bool IsPresent()
        {
            return true;
        }

        public void NotifyAboutTestSuccess(string testName, TestSession session, BrowserPattern pattern)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Test passed: {0}", testName);
            Console.ResetColor();
        }

        public void NotifyAboutTestFail(string testName, TestSession session, BrowserPattern pattern)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Test failed: {0}", testName);
            Console.ResetColor();
        }

        public void NotifyAboutError(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.GetFullExceptionMessage());
            Console.ResetColor();
        }
    }
}
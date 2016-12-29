﻿using System;
using System.Collections.Generic;
using System.Linq;
using MaintainableSelenium.MvcPages.BrowserCamera;
using MaintainableSelenium.VisualAssertions.Infrastructure;
using MaintainableSelenium.VisualAssertions.Infrastructure.Persistence;
using MaintainableSelenium.VisualAssertions.Screenshots.Domain;
using MaintainableSelenium.VisualAssertions.Screenshots.Queries;
using MaintainableSelenium.VisualAssertions.TestRunersAdapters;

namespace MaintainableSelenium.VisualAssertions.Screenshots
{
    public class VisualAssertionsService 
    {
        private readonly IRepository<Project> projectRepository;
        private readonly ITestRunnerAdapter testRunnerAdapter;
        private readonly ISet<ScreenshotIdentity> takenScreenshots = new HashSet<ScreenshotIdentity>();
        public string ProjectName { get; set; }
        public string ScreenshotCategory { get; set; }
        public string BrowserName { get; set; }

        public VisualAssertionsService(IRepository<Project> projectRepository, ITestRunnerAdapter testRunnerAdapter)
        {
            this.projectRepository = projectRepository;
            this.testRunnerAdapter = testRunnerAdapter;
        }

        private TestSession GetCurrentTestSession(Project project)
        {
            if (project.Sessions == null)
            {
                throw new ApplicationException("Sessions cannot be null");
            }
            var currentStartDate = TestSessionContext.Current.StartDate;
            var testSession = project.Sessions.FirstOrDefault(x => x.StartDate == currentStartDate);
            if (testSession == null)
            {
                testSession = new TestSession
                {
                    StartDate = currentStartDate
                };
                project.AddSession(testSession);
            }
            return testSession;
        }

        public void CheckViewWithPattern(IBrowserCamera browserCamera, string viewName)
        {
            var screenshotIdentity = new ScreenshotIdentity(ProjectName, BrowserName, ScreenshotCategory, viewName);
            if (takenScreenshots.Contains(screenshotIdentity))
            {
                throw new DuplicatedScreenshotInSession(screenshotIdentity);
            }
            var screenshot = browserCamera.TakeScreenshot();
            takenScreenshots.Add(screenshotIdentity);
            CheckScreenshotWithPattern(screenshot, screenshotIdentity);
        }

        private void CheckScreenshotWithPattern(byte[] image, ScreenshotIdentity screenshotIdentity)
        {
            try
            {
                Action finishNotification;
                using (var tx = PersistanceEngine.GetSession().BeginTransaction())
                {
                    var project = this.GetProject(screenshotIdentity.ProjectName);
                    var testCase = GetTestCase(project, screenshotIdentity);
                    var browserPattern = testCase.GetActivePatternForBrowser(screenshotIdentity.BrowserName);
                    if (browserPattern == null)
                    {
                        testCase.AddNewPattern(image, screenshotIdentity.BrowserName);
                        finishNotification = () => testRunnerAdapter.NotifyAboutTestSuccess(screenshotIdentity.FullName);
                    }
                    else
                    {
                        var testSession = GetCurrentTestSession(project);
                        var testResult = GetTestResult(image, screenshotIdentity, browserPattern);
                        testSession.AddTestResult(testResult);
                        finishNotification = () =>
                        {
                            if (testResult.TestPassed)
                            {
                                testRunnerAdapter.NotifyAboutTestSuccess(screenshotIdentity.FullName);
                            }
                            else
                            {
                                testRunnerAdapter.NotifyAboutTestFail(screenshotIdentity.FullName, testSession, browserPattern);
                            }
                        };
                    }
                    tx.Commit();
                }
                finishNotification.Invoke();
            }
            catch (Exception ex)
            {
                testRunnerAdapter.NotifyAboutError(ex);
            }
        }

        private TestResult GetTestResult(byte[] image, ScreenshotIdentity screenshotIdentity, BrowserPattern browserPattern)
        {
            var testResult = new TestResult
            {
                Pattern = browserPattern,
                ScreenshotName = screenshotIdentity.ScreenshotName,
                Category = screenshotIdentity.Category,
                BrowserName = screenshotIdentity.BrowserName
            };

            if (browserPattern.MatchTo(image))
            {
                testResult.TestPassed = true;
               
            }
            else
            {
                testResult.TestPassed = false;
                testResult.ErrorScreenshot = image;
                testResult.BlindRegionsSnapshot = browserPattern.GetCopyOfAllBlindRegions();
            }
            return testResult;
        }

        private static TestCase GetTestCase(Project project, ScreenshotIdentity screenshotIdentity)
        {
            var testCaseCategory = project.TestCaseCategories.FirstOrDefault(x=> x.Name == screenshotIdentity.Category);
            if (testCaseCategory == null)
            {
                testCaseCategory = project.AddTestCaseCategory(screenshotIdentity.Category);
            }
            var testCase = testCaseCategory.TestCases.FirstOrDefault(x => x.PatternScreenshotName == screenshotIdentity.ScreenshotName);
            if (testCase == null)
            {
                testCase = testCaseCategory.AddTestCase(screenshotIdentity.ScreenshotName);
            }
            return testCase;
        }

        private Project GetProject(string projectName)
        {
            var project = projectRepository.FindOne(new FindProjectByName(projectName));
            if (project == null)
            {
                project = new Project
                {
                    Name = projectName
                };
                projectRepository.Save(project);
            }
            return project;
        }
    }
}
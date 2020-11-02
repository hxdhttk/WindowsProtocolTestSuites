﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using CommandLine;
using Microsoft.Protocols.TestManager.Kernel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Protocols.TestManager.CLI
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA1016 Mark assemblies with AssemblyVersionAttribute")]
    class Program
    {
        static void Main(string[] args)
        {
            CheckParameters(args);
            var parser = new Parser(cfg =>
            {
                cfg.CaseInsensitiveEnumValues = true;
                cfg.HelpWriter = Console.Error;
            });
            parser.ParseArguments<Options>(args)
                .WithParsed<Options>(opts => Run(opts))
                .WithNotParsed<Options>(errs => HandleArgumentError(errs));
        }

        static void Run(Options options)
        {
            try
            {
                Program p = new Program();
                p.Init();

                var resultFileDir = Path.GetDirectoryName(options.ReportFile);
                if (!string.IsNullOrEmpty(resultFileDir) && !Directory.Exists(resultFileDir))
                {
                    throw new ArgumentException(String.Format(StringResources.InvalidTestResultDir, resultFileDir));
                }

                if (!Path.IsPathRooted(options.Profile))
                {
                    options.Profile = Utility.RelativePath2AbsolutePath(options.Profile);
                }

                if (!Path.IsPathRooted(options.TestSuite))
                {
                    options.TestSuite = Utility.RelativePath2AbsolutePath(options.TestSuite);
                }
                p.LoadTestSuite(options.Profile, options.TestSuite);

                List<TestCase> testCases = (options.Categories.Count() > 0) ? p.GetTestCases(options.Categories.ToList()) : p.GetTestCases(options.SelectedOnly);

                Console.CancelKeyPress += (sender, args) =>
                {
                    Console.WriteLine("\nAborting test suite.");
                    p.AbortExecution();
                };

                p.RunTestSuite(testCases, options.TestSuite);

                if (options.ReportFile == null)
                {
                    p.PrintTestReport(options.Outcome);
                }
                else
                {
                    p.SaveTestReport(options.ReportFile, options.ReportFormat, options.Outcome);
                }

                Console.WriteLine(String.Format(StringResources.TestResultPath, Path.Combine(options.TestSuite, p.util.GetTestEngineResultPath())));
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(StringResources.ErrorMessage);
                Console.Error.WriteLine(e.Message);
                Environment.Exit(-1);
            }
        }

        static void HandleArgumentError(IEnumerable<Error> errors)
        {
            Environment.Exit(-1);
        }

        static void CheckParameters(string[] args)
        {
            var _param = new Dictionary<string, string>();

            // Parse parameters to dictionary
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == null) continue;
                string key = null;
                string val = null;

                // If the beginning of this string instance matches the specified string '-', it should be key
                if (args[i].StartsWith('-'))
                {
                    key = args[i].Replace("-", string.Empty);

                    // Get the value from next
                    if (i + 1 < args.Length && !args[i + 1].StartsWith('-'))
                    {
                        val = args[i + 1];
                        i++;
                    }
                }
                else
                {
                    val = args[i];
                }

                // Adjustment
                if (key == null)
                {
                    key = val;
                    val = null;
                }
                _param[key] = val;
            }

            if (_param.ContainsKey("outcome"))
            {
                var outcomeValue = _param["outcome"].Split(',');
                foreach (var e in outcomeValue)
                {
                    // if the value converts integer succeeded, it shows error. valid values are: pass, fail, inconclusive. 
                    if (int.TryParse(e, out _))
                    {
                        Console.Error.WriteLine(StringResources.ErrorMessage);
                        Console.WriteLine(StringResources.OutcomeErrorMessage);
                        Environment.Exit(-1);
                    }
                }
            }

            if (_param.ContainsKey("f") || _param.ContainsKey("format"))
            {
                var formatValue = _param.ContainsKey("f") ?
                    _param["f"] :
                    _param["format"];
                if (int.TryParse(formatValue, out _))
                {
                    Console.Error.WriteLine(StringResources.ErrorMessage);
                    Console.WriteLine(StringResources.FormatErrorMessage);
                    Environment.Exit(-1);
                }
            }
        }
        Utility util;
        List<TestSuiteInfo> testSuites;

        /// <summary>
        /// Initialize
        /// </summary>
        public void Init()
        {
            util = new Utility();
            testSuites = util.TestSuiteIntroduction.SelectMany(tsFamily => tsFamily).ToList();
        }

        /// <summary>
        /// Load test suite.
        /// </summary>
        /// <param name="filename">Filename of the profile</param>
        public void LoadTestSuite(string filename, string testSuiteFolder)
        {
            TestSuiteInfo tsinfo;
            using (ProfileUtil profile = ProfileUtil.LoadProfile(filename))
            {
                tsinfo = testSuites.Find(ts => ts.TestSuiteName == profile.Info.TestSuiteName);
                if (tsinfo == null)
                {
                    throw new ArgumentException(String.Format(StringResources.UnknownTestSuiteMessage, profile.Info.TestSuiteName));
                }
                string testSuiteFolderBin = Path.Combine(testSuiteFolder, "Bin");
                tsinfo.TestSuiteFolder = testSuiteFolder;
                tsinfo.TestSuiteVersion = LoadTestsuiteVersion(testSuiteFolderBin);
            }

            util.LoadTestSuiteConfig(tsinfo);
            util.LoadTestSuiteAssembly();

            string newProfile;
            if (util.TryUpgradeProfileSettings(filename, out newProfile))
            {
                Console.WriteLine(String.Format(StringResources.PtmProfileUpgraded, newProfile));
                filename = newProfile;
            }
            util.LoadProfileSettings(filename);
        }

        /// <summary>
        /// Get test cases using profile
        /// </summary>
        /// <param name="selectedOnly">True to run only the test cases selected in the run page.</param>
        public List<TestCase> GetTestCases(bool selectedOnly)
        {
            List<TestCase> testCaseList = new List<TestCase>();
            foreach (TestCase testcase in util.GetSelectedCaseList())
            {
                if (!selectedOnly || testcase.IsChecked) testCaseList.Add(testcase);
            }
            return testCaseList;
        }

        /// <summary>
        /// Get test cases using category paramter
        /// </summary>
        /// <param name="category">The specific category of test cases to run</param>
        public List<TestCase> GetTestCases(List<string> categories)
        {
            List<TestCase> testCaseList = new List<TestCase>();

            Filter filter = new Filter(categories, RuleType.Selector);
            foreach (TestCase testcase in util.GetTestSuite().TestCaseList)
            {
                if (filter.FilterTestCase(testcase.Category)) testCaseList.Add(testcase);
            }
            return testCaseList;
        }

        /// <summary>
        /// Run test suite
        /// </summary>
        /// <param name="testCases">The list of test cases to run</param>
        public void RunTestSuite(List<TestCase> testCases, string testCasePath)
        {
            using (ProgressBar progress = new ProgressBar())
            {
                util.InitializeTestEngine();

                int total = testCases.Count;
                int executed = 0;

                Logger logger = util.GetLogger();
                var caseSet = new HashSet<string>();
                logger.GroupByOutcome.UpdateTestCaseList = (group, runningcase) =>
                {
                    if (caseSet.Contains(runningcase.Name))
                    {
                        return;
                    }
                    caseSet.Add(runningcase.Name);
                    executed++;
                    progress.Update((double)executed / total, $"({executed}/{total}) Executing {runningcase.Name}");
                };

                progress.Update(0, "Loading test suite");
                util.SyncRunByCases(testCases);
            }

            Console.Clear();
            Console.WriteLine(StringResources.FinishRunningTips);
        }

        /// <summary>
        /// Abort test suite
        /// </summary>
        public void AbortExecution()
        {
            Console.CursorVisible = true;
            util.AbortExecution();
        }

        /// <summary>
        /// Print plain test report to console.
        /// </summary>
        public void PrintTestReport(IEnumerable<Outcome> outcomes)
        {
            bool pass = outcomes.Contains(Outcome.Pass);
            bool fail = outcomes.Contains(Outcome.Fail);
            bool inconclusive = outcomes.Contains(Outcome.Inconclusive);

            var testcases = util.SelectTestCases(pass, fail, inconclusive, false);
            PlainReport report = new PlainReport(testcases);
            Console.Write(report.GetPlainReport());
        }

        /// <summary>
        /// Save test report to disk.
        /// </summary>
        public void SaveTestReport(string filename, ReportFormat format, IEnumerable<Outcome> outcomes)
        {
            bool pass = outcomes.Contains(Outcome.Pass);
            bool fail = outcomes.Contains(Outcome.Fail);
            bool inconclusive = outcomes.Contains(Outcome.Inconclusive);

            var testcases = util.SelectTestCases(pass, fail, inconclusive, false);

            TestReport report = TestReport.GetInstance(format.ToString(), testcases);
            if (report == null)
            {
                throw new Exception(String.Format(StringResources.UnknownReportFormat, format.ToString()));
            }

            report.ExportReport(filename);
        }

        /// <summary>
        /// load test suite version info
        /// </summary>
        /// <param name="testsuitepath">The path of test case</param>
        public string LoadTestsuiteVersion(string testsuitepath)
        {
            List<string> paths = new List<string>();
            var allVersions = new HashSet<string>();

            // Version info is saved in ".version" file.
            var versionPath = Directory.GetFiles(testsuitepath, ".version", SearchOption.TopDirectoryOnly);
            if (versionPath != null && versionPath.Length == 1)
            {
                var version = File.ReadAllLines(versionPath[0]);
                if (version != null)
                {
                    return version[0];
                }
            }

            throw new Exception(StringResources.InvalidTestSuiteVersion);
        }
    }
}

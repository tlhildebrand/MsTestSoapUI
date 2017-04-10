using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MsTestSoapUI
{
    [TestClass]
    public class RunSoapUITests
    {
        private TestContext _testContext;
        public TestContext TestContext
        {
            get { return this._testContext; }
            set { this._testContext = value; }
        }
        
        /// <summary>
        /// Test Soap Stuff
        /// </summary>
        [TestMethod]
        [DeploymentItem(@"test-service.xml", "TestData")]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "TestData\\test-service.xml", "testCase", DataAccessMethod.Random)]
        public void RunSoapServiceTest()
        {
            var testCaseName = TestContext.DataRow["name"].ToString();
            RunSoapUItest(@"TestData\test-service.xml", "BasicHttpBinding_ILicenseService TestSuite", testCaseName);
        }

        /// <summary>
        /// Runs soapUI test named testName
        /// </summary>
        private void RunSoapUItest(string soapProject, string testSuiteName, string testName)
        {
            const string fileName = "cmd.exe";
            var soapProjectFileName = Path.GetFullPath(soapProject);

            var arguments = $"/C testrunner.bat -s\"{testSuiteName}\" -c\"{testName}\" \"{soapProjectFileName}\" ";

            //SoapUI bin directory
            var soapHome = System.Configuration.ConfigurationManager.AppSettings["SoapUIHome"];

            //Start a process and hook up the in/output
            var proces = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    WorkingDirectory = soapHome,
                    CreateNoWindow = true,
                    ErrorDialog = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                },
                EnableRaisingEvents = true
            };

            //Pipe the output to Console.WriteLine
            proces.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);

            //Store the errors in a stringbuilder
            var errorBuilder = new StringBuilder();
            proces.ErrorDataReceived += (sender, args) =>
            {
                if (args != null && args.Data != null)
                {
                    errorBuilder.AppendLine(args.Data);
                }
            };

            proces.Start();
            proces.BeginOutputReadLine();
            proces.BeginErrorReadLine();

            //Wait for SoapUI to finish
            proces.WaitForExit();

            //Fail the test if anything fails
            var errorMessage = errorBuilder.ToString();
            if (!string.IsNullOrEmpty(errorMessage))
            {
                Assert.Fail("Test with name '{0}' failed. {1} {2}", testName, Environment.NewLine, errorMessage);
            }
        }
    }
}

using System;
using System.IO;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Configuration.Detectors;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    [TestFixture]
    public class EnvironmentBasedProjectDetectorTests
    {
        private static System.IO.DirectoryInfo CreateTempDir()
        {
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "fuis_" + System.Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(path);
            return new System.IO.DirectoryInfo(path);
        }
        [Test]
        public void DetectProjectPath_FromEnvironmentVariable_ReturnsPath()
        {
            var tempFile = Path.GetTempFileName();
            var csproj = Path.ChangeExtension(tempFile, ".csproj");
            File.Move(tempFile, csproj);

            Environment.SetEnvironmentVariable("PROJECT_PATH", csproj);

            try
            {
                var detector = new EnvironmentBasedProjectDetector();
                var ctx = new ProjectDetectionContext
                {
                    CurrentDirectory = Directory.GetCurrentDirectory(),
                    TestAssemblyDirectory = Directory.GetCurrentDirectory(),
                    ProjectType = "csproj"
                };

                var result = detector.DetectProjectPath(ctx);

                Assert.That(result, Is.EqualTo(csproj));
            }
            finally
            {
                Environment.SetEnvironmentVariable("PROJECT_PATH", null);
                File.Delete(csproj);
            }
        }

        [Test]
        public void DetectProjectPath_FromConfigFile_ReturnsPath()
        {
            var tempDir = CreateTempDir();
            var projectPath = Path.Combine(tempDir.FullName, "MyApp.csproj");
            File.WriteAllText(projectPath, "<Project></Project>");

            var configPath = Path.Combine(tempDir.FullName, "fluentui.config.json");
            File.WriteAllText(configPath, "{ \"projectPath\": \"MyApp.csproj\" }");

            var detector = new EnvironmentBasedProjectDetector();
            var ctx = new ProjectDetectionContext
            {
                CurrentDirectory = tempDir.FullName,
                TestAssemblyDirectory = tempDir.FullName,
                ProjectType = "csproj"
            };

            var result = detector.DetectProjectPath(ctx);
            Assert.That(result, Is.EqualTo(projectPath));

            Directory.Delete(tempDir.FullName, true);
        }

        [Test]
        public void DetectProjectPath_FromAdditionalPaths_ReturnsPath()
        {
            var tempDir = CreateTempDir();
            var projectPath = Path.Combine(tempDir.FullName, "WebApp.csproj");
            File.WriteAllText(projectPath, "<Project></Project>");

            var detector = new EnvironmentBasedProjectDetector();
            var ctx = new ProjectDetectionContext
            {
                CurrentDirectory = Directory.GetCurrentDirectory(),
                TestAssemblyDirectory = Directory.GetCurrentDirectory(),
                ProjectType = "csproj"
            };
            ctx.AdditionalSearchPaths.Add(tempDir.FullName);

            var result = detector.DetectProjectPath(ctx);
            Assert.That(result, Is.EqualTo(projectPath));

            Directory.Delete(tempDir.FullName, true);
        }
    }
}
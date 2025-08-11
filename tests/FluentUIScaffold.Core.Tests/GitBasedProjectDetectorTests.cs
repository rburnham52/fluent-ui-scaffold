using System;
using System.IO;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Configuration.Detectors;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    [TestFixture]
    public class GitBasedProjectDetectorTests
    {
        [Test]
        public void DetectProjectPath_InGitRepo_FindsProjectInSrc()
        {
            var repoRoot = Directory.CreateTempSubdirectory();
            Directory.CreateDirectory(Path.Combine(repoRoot.FullName, ".git"));
            var srcDir = Path.Combine(repoRoot.FullName, "src");
            Directory.CreateDirectory(srcDir);
            var projectPath = Path.Combine(srcDir, "App.csproj");
            File.WriteAllText(projectPath, "<Project></Project>");

            var detector = new GitBasedProjectDetector();
            var ctx = new ProjectDetectionContext
            {
                CurrentDirectory = Path.Combine(repoRoot.FullName, "src"),
                ProjectType = "csproj"
            };

            var result = detector.DetectProjectPath(ctx);
            Assert.That(result, Is.EqualTo(projectPath));

            Directory.Delete(repoRoot.FullName, true);
        }

        [Test]
        public void DetectProjectPath_NoGit_FallsBackToDirectorySearch()
        {
            var root = Directory.CreateTempSubdirectory();
            var pkg = Path.Combine(root.FullName, "package.json");
            File.WriteAllText(pkg, "{ } ");

            var detector = new GitBasedProjectDetector();
            var ctx = new ProjectDetectionContext
            {
                CurrentDirectory = root.FullName,
                ProjectType = "package.json"
            };

            var result = detector.DetectProjectPath(ctx);
            Assert.That(result, Is.EqualTo(pkg));

            Directory.Delete(root.FullName, true);
        }
    }
}
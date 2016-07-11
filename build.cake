#tool "xunit.runner.console"

var target = Argument("target", "Build");
var configuration = Argument("configuration", "Debug");

var testResultsDir = "./test-results";
var solutionName = "Cake.EnvXmlTransform/Cake.EnvXmlTransform.sln";
var testAssemblies = new List<FilePath> { MakeAbsolute(File("./Cake.EnvXmlTransform/Cake.EnvXmlTransform.Tests/bin/" + configuration + "/Cake.EnvXmlTransform.Tests.dll")) };

Task("Build")
    .IsDependentOn("Restore-Nuget-Packages")
    .Does(() => {
        Information("Building {0}", solutionName);
        MSBuild(solutionName, settings =>
            settings.SetPlatformTarget(PlatformTarget.MSIL)
                    .WithProperty("TreatWarningsAsErrors", "true")
                    .SetConfiguration(configuration));
    });

Task("Run-Tests")
    .IsDependentOn("Build")
    .Does(() => {
        CreateDirectory(testResultsDir);
        var settings = new XUnit2Settings {
            XmlReportV1 = true,
            NoAppDomain = true,
            OutputDirectory = testResultsDir,
        };
        XUnit2(testAssemblies, settings);
    });

Task("Restore-Nuget-Packages")
    .IsDependentOn("Clean")
    .Does(() => {
        Information("Restoring {0}", solutionName);
        NuGetRestore(solutionName, new NuGetRestoreSettings());
    });

Task("Clean")
    .Does(() => {
        Information("Cleaning old files");
        var binDirs = GetDirectories("Cake.EnvXmlTransform/Cake.EnvXmlTransform/**/bin");
        var objDirs = GetDirectories("Cake.EnvXmlTransform/Cake.EnvXmlTransform/**/obj");
        CleanDirectories(binDirs);
        CleanDirectories(objDirs);
    });

RunTarget(target);

#tool "xunit.runner.console"
#tool "GitVersion.CommandLine"

var target = Argument("target", "Build");
var configuration = Argument("configuration", "Debug");

var artifacts = MakeAbsolute(Directory("./artifacts"));
var testResultsDir = MakeAbsolute(Directory("./test-results"));
var buildDirectory = MakeAbsolute(Directory(artifacts + "/build"));
var solutionName = MakeAbsolute(File("./src/Cake.EnvXmlTransform.sln"));
var testAssemblies = new List<FilePath> { MakeAbsolute(File("./src/Cake.EnvXmlTransform/Cake.EnvXmlTransform.Tests/bin/" + configuration + "/Cake.EnvXmlTransform.Tests.dll")) };

SolutionProject project = null;

var semVersion = GitVersion().SemVer;

Setup(ctx => {
    CreateDirectory(artifacts);
    var solution = ParseSolution(solutionName);
    project = solution.Projects.FirstOrDefault(x => x.Name == "Cake.EnvXmlTransform");
    Information("Building version {0}", semVersion, project.Name);
});

Task("Build")
    .IsDependentOn("Update-AppVeyor-Build-Number")
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
        var binDirs = GetDirectories("./src/Cake.EnvXmlTransform/Cake.EnvXmlTransform/**/bin");
        var objDirs = GetDirectories("./src/Cake.EnvXmlTransform/Cake.EnvXmlTransform/**/obj");
        CleanDirectory(artifacts);
        CleanDirectories(binDirs);
        CleanDirectories(objDirs);
    });

Task("Copy-Files")
    .IsDependentOn("Build")
    .Does(() => {
        CreateDirectory(buildDirectory);
        var files = GetFiles(project.Path.GetDirectory() +"/bin/" + configuration + "/netstandard2.0/" + project.Name +".*");
        CopyFiles(files, buildDirectory);
        CopyFileToDirectory("./src/lib/Cake.XdtTransform.dll", buildDirectory);
        CopyFileToDirectory("./src/lib/Microsoft.Web.XmlTransform.dll", buildDirectory);
    });

Task("Package")
    .IsDependentOn("Clean")
    .IsDependentOn("Copy-Files")
    .Does(() => {
        CreateDirectory(Directory(artifacts + "/packages"));

        var nuspec = project.Path.GetDirectory() +"/" + project.Name +".nuspec";
        Information("Packing: {0}", nuspec);
        NuGetPack(nuspec, new NuGetPackSettings {
            BasePath = buildDirectory,
            NoPackageAnalysis = false,
            Version = semVersion,
            OutputDirectory = Directory(artifacts + "/packages"),
            IconUrl = new Uri("https://cdn.jsdelivr.net/gh/cake-contrib/graphics/png/cake-contrib-medium.png"),
            Properties = new Dictionary<string, string>() { { "Configuration", configuration } },
            ReleaseNotes = new List<string>() { "Updates using new Cake Contrib icon. References Cake.Core & Cake.Common 0.33.0"
        });
});

Task("Update-AppVeyor-Build-Number")
    .WithCriteria(() => AppVeyor.IsRunningOnAppVeyor)
    .Does(() => {
        AppVeyor.UpdateBuildVersion(semVersion + " | " + AppVeyor.Environment.Build.Number);
});

RunTarget(target);
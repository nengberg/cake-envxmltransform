#tool "xunit.runner.console"
#tool "GitVersion.CommandLine"

var target = Argument("target", "Build");
var configuration = Argument("configuration", "Debug");

var artifacts = MakeAbsolute(Directory("./artifacts"));
var testResultsDir = MakeAbsolute(Directory("./test-results"));
var buildDirectory = MakeAbsolute(Directory(artifacts + "/build"));
var solutionName = MakeAbsolute(File("Cake.EnvXmlTransform/Cake.EnvXmlTransform.sln"));
var versionAssemblyInfo = MakeAbsolute(File("./VersionAssemblyInfo.cs"));
var testAssemblies = new List<FilePath> { MakeAbsolute(File("./Cake.EnvXmlTransform/Cake.EnvXmlTransform.Tests/bin/" + configuration + "/Cake.EnvXmlTransform.Tests.dll")) };
SolutionProject project = null;
GitVersion versionInfo = null;

Setup(ctx => {
    CreateDirectory(artifacts);
    var solution = ParseSolution(solutionName);
    project = solution.Projects.FirstOrDefault(x => x.Name == "Cake.EnvXmlTransform");
});

Task("Build")
    .IsDependentOn("Restore-Nuget-Packages")
    .IsDependentOn("Update-Version-Info")
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
        var binDirs = GetDirectories("Cake.EnvXmlTransform/Cake.EnvXmlTransform/**/bin");
        var objDirs = GetDirectories("Cake.EnvXmlTransform/Cake.EnvXmlTransform/**/obj");
        CleanDirectory(artifacts);
        CleanDirectories(binDirs);
        CleanDirectories(objDirs);
    });

Task("Copy-Files")
    .IsDependentOn("Build")
    .Does(() => {
        CreateDirectory(buildDirectory);
        var files = GetFiles(project.Path.GetDirectory() +"/bin/" + configuration + "/" + project.Name +".*");
        CopyFiles(files, buildDirectory);
    });

Task("Update-Version-Info")
    .IsDependentOn("CreateVersionAssemblyInfo")
    .Does(() => {
        versionInfo = GitVersion(new GitVersionSettings {
            UpdateAssemblyInfo = true,
            UpdateAssemblyInfoFilePath = versionAssemblyInfo
        });

        if(versionInfo != null) {
            Information("Version: {0}", versionInfo.FullSemVer);
        } else {
            throw new Exception("Unable to determine version");
        }
    });

Task("CreateVersionAssemblyInfo")
    .WithCriteria(() => !FileExists(versionAssemblyInfo))
    .Does(() =>
{
    Information("Creating version assembly info");
    CreateAssemblyInfo(versionAssemblyInfo, new AssemblyInfoSettings {
        Version = "0.0.0.1",
        FileVersion = "0.0.0.1",
        InformationalVersion = "",
    });
});

Task("Package")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore-NuGet-Packages")
    .IsDependentOn("Update-Version-Info")
    .IsDependentOn("Copy-Files")
    .Does(() => {
        CreateDirectory(Directory(artifacts + "/packages"));

        var nuspec = project.Path.GetDirectory() +"/" + project.Name +".nuspec";
        Information("Packing: {0}", nuspec);
        NuGetPack(nuspec, new NuGetPackSettings {
            BasePath = buildDirectory,
            NoPackageAnalysis = false,
            Version = versionInfo.NuGetVersionV2,
            OutputDirectory = Directory(artifacts +"/packages"),
            Properties = new Dictionary<string, string>() { { "Configuration", configuration } }
        });
});

Task("Update-AppVeyor-Build-Number")
    .IsDependentOn("Update-Version-Info")
    .WithCriteria(() => AppVeyor.IsRunningOnAppVeyor)
    .Does(() => {
        AppVeyor.UpdateBuildVersion(versionInfo.FullSemVer +" | " +AppVeyor.Environment.Build.Number);
});

RunTarget(target);

using Cake.Common;
using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Clean;
using Cake.Common.Tools.DotNet.Publish;
using Cake.Core;
using Cake.Frosting;
using Cake.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using Vintagestory.API.Common;

namespace CakeBuild;

public static class Program
{
    public static int Main(string[] args)
    {
        return new CakeHost()
            .UseContext<BuildContext>()
            .Run(args);
    }
}

public class BuildContext : FrostingContext
{
    // In dependency order
    public static readonly string[] AllProjects = { "Flock.Shared", "Flock.Attributes", "Flock.Traits" };

    public string BuildConfiguration { get; }
    public bool SkipJsonValidation { get; }
    public string[] Projects { get; }

    public BuildContext(ICakeContext context)
        : base(context)
    {
        BuildConfiguration = context.Argument("configuration", "Release");
        SkipJsonValidation = context.Argument("skipJsonValidation", false);
        var project = context.Argument("project", "");
        Projects = string.IsNullOrWhiteSpace(project)
            ? AllProjects
            : project.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var unknown = Projects.Except(AllProjects).ToArray();
        if (unknown.Length > 0)
        {
            throw new Exception($"Unknown project(s): {string.Join(", ", unknown)}. Expected one of: {string.Join(", ", AllProjects)}");
        }
    }
}

[TaskName("ValidateJson")]
public sealed class ValidateJsonTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        if (context.SkipJsonValidation)
        {
            return;
        }
        foreach (var project in context.Projects)
        {
            var assetsDir = $"../{project}/assets";
            if (!context.DirectoryExists(assetsDir))
            {
                continue;
            }
            var jsonFiles = context.GetFiles($"{assetsDir}/**/*.json");
            foreach (var file in jsonFiles)
            {
                try
                {
                    var json = File.ReadAllText(file.FullPath);
                    JToken.Parse(json);
                }
                catch (JsonException ex)
                {
                    throw new Exception($"Validation failed for JSON file: {file.FullPath}{Environment.NewLine}{ex.Message}", ex);
                }
            }
        }
    }
}

[TaskName("Build")]
[IsDependentOn(typeof(ValidateJsonTask))]
public sealed class BuildTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        foreach (var project in context.Projects)
        {
            context.DotNetClean($"../{project}/{project}.csproj",
                new DotNetCleanSettings
                {
                    Configuration = context.BuildConfiguration
                });


            context.DotNetPublish($"../{project}/{project}.csproj",
                new DotNetPublishSettings
                {
                    Configuration = context.BuildConfiguration
                });
        }
    }
}

[TaskName("Package")]
[IsDependentOn(typeof(BuildTask))]
public sealed class PackageTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.EnsureDirectoryExists("../Releases");
        foreach (var project in context.Projects)
        {
            var modInfo = context.DeserializeJsonFromFile<ModInfo>($"../{project}/modinfo.json");
            var releaseDir = $"../Releases/{modInfo.ModID}";
            context.EnsureDirectoryExists(releaseDir);
            context.CleanDirectory(releaseDir);

            // Copy the publish output, but never bundle a sibling Flock.* mod assembly
            var publishFiles = context.GetFiles($"../{project}/bin/{context.BuildConfiguration}/Mods/publish/*");
            foreach (var other in BuildContext.AllProjects.Where(p => p != project))
            {
                publishFiles -= context.GetFiles($"../{project}/bin/{context.BuildConfiguration}/Mods/publish/{other}.*");
            }
            context.CopyFiles(publishFiles, releaseDir);

            if (context.DirectoryExists($"../{project}/assets"))
            {
                context.CopyDirectory($"../{project}/assets", $"{releaseDir}/assets");
            }
            context.CopyFile($"../{project}/modinfo.json", $"{releaseDir}/modinfo.json");
            if (context.FileExists($"../{project}/modicon.png"))
            {
                context.CopyFile($"../{project}/modicon.png", $"{releaseDir}/modicon.png");
            }
            context.Zip(releaseDir, $"../Releases/{modInfo.ModID}_{modInfo.Version}.zip");
        }
    }
}

[TaskName("Default")]
[IsDependentOn(typeof(PackageTask))]
public class DefaultTask : FrostingTask
{
}

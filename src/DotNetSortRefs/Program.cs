using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetSortRefs
{
    [Command(
        Name = "dotnet sort-refs",
        FullName = "A .NET Core global tool to alphabetically sort package references in csproj or fsproj.")]
    [VersionOptionFromMember(MemberName = nameof(GetVersion))]
    internal class Program : CommandBase
    {
        private readonly IFileSystem _fileSystem;
        private readonly IReporter _reporter;

        public Program(IFileSystem fileSystem, IReporter reporter)
        {
            _fileSystem = fileSystem;
            _reporter = reporter;
        }

        static int Main(string[] args)
        {
            using (var services = new ServiceCollection()
                .AddSingleton<IConsole, PhysicalConsole>()
                .AddSingleton<IReporter>(provider => new ConsoleReporter(provider.GetService<IConsole>()))
                .AddSingleton<IFileSystem, FileSystem>()
                .BuildServiceProvider())
            {
                var app = new CommandLineApplication<Program>
                {
                    UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.Throw
                };

                app.Conventions
                    .UseDefaultConventions()
                    .UseConstructorInjection(services);
                
                try
                {
                    return app.Execute(args);
                }
                catch (UnrecognizedCommandParsingException)
                {
                    app.ShowHelp();
                    return 1;
                }
            }
        }

        [Argument(0, Description =
            "The path to a .csproj, .fsproj or directory. If a directory is specified, all .csproj and .fsproj files within folder tree will be processed. If none specified, it will use the current directory.")]
        private string Path { get; set; }

        [Option(CommandOptionType.NoValue, Description = "Specifies whether to inspect and return a non-zero exit code if one or more projects have non-sorted package references.",
            ShortName = "i", LongName = "inspect")]
        private bool IsInspect { get; set; } = false;

        private static string GetVersion() => typeof(Program)
            .Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            .InformationalVersion;

        private async Task<int> OnExecute(CommandLineApplication app, IConsole console)
        {
            try
            {
                if (string.IsNullOrEmpty(Path))
                    Path = _fileSystem.Directory.GetCurrentDirectory();

                if (!(_fileSystem.File.Exists(Path) || _fileSystem.Directory.Exists(Path)))
                {
                    _reporter.Error("Directory or file does not exist.");
                    return 1;
                }

                var projFiles = new List<string>();

                if (_fileSystem.File.Exists(Path))
                {
                    projFiles.Add(Path);
                }
                else
                {
                    projFiles = _fileSystem.Directory.GetFiles(Path, "*.csproj", SearchOption.AllDirectories)
                        .Concat(_fileSystem.Directory.GetFiles(Path, "*.fsproj", SearchOption.AllDirectories)).ToList();
                }

                if (projFiles.Count == 0)
                {
                    _reporter.Error(".csproj or .fsproj files not found.");
                    return 1;
                }

                var projFilesWithNonSortedReferences = await Inspect(projFiles);

                if (IsInspect)
                {
                    Console.WriteLine("Running inspection...");
                    PrintInspectionResults(projFiles, projFilesWithNonSortedReferences);
                    return projFilesWithNonSortedReferences.Count > 0 ? 1 : 0;
                }
                else
                {
                    Console.WriteLine("Running sort package references...");
                    return await SortReferences(projFilesWithNonSortedReferences);
                }
            }
            catch (Exception e)
            {
                _reporter.Error(e.StackTrace);
                return 1;
            }
        }

        private static async Task<List<string>> Inspect(IEnumerable<string> projFiles)
        {
            var projFilesWithNonSortedReferences = new List<string>();

            foreach (var proj in projFiles)
            {
                using (var sw = new StringWriter())
                {
                    var doc = XDocument.Parse(System.IO.File.ReadAllText(proj));

                    var itemGroups = doc.XPathSelectElements("//ItemGroup[PackageReference|Reference]");

                    foreach (var itemGroup in itemGroups)
                    {
                        var references = itemGroup.XPathSelectElements("PackageReference|Reference")
                            .Select(x => x.Attribute("Include")?.Value.ToLowerInvariant()).ToList();

                        if (references.Count <= 1) continue;

                        var sortedReferences = references.OrderBy(x => x).ToList();

                        var result = references.SequenceEqual(sortedReferences);

                        if (!result && !projFilesWithNonSortedReferences.Contains(proj))
                        {
                            projFilesWithNonSortedReferences.Add(proj);
                        }
                    }
                }
            }

            return await Task.FromResult(projFilesWithNonSortedReferences);
        }

        private void PrintInspectionResults(IEnumerable<string> projFiles,
            ICollection<string> projFilesWithNonSortedReferences)
        {
            foreach (var proj in projFiles)
            {
                if (projFilesWithNonSortedReferences.Contains(proj))
                {
                    _reporter.Error($"» {proj} X");
                }
                else
                {
                    _reporter.Output($"» {proj} ✓");
                }
            }
        }

        private async Task<int> SortReferences(IEnumerable<string> projFiles)
        {
            var xslt = GetXslTransform();

            foreach (var proj in projFiles)
            {
                _reporter.Output($"» {proj}");

                using (var sw = new StringWriter())
                {
                    var doc = XDocument.Parse(System.IO.File.ReadAllText(proj));
                    xslt.Transform(doc.CreateNavigator(), null, sw);
                    File.WriteAllText(proj, sw.ToString());
                }
            }

            return await Task.FromResult(0);
        }

        private static XslCompiledTransform GetXslTransform()
        {   
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("DotNetSortRefs.Sort.xsl"))
            using (var reader = XmlReader.Create(stream))
            {
                var xslt = new XslCompiledTransform();
                xslt.Load(reader);
                return xslt;
            }
        }
    }
}

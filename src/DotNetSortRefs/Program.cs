using System;
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
                    ThrowOnUnexpectedArgument = false
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
                    _reporter.Error("Directory or file does not exist");
                    return 1;
                }

                var xslt = GetXslTransform();
                
                if (_fileSystem.File.Exists(Path))
                {
                    SortReferences(Path, console, xslt);
                    return 0;
                }

                var projFiles = _fileSystem.Directory.GetFiles(Path, "*.csproj", SearchOption.AllDirectories)
                    .Concat(_fileSystem.Directory.GetFiles(Path, "*.fsproj", SearchOption.AllDirectories))
                    .ToArray();

                if (projFiles.Length == 0)
                {
                    _reporter.Error(".csproj or .fsproj files not found in ");
                }
                else
                {
                    foreach (var proj in projFiles)
                    {
                        SortReferences(proj, console, xslt);
                    }
                }
            }
            catch (Exception e)
            {
                _reporter.Error(e.StackTrace);
                return 1;
            }

            return await Task.FromResult(0);
        }

        private static void SortReferences(string path, IConsole console, XslCompiledTransform xslt)
        {
            console.Write($"» {path}");
            console.WriteLine();
            
            using (StringWriter sw = new StringWriter())
            {
                var doc = XDocument.Parse(System.IO.File.ReadAllText(path));
                xslt.Transform(doc.CreateNavigator(), null, sw);
                File.WriteAllText(path, sw.ToString());
            }
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

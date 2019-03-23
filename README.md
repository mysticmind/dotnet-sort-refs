# dotnet-sort-refs
[![Build status](https://ci.appveyor.com/api/projects/status/xse0bia9olr5shxr?svg=true)](https://ci.appveyor.com/project/BabuAnnamalai/dotnet-sort-refs) [![NuGet Version](https://badgen.net/nuget/v/dotnet-sort-refs)](https://www.nuget.org/packages/dotnet-sort-refs/)

A .NET Core global tool to alphabetically sort package references in your .NET Core and .NET Standard projects.

## Why use this tool?
References and package references in a project file are the most updated parts. Sorting the references helps with the following:
- Easier merges on source control (git). Without sorting the package references in the project file, you may end up with more merge conflicts to fix. 
- It will be easier to go through the list of package references if you manually edit the file or view changes using a diff tool.

## Installation
```bash
dotnet tool install --global dotnet-sort-refs
```

## Usage
```text
dotnet sort-refs [arguments] [options]

Arguments:
  Path          The path to a .csproj, .fsproj or directory. If a directory is specified, all .csproj and .fsproj files within folder tree will be processed. If none specified, it will use the current directory.

Options:
  --version     Show version information
  -?|-h|--help  Show help information
```

Note: `dotnet sort-refs` and `dotnet-sort-refs` are valid usages to run the tool.

# dotnet-sort-refs
[![Build status](https://ci.appveyor.com/api/projects/status/xse0bia9olr5shxr?svg=true)](https://ci.appveyor.com/project/BabuAnnamalai/dotnet-sort-refs) [![NuGet Version](https://badgen.net/nuget/v/dotnet-sort-refs)](https://www.nuget.org/packages/dotnet-sort-refs/)

A .NET Core global tool to alphabetically sort package references in your .NET Core and .NET Standard projects.

If you have benefitted from this library and has saved you a bunch of time, please feel free to buy me a coffee!<br>
<a href="https://github.com/sponsors/mysticmind" target="_blank"><img height="30" style="border:0px;height:36px;" src="https://img.shields.io/static/v1?label=GitHub Sponsor&message=%E2%9D%A4&logo=GitHub" border="0" alt="GitHub Sponsor" /></a> <a href="https://ko-fi.com/babuannamalai" target="_blank"><img height="36" style="border:0px;height:36px;" src="https://cdn.ko-fi.com/cdn/kofi4.png?v=3" border="0" alt="Buy Me a Coffee at ko-fi.com" /></a> <a href="https://www.buymeacoffee.com/babuannamalai" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/default-orange.png" alt="Buy Me A Coffee" height="36" width="174"></a>

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
  -i|--inspect  Specifies whether to inspect and return a non-zero exit code if one or more projects have non-sorted package references.
```

Note: `dotnet sort-refs` and `dotnet-sort-refs` are valid usages to run the tool.

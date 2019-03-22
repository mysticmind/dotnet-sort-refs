# dotnet-sort-refs
A .NET Core global tool to alphabetically sort package references in your .NET Core and .NET Standard projects.

## Installation
This initial version is not yet available in NuGet hence the only way to install is to clone the repository, build and install it as a global tool from local source

```bash
# Goto folder src\DotNetSortRefs
dotnet pack
dotnet tool install --global --add-source .\nupkg dotnet-sort-refs
```

## Usage
```text
Usage: dotnet sort-refs [arguments] [options]

Arguments:
  Path          The path to a .csproj, .fsproj or directory. If a directory is specified, all .csproj and .fsproj files within folder tree will be processed. If none specified, it will use the current directory.

Options:
  --version     Show version information
  -?|-h|--help  Show help information
```
using System.Text;
using Visual_Block_Studio.DTOs.Explorer;
using Visual_Block_Studio.Enums;

namespace Visual_Block_Studio.Services;

public static class MSBuildProjectGenerator
{
    private static string Escape(string value) => value.Replace("\\", "\\\\").Replace("\"", "\\\"");

    public static string EmitCsProj(VBSProjectDto dto)
    {
        var csharpFiles = dto.LayoutFiles
            .SelectMany(item => item.Files)
            .Where(f => f.Type == ProjectFileType.Cs)
            .ToList();

        var xamlFiles = dto.LayoutFiles
            .SelectMany(item => item.Files)
            .Where(f => f.Type == ProjectFileType.Xaml)
            .ToList();

        var sb = new StringBuilder();

        sb.AppendLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
        sb.AppendLine();
        sb.AppendLine("  <PropertyGroup>");
        sb.AppendLine("    <OutputType>WinExe</OutputType>");
        sb.AppendLine("    <TargetFramework>net10.0-windows10.0.19041.0</TargetFramework>");
        sb.AppendLine("    <TargetPlatformMinVersion>10.0.17763.0</TargetPlatformMinVersion>");
        sb.AppendLine("    <RootNamespace>" + Escape(dto.Name) + "</RootNamespace>");
        sb.AppendLine("    <ApplicationManifest>app.manifest</ApplicationManifest>");
        sb.AppendLine("    <Platforms>" + Escape(dto.TargetArchitecture) + "</Platforms>");
        sb.AppendLine("    <PlatformTarget>" + Escape(dto.TargetArchitecture) + "</PlatformTarget>");
        sb.AppendLine("    <UseWinUI>true</UseWinUI>");
        sb.AppendLine("    <EnableDefaultApplicationDefinition>true</EnableDefaultApplicationDefinition>");
        sb.AppendLine("    <EnableDefaultPageItems>true</EnableDefaultPageItems>");
        sb.AppendLine("  </PropertyGroup>");
        sb.AppendLine();
        sb.AppendLine("  <ItemGroup>");

        foreach (var file in csharpFiles)
            sb.AppendLine($"    <Compile Include=\"{Escape(file.FilePath)}\" />");

        sb.AppendLine("  </ItemGroup>");
        sb.AppendLine();
        sb.AppendLine("  <ItemGroup>");

        foreach (var file in xamlFiles)
            sb.AppendLine($"    <Page Include=\"{Escape(file.FilePath)}\" />");

        sb.AppendLine("  </ItemGroup>");
        sb.AppendLine();
        sb.AppendLine("  <ItemGroup>");
        sb.AppendLine("    <PackageReference Include=\"Microsoft.WindowsAppSDK\" Version=\"1.8.260602001\" />");
        sb.AppendLine("  </ItemGroup>");
        sb.AppendLine();
        sb.AppendLine("</Project>");

        var csProjDir = Path.GetDirectoryName(dto.CsProjFilePath)
            ?? throw new InvalidOperationException(
                $"Could not resolve directory for '{dto.CsProjFilePath}'.");

        Directory.CreateDirectory(csProjDir);

        File.WriteAllText(dto.CsProjFilePath, sb.ToString());

        return dto.CsProjFilePath;
    }
}

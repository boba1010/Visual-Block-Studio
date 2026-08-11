using System.Text;
using System.Text.Json;
using Visual_Block_Studio.Contracts;
using Visual_Block_Studio.DTOs;
using Visual_Block_Studio.DTOs.Explorer;
using Visual_Block_Studio.Enums;
using Visual_Block_Studio.Helpers;
using Visual_Block_Studio.Json;
using Visual_Block_Studio.Models;
using Visual_Block_Studio.Models.Explorer;

namespace Visual_Block_Studio.Services;

// TODO: CodeBlockService
public class VBSProjectManager(XamlCodeBlockService xamlCodeService,
    CodeBlockService codeBlockService,
    TemplateProjectService templateService)
{
    private const string BlockFileExtension = ".vblock";
    private const string VBSProjectExtension = ".vbsproj";
    private const string VcxProjExtension = ".vcxproj";
    private const string XamlExtension = ".xaml";
    private const string CSharpExtension = ".cs";
    private const string GeneratedCSharpExtension = ".g.cs";
    private const string CppExtension = ".cpp";
    private const string HeaderExtension = ".h";
    private const string IdlExtension = ".idl";
    private static readonly string[] LayoutFileExtensions =
    [
        BlockFileExtension,
        XamlExtension,
        CSharpExtension,
        CppExtension,
        HeaderExtension,
        IdlExtension
    ];

    public VBSProject Load(string filePath) => LoadDto(filePath).MapDtoToProj();

    public bool TryLoad(string filePath, out VBSProject proj)
    {
        try
        {
            proj = Load(filePath);
            return true;
        }
        catch (Exception)
        {
            proj = null!;
            return false;
        }
    }

    public VBSProjectDto LoadDto(string filePath)
    {
        using var fileStream = File.OpenRead(filePath);

        var dto = JsonSerializer.Deserialize(fileStream, VBSJsonContext.Default.VBSProjectDto)
            ?? throw new InvalidOperationException("Invalid visual block file.");

        ResolveProjectDynamically(dto, filePath);

        return dto;
    }

    public bool TryLoadDto(string filePath, out VBSProjectDto dto)
    {
        try
        {
            dto = LoadDto(filePath);
            return true;
        }
        catch (Exception)
        {
            dto = null!;
            return false;
        }
    }

    public bool Save(string filePath, VBSProject proj)
    {
        try
        {
            var dto = proj.MapProjToDto();

            using var fileStream = File.Create(filePath);
            JsonSerializer.Serialize(fileStream, dto, VBSJsonContext.Default.VBSProjectDto);

            // Keep the read-only mirrors in sync
            EmitGeneratedConstants(dto);
            EmitVcxProj(dto);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public VBSProject CreateProject(CreateProjectRequest request)
    {
        if (string.IsNullOrEmpty(request.FilePath))
            throw new ArgumentException("The file path cannot be null or empty.", nameof(request));

        if (string.IsNullOrEmpty(request.ProjectName))
            throw new ArgumentException("The project name cannot be null or empty.", nameof(request));

        string projDir = Path.GetDirectoryName(request.FilePath) ?? request.FilePath;
        Directory.CreateDirectory(projDir);

        var projectFileName = Path.GetFileName(request.FilePath);
        var projectFilePath = Path.GetFullPath(request.FilePath);
        var projectFileStem = Path.GetFileNameWithoutExtension(projectFileName);

        VBSProjectDto project = new()
        {
            Name = request.ProjectName,
            TargetArchitecture = "x64",
            OutputPath = Path.Combine(projDir, "bin"),
            MemoryLimitBytes = 2_147_483_648,

            FileName = projectFileName,
            FilePath = projectFilePath,

            VcxProjFileName = projectFileStem + VcxProjExtension,
            VcxProjFilePath = Path.Combine(projDir, projectFileStem + VcxProjExtension),

            GeneratedFileName = projectFileName + GeneratedCSharpExtension,
            GeneratedFilePath = Path.Combine(projDir, "obj", projectFileName + GeneratedCSharpExtension),

            LayoutFiles = []
        };

        var mainWindowBlock = new WindowBlock
        {
            BaseClass = request.ProjectName,
            WindowName = "MainWindow",
            Namespace = request.ProjectName,
            Position = new(50, 50),
            Size = new(800, 600)
        };

        mainWindowBlock.Children.Add(new ButtonBlock
        {
            Position = new(80, 100),
            Size = new(140, 50)
        });

        var blockFilePath = Path.Combine(projDir, "MainWindow" + BlockFileExtension);

        SaveBlockFile(blockFilePath, mainWindowBlock);
        var xamlFilePath = EmitGeneratedXaml(blockFilePath, mainWindowBlock, projDir);

        // The block file is now tracked as its own independent entry rather
        // than being embedded (via BlockFileName/BlockFilePath) inside the
        // generated file's DTO.
        var blockFile = CreateProjectFile(blockFilePath);
        var xamlFile = CreateProjectFile(xamlFilePath);

        // An item's "single" typing is driven by the generated file, not the
        // block file that produced it.
        project.LayoutFiles.Add(new ProjectItemDto
        {
            Name = "MainWindow",
            Type = ResolveSingleItemType(xamlFile),
            Files = [blockFile, xamlFile]
        });

        // Write the .vbsproj JSON — this is the actual project file
        using var fileStream = File.Create(project.FilePath);
        JsonSerializer.Serialize(fileStream, project, VBSJsonContext.Default.VBSProjectDto);

        // Write the .g.cs mirror and .vcxproj — neither drives the project itself
        EmitGeneratedConstants(project);
        EmitVcxProj(project);

        return project.MapDtoToProj();
    }

    /// <summary>
    /// Emits a .g.cs file that mirrors the .vbsproj JSON as readable constant
    /// fields. This file is generated purely for human readability (e.g.
    /// browsing project settings in an editor with syntax highlighting).
    ///
    /// IMPORTANT: This file is completely read-only and has NO effect on the
    /// project whatsoever. It is not compiled, not referenced, and not parsed
    /// by anything. Editing it does nothing — the .vbsproj JSON file is the
    /// only source of truth. This file is fully overwritten every time the
    /// project is saved.
    /// </summary>
    public string EmitGeneratedConstants(VBSProjectDto dto)
    {
        var generatedDir = Path.GetDirectoryName(dto.GeneratedFilePath)
            ?? throw new InvalidOperationException($"Could not resolve directory for '{dto.GeneratedFilePath}'.");

        Directory.CreateDirectory(generatedDir);

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated>");
        sb.AppendLine("//");
        sb.AppendLine($"//     This file is a READ-ONLY, human-readable mirror of '{dto.FileName}'.");
        sb.AppendLine("//     It exists purely for readability and has NO EFFECT on the project.");
        sb.AppendLine("//     It is not compiled, not referenced, and not parsed by anything.");
        sb.AppendLine("//     Editing this file does absolutely nothing — the .vbsproj JSON file");
        sb.AppendLine("//     is the only source of truth, and this file is fully overwritten");
        sb.AppendLine("//     every time the project is saved.");
        sb.AppendLine("//");
        sb.AppendLine("// </auto-generated>");
        sb.AppendLine();
        sb.AppendLine("internal static class ProjectConstants");
        sb.AppendLine("{");
        sb.AppendLine($"    public const string Name = \"{Escape(dto.Name)}\";");
        sb.AppendLine($"    public const string TargetArchitecture = \"{Escape(dto.TargetArchitecture)}\";");
        sb.AppendLine($"    public const uint MemoryLimitBytes = {dto.MemoryLimitBytes};");
        sb.AppendLine($"    public const string OutputPath = \"{Escape(dto.OutputPath)}\";");
        sb.AppendLine($"    public const string FileName = \"{Escape(dto.FileName)}\";");
        sb.AppendLine($"    public const string FilePath = \"{Escape(dto.FilePath)}\";");
        sb.AppendLine($"    public const string VcxProjFileName = \"{Escape(dto.VcxProjFileName)}\";");
        sb.AppendLine($"    public const string VcxProjFilePath = \"{Escape(dto.VcxProjFilePath)}\";");
        sb.AppendLine($"    public const string GeneratedFileName = \"{Escape(dto.GeneratedFileName)}\";");
        sb.AppendLine($"    public const string GeneratedFilePath = \"{Escape(dto.GeneratedFilePath)}\";");
        sb.AppendLine();
        sb.Append("    public static readonly LayoutItemInfo[] LayoutFiles = ");
        AppendLayoutFiles(dto.LayoutFiles, sb);
        sb.AppendLine(";");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("internal readonly struct LayoutItemInfo");
        sb.AppendLine("{");
        sb.AppendLine("    public LayoutItemInfo(string name, string type, LayoutFileInfo[] files)");
        sb.AppendLine("    {");
        sb.AppendLine("        Name = name;");
        sb.AppendLine("        Type = type;");
        sb.AppendLine("        Files = files;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public string Name { get; }");
        sb.AppendLine("    public string Type { get; }");
        sb.AppendLine("    public LayoutFileInfo[] Files { get; }");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("internal readonly struct LayoutFileInfo");
        sb.AppendLine("{");
        sb.AppendLine("    public LayoutFileInfo(string fileName, string filePath, string type)");
        sb.AppendLine("    {");
        sb.AppendLine("        FileName = fileName;");
        sb.AppendLine("        FilePath = filePath;");
        sb.AppendLine("        Type = type;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public string FileName { get; }");
        sb.AppendLine("    public string FilePath { get; }");
        sb.AppendLine("    public string Type { get; }");
        sb.AppendLine("}");

        File.WriteAllText(dto.GeneratedFilePath, sb.ToString());

        return dto.GeneratedFilePath;
    }

    /// <summary>
    /// Emits a minimal starting .vcxproj scaffold covering the generated
    /// C++/WinRT files tracked in LayoutFiles. This is a starting point only:
    /// WinUI/WinRT-specific properties (WindowsTargetPlatformVersion,
    /// C++/WinRT NuGet imports, PlatformToolset, ApplicationType, etc.) still
    /// need to be added before this can actually build a WinUI 3 app.
    /// </summary>
    public string EmitVcxProj(VBSProjectDto dto)
    {
        // Cpp/Header/Idl are now distinct ProjectFileType values, so each
        // group can be filtered by type directly instead of re-deriving it
        // from the file extension. Block (and Xaml) files are naturally
        // excluded since they don't match any of these three types.
        var clCompile = dto.LayoutFiles
            .SelectMany(item => item.Files)
            .Where(f => f.Type == ProjectFileType.Cpp)
            .ToList();

        var clInclude = dto.LayoutFiles
            .SelectMany(item => item.Files)
            .Where(f => f.Type == ProjectFileType.Header)
            .ToList();

        var midl = dto.LayoutFiles
            .SelectMany(item => item.Files)
            .Where(f => f.Type == ProjectFileType.Idl)
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine("<!-- Auto-generated scaffold. Add WinUI/WinRT-specific settings before building. -->");
        sb.AppendLine("<Project DefaultTargets=\"Build\" ToolsVersion=\"Current\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">");
        sb.AppendLine("  <ItemGroup Label=\"ProjectConfigurations\">");
        sb.AppendLine($"    <ProjectConfiguration Include=\"Debug|{dto.TargetArchitecture}\">");
        sb.AppendLine("      <Configuration>Debug</Configuration>");
        sb.AppendLine($"      <Platform>{dto.TargetArchitecture}</Platform>");
        sb.AppendLine("    </ProjectConfiguration>");
        sb.AppendLine($"    <ProjectConfiguration Include=\"Release|{dto.TargetArchitecture}\">");
        sb.AppendLine("      <Configuration>Release</Configuration>");
        sb.AppendLine($"      <Platform>{dto.TargetArchitecture}</Platform>");
        sb.AppendLine("    </ProjectConfiguration>");
        sb.AppendLine("  </ItemGroup>");
        sb.AppendLine("  <PropertyGroup Label=\"Globals\">");
        sb.AppendLine($"    <ProjectGuid>{{{Guid.NewGuid()}}}</ProjectGuid>");
        sb.AppendLine($"    <RootNamespace>{Escape(dto.Name)}</RootNamespace>");
        sb.AppendLine("    <Keyword>Win32Proj</Keyword>");
        sb.AppendLine("  </PropertyGroup>");
        sb.AppendLine("  <Import Project=\"$(VCTargetsPath)\\Microsoft.Cpp.Default.props\" />");
        sb.AppendLine("  <PropertyGroup Label=\"Configuration\">");
        sb.AppendLine("    <ConfigurationType>Application</ConfigurationType>");
        sb.AppendLine("  </PropertyGroup>");
        sb.AppendLine("  <Import Project=\"$(VCTargetsPath)\\Microsoft.Cpp.props\" />");
        sb.AppendLine("  <ItemGroup>");
        foreach (var file in clCompile)
            sb.AppendLine($"    <ClCompile Include=\"{Escape(file.FilePath)}\" />");
        sb.AppendLine("  </ItemGroup>");
        sb.AppendLine("  <ItemGroup>");
        foreach (var file in clInclude)
            sb.AppendLine($"    <ClInclude Include=\"{Escape(file.FilePath)}\" />");
        sb.AppendLine("  </ItemGroup>");
        sb.AppendLine("  <ItemGroup>");
        foreach (var file in midl)
            sb.AppendLine($"    <Midl Include=\"{Escape(file.FilePath)}\" />");
        sb.AppendLine("  </ItemGroup>");
        sb.AppendLine("  <Import Project=\"$(VCTargetsPath)\\Microsoft.Cpp.targets\" />");
        sb.AppendLine("</Project>");

        var vcxProjDir = Path.GetDirectoryName(dto.VcxProjFilePath)
            ?? throw new InvalidOperationException($"Could not resolve directory for '{dto.VcxProjFilePath}'.");
        Directory.CreateDirectory(vcxProjDir);

        File.WriteAllText(dto.VcxProjFilePath, sb.ToString());

        return dto.VcxProjFilePath;
    }

    private static bool HasExtension(string fileName, string extension) =>
        string.Equals(Path.GetExtension(fileName), extension, StringComparison.OrdinalIgnoreCase);

    private static bool IsBlockFile(string path) => HasExtension(path, BlockFileExtension);

    private static ProjectFileDto CreateProjectFile(string path)
    {
        return new()
        {
            FileName = Path.GetFileName(path),
            FilePath = path,
            Type = ResolveFileType(Path.GetExtension(path))
        };
    }

    private static void AppendLayoutFiles(List<ProjectItemDto> layoutFiles, StringBuilder sb)
    {
        if (layoutFiles.Count == 0)
        {
            sb.Append("[]");
            return;
        }

        sb.AppendLine("new LayoutItemInfo[]");
        sb.AppendLine("    {");
        for (int i = 0; i < layoutFiles.Count; i++)
        {
            var item = layoutFiles[i];
            sb.AppendLine($"        new(\"{Escape(item.Name)}\", \"{item.Type}\", new LayoutFileInfo[]");
            sb.AppendLine("        {");
            for (int j = 0; j < item.Files.Count; j++)
            {
                var file = item.Files[j];
                sb.Append($"            new(\"{Escape(file.FileName)}\", \"{Escape(file.FilePath)}\", \"{file.Type}\")");
                sb.AppendLine(j < item.Files.Count - 1 ? "," : "");
            }
            sb.Append("        })");
            sb.AppendLine(i < layoutFiles.Count - 1 ? "," : "");
        }
        sb.Append("    }");
    }

    private static string Escape(string value) => value.Replace("\\", "\\\\").Replace("\"", "\\\"");

    /// <summary>
    /// Finds the independent block-file entry within a project item's Files,
    /// if one is present.
    /// </summary>
    public static ProjectFileDto? GetBlockFile(ProjectItemDto item) =>
        item.Files.FirstOrDefault(f => f.Type == ProjectFileType.Block);

    public XamlBlock LoadBlockFile(ProjectFileDto file)
    {
        using var fs = File.OpenRead(file.FilePath);

        var document = JsonSerializer.Deserialize(fs, VBSJsonContext.Default.VBlockDocumentDto);

        if (document?.Root is null)
        {
            throw new InvalidOperationException($"Invalid block file '{file.FilePath}'.");
        }

        return (XamlBlock)document.Root.MapDtoToBlock();
    }

    public void SaveBlockFile(string blockFilePath, XamlBlock block)
    {
        var document = new VBlockDocumentDto
        {
            Root = block.MapBlockToDto()
        };

        using var fs = File.Create(blockFilePath);

        JsonSerializer.Serialize(fs, document, VBSJsonContext.Default.VBlockDocumentDto);
    }

    public string EmitGeneratedXaml(string blockFilePath, XamlBlock block, string projDir)
    {
        var xamlFileName = Path.GetFileNameWithoutExtension(blockFilePath) + XamlExtension;
        var xamlPath = Path.Combine(projDir, xamlFileName);

        var xamlText = block.GenerateXaml();
        File.WriteAllText(xamlPath, xamlText);

        return xamlPath;
    }

    private void ResolveProjectDynamically(VBSProjectDto dto, string projectFilePath)
    {
        var absoluteProjectPath = Path.GetFullPath(projectFilePath);
        var projectDir = Path.GetDirectoryName(absoluteProjectPath)
            ?? throw new InvalidOperationException($"Could not resolve directory for '{absoluteProjectPath}'.");
        var projectFileStem = Path.GetFileNameWithoutExtension(absoluteProjectPath);

        dto.FilePath = absoluteProjectPath;
        dto.FileName = Path.GetFileName(absoluteProjectPath);

        dto.VcxProjFileName = projectFileStem + VcxProjExtension;
        dto.VcxProjFilePath = Path.Combine(projectDir, dto.VcxProjFileName);

        dto.GeneratedFileName = dto.FileName + GeneratedCSharpExtension;
        dto.GeneratedFilePath = Path.Combine(projectDir, "obj", dto.GeneratedFileName);

        dto.LayoutFiles = ScanLayoutFiles(projectDir);
    }

    /// <summary>
    /// Scans the project directory for tracked files and groups them into
    /// logical items by base name. Each generated file (.xaml/.cpp/.h/.idl)
    /// is grouped alongside the .vblock it was generated from — the block
    /// file is now its own independent entry in Files (ordered first),
    /// rather than being embedded into every sibling file's DTO.
    /// </summary>
    private static List<ProjectItemDto> ScanLayoutFiles(string projectDir)
    {
        var files = Directory
            .EnumerateFiles(projectDir, "*.*", SearchOption.AllDirectories)
            .Where(IsTrackedFile)
            .Where(f => !IsUnderBuildOutputFolder(f, projectDir))
            .ToList();

        return files
            .GroupBy(GetLogicalItemName)
            .Select(group =>
            {
                // Block file first for stable, predictable ordering.
                var orderedFiles = group
                    .OrderByDescending(IsBlockFile)
                    .Select(CreateProjectFile)
                    .ToList();

                return new ProjectItemDto
                {
                    Name = group.Key,
                    Type = ResolveProjectItemType(orderedFiles),
                    Files = orderedFiles
                };
            })
            .ToList();
    }

    private static bool IsTrackedFile(string path)
    {
        return LayoutFileExtensions.Contains(
            Path.GetExtension(path),
            StringComparer.OrdinalIgnoreCase);
    }

    private static string GetLogicalItemName(string path)
    {
        var file = Path.GetFileName(path);

        foreach (var extension in new[]
        {
            ".xaml.cpp",
            ".xaml.h",
            ".xaml",
            IdlExtension,
            BlockFileExtension,
            CppExtension,
            CSharpExtension,
            HeaderExtension
        })
        {
            if (file.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                return file[..^extension.Length];
            }
        }

        return Path.GetFileNameWithoutExtension(file);
    }

    private static bool IsUnderBuildOutputFolder(string filePath, string projectDir)
    {
        var relative = Path.GetRelativePath(projectDir, filePath);
        var firstSegment = relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)[0];
        return firstSegment.Equals("obj", StringComparison.OrdinalIgnoreCase)
            || firstSegment.Equals("bin", StringComparison.OrdinalIgnoreCase);
    }

    // An item is "single" when it has exactly one *generated* file (the
    // block file that produced it doesn't count towards this, since it's
    // now tracked as its own independent entry). Otherwise it's a
    // FilesParent grouping multiple generated files.
    private static ProjectItemType ResolveProjectItemType(List<ProjectFileDto> files)
    {
        var generatedFiles = files.Where(f => f.Type != ProjectFileType.Block).ToList();

        return generatedFiles.Count == 1
            ? ResolveSingleItemType(generatedFiles[0])
            : ProjectItemType.FilesParent;
    }

    // ProjectItemType has no dedicated "single Idl" or "single Block" member,
    // so those fall back to SingleCppFile. Block items shouldn't normally
    // reach here anyway, since ResolveProjectItemType filters block files out
    // before calling this.
    private static ProjectItemType ResolveSingleItemType(ProjectFileDto file) => file.Type switch
    {
        ProjectFileType.Xaml => ProjectItemType.SingleXamlFile,
        ProjectFileType.Header => ProjectItemType.SingleHeaderFile,
        ProjectFileType.Cpp => ProjectItemType.SingleCppFile,
        ProjectFileType.Idl => ProjectItemType.SingleCppFile,
        _ => ProjectItemType.SingleCppFile
    };

    // ProjectFileType now distinguishes Cpp, Xaml, Block, Header, and Idl.
    // Anything unmatched (currently just .cs) falls back to Cpp.
    private static ProjectFileType ResolveFileType(string extension) => extension.ToLowerInvariant() switch
    {
        XamlExtension => ProjectFileType.Xaml,
        BlockFileExtension => ProjectFileType.Block,
        HeaderExtension => ProjectFileType.Header,
        IdlExtension => ProjectFileType.Idl,
        CppExtension => ProjectFileType.Cpp,
        _ => ProjectFileType.Cpp
    };
}
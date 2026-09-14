using System.Text.Json;
using Visual_Block_Studio.Contracts;
using Visual_Block_Studio.DTOs;
using Visual_Block_Studio.DTOs.Explorer;
using Visual_Block_Studio.Enums;
using Visual_Block_Studio.Helpers;
using Visual_Block_Studio.Models;
using Visual_Block_Studio.Models.CodeBlocks;
using Visual_Block_Studio.Models.XamlBlocks;

namespace Visual_Block_Studio.Services;

public class TemplateProjectService
{
    private const string BlockFileExtension = ".vblock";
    private const string CsProjExtension = ".csproj";
    private const string XamlExtension = ".xaml";
    private const string CSharpExtension = ".cs";
    private const string GeneratedCSharpExtension = ".g.cs";

    private readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public async Task<VBSProjectDto> CreateWinUITemplateProjectAsync(CreateProjectRequest request)
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

            CsProjFileName = projectFileStem + CsProjExtension,
            CsProjFilePath = Path.Combine(projDir, projectFileStem + CsProjExtension),

            GeneratedFileName = projectFileName + GeneratedCSharpExtension,
            GeneratedFilePath = Path.Combine(projDir, "obj", projectFileName + GeneratedCSharpExtension),

            LayoutFiles = []
        };


        var windowXamlFilePaths = await CreateVisualBlockAndSourceAsync<WindowBlock>(request.ProjectName, projDir);
        var windowCSharpFilePaths = await CreateVisualBlockAndSourceAsync<WindowCodeBlock>(request.ProjectName, projDir);

        var windowXamlBlockFile = CreateProjectFile(windowXamlFilePaths.blockPath);
        var windowXamlFile = CreateProjectFile(windowXamlFilePaths.sourcePath);
        var windowCSharpFile = CreateProjectFile(windowCSharpFilePaths.sourcePath);
        var windowCSharpBlockFile = CreateProjectFile(windowCSharpFilePaths.blockPath);

        project.LayoutFiles.Add(new ProjectItemDto
        {
            Name = "MainWindow",
            Type = ProjectItemType.FilesParent,
            Files = [windowXamlBlockFile, windowXamlFile, windowCSharpFile, windowCSharpBlockFile]
        });

        return project;
    }

    private async Task<(string sourcePath, string blockPath)> CreateVisualBlockAndSourceAsync<T>(string @namespace, string projDir) where T : Block, new()
    {
        if (typeof(T) == typeof(WindowBlock))
        {
            var mainWindowBlock = new WindowBlock
            {
                BaseClass = "MainWindow",
                WindowName = "MainWindow",
                Namespace = @namespace,
                Position = new(50, 50),
                Size = new(800, 600)
            };

            var blockFilePath = Path.Combine(projDir, "MainWindow" + BlockFileExtension);
            await SaveBlockFile(blockFilePath, mainWindowBlock);

            return (await EmitGeneratedXaml(blockFilePath, mainWindowBlock, projDir), blockFilePath);
        }
        else if (typeof(T) == typeof(WindowCodeBlock))
        {
            var namespaceBlock = new NamespaceBlock
            {
                Name = @namespace,
                Position = new(50, 50),
                Size = new(800, 600),
            };

            var mainWindowBlock = new WindowCodeBlock
            {
                Size = new(600, 300),
            };

            namespaceBlock.Members.Add(mainWindowBlock);

            var blockFilePath = Path.Combine(projDir, "MainWindow" + XamlExtension + CSharpExtension + BlockFileExtension);
            await SaveBlockFile(blockFilePath, mainWindowBlock);

            return (await EmitGeneratedCSharp(blockFilePath, namespaceBlock, projDir), blockFilePath);
        }

        throw new NotSupportedException($"Block type '{typeof(T).Name}' is not supported.");
    }

    private static ProjectFileDto CreateProjectFile(string path)
    {
        return new()
        {
            FileName = Path.GetFileName(path),
            FilePath = path,
            Type = ResolveFileType(Path.GetExtension(path))
        };
    }

    private static ProjectFileType ResolveFileType(string extension) => extension.ToLowerInvariant() switch
    {
        XamlExtension => ProjectFileType.Xaml,
        BlockFileExtension => ProjectFileType.Block,
        CSharpExtension => ProjectFileType.Cs,
        _ => ProjectFileType.Cs
    };

    public async Task SaveBlockFile(string blockFilePath, Block block)
    {
        var document = new VBlockDocumentDto
        {
            Root = block.MapBlockToDto()
        };

        using var fs = File.Create(blockFilePath);

        JsonSerializer.Serialize(fs, document, Options);
    }

    public async Task<string> EmitGeneratedXaml(string blockFilePath, XamlBlock block, string projDir)
    {
        var xamlFileName = Path.GetFileNameWithoutExtension(blockFilePath) + XamlExtension;
        var xamlPath = Path.Combine(projDir, xamlFileName);

        var xamlText = block.GenerateXaml();
        File.WriteAllText(xamlPath, xamlText);

        return xamlPath;
    }

    public async Task<string> EmitGeneratedCSharp(string blockFilePath, CodeBlock block, string projDir)
    {
        if (string.IsNullOrWhiteSpace(blockFilePath))
            throw new ArgumentException("The block file path cannot be null or empty.", nameof(blockFilePath));
        ArgumentNullException.ThrowIfNull(block);
        if (string.IsNullOrWhiteSpace(projDir))
            throw new ArgumentException("The project directory cannot be null or empty.", nameof(projDir));

        Directory.CreateDirectory(projDir);

        var blockFileName = Path.GetFileNameWithoutExtension(blockFilePath);

        if (blockFileName.EndsWith(CSharpExtension, StringComparison.OrdinalIgnoreCase))
            blockFileName = blockFileName[..^CSharpExtension.Length];

        var sourceFilePath = Path.Combine(projDir, blockFileName + CSharpExtension);
        await File.WriteAllTextAsync(sourceFilePath, block.BuildCode());

        return sourceFilePath;
    }
}

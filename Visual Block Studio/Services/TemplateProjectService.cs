using System.Text.Json;
using Visual_Block_Studio.Contracts;
using Visual_Block_Studio.DTOs;
using Visual_Block_Studio.DTOs.Explorer;
using Visual_Block_Studio.Enums;
using XamlBlocksCompiler.Generation;
using XamlBlocksCompiler.Syntax;
using XamlBlocksCompiler.Syntax.Controls;

namespace Visual_Block_Studio.Services;

public class TemplateProjectService
{
    private const string XamlBlockFileExtension = ".xamlvblock";
    private const string CodeBlockFileExtension = ".csvblock";
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

            FileName = projectFileName,
            FilePath = projectFilePath,

            CsProjFileName = projectFileStem + CsProjExtension,
            CsProjFilePath = Path.Combine(projDir, projectFileStem + CsProjExtension),

            GeneratedFileName = projectFileName + GeneratedCSharpExtension,
            GeneratedFilePath = Path.Combine(projDir, "obj", projectFileName + GeneratedCSharpExtension),

            LayoutFiles = []
        };


        var windowXamlFilePaths = await CreateVisualBlockAndSourceAsync<XamlWindowSyntax>(request.ProjectName, projDir);

        var windowXamlBlockFile = CreateProjectFile(windowXamlFilePaths.blockPath);
        var windowXamlFile = CreateProjectFile(windowXamlFilePaths.sourcePath);

        project.LayoutFiles.Add(new()
        {
            Name = "MainWindow",
            Type = ProjectItemType.FilesParent,
            Files = [windowXamlBlockFile, windowXamlFile]
        });

        return project;
    }

    private async Task<(string sourcePath, string blockPath)> CreateVisualBlockAndSourceAsync<T>(string @namespace, string projDir) where T : XamlElementSyntax, new()
    {
        if (typeof(T) == typeof(XamlWindowSyntax))
        {
            var mainWindowBlock = new XamlWindowSyntax
            {
            };

            var blockFilePath = Path.Combine(projDir, "MainWindow" + XamlBlockFileExtension);
            await SaveBlockFile(blockFilePath, mainWindowBlock);

            return (await EmitGeneratedXaml(blockFilePath, mainWindowBlock, projDir), blockFilePath);
        }
        //else if (typeof(T) == typeof(WindowCodeBlock))
        //{
        //    var namespaceBlock = new NamespaceBlock
        //    {
        //        Name = @namespace,
        //        Position = new(50, 50),
        //        Size = new(800, 600),
        //    };

        //    var mainWindowBlock = new WindowCodeBlock
        //    {
        //        Size = new(600, 300),
        //    };

        //    namespaceBlock.Members.Add(mainWindowBlock);

        //    var blockFilePath = Path.Combine(projDir, "MainWindow" + XamlExtension + CSharpExtension + BlockFileExtension);
        //    await SaveBlockFile(blockFilePath, mainWindowBlock);

        //    return (await EmitGeneratedCSharp(blockFilePath, namespaceBlock, projDir), blockFilePath);
        //}

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
        XamlBlockFileExtension => ProjectFileType.Block,
        CSharpExtension => ProjectFileType.Cs,
        _ => ProjectFileType.Cs
    };

    public async Task SaveBlockFile(string blockFilePath, XamlElementSyntax control)
    {
        var document = new VBlockDocumentDto
        {
            Root = control
        };

        using var fs = File.Create(blockFilePath);

        JsonSerializer.Serialize(fs, document, Options);
    }

    public async Task<string> EmitGeneratedXaml(string blockFilePath, XamlElementSyntax control, string projDir)
    {
        var xamlFileName = Path.GetFileNameWithoutExtension(blockFilePath) + XamlExtension;
        var xamlPath = Path.Combine(projDir, xamlFileName);

        XamlCodeGenerator generator = new();
        var xaml = generator.Generate(control);

        await File.WriteAllTextAsync(xamlFileName, xaml);

        return xamlPath;
    }

    //public async Task<string> EmitGeneratedCSharp(string blockFilePath, CodeBlock block, string projDir)
    //{
    //    if (string.IsNullOrWhiteSpace(blockFilePath))
    //        throw new ArgumentException("The block file path cannot be null or empty.", nameof(blockFilePath));
    //    ArgumentNullException.ThrowIfNull(block);
    //    if (string.IsNullOrWhiteSpace(projDir))
    //        throw new ArgumentException("The project directory cannot be null or empty.", nameof(projDir));

    //    Directory.CreateDirectory(projDir);

    //    var blockFileName = Path.GetFileNameWithoutExtension(blockFilePath);

    //    if (blockFileName.EndsWith(CSharpExtension, StringComparison.OrdinalIgnoreCase))
    //        blockFileName = blockFileName[..^CSharpExtension.Length];

    //    var sourceFilePath = Path.Combine(projDir, blockFileName + CSharpExtension);
    //    await File.WriteAllTextAsync(sourceFilePath, block.BuildCode());

    //    return sourceFilePath;
    //}
}

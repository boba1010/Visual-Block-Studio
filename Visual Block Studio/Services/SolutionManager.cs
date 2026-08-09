using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using Visual_Block_Studio.Contracts;
using Visual_Block_Studio.DTOs.Explorer;
using Visual_Block_Studio.Helpers;
using Visual_Block_Studio.Json;
using Visual_Block_Studio.Models;
using Visual_Block_Studio.Models.Explorer;
using Path = System.IO.Path;

namespace Visual_Block_Studio.Services;

public class SolutionManager(VBSProjectManager projManager)
{
    private const string VBSProjectExtension = ".vbsproj";

    // .vbsslnx — the JSON solution file (source of truth)
    private const string SolutionFileExtension = ".vbsslnx";

    // .slnx — generated, read-only XML mirror listing project references
    private const string SlnxExtension = ".slnx";

    public Solution Load(string filePath)
    {
        using var fileStream = File.OpenRead(filePath);

        var dto = JsonSerializer.Deserialize(fileStream, VBSJsonContext.Default.SolutionDto)
            ?? throw new InvalidOperationException("Invalid solution file.");

        ResolveSolutionDynamically(dto, filePath);

        return dto.MapDtoToSlnx();
    }

    public bool TryLoad(string filePath, out Solution solution)
    {
        try
        {
            solution = Load(filePath);
            return true;
        }
        catch (Exception)
        {
            solution = null!;
            return false;
        }
    }

    public bool Save(string filePath, Solution solution)
    {
        try
        {
            var dto = solution.MapSlnxToDto();

            using var fileStream = File.Create(filePath);
            JsonSerializer.Serialize(fileStream, dto, VBSJsonContext.Default.SolutionDto);

            // Keep the .slnx mirror in sync — it has no effect on the solution itself
            EmitGeneratedSlnx(dto);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public Solution CreateSolution(CreateSolutionRequest request)
    {
        if (string.IsNullOrEmpty(request.FilePath))
            throw new ArgumentException("The file path cannot be null or empty.", nameof(request));

        if (string.IsNullOrEmpty(request.FileName))
            throw new ArgumentException("The solution name cannot be null or empty.", nameof(request));

        // Visual Studio layout: <root>/<SolutionName>/<SolutionName>.vbsslnx
        var solutionDir = Path.GetFullPath(Path.Combine(request.FilePath, request.FileName));
        Directory.CreateDirectory(solutionDir);

        var vbsslnxFileName = request.FileName + SolutionFileExtension;
        var vbsslnxFilePath = Path.Combine(solutionDir, vbsslnxFileName);

        // Visual Studio layout: <root>/<SolutionName>/<ProjectName>/<ProjectName>.vbsproj
        // Default project name = solution name, matching VS's default new-project flow.
        var projectDir = Path.Combine(solutionDir, request.FileName);
        var projectFilePath = Path.Combine(projectDir, request.FileName + VBSProjectExtension);

        // Actually scaffold the project on disk (MainWindow.vblock, generated .xaml, .vcxproj, etc.)
        projManager.CreateProject(new CreateProjectRequest
        {
            FilePath = projectFilePath,
            ProjectName = request.FileName,
            FileName = request.FileName,
        });

        // Reload the DTO from what was just written, rather than trusting an
        // in-memory copy — same "disk is the source of truth" pattern used
        // everywhere else in this class.
        if (!projManager.TryLoadDto(projectFilePath, out var projectDto))
            throw new InvalidOperationException($"Failed to load newly created project at '{projectFilePath}'.");

        var dto = new SolutionDto
        {
            FileName = vbsslnxFileName,
            FilePath = vbsslnxFilePath,

            SlnxFileName = request.FileName + SlnxExtension,
            SlnxFilePath = Path.Combine(solutionDir, request.FileName + SlnxExtension),

            Projects = [projectDto]
        };

        // Write the .vbsslnx JSON — this is the actual solution file
        using var fileStream = File.Create(dto.FilePath);
        JsonSerializer.Serialize(fileStream, dto, VBSJsonContext.Default.SolutionDto);

        // Write the .slnx mirror alongside it
        EmitGeneratedSlnx(dto);

        return dto.MapDtoToSlnx();
    }

    /// <summary>
    /// Emits a .slnx file listing project references as relative paths.
    /// This file exists for tooling/readability alongside the .vbsslnx JSON;
    /// the .vbsslnx file remains the only source of truth and this file is
    /// fully overwritten every time the solution is saved.
    /// </summary>
    public string EmitGeneratedSlnx(SolutionDto dto)
    {
        var solutionDir = Path.GetDirectoryName(dto.FilePath)
            ?? throw new InvalidOperationException($"Could not resolve directory for '{dto.FilePath}'.");

        Directory.CreateDirectory(solutionDir);

        var solutionElement = new XElement("Solution");

        foreach (var project in dto.Projects)
        {
            if (string.IsNullOrWhiteSpace(project.FilePath))
                continue;

            var relativePath = Path.GetRelativePath(solutionDir, project.FilePath).Replace('\\', '/');
            solutionElement.Add(new XElement("Project", new XAttribute("Path", relativePath)));
        }

        var doc = new XDocument(solutionElement);
        var settings = new XmlWriterSettings
        {
            Indent = true,
            Encoding = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
        };

        using var writer = XmlWriter.Create(dto.SlnxFilePath, settings);
        doc.Save(writer);

        return dto.SlnxFilePath;
    }

    /// <summary>
    /// Resolves each project reference by fully reloading its .vbsproj file
    /// from disk via VBSProjectManager, so the solution reflects live project
    /// state (layout files, icons, generated paths) instead of a stale copy
    /// embedded in the solution's own JSON. References that no longer resolve
    /// to a valid project file are dropped.
    /// </summary>
    private void ResolveSolutionDynamically(SolutionDto dto, string solutionFilePath)
    {
        var absoluteSolutionPath = Path.GetFullPath(solutionFilePath);
        dto.FilePath = absoluteSolutionPath;
        dto.FileName = Path.GetFileName(absoluteSolutionPath);

        var solutionDir = Path.GetDirectoryName(absoluteSolutionPath)
            ?? throw new InvalidOperationException($"Could not resolve directory for '{absoluteSolutionPath}'.");
        var solutionFileStem = Path.GetFileNameWithoutExtension(absoluteSolutionPath);

        dto.SlnxFileName = solutionFileStem + SlnxExtension;
        dto.SlnxFilePath = Path.Combine(solutionDir, dto.SlnxFileName);

        var resolvedProjects = new List<VBSProjectDto>();

        foreach (var project in dto.Projects)
        {
            if (string.IsNullOrWhiteSpace(project.FilePath))
                continue;

            var absoluteProjectPath = Path.IsPathRooted(project.FilePath)
                ? project.FilePath
                : Path.GetFullPath(Path.Combine(solutionDir, project.FilePath));

            // Fully reload the project file rather than trusting the stale
            // embedded copy — this re-derives layout files, icons, etc.
            if (!projManager.TryLoadDto(absoluteProjectPath, out var resolvedProject))
                continue; // missing, moved, or corrupt — drop rather than fail the whole load

            resolvedProjects.Add(resolvedProject);
        }

        dto.Projects = resolvedProjects;
    }
}
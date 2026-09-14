using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using Visual_Block_Studio.Contracts;
using Visual_Block_Studio.DTOs.Explorer;
using Visual_Block_Studio.Helpers;
using Visual_Block_Studio.Models.Explorer;
using Path = System.IO.Path;

namespace Visual_Block_Studio.Services;

public class SolutionManager(VBSProjectManager projManager)
{
    private const string VBSProjectExtension = ".vbsproj";
    private const string SolutionFileExtension = ".vbsslnx";
    private const string SlnxExtension = ".slnx";
    private readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public Solution Load(string filePath)
    {
        using var fileStream = File.OpenRead(filePath);

        var dto = JsonSerializer.Deserialize<SolutionDto>(fileStream)
            ?? throw new InvalidOperationException("Invalid solution file.");

        var projects = ResolveSolutionDynamically(dto, filePath);

        return new()
        {
            FileName = dto.FileName,
            FilePath = dto.FilePath,
            SlnxFileName = dto.SlnxFileName,
            SlnxFilePath = dto.SlnxFilePath,
            Projects = [.. projects.Select(item => item)]
        };
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

    public bool Save(string filePath, Solution solution, List<VBSProjectDto> projects)
    {
        try
        {
            var dto = solution.MapSlnxToDto();

            using var fileStream = File.Create(filePath);
            JsonSerializer.Serialize(fileStream, dto, Options);

            EmitGeneratedSlnx(dto, projects);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<Solution> CreateSolution(CreateSolutionRequest request)
    {
        if (string.IsNullOrEmpty(request.FilePath))
            throw new ArgumentException("The file path cannot be null or empty.", nameof(request));

        if (string.IsNullOrEmpty(request.FileName))
            throw new ArgumentException("The solution name cannot be null or empty.", nameof(request));

        var solutionDir = Path.GetFullPath(Path.Combine(request.FilePath, request.FileName));
        Directory.CreateDirectory(solutionDir);

        var vbsslnxFileName = request.FileName + SolutionFileExtension;
        var vbsslnxFilePath = Path.Combine(solutionDir, vbsslnxFileName);

        var projectDir = Path.Combine(solutionDir, request.FileName);
        var projectFilePath = Path.Combine(projectDir, request.FileName + VBSProjectExtension);

        var proj = await projManager.CreateProjectAsync(new CreateProjectRequest
        {
            FilePath = projectFilePath,
            ProjectName = request.FileName,
            FileName = request.FileName,
        });

        if (!projManager.TryLoadDto(projectFilePath, out var projectDto))
            throw new InvalidOperationException($"Failed to load newly created project at '{projectFilePath}'.");

        var dto = new SolutionDto
        {
            FileName = vbsslnxFileName,
            FilePath = vbsslnxFilePath,

            SlnxFileName = request.FileName + SlnxExtension,
            SlnxFilePath = Path.Combine(solutionDir, request.FileName + SlnxExtension),

            Projects = [proj.FilePath]
        };

        using var fileStream = File.Create(dto.FilePath);
        JsonSerializer.Serialize(fileStream, dto, Options);

        EmitGeneratedSlnx(dto, [proj.MapProjToDto()]);

        return new()
        {
            FileName = dto.FileName,
            FilePath = dto.FilePath,
            SlnxFileName = dto.SlnxFileName,
            SlnxFilePath = dto.SlnxFilePath,
            Projects = [proj]
        };
    }

    public string EmitGeneratedSlnx(SolutionDto dto, List<VBSProjectDto> projects)
    {
        var solutionDir = Path.GetDirectoryName(dto.FilePath)
            ?? throw new InvalidOperationException($"Could not resolve directory for '{dto.FilePath}'.");

        Directory.CreateDirectory(solutionDir);

        var solutionElement = new XElement("Solution");

        foreach (var project in projects)
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

    private List<VBSProject> ResolveSolutionDynamically(SolutionDto dto, string solutionFilePath)
    {
        var resolvedProjects = new List<VBSProject>();

        foreach (var projectPath in dto.Projects)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                continue;

            if (!projManager.TryLoadDto(projectPath, out var resolvedProject))
                continue;

            resolvedProjects.Add(resolvedProject.MapDtoToProj());
        }
        return resolvedProjects;
    }
}
using System.Xml;
using System.Xml.Linq;
using Visual_Block_Studio.DTOs.Explorer;

namespace Visual_Block_Studio.Services;

public static class SolutionDtoSlnxGenerator
{
    /// <summary>
    /// Writes solution.FilePath (expected to end in .slnx) using the project
    /// paths in solution.Projects. Each VBSProjectDto.FilePath is converted to
    /// a path relative to the solution file's own directory, since that's what
    /// the .slnx format expects.
    /// </summary>
    public static void Write(SolutionDto solution)
    {
        if (string.IsNullOrWhiteSpace(solution.FilePath))
            throw new ArgumentException("SolutionDto.FilePath must be set to the target .slnx path.");

        var solutionDir = Path.GetDirectoryName(solution.FilePath)
            ?? throw new InvalidOperationException($"Could not resolve directory for '{solution.FilePath}'.");

        Directory.CreateDirectory(solutionDir);

        var solutionElement = new XElement("Solution");

        foreach (var project in solution.Projects)
        {
            if (string.IsNullOrWhiteSpace(project.FilePath))
                continue; // skip malformed entries rather than fail the whole write

            var relativePath = ToRelative(solutionDir, project.FilePath);
            solutionElement.Add(new XElement("Project", new XAttribute("Path", relativePath)));
        }

        Save(solution.FilePath, solutionElement);
    }

    private static string ToRelative(string solutionDir, string projectFilePath)
    {
        var relative = Path.GetRelativePath(solutionDir, projectFilePath);
        return relative.Replace('\\', '/');
    }

    private static void Save(string slnxPath, XElement solutionElement)
    {
        var doc = new XDocument(solutionElement);

        var settings = new XmlWriterSettings
        {
            Indent = true,
            Encoding = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
        };

        using var writer = XmlWriter.Create(slnxPath, settings);
        doc.Save(writer);
    }
}
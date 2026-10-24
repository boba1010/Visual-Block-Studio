using System.Collections.ObjectModel;
using System.Numerics;
using Visual_Block_Studio.DTOs;
using Visual_Block_Studio.DTOs.Explorer;
using Visual_Block_Studio.Models;
using Visual_Block_Studio.Models.Explorer;

namespace Visual_Block_Studio.Helpers;

public static class RecentMappers
{
    public static Recent MapDtoToModel(this RecentDto dto)
    {
        return new()
        {
            Name = dto.Name,
            FilePath = dto.FilePath,
            Type = dto.Type,
            UpdatedAt = dto.UpdatedAt
        };
    }

    public static RecentDto MapModelToDto(this Recent model)
    {
        return new()
        {
            Name = model.Name,
            UpdatedAt = model.UpdatedAt,
            Type = model.Type,
            FilePath = model.FilePath,
        };
    }

    public static List<RecentDto> MapItemsToDtos(this ObservableCollection<Recent> blocks)
    {
        return [.. blocks.Select(r => new RecentDto 
        { 
            FilePath = r.FilePath, 
            Type = r.Type,
            UpdatedAt = r.UpdatedAt,
            Name = r.Name
        })];
    }
    public static List<RecentDto> MapItemsToDtos(this List<Recent> blocks)
    {
        return [.. blocks.Select(r => new RecentDto
        {
            FilePath = r.FilePath,
            Type = r.Type,
            UpdatedAt = r.UpdatedAt,
            Name = r.Name
        })];
    }


    public static List<Recent> MapDtosToItems(this List<RecentDto> blocks)
    {
        return [.. blocks.Select(r => new Recent
        {
            FilePath = r.FilePath,
            Type = r.Type,
            UpdatedAt = r.UpdatedAt,
            Name = r.Name
        })];
    }
}

public static class SolutionMappers
{
    public static SolutionDto MapSlnxToDto(this Solution solution)
    {
        return new()
        {
            FileName = solution.FileName,
            FilePath = solution.FilePath,
            Projects = [.. solution.Projects.Select(p => p.FilePath)],
            SlnxFilePath = solution.SlnxFilePath,
            SlnxFileName = solution.SlnxFileName,
        };
    }
}

public static class VBSProjectMappers
{
    public static VBSProject MapDtoToProj(this VBSProjectDto dto)
    {
        return new()
        {
            LayoutFiles = dto.LayoutFiles.MapDtosToItems(),
            Name = dto.Name,
            OutputPath = dto.OutputPath,
            TargetArchitecture = dto.TargetArchitecture,
            FileName = dto.FileName,
            FilePath = dto.FilePath,
            GeneratedFileName = dto.GeneratedFileName,
            GeneratedFilePath = dto.GeneratedFilePath,
            CsProjFileName = dto.CsProjFileName,
            CsProjFilePath = dto.CsProjFilePath,
        };
    }
    public static VBSProjectDto MapProjToDto(this VBSProject project)
    {
        return new()
        {
            LayoutFiles = project.LayoutFiles.MapItemsToDtos(),
            Name = project.Name,
            OutputPath = project.OutputPath,
            TargetArchitecture = project.TargetArchitecture,
            CsProjFilePath = project.CsProjFilePath,
            CsProjFileName = project.CsProjFileName,
            GeneratedFilePath = project.GeneratedFilePath,
            GeneratedFileName = project.GeneratedFileName,
            FilePath = project.FilePath,
            FileName = project.FileName,
        };
    }
    public static ProjectItem MapDtoToItem(this ProjectItemDto dto)
    {
        return new()
        {
            Type = dto.Type,
            Files = dto.Files.MapDtosToFileItems(),
            Name = dto.Name,
        };
    }
    public static ProjectItemDto MapItemToDto(this ProjectItem item)
    {
        return new()
        {
            Files = item.Files.MapFileItemsToDtos(),
            Type = item.Type,
            Name = item.Name,
        };
    }
    public static ProjectFile MapDtoToFileItem(this ProjectFileDto dto)
    {
        return new()
        {
            Type = dto.Type,
            FilePath = dto.FilePath,
            FileName = dto.FileName,
        };
    }
    public static ProjectFileDto MapFileItemToDto(this ProjectFile file)
    {
        return new()
        {
            FileName = file.FileName,
            FilePath = file.FilePath,
            Type = file.Type
        }; 
    }

    public static ObservableCollection<ProjectItem> MapDtosToItems(this List<ProjectItemDto> dtos)
    {
        return [.. dtos.Select(p => new ProjectItem { Type = p.Type, Files = p.Files.MapDtosToFileItems(), Name = p.Name })];
    }
    public static List<ProjectItemDto> MapItemsToDtos(this ObservableCollection<ProjectItem> blocks)
    {
        return [.. blocks.Select(p => new ProjectItemDto { Type = p.Type, Files = p.Files.MapFileItemsToDtos(), Name = p.Name })];
    }

    public static ObservableCollection<VBSProject> MapDtosToProjects(this List<VBSProjectDto> dtos)
    {
        return 
        [.. dtos.Select(p => new VBSProject
        {
            GeneratedFileName = p.GeneratedFileName,
            GeneratedFilePath = p.GeneratedFilePath,
            LayoutFiles = p.LayoutFiles.MapDtosToItems(),
            Name = p.Name,
            OutputPath = p.OutputPath,
            TargetArchitecture = p.TargetArchitecture,
            FileName = p.FileName,
            FilePath = p.FilePath,
            CsProjFileName = p.CsProjFileName,
            CsProjFilePath = p.CsProjFilePath,
        })];
    }
    public static List<VBSProjectDto> MapProjectsToDtos(this ObservableCollection<VBSProject> projects)
    {
        return 
        [.. projects.Select(p => new VBSProjectDto 
        {
            GeneratedFileName = p.GeneratedFileName,
            GeneratedFilePath = p.GeneratedFilePath,
            LayoutFiles = p.LayoutFiles.MapItemsToDtos(),
            Name = p.Name,
            OutputPath = p.OutputPath,
            TargetArchitecture = p.TargetArchitecture,
            FileName = p.FileName,
            FilePath = p.FilePath,
            CsProjFileName = p.CsProjFileName,
            CsProjFilePath = p.CsProjFilePath,
        })];
    }
    public static ObservableCollection<ProjectFile> MapDtosToFileItems(this List<ProjectFileDto> dtos)
    {
        return 
        [.. dtos.Select(f => new ProjectFile
        {
            FileName = f.FileName,
            FilePath = f.FilePath,
            Type = f.Type,
        })];
    }
    public static List<ProjectFileDto> MapFileItemsToDtos(this ObservableCollection<ProjectFile> files)
    {
        return 
        [.. files.Select(f => new ProjectFileDto
        {
            FileName = f.FileName,
            FilePath = f.FilePath,
            Type = f.Type,
        })];
    }
}
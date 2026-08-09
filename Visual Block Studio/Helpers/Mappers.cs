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
    public static Solution MapDtoToSlnx(this SolutionDto dto)
    {
        return new()
        {
            FileName = dto.FileName,
            FilePath = dto.FilePath,
            Projects = dto.Projects.MapDtosToProjects(),
            SlnxFileName = dto.SlnxFileName,
            SlnxFilePath = dto.SlnxFilePath,
        };
    }
    public static SolutionDto MapSlnxToDto(this Solution solution)
    {
        return new()
        {
            FileName = solution.FileName,
            FilePath = solution.FilePath,
            Projects = solution.Projects.MapProjectsToDtos(),
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
            MemoryLimitBytes = dto.MemoryLimitBytes,
            Name = dto.Name,
            OutputPath = dto.OutputPath,
            TargetArchitecture = dto.TargetArchitecture,
            FileName = dto.FileName,
            FilePath = dto.FilePath,
            GeneratedFileName = dto.GeneratedFileName,
            GeneratedFilePath = dto.GeneratedFilePath,
            VcxProjFileName = dto.VcxProjFileName,
            VcxProjFilePath = dto.VcxProjFilePath,
        };
    }
    public static VBSProjectDto MapProjToDto(this VBSProject project)
    {
        return new()
        {
            LayoutFiles = project.LayoutFiles.MapItemsToDtos(),
            MemoryLimitBytes = project.MemoryLimitBytes,
            Name = project.Name,
            OutputPath = project.OutputPath,
            TargetArchitecture = project.TargetArchitecture,
            VcxProjFilePath = project.VcxProjFilePath,
            VcxProjFileName = project.VcxProjFileName,
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
            MemoryLimitBytes = p.MemoryLimitBytes,
            Name = p.Name,
            OutputPath = p.OutputPath,
            TargetArchitecture = p.TargetArchitecture,
            FileName = p.FileName,
            FilePath = p.FilePath,
            VcxProjFileName = p.VcxProjFileName,
            VcxProjFilePath = p.VcxProjFilePath,
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
            MemoryLimitBytes = p.MemoryLimitBytes,
            Name = p.Name,
            OutputPath = p.OutputPath,
            TargetArchitecture = p.TargetArchitecture,
            FileName = p.FileName,
            FilePath = p.FilePath,
            VcxProjFileName = p.VcxProjFileName,
            VcxProjFilePath = p.VcxProjFilePath,
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

public static class PropertyBlockMappers
{
    public static PropertyBlock MapDtoToBlock(this PropertyBlockDto dto)
    {
        return new()
        {
            Property = dto.Property.MapDtoToProperty(),
            Position = (Vector2)dto.Position,
            Size = (Vector2)dto.Size,
        };
    }
    public static PropertyBlockDto MapBlockToDto(this PropertyBlock block)
    {
        return new()
        {
            Property = block.Property.MapPropertyToDto(),
            Position = (Vector2Dto)block.Position,
            Size = (Vector2Dto)block.Size,
        };
    }
    public static List<PropertyBlock> MapDtosToBlocks(this List<PropertyBlockDto> dtos)
    {
        return [.. dtos.Select(p => new PropertyBlock { Property = p.Property.MapDtoToProperty(), Position = (Vector2)p.Position, Size = (Vector2)p.Size })];
    }
    public static List<PropertyBlockDto> MapBlocksToDtos(this List<PropertyBlock> blocks)
    {
        return [.. blocks.Select(p => new PropertyBlockDto { Property = p.Property.MapPropertyToDto(), Position = (Vector2Dto)p.Position, Size = (Vector2Dto)p.Size })];
    }
}

public static class PropertyMappers
{
    public static Property MapDtoToProperty(this PropertyDto dto)
    {
        return new()
        {
            Name = dto.Name,
            Value = dto.Value
        };
    }
    public static PropertyDto MapPropertyToDto(this Property property)
    {
        return new()
        {
            Name = property.Name,
            Value = property.Value
        };
    }
}

public static class XamlBlockMappers
{
    public static XamlBlock MapDtoToBlock(this XamlBlockDto dto)
    {
        return dto switch
        {
            WindowBlockDto _dto => _dto.MapDtoToBlock(),
            ButtonBlockDto _dto => _dto.MapDtoToBlock(),
            _ => throw new NotSupportedException()
        };
    }
    public static XamlBlockDto MapBlockToDto(this XamlBlock block)
    {
        return block switch
        {
            WindowBlock _block => _block.MapBlockToDto(),
            ButtonBlock _block => _block.MapBlockToDto(),
            _ => throw new NotSupportedException()
        };
    }

    public static List<XamlBlock> MapDtosToBlocks(this List<XamlBlockDto> dtos)
    {
        return [.. dtos.Select(p => p.MapDtoToBlock())];
    }
    public static List<XamlBlockDto> MapBlocksToDtos(this List<XamlBlock> blocks)
    {
        return [.. blocks.Select(p => p.MapBlockToDto())];
    }
}

public static class WindowBlockMappers
{
    public static WindowBlock MapDtoToBlock(this WindowBlockDto dto)
    {
        return new()
        {
            BaseClass = dto.BaseClass,
            Namespace = dto.Namespace,
            Prefix = dto.Prefix,
            Suffix = dto.Suffix,
            WindowName = dto.WindowName,
            PropertyBlocks = dto.PropertyBlocks.MapDtosToBlocks(),
            Children = dto.Children.MapDtosToBlocks(),
            Position = (Vector2)dto.Position,
            Size = (Vector2)dto.Size,
        };
    }
    public static WindowBlockDto MapBlockToDto(this WindowBlock block)
    {
        return new()
        {
            BaseClass = block.BaseClass,
            Namespace = block.Namespace,
            Prefix = block.Prefix,
            Suffix = block.Suffix,
            WindowName = block.WindowName,
            PropertyBlocks = block.PropertyBlocks.MapBlocksToDtos(),
            Children = block.Children.MapBlocksToDtos(),
            Position = (Vector2Dto)block.Position,
            Size = (Vector2Dto)block.Size,
        };
    }
}

public static class ButtonBlockMappers
{
    public static ButtonBlock MapDtoToBlock(this ButtonBlockDto dto)
    {
        return new()
        {
            Prefix = dto.Prefix,
            Suffix = dto.Suffix,
            PropertyBlocks = dto.PropertyBlocks.MapDtosToBlocks(),
            Children = dto.Children.MapDtosToBlocks(),
            Position = (Vector2)dto.Position,
            Size = (Vector2)dto.Size,
        };
    }
    public static ButtonBlockDto MapBlockToDto(this ButtonBlock block)
    {
        return new()
        {
            Prefix = block.Prefix,
            Suffix = block.Suffix,
            PropertyBlocks = block.PropertyBlocks.MapBlocksToDtos(),
            Children = block.Children.MapBlocksToDtos(),
            Position = (Vector2Dto)block.Position,
            Size = (Vector2Dto)block.Size,
        };
    }
}

public static class BlockMappers
{
    public static Block MapDtoToBlock(this BlockDto dto)
    {
        return dto switch
        {
            WindowBlockDto _dto => _dto.MapDtoToBlock(),
            ButtonBlockDto _dto => dto.MapDtoToBlock(),
            _ => throw new NotSupportedException()
        };
    }
    public static BlockDto MapBlockToDto(this Block block)
    {
        return block switch
        {
            WindowBlock _block => _block.MapBlockToDto(),
            ButtonBlock _block => _block.MapBlockToDto(),
            _ => throw new NotSupportedException()
        };
    }

    public static List<Block> MapDtosToBlocks(this List<BlockDto> dtos)
    {
        return [.. dtos.Select(p => p.MapDtoToBlock())];
    }
    public static List<BlockDto> MapBlocksToDtos(this List<Block> blocks)
    {
        return [.. blocks.Select(p => p.MapBlockToDto())];
    }
}
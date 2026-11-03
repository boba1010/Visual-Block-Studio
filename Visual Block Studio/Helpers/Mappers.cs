using System.Collections.ObjectModel;
using Visual_Block_Studio.DTOs;
using Visual_Block_Studio.Models;

namespace Visual_Block_Studio.Helpers;

public static class RecentMappers
{
    public static Recent MapDtoToModel(this RecentDto dto)
    {
        return new()
        {
            Id = dto.Id,
            Name = dto.Name,
            UpdatedAt = dto.UpdatedAt,
            FileContent = dto.FileContent,
        };
    }

    public static RecentDto MapModelToDto(this Recent model)
    {
        return new()
        {
            Id = model.Id,
            Name = model.Name,
            UpdatedAt = model.UpdatedAt,
            FileContent = model.FileContent,
        };
    }

    public static List<RecentDto> MapItemsToDtos(this ObservableCollection<Recent> recents)
    {
        return [.. recents.Select(r => new RecentDto 
        {
            Id = r.Id,
            UpdatedAt = r.UpdatedAt,
            Name = r.Name,
            FileContent = r.FileContent,
        })];
    }
    public static List<RecentDto> MapItemsToDtos(this List<Recent> recents)
    {
        return [.. recents.Select(r => new RecentDto
        {
            Id = r.Id,
            UpdatedAt = r.UpdatedAt,
            Name = r.Name,
            FileContent = r.FileContent,
        })];
    }


    public static List<Recent> MapDtosToItems(this List<RecentDto> recents)
    {
        return [.. recents.Select(r => new Recent
        {
            Id = r.Id,
            UpdatedAt = r.UpdatedAt,
            Name = r.Name,
            FileContent = r.FileContent,
        })];
    }
}
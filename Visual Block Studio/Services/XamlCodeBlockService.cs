using System.Text.Json;
using Visual_Block_Studio.DTOs;
using Visual_Block_Studio.Helpers;
using Visual_Block_Studio.Models.XamlBlocks;

namespace Visual_Block_Studio.Services;

public class XamlCodeBlockService
{
    public XamlBlock Load(string filePath)
    {
        using var fileStream = File.OpenRead(filePath);

        var doc = JsonSerializer.Deserialize<VBlockDocumentDto>(fileStream);

        return doc is null ? throw new InvalidOperationException("Invalid block file.") : (XamlBlock)doc.Root.MapDtoToBlock();
    }

    public bool TryLoad(string filePath, out XamlBlock block)
    {
        try
        {
            using var fileStream = File.OpenRead(filePath);

            var doc = JsonSerializer.Deserialize<VBlockDocumentDto>(fileStream);

            if (doc is null)
            {
                block = null!;
                return false;
            }

            block = (XamlBlock)doc.Root.MapDtoToBlock();

            return true;
        }
        catch (Exception)
        {
            block = null!;
            return false;
        }
    }

    public bool Save(string filePath, XamlBlock block)
    {
        try
        {
            using var fileStream = File.Create(filePath);

            JsonSerializer.Serialize(fileStream, block.MapBlockToDto());

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}

using Visual_Block_Studio.Models;

namespace Visual_Block_Studio.Editing;

public sealed class BlockSelection
{
    public Block? SelectedBlock { get; private set; }

    public void Select(Block block)
    {
        SelectedBlock = block;
    }

    public void Clear()
    {
        SelectedBlock = null;
    }

    public bool IsSelected(Block block)
    {
        return ReferenceEquals(SelectedBlock, block);
    }
}
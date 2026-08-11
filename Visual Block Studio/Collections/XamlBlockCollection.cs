using Visual_Block_Studio.Models;

namespace Visual_Block_Studio.Collections;

public sealed partial class XamlBlockCollection(XamlBlock parent) : List<XamlBlock>
{
    public new void Add(XamlBlock block)
    {
        if (Contains(block))
            return;

        block.Parent = new ParentXamlBlock
        {
            BlockType = parent.GetType(),
            Position = parent.Position,
            Size = parent.Size,
        };

        base.Add(block);
    }

    public new bool Remove(XamlBlock block)
    {
        if (!base.Remove(block))
            return false;

        block.Parent = null;
        return true;
    }
}

using Visual_Block_Studio.Models;
using Visual_Block_Studio.Models.CodeBlocks;

namespace Visual_Block_Studio.Collections;

public sealed partial class CodeBlockCollection(CodeBlock parent) : List<CodeBlock>
{
    public new void Add(CodeBlock block)
    {
        if (Contains(block))
            return;

        block.Parent = new ParentCodeBlock
        {
            BlockType = parent.GetType(),
        };

        base.Add(block);
    }

    public new bool Remove(CodeBlock block)
    {
        if (!base.Remove(block))
            return false;

        block.Parent = null;
        return true;
    }
}

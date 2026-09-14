using Visual_Block_Studio.Collections;

namespace Visual_Block_Studio.Models.CodeBlocks;

public sealed class NamespaceBlock : CodeBlock
{
    public string Name { get; set; } = null!;
    public CodeBlockCollection Members { get; }

    public NamespaceBlock()
    {
        Members = new(this);
    }
}

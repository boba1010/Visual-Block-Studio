using Visual_Block_Studio.Collections;

namespace Visual_Block_Studio.Models.CodeBlocks;

public class ClassBlock : CodeBlock
{
    public string Name { get; set; } = null!;
    public CodeBlockCollection Members { get; set; }

    public ClassBlock()
    {
        Members = new(this);
    }

    protected override IEnumerable<CodeBlock> GetMembers() => Members;
}

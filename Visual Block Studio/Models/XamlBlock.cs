using System.Text;
using Visual_Block_Studio.Collections;

namespace Visual_Block_Studio.Models;

public abstract class XamlBlock : Block
{
    public ParentXamlBlock? Parent { get; set; }
    public XamlBlockCollection Children { get; set; } = null!;

    protected XamlBlock()
    {
        Children = new XamlBlockCollection(this);
    }

    public abstract List<PropertyBlock> PropertyBlocks { get; set; }

    private string? _prefixOverride;
    public string Prefix
    {
        get => _prefixOverride ?? ComputePrefix();
        set => _prefixOverride = value;
    }

    private string? _suffixOverride;
    public string Suffix
    {
        get => _suffixOverride ?? ComputeSuffix();
        set => _suffixOverride = value;
    }

    public virtual string GenerateXaml(int indentLevel = 0)
    {
        // 4 spaces per indentation level
        string indent = new string(' ', indentLevel * 4);
        StringBuilder sb = new();

        // 1. Format and append the opening tag (Prefix)
        // Strips any internal messy newlines from raw string blocks before formatting
        string cleanPrefix = ComputePrefix().Trim();
        sb.AppendLine($"{indent}{cleanPrefix}");

        // 2. Recursively compile children with an increased indentation level
        foreach (var child in Children)
        {
            sb.Append(child.GenerateXaml(indentLevel + 1));
        }

        // 3. Format and append the closing tag (Suffix)
        string cleanSuffix = ComputeSuffix().Trim();
        sb.AppendLine($"{indent}{cleanSuffix}");

        return sb.ToString();
    }

    protected abstract string ComputePrefix();
    protected abstract string ComputeSuffix();
}

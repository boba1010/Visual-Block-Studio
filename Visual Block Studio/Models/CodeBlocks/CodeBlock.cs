using System.Text;

namespace Visual_Block_Studio.Models.CodeBlocks;

public abstract class CodeBlock : Block
{
    public ParentCodeBlock? Parent { get; set; }
    public string Header { get; set; } = null!;

    public virtual string BuildCode()
    {
        var builder = new StringBuilder();

        builder.Append(Header);

        var members = GetMembers().ToList();

        if (members.Count == 0)
            return builder.ToString();

        builder.AppendLine();
        builder.AppendLine("{");

        foreach (var member in members)
        {
            var memberCode = member.BuildCode();

            foreach (var line in memberCode.Split(
                Environment.NewLine,
                StringSplitOptions.None))
            {
                if (!string.IsNullOrWhiteSpace(line))
                    builder.Append("    ");

                builder.AppendLine(line);
            }
        }

        builder.Append('}');

        return builder.ToString();
    }

    protected virtual IEnumerable<CodeBlock> GetMembers() => [];
}

using XamlBlocksCompiler.Generation;
using XamlBlocksCompiler.Parsing;
using XamlBlocksCompiler.Syntax;

namespace XamlBlocksCompiler.Compilation;

public sealed class XamlCompiler
{
    private readonly XamlBlockDocumentReader _reader = new();
    private readonly XamlBlockParser _parser = new();
    private readonly XamlCodeGenerator _generator = new();

    public XamlBlockSyntax Compile(string inputPath, string outputPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        string json = _reader.Read(inputPath);
        XamlBlockSyntax syntax = _parser.Parse(json);

        string xaml = _generator.Generate(syntax);

        File.WriteAllText(outputPath, xaml);

        return syntax;
    }
}

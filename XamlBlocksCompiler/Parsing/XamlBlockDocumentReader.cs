namespace XamlBlocksCompiler.Parsing;

public sealed class XamlBlockDocumentReader
{
    public string Read(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return File.ReadAllText(path);
    }
}

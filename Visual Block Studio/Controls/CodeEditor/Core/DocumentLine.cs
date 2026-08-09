namespace Visual_Block_Studio.Controls.CodeEditor.Core
{
    public sealed class DocumentLine(string text = "")
    {
        public string Text { get; internal set; } = text;
    }
}

namespace Visual_Block_Studio.Services;

public class TextEditorService
{
    public string LoadText(string path)
    {
        return File.ReadAllText(path);
    }

    //public bool SaveText(string text, string path)
    //{

    //}
}

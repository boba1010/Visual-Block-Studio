using Visual_Block_Studio.Controls.CodeEditor.Core;

namespace Visual_Block_Studio.Controls.CodeEditor.UndoRedo;

public interface IUndoableEdit
{
    void Undo(EditorEngine editor);

    void Redo(EditorEngine editor);
}
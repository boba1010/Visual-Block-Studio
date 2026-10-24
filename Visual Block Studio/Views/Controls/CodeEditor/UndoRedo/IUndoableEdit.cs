using Visual_Block_Studio.Views.Controls.CodeEditor.Core;

namespace Visual_Block_Studio.Views.Controls.CodeEditor.UndoRedo;

public interface IUndoableEdit
{
    void Undo(EditorEngine editor);

    void Redo(EditorEngine editor);
}
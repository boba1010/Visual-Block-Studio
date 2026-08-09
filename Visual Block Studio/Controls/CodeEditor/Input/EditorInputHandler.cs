using Visual_Block_Studio.Controls.CodeEditor.Core;
using Windows.System;

namespace Visual_Block_Studio.Controls.CodeEditor.Input;

public sealed class EditorInputHandler(EditorEngine engine)
{
    public void HandleKey(VirtualKey key, VirtualKeyModifiers modifiers)
    {
        bool shift = (modifiers & VirtualKeyModifiers.Shift) != 0;
        bool ctrl = (modifiers & VirtualKeyModifiers.Control) != 0;

        if (shift && ctrl)
        {

        }
        else if (ctrl)
        {
            switch (key)
            {
                case VirtualKey.A:
                    engine.SelectAll();
                    break;
                case VirtualKey.Z:
                    engine.Undo();
                    break;
                case VirtualKey.Y:
                    engine.Redo();
                    break;
                case VirtualKey.C:
                    engine.Copy();
                    break;
                case VirtualKey.X:
                    engine.Cut();
                    break;
                case VirtualKey.V:
                    engine.Paste();
                    break;
            }
        }
        else if (shift)
        {
            switch (key)
            {
                case VirtualKey.Left:
                    engine.MoveLeft(true);
                    break;
                case VirtualKey.Right:
                    engine.MoveRight(true);
                    break;
                case VirtualKey.Down:
                    engine.MoveDown(true);
                    break;
                case VirtualKey.Up:
                    engine.MoveUp(true);
                    break;
                case VirtualKey.Home:
                    engine.MoveHome(true);
                    break;
                case VirtualKey.End:
                    engine.MoveToLineEnd(true);
                    break;
                case VirtualKey.Tab:
                    engine.RemoveIndent();
                    break;
            }
        }
        else
        {
            switch (key)
            {
                case VirtualKey.Left:
                    engine.MoveLeft();
                    break;
                case VirtualKey.Right:
                    engine.MoveRight();
                    break;
                case VirtualKey.Down:
                    engine.MoveDown();
                    break;
                case VirtualKey.Up:
                    engine.MoveUp();
                    break;
                case VirtualKey.Back:
                    engine.Backspace();
                    break;
                case VirtualKey.Enter:
                    engine.NewLine();
                    break;
                case VirtualKey.Delete:
                    engine.Delete();
                    break;
                case VirtualKey.Home:
                    engine.MoveHome();
                    break;
                case VirtualKey.End:
                    engine.MoveToLineEnd();
                    break;
                case VirtualKey.Tab:
                    engine.InsertTab();
                    break;
            }
        }

    }
}
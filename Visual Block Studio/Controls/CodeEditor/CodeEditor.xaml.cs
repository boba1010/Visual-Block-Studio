using Microsoft.Graphics.Canvas.UI.Xaml;
using Visual_Block_Studio.Controls.CodeEditor.Core;
using Visual_Block_Studio.Controls.CodeEditor.Input;
using Visual_Block_Studio.Controls.CodeEditor.Rendering;
using Windows.System;

namespace Visual_Block_Studio.Controls.CodeEditor;

public sealed partial class CodeEditor : UserControl
{
    public EditorEngine Engine { get; }
    private readonly EditorRenderer _renderer = new();
    private readonly EditorTextStyle _style = new();
    private readonly EditorInputHandler _inputHandler;
    private bool _isSelecting;

    public CodeEditor()
    {
        InitializeComponent();

        IsTabStop = true;

        Engine = new EditorEngine();

        _inputHandler = new(Engine);

        inputBox.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(InputBox_PointerPressed), true);

        inputBox.AddHandler(UIElement.PointerMovedEvent, new PointerEventHandler(InputBox_PointerMoved), true);

        inputBox.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(InputBox_PointerReleased), true);

        inputBox.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(InputBox_KeyDown), true);
    }

    private void EditorCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        if (!_style.IsMeasured)
            _style.Measure(args.DrawingSession);

        _renderer.Render(args.DrawingSession, Engine, _style);
    }

    private void InputBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Tab)
        {
            e.Handled = true;

            _inputHandler.HandleKey(VirtualKey.Tab, VirtualKeyModifiers.None);

            editor.Invalidate();
        }
    }

    private void EditorKey_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        _inputHandler.HandleKey(sender.Key, sender.Modifiers);

        editor.Invalidate();

        args.Handled = true;
    }

    private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (inputBox.Text.Length == 0)
            return;

        Engine.Insert(inputBox.Text);

        inputBox.Text = "";

        editor.Invalidate();
    }

    private TextPosition GetPosition(Point p)
    {
        float x = (float)editor.ActualWidth - (float)p.X;

        int line = (int)(p.Y / _style.LineHeight);
        int column = (int)((x - 10) / _style.CharacterWidth);

        return new TextPosition(line, column);
    }

    private void InputBox_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        TextPosition position = GetPosition(e.GetCurrentPoint(editor).Position);

        Engine.MoveCaretTo(position.Line, position.Column);

        Engine.Selection.Clear(Engine.Caret.Position);

        _isSelecting = true;

        inputBox.CapturePointer(e.Pointer);

        editor.Invalidate();
    }

    private void InputBox_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!_isSelecting)
            return;

        if (!e.GetCurrentPoint(editor).Properties.IsLeftButtonPressed)
            return;

        TextPosition position = GetPosition(e.GetCurrentPoint(editor).Position);

        Engine.MoveCaretTo(position.Line, position.Column, _isSelecting);

        Engine.Selection.SetActive(Engine.Caret.Position);

        editor.Invalidate();
    }

    private void InputBox_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (inputBox.PointerCaptures.Contains(e.Pointer))
            inputBox.ReleasePointerCapture(e.Pointer);

        _isSelecting = false;
    }
}
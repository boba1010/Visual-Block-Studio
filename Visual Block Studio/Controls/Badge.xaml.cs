using Windows.UI;

namespace Visual_Block_Studio.Controls;

public sealed partial class Badge : UserControl
{
    public Badge()
    {
        InitializeComponent();
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public readonly static DependencyProperty TextProperty = 
        DependencyProperty.Register(nameof(Text),
            typeof(string),
            typeof(Badge),
            new(null));

    public new Brush Background
    {
        get => (Brush)GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public static readonly new DependencyProperty BackgroundProperty =
        DependencyProperty.Register(
            nameof(Background),
            typeof(Brush),
            typeof(Badge),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 230, 230, 230))));
}

using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Visual_Block_Studio.Messages;

public class OpenTabRequestMessage(OpenTabRequestArgs value) : ValueChangedMessage<OpenTabRequestArgs>(value)
{
}

public class OpenTabRequestArgs
{
    public string Title { get; set; } = null!;
    public Type TargetPageType { get; set; } = null!;
    public object Parameter { get; set; } = null!;
}

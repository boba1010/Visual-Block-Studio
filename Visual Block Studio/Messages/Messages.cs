using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Visual_Block_Studio.Messages;

public sealed record SaveMessage(string Id);
public sealed record SaveAllMessage();

public sealed class ClosingTabMessage(string id) : AsyncRequestMessage<bool>
{
    public string Id => id;
}

public sealed class ClosingAllMessage : AsyncCollectionRequestMessage<bool>;
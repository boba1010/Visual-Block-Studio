using CommunityToolkit.Mvvm.Messaging.Messages;
using Visual_Block_Studio.Models;

namespace Visual_Block_Studio.Messages;

public class AddBlockRequestMessage : ValueChangedMessage<AddBlockRequestArgs>
{
    public AddBlockRequestMessage(AddBlockRequestArgs value) : base(value) { }
}

public class AddBlockRequestArgs
{
    public Block Block { get; set; } = null!;
}
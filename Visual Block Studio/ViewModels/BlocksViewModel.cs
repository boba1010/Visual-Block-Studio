using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Visual_Block_Studio.Editing;
using Visual_Block_Studio.Messages;
using Visual_Block_Studio.Models;
using Visual_Block_Studio.Services;

namespace Visual_Block_Studio.ViewModels;

public partial class BlocksViewModel : ObservableObject
{
    [ObservableProperty]
    public partial XamlBlock Block { get; set; }

    partial void OnBlockChanged(XamlBlock value)
    {
        Editor.SetRoot(value);
    }

    public VBlockEditor Editor { get; }

    private readonly XamlCodeBlockService _xamlService;

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial bool IsNotLoading { get; set; }

    partial void OnIsLoadingChanged(bool value)
    {
        IsNotLoading = !value;
    }

    public BlocksViewModel(XamlCodeBlockService xamlService)
    {
        _xamlService = xamlService;

        Editor = new VBlockEditor();

        WeakReferenceMessenger.Default.Register<AddBlockRequestMessage>(this,
            (recipient, message) =>
            {
                AddBlock((XamlBlock)message.Value.Block);
            });
    }

    public async Task LoadBlocksAsync(string path)
    {
        try
        {
            if (IsLoading)
                return;

            IsLoading = true;
            IsNotLoading = false;

            Block = _xamlService.Load(path);
        }
        catch (Exception)
        {
            // TODO: proper error handling
        }
        finally
        {
            IsLoading = false;
            IsNotLoading = true;
        }
    }

    public void AddBlock(XamlBlock block)
    {
        Block.Children.Add(block);
    }
}
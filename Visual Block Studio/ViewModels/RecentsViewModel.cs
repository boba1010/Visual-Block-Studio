using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Visual_Block_Studio.Models;
using Visual_Block_Studio.Services;

namespace Visual_Block_Studio.ViewModels;

public sealed partial class RecentsViewModel(RecentsService recents) : ObservableObject
{
    public ObservableCollection<Recent> Recents { get; } = [];

    public void LoadRecents()
    {
        foreach (var item in recents.Get())
            Recents.Add(item);
    }

    public void UpdateRecent(Recent recent)
    {
        recent.UpdatedAt = DateTime.Now;
        recents.Update(recent);
    }
}

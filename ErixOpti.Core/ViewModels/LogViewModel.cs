using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ErixOpti.Core.ViewModels;

public sealed partial class LogViewModel : ObservableObject
{
    public ObservableCollection<string> Lines { get; } = new();

    public void Append(string line)
    {
        var text = $"{DateTime.Now:O}  {line}";
        Lines.Insert(0, text);
        while (Lines.Count > 5000)
        {
            Lines.RemoveAt(Lines.Count - 1);
        }
    }

    [RelayCommand]
    private Task ExportAsync()
    {
        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            $"erixopti-log-{DateTime.Now:yyyyMMdd-HHmmss}.txt");
        File.WriteAllLines(path, Lines);
        Append($"Exported log to {path}");
        return Task.CompletedTask;
    }
}

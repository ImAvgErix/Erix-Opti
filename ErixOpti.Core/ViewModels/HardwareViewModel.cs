using CommunityToolkit.Mvvm.ComponentModel;
using ErixOpti.Core.Interfaces;
using ErixOpti.Core.Models;

namespace ErixOpti.Core.ViewModels;

public sealed partial class HardwareViewModel : ObservableObject
{
    public HardwareViewModel(IHardwareService hardware) => Model = hardware.Current;
    public HardwareInfo Model { get; }
}

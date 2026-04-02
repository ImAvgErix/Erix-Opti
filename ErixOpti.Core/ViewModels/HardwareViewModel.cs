using CommunityToolkit.Mvvm.ComponentModel;
using ErixOpti.Core.Interfaces;
using ErixOpti.Core.Models;

namespace ErixOpti.Core.ViewModels;

public sealed partial class HardwareViewModel : ObservableObject
{
    private readonly IHardwareService _hardware;

    public HardwareViewModel(IHardwareService hardware)
    {
        _hardware = hardware;
    }

    public HardwareInfo Model => _hardware.Current;
}

namespace ErixOpti.Core.Models;

public static class DownloadCatalog
{
    public static IReadOnlyList<DownloadItem> All =>
    [
        new() { Id = "vcredist-x64", Name = "VC++ 2015-2022 (x64)", Description = "Visual C++ Redistributable", Category = "Runtimes", Url = "https://aka.ms/vs/17/release/vc_redist.x64.exe", FileName = "vc_redist.x64.exe", SilentArgs = "/quiet /norestart" },
        new() { Id = "vcredist-x86", Name = "VC++ 2015-2022 (x86)", Description = "Visual C++ Redistributable 32-bit", Category = "Runtimes", Url = "https://aka.ms/vs/17/release/vc_redist.x86.exe", FileName = "vc_redist.x86.exe", SilentArgs = "/quiet /norestart" },
        new() { Id = "directx", Name = "DirectX Runtime", Description = "DirectX End-User Runtime", Category = "Runtimes", Url = "https://download.microsoft.com/download/1/7/1/1718CCC4-6315-4D8E-9543-8E28A4E18C4C/dxwebsetup.exe", FileName = "dxwebsetup.exe", SilentArgs = "/Q" },
        new() { Id = "dotnet8", Name = ".NET 8 Desktop Runtime", Description = ".NET 8.0 Desktop Runtime", Category = "Runtimes", Url = "https://dotnet.microsoft.com/en-us/download/dotnet/8.0", IsDirectDownload = false },
        new() { Id = "dotnet9", Name = ".NET 9 Desktop Runtime", Description = ".NET 9.0 Desktop Runtime", Category = "Runtimes", Url = "https://dotnet.microsoft.com/en-us/download/dotnet/9.0", IsDirectDownload = false },
        new() { Id = "nvidia-drv", Name = "NVIDIA Drivers", Description = "GeForce driver download", Category = "GPU Drivers", Url = "https://www.nvidia.com/Download/index.aspx", IsDirectDownload = false },
        new() { Id = "amd-drv", Name = "AMD Adrenalin", Description = "Radeon driver download", Category = "GPU Drivers", Url = "https://www.amd.com/en/support", IsDirectDownload = false },
        new() { Id = "intel-drv", Name = "Intel Graphics", Description = "Intel driver download", Category = "GPU Drivers", Url = "https://www.intel.com/content/www/us/en/download-center/home.html", IsDirectDownload = false },
        new() { Id = "amd-chipset", Name = "AMD Chipset", Description = "AMD chipset drivers", Category = "Chipset", Url = "https://www.amd.com/en/support/download/drivers.html", IsDirectDownload = false },
        new() { Id = "intel-chipset", Name = "Intel Chipset", Description = "Intel chipset drivers", Category = "Chipset", Url = "https://www.intel.com/content/www/us/en/download/19347/chipset-inf-utility.html", IsDirectDownload = false },
        new() { Id = "gpu-z", Name = "GPU-Z", Description = "GPU info utility", Category = "Utilities", Url = "https://www.techpowerup.com/download/techpowerup-gpu-z/", IsDirectDownload = false },
        new() { Id = "hwinfo", Name = "HWiNFO64", Description = "System diagnostics", Category = "Utilities", Url = "https://www.hwinfo.com/download/", IsDirectDownload = false },
        new() { Id = "ddu", Name = "DDU", Description = "Display Driver Uninstaller", Category = "Utilities", Url = "https://www.wagnardsoft.com/display-driver-uninstaller-DDU-", IsDirectDownload = false },
        new() { Id = "nvclean", Name = "NVCleanInstall", Description = "Clean NVIDIA installer", Category = "Utilities", Url = "https://www.techpowerup.com/download/techpowerup-nvcleanstall/", IsDirectDownload = false },
        new() { Id = "npi", Name = "NVIDIA Profile Inspector", Description = "NV profile editor", Category = "Utilities", Url = "https://github.com/Orbmu2k/nvidiaProfileInspector/releases", IsDirectDownload = false },
    ];
}

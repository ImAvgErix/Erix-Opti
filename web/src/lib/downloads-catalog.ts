export type DownloadCatalogEntry = {
  id: string;
  name: string;
  description: string;
  category: "Runtimes" | "GPU Drivers" | "Chipset" | "Utilities";
  direct: boolean;
  url: string;
};

export const defaultDownloadCatalog: DownloadCatalogEntry[] = [
  {
    id: "vcredist-x64",
    name: "VC++ 2015–2022 (x64)",
    description: "Visual C++ Redistributable — required by most games",
    category: "Runtimes",
    direct: true,
    url: "https://aka.ms/vs/17/release/vc_redist.x64.exe",
  },
  {
    id: "vcredist-x86",
    name: "VC++ 2015–2022 (x86)",
    description: "Visual C++ Redistributable 32-bit",
    category: "Runtimes",
    direct: true,
    url: "https://aka.ms/vs/17/release/vc_redist.x86.exe",
  },
  {
    id: "directx",
    name: "DirectX End-User Runtime",
    description: "Legacy DirectX components for older titles",
    category: "Runtimes",
    direct: true,
    url: "https://download.microsoft.com/download/1/7/1/1718CCC4-6315-4D8E-9543-8E28A4E18C4C/dxwebsetup.exe",
  },
  {
    id: "dotnet8",
    name: ".NET 8 Desktop Runtime",
    description: "Required by apps targeting .NET 8",
    category: "Runtimes",
    direct: false,
    url: "https://dotnet.microsoft.com/en-us/download/dotnet/8.0",
  },
  {
    id: "dotnet9",
    name: ".NET 9 Desktop Runtime",
    description: "Required by apps targeting .NET 9",
    category: "Runtimes",
    direct: false,
    url: "https://dotnet.microsoft.com/en-us/download/dotnet/9.0",
  },
  {
    id: "nvidia-drv",
    name: "NVIDIA Drivers",
    description: "Game Ready / Studio driver",
    category: "GPU Drivers",
    direct: false,
    url: "https://www.nvidia.com/Download/index.aspx",
  },
  {
    id: "amd-drv",
    name: "AMD Adrenalin",
    description: "Radeon Software for AMD GPUs",
    category: "GPU Drivers",
    direct: false,
    url: "https://www.amd.com/en/support",
  },
  {
    id: "intel-drv",
    name: "Intel Arc / UHD Graphics",
    description: "Intel graphics driver",
    category: "GPU Drivers",
    direct: false,
    url: "https://www.intel.com/content/www/us/en/download-center/home.html",
  },
  {
    id: "amd-chipset",
    name: "AMD Chipset Drivers",
    description: "Ryzen chipset & USB drivers",
    category: "Chipset",
    direct: false,
    url: "https://www.amd.com/en/support/download/drivers.html",
  },
  {
    id: "intel-chipset",
    name: "Intel Chipset INF",
    description: "Intel chipset device software",
    category: "Chipset",
    direct: false,
    url: "https://www.intel.com/content/www/us/en/download/19347/chipset-inf-utility.html",
  },
  {
    id: "npi",
    name: "NVIDIA Profile Inspector",
    description: "Advanced NVIDIA driver profile editor",
    category: "Utilities",
    direct: false,
    url: "https://github.com/Orbmu2k/nvidiaProfileInspector/releases",
  },
  {
    id: "gpu-z",
    name: "GPU-Z",
    description: "GPU information & monitoring utility",
    category: "Utilities",
    direct: false,
    url: "https://www.techpowerup.com/download/techpowerup-gpu-z/",
  },
  {
    id: "hwinfo",
    name: "HWiNFO64",
    description: "In-depth hardware diagnostics & sensors",
    category: "Utilities",
    direct: false,
    url: "https://www.hwinfo.com/download/",
  },
  {
    id: "ddu",
    name: "Display Driver Uninstaller",
    description: "Completely remove GPU drivers in safe mode",
    category: "Utilities",
    direct: false,
    url: "https://www.wagnardsoft.com/display-driver-uninstaller-DDU-",
  },
  {
    id: "nvclean",
    name: "NVCleanInstall",
    description: "Lean NVIDIA driver installer — strip telemetry",
    category: "Utilities",
    direct: false,
    url: "https://www.techpowerup.com/download/techpowerup-nvcleanstall/",
  },
];

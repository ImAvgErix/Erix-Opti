/** Shared catalog for GET /api/downloads and mock POST handling. */
export type DownloadCatalogEntry = {
  id: string;
  name: string;
  description: string;
  category: string;
  direct: boolean;
  url?: string;
};

export const defaultDownloadCatalog: DownloadCatalogEntry[] = [
  {
    id: "vcredist-x64",
    name: "VC++ 2015–2022 (x64)",
    description: "Visual C++ Redistributable",
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
    description: "dxwebsetup",
    category: "Runtimes",
    direct: true,
    url: "https://download.microsoft.com/download/1/7/1/1718CCC4-6315-4D8E-9543-8E28A4E18C4C/dxwebsetup.exe",
  },
  {
    id: "dotnet8",
    name: ".NET 8 Desktop Runtime",
    description: "Download page",
    category: "Runtimes",
    direct: false,
    url: "https://dotnet.microsoft.com/en-us/download/dotnet/8.0",
  },
  {
    id: "nvidia-drv",
    name: "NVIDIA Drivers",
    description: "GeForce / Studio",
    category: "GPU Drivers",
    direct: false,
    url: "https://www.nvidia.com/Download/index.aspx",
  },
  {
    id: "amd-drv",
    name: "AMD Adrenalin",
    description: "Radeon drivers",
    category: "GPU Drivers",
    direct: false,
    url: "https://www.amd.com/en/support",
  },
  {
    id: "npi",
    name: "NVIDIA Profile Inspector",
    description: "GitHub releases",
    category: "Utilities",
    direct: false,
    url: "https://github.com/Orbmu2k/nvidiaProfileInspector/releases",
  },
];

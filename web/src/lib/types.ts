export type TweakStatus = "applied" | "not_applied" | "partial" | "unknown";

export type TweakCategory =
  | "Input"
  | "System"
  | "Memory"
  | "Gaming"
  | "Privacy"
  | "Explorer"
  | "Visual"
  | "Storage"
  | "Services"
  | "Power"
  | "GPU"
  | "Network"
  | "Cleanup";

export type TweakItem = {
  id: string;
  name: string;
  category: TweakCategory;
  description: string;
  status: TweakStatus;
  conditional?: string;
};

export type HardwareSummary = {
  pcName: string;
  os: string;
  formFactor: string;
  cpu: string;
  cpuDetail: string;
  gpu: string;
  gpuDetail: string;
  ram: string;
  ramDetail: string;
  storage: string;
  storageDetail: string;
  motherboard: string;
  network: string;
  monitors: number;
  usbDevices: number;
};

export type OptimizePayload = {
  source?: string;
  ok?: boolean;
  message?: string;
  succeeded?: number;
  failed?: number;
  steps?: string[];
};

export type PlanPayload = {
  source?: string;
  tweakCount?: number;
  categories?: { name: string; count: number }[];
  notes?: string;
};

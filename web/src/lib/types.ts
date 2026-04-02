export type NavId = "overview" | "optimize" | "downloads" | "tools";

export type CatalogItem = {
  id: string;
  name: string;
  description: string;
  category: string;
  direct: boolean;
};

export type PlanPayload = {
  source?: string;
  tweakCount?: number;
  categories?: { name: string; count: number }[];
  notes?: string;
};

export type OptimizePayload = {
  source?: string;
  ok?: boolean;
  message?: string;
  succeeded?: number;
  failed?: number;
  steps?: string[];
};

export type ToolResult = {
  source?: string;
  ok?: boolean;
  message?: string;
  details?: string[];
};

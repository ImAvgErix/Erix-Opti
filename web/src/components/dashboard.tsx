"use client";

import * as React from "react";
import {
  CheckCircle2,
  ChevronDown,
  ChevronRight,
  Circle,
  Cpu,
  Gamepad2,
  Eye,
  FolderOpen,
  HardDrive,
  Loader2,
  MemoryStick,
  Monitor,
  MousePointer2,
  Server,
  ShieldCheck,
  Sparkles,
  Trash2,
  Wifi,
  XCircle,
  Zap,
} from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Progress } from "@/components/ui/progress";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Separator } from "@/components/ui/separator";
import { TooltipProvider } from "@/components/ui/tooltip";
import type { HardwareSummary, OptimizePayload, TweakItem, TweakStatus } from "@/lib/types";
import { tweakCatalog, categoryOrder } from "@/lib/tweak-catalog";
import { cn } from "@/lib/utils";

const iconMap: Record<string, React.ElementType> = {
  Input: MousePointer2,
  System: Cpu,
  Memory: MemoryStick,
  Gaming: Gamepad2,
  Privacy: ShieldCheck,
  Explorer: FolderOpen,
  Visual: Eye,
  Storage: HardDrive,
  Services: Server,
  Power: Zap,
  GPU: Monitor,
  Network: Wifi,
  Cleanup: Trash2,
};

const statusConfig: Record<TweakStatus, { label: string; color: string; icon: React.ElementType }> = {
  applied: { label: "Applied", color: "text-emerald-400", icon: CheckCircle2 },
  not_applied: { label: "Not applied", color: "text-muted-foreground/60", icon: Circle },
  partial: { label: "Partial", color: "text-amber-400", icon: Circle },
  unknown: { label: "Checking…", color: "text-muted-foreground/40", icon: Circle },
};

const mockHardware: HardwareSummary = {
  pcName: "DESKTOP-3QG2FRU",
  os: "Windows 10 IoT Enterprise LTSC 2024 · Build 26100.1742",
  formFactor: "Desktop",
  cpu: "AMD Ryzen 5 5600X 6-Core Processor",
  cpuDetail: "6C / 12T · 3700 MHz · L3 32 MB",
  gpu: "NVIDIA GeForce RTX 3070",
  gpuDetail: "Nvidia · 8 GB · Driver 32.0.15.5603",
  ram: "15.9 GB",
  ramDetail: "DDR4 · 3600 MHz · 2 sticks",
  storage: "Samsung SSD 990 PRO with Heatsink 2TB (1863 GB SSD)",
  storageDetail: "NVMe SSD boot volume",
  motherboard: "ASUSTeK COMPUTER INC. ROG STRIX B550-F GAMING WIFI II",
  network: "Ethernet I225-V (2500 Mbps)",
  monitors: 2,
  usbDevices: 18,
};

function StatusIndicator({ status }: { status: TweakStatus }) {
  const cfg = statusConfig[status];
  const Icon = cfg.icon;
  if (status === "applied") {
    return <CheckCircle2 className="h-4 w-4 shrink-0 text-emerald-400" />;
  }
  return <Icon className={cn("h-4 w-4 shrink-0", cfg.color)} />;
}

function CategorySection({
  category,
  tweaks,
  isOpen,
  onToggle,
}: {
  category: string;
  tweaks: TweakItem[];
  isOpen: boolean;
  onToggle: () => void;
}) {
  const Icon = iconMap[category] ?? Cpu;
  const appliedCount = tweaks.filter((t) => t.status === "applied").length;

  return (
    <div className="group/cat">
      <button
        type="button"
        onClick={onToggle}
        className="flex w-full items-center gap-3 rounded-xl border border-transparent px-4 py-3 text-left transition-all duration-200 hover:border-border/40 hover:bg-card/60"
      >
        <div className="flex h-8 w-8 items-center justify-center rounded-lg border border-border/30 bg-muted/10 transition-colors group-hover/cat:border-primary/20 group-hover/cat:bg-primary/[0.06]">
          <Icon className="h-4 w-4 text-primary" />
        </div>
        <div className="min-w-0 flex-1">
          <div className="flex items-center gap-2">
            <span className="text-sm font-semibold">{category}</span>
            <span className="text-[11px] tabular-nums text-muted-foreground">
              {appliedCount}/{tweaks.length}
            </span>
          </div>
        </div>
        <div className="flex items-center gap-2">
          {appliedCount === tweaks.length && tweaks.length > 0 ? (
            <Badge variant="outline" className="border-emerald-500/25 text-emerald-400 text-[10px] px-1.5 py-0">
              All applied
            </Badge>
          ) : appliedCount > 0 ? (
            <Badge variant="outline" className="border-amber-500/25 text-amber-400 text-[10px] px-1.5 py-0">
              {appliedCount} applied
            </Badge>
          ) : null}
          <ChevronRight
            className={cn(
              "h-4 w-4 text-muted-foreground transition-transform duration-200",
              isOpen && "rotate-90",
            )}
          />
        </div>
      </button>
      {isOpen && (
        <div className="ml-4 mt-1 space-y-0.5 border-l border-border/20 pl-4 animate-in slide-in-from-top-1 fade-in duration-200">
          {tweaks.map((tweak) => (
            <div
              key={tweak.id}
              className="flex items-start gap-3 rounded-lg px-3 py-2.5 transition-colors hover:bg-muted/[0.06]"
            >
              <StatusIndicator status={tweak.status} />
              <div className="min-w-0 flex-1">
                <div className="flex items-center gap-2">
                  <span className="text-[13px] font-medium">{tweak.name}</span>
                  {tweak.conditional && (
                    <span className="text-[10px] text-muted-foreground bg-muted/20 rounded px-1.5 py-0.5">
                      {tweak.conditional}
                    </span>
                  )}
                </div>
                <p className="mt-0.5 text-[12px] leading-relaxed text-muted-foreground">
                  {tweak.description}
                </p>
              </div>
              <span
                className={cn(
                  "shrink-0 text-[11px] font-medium tabular-nums",
                  tweak.status === "applied" ? "text-emerald-400" : "text-muted-foreground/50",
                )}
              >
                {statusConfig[tweak.status].label}
              </span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

function HardwareBar({ hw }: { hw: HardwareSummary }) {
  const items = [
    { label: "CPU", value: hw.cpu.replace(/ Processor$/i, ""), sub: hw.cpuDetail },
    { label: "GPU", value: hw.gpu, sub: hw.gpuDetail },
    { label: "RAM", value: hw.ram, sub: hw.ramDetail },
    { label: "Storage", value: hw.storage.split("(")[0].trim(), sub: hw.storageDetail },
  ];

  return (
    <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
      {items.map((item, i) => (
        <div
          key={item.label}
          className="opacity-0 animate-fade-up"
          style={{ animationDelay: `${i * 50}ms` }}
        >
          <div className="rounded-xl border border-border/40 bg-card/40 px-4 py-3 backdrop-blur-sm transition-all duration-300 hover:border-primary/20 hover:bg-card/60 hover:shadow-lift">
            <p className="text-[10px] font-medium uppercase tracking-wider text-muted-foreground">
              {item.label}
            </p>
            <p className="mt-1 truncate text-[13px] font-semibold">{item.value}</p>
            <p className="mt-0.5 truncate font-mono text-[11px] text-muted-foreground">
              {item.sub}
            </p>
          </div>
        </div>
      ))}
    </div>
  );
}

export function Dashboard() {
  const [tweaks, setTweaks] = React.useState<TweakItem[]>(tweakCatalog);
  const [openCategories, setOpenCategories] = React.useState<Set<string>>(new Set());
  const [optimizing, setOptimizing] = React.useState(false);
  const [progress, setProgress] = React.useState(0);
  const [resultOpen, setResultOpen] = React.useState(false);
  const [result, setResult] = React.useState<OptimizePayload | null>(null);
  const hw = mockHardware;

  React.useEffect(() => {
    fetch("/api/tweaks")
      .then((r) => r.json())
      .then((data: { tweaks?: TweakItem[] }) => {
        if (data.tweaks?.length) setTweaks(data.tweaks);
      })
      .catch(() => {});
  }, []);

  const grouped = React.useMemo(() => {
    const map = new Map<string, TweakItem[]>();
    for (const t of tweaks) {
      const arr = map.get(t.category) ?? [];
      arr.push(t);
      map.set(t.category, arr);
    }
    return categoryOrder
      .filter((c) => map.has(c))
      .map((c) => ({ category: c, tweaks: map.get(c)! }));
  }, [tweaks]);

  const stats = React.useMemo(() => {
    const total = tweaks.length;
    const applied = tweaks.filter((t) => t.status === "applied").length;
    return { total, applied, pct: total ? Math.round((applied / total) * 100) : 0 };
  }, [tweaks]);

  const toggleCategory = React.useCallback((cat: string) => {
    setOpenCategories((prev) => {
      const next = new Set(prev);
      if (next.has(cat)) next.delete(cat);
      else next.add(cat);
      return next;
    });
  }, []);

  const expandAll = React.useCallback(() => {
    setOpenCategories(new Set(categoryOrder));
  }, []);

  const collapseAll = React.useCallback(() => {
    setOpenCategories(new Set());
  }, []);

  const runOptimize = React.useCallback(async () => {
    setOptimizing(true);
    setProgress(0);
    setResult(null);
    const tick = window.setInterval(() => {
      setProgress((p) => (p >= 92 ? p : p + 2));
    }, 160);
    try {
      const r = await fetch("/api/optimize", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: "{}",
      });
      const data = (await r.json()) as OptimizePayload;
      setResult(data);
      setProgress(100);
      setResultOpen(true);
      if (data.ok) {
        setTweaks((prev) =>
          prev.map((t) => (t.status !== "applied" ? { ...t, status: "applied" as const } : t)),
        );
      }
    } catch (e) {
      setResult({
        ok: false,
        message: e instanceof Error ? e.message : "Network error",
      });
      setResultOpen(true);
    } finally {
      clearInterval(tick);
      setOptimizing(false);
    }
  }, []);

  return (
    <TooltipProvider delayDuration={300}>
      <div className="relative min-h-screen overflow-hidden">
        <div
          className="pointer-events-none fixed inset-0 bg-[size:48px_48px] bg-grid-fade opacity-40"
          aria-hidden
        />
        <div
          className="pointer-events-none fixed -left-1/4 top-0 h-[min(60vh,540px)] w-[min(60vw,540px)] rounded-full bg-glow-conic blur-3xl opacity-60"
          aria-hidden
        />
        <div
          className="pointer-events-none fixed bottom-0 right-0 h-[min(40vh,380px)] w-[min(40vw,380px)] rounded-full bg-primary/6 blur-[120px]"
          aria-hidden
        />

        <div className="relative z-10 mx-auto max-w-6xl px-4 pb-20 pt-8 sm:px-6 lg:px-8">
          {/* ── Header ── */}
          <header className="mb-8 opacity-0 animate-fade-up">
            <div className="flex items-center gap-3 mb-6">
              <div className="flex h-10 w-10 items-center justify-center rounded-xl border border-border/60 bg-card/60 shadow-glass backdrop-blur-xl">
                <Sparkles className="h-5 w-5 text-primary" />
              </div>
              <div>
                <h1 className="text-xl font-bold tracking-tight">ErixOpti</h1>
                <p className="text-[11px] text-muted-foreground">Hardware-aware Windows optimizer</p>
              </div>
            </div>

            <div className="flex flex-wrap items-center gap-2 mb-4">
              <Badge variant="secondary" className="font-normal text-[11px]">
                {hw.pcName}
              </Badge>
              <Badge variant="outline" className="border-primary/25 text-primary text-[11px]">
                {hw.formFactor}
              </Badge>
              <Badge variant="outline" className="text-[11px] text-muted-foreground">
                {hw.os}
              </Badge>
            </div>
          </header>

          {/* ── Hardware Summary ── */}
          <section className="mb-8">
            <HardwareBar hw={hw} />
          </section>

          {/* ── Platform details row ── */}
          <section className="mb-8 opacity-0 animate-fade-up [animation-delay:200ms]">
            <div className="grid gap-3 grid-cols-2 sm:grid-cols-4">
              {[
                { label: "Motherboard", value: hw.motherboard.replace(/ASUSTeK COMPUTER INC\.\s*/i, "") },
                { label: "Network", value: hw.network },
                { label: "Monitors", value: String(hw.monitors) },
                { label: "USB Devices", value: String(hw.usbDevices) },
              ].map((d) => (
                <div
                  key={d.label}
                  className="rounded-lg border border-border/25 bg-muted/[0.04] px-3 py-2.5 transition-colors hover:border-border/40"
                >
                  <p className="text-[10px] uppercase tracking-wider text-muted-foreground">{d.label}</p>
                  <p className="mt-0.5 truncate text-[13px] font-medium">{d.value}</p>
                </div>
              ))}
            </div>
          </section>

          <Separator className="mb-8 opacity-30" />

          {/* ── Optimize Hero ── */}
          <section className="mb-8 opacity-0 animate-fade-up [animation-delay:250ms]">
            <Card className="border-primary/15 bg-gradient-to-br from-primary/[0.03] via-transparent to-transparent shadow-lift">
              <CardContent className="flex flex-col gap-6 py-6 sm:flex-row sm:items-center sm:justify-between">
                <div className="space-y-2 min-w-0 flex-1">
                  <div className="flex items-center gap-2">
                    <Zap className="h-5 w-5 text-primary" />
                    <h2 className="text-lg font-bold">Auto Optimize</h2>
                  </div>
                  <p className="text-sm text-muted-foreground max-w-lg">
                    One-click hardware-aware optimization. Creates a restore point, backs up registry,
                    then applies all eligible tweaks matched to your hardware.
                  </p>
                  <div className="flex items-center gap-3 text-[12px] text-muted-foreground">
                    <span className="tabular-nums">
                      <strong className="text-foreground">{stats.applied}</strong> / {stats.total} applied
                    </span>
                    <span className="text-border">|</span>
                    <span className="tabular-nums font-mono text-primary">{stats.pct}%</span>
                  </div>
                  {optimizing && (
                    <div className="space-y-1.5 pt-1">
                      <Progress value={progress} className="h-1.5" />
                      <p className="text-[11px] tabular-nums text-muted-foreground">{progress}%</p>
                    </div>
                  )}
                </div>
                <Button
                  size="lg"
                  className="shrink-0 shadow-lift gap-2 px-8"
                  disabled={optimizing}
                  onClick={() => void runOptimize()}
                >
                  {optimizing ? (
                    <Loader2 className="h-4 w-4 animate-spin" />
                  ) : (
                    <Zap className="h-4 w-4" />
                  )}
                  {optimizing ? "Optimizing…" : "Auto Optimize"}
                </Button>
              </CardContent>
            </Card>
          </section>

          {/* ── Tweak List ── */}
          <section className="opacity-0 animate-fade-up [animation-delay:300ms]">
            <div className="flex items-center justify-between mb-4">
              <div>
                <h2 className="text-lg font-bold">Optimizations</h2>
                <p className="text-[12px] text-muted-foreground">
                  {stats.total} tweaks across {grouped.length} categories
                </p>
              </div>
              <div className="flex gap-2">
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={expandAll}
                  className="text-[11px] h-7 px-2"
                >
                  Expand all
                </Button>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={collapseAll}
                  className="text-[11px] h-7 px-2"
                >
                  Collapse
                </Button>
              </div>
            </div>

            <Card className="overflow-hidden">
              <ScrollArea className="max-h-[calc(100vh-200px)]">
                <div className="divide-y divide-border/15 p-2">
                  {grouped.map((g) => (
                    <CategorySection
                      key={g.category}
                      category={g.category}
                      tweaks={g.tweaks}
                      isOpen={openCategories.has(g.category)}
                      onToggle={() => toggleCategory(g.category)}
                    />
                  ))}
                </div>
              </ScrollArea>
            </Card>
          </section>

          {/* ── Safety footer ── */}
          <footer className="mt-8 flex items-center gap-2 text-[11px] text-muted-foreground opacity-0 animate-fade-up [animation-delay:400ms]">
            <ShieldCheck className="h-3.5 w-3.5 text-primary" />
            Automatic restore point + targeted registry backup before any changes.
          </footer>
        </div>

        {/* ── Result Dialog ── */}
        <Dialog open={resultOpen} onOpenChange={setResultOpen}>
          <DialogContent>
            <DialogHeader>
              <DialogTitle className="flex items-center gap-2">
                {result?.ok ? (
                  <CheckCircle2 className="h-5 w-5 text-emerald-400" />
                ) : (
                  <XCircle className="h-5 w-5 text-red-400" />
                )}
                Optimization {result?.ok ? "complete" : "finished with issues"}
              </DialogTitle>
              <DialogDescription>
                {result?.message}
              </DialogDescription>
            </DialogHeader>
            {result && (
              <div className="space-y-4 text-sm">
                {(result.succeeded !== undefined || result.failed !== undefined) && (
                  <div className="flex gap-4">
                    {result.succeeded !== undefined && (
                      <div className="rounded-md border border-emerald-500/20 bg-emerald-500/[0.06] px-4 py-2 text-center">
                        <p className="text-xl font-semibold tabular-nums text-emerald-400">
                          {result.succeeded}
                        </p>
                        <p className="text-xs text-muted-foreground">succeeded</p>
                      </div>
                    )}
                    {result.failed !== undefined && result.failed > 0 && (
                      <div className="rounded-md border border-red-500/20 bg-red-500/[0.06] px-4 py-2 text-center">
                        <p className="text-xl font-semibold tabular-nums text-red-400">
                          {result.failed}
                        </p>
                        <p className="text-xs text-muted-foreground">failed</p>
                      </div>
                    )}
                  </div>
                )}
                {result.steps?.length ? (
                  <>
                    <Separator />
                    <ul className="space-y-1 text-muted-foreground">
                      {result.steps.map((s) => (
                        <li key={s} className="flex gap-2">
                          <span className="text-primary">›</span>
                          {s}
                        </li>
                      ))}
                    </ul>
                  </>
                ) : null}
              </div>
            )}
            <DialogFooter>
              <Button onClick={() => setResultOpen(false)}>Done</Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </div>
    </TooltipProvider>
  );
}

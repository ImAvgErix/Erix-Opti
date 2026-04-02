"use client";

import * as React from "react";
import {
  Activity,
  ChevronDown,
  Cpu,
  Download,
  Gauge,
  HardDrive,
  LayoutDashboard,
  Loader2,
  Monitor,
  MoreHorizontal,
  ShieldCheck,
  Sparkles,
  Wrench,
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
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Progress } from "@/components/ui/progress";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Separator } from "@/components/ui/separator";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { cn } from "@/lib/utils";

const nav = [
  { id: "overview", label: "Overview", icon: LayoutDashboard },
  { id: "optimize", label: "Optimize", icon: Zap },
  { id: "downloads", label: "Downloads", icon: Download },
  { id: "tools", label: "Tools", icon: Wrench },
] as const;

const stats = [
  { label: "CPU", value: "Ryzen 9 7950X", sub: "16c / 32t", icon: Cpu },
  { label: "GPU", value: "RTX 4080", sub: "16 GB VRAM", icon: Monitor },
  { label: "Memory", value: "64 GB", sub: "DDR5-6000", icon: Activity },
  { label: "Storage", value: "2 TB NVMe", sub: "SSD boot", icon: HardDrive },
];

type CatalogItem = {
  id: string;
  name: string;
  description: string;
  category: string;
  direct: boolean;
};

type PlanPayload = {
  source?: string;
  tweakCount?: number;
  categories?: { name: string; count: number }[];
  notes?: string;
};

type OptimizePayload = {
  source?: string;
  ok?: boolean;
  message?: string;
  succeeded?: number;
  failed?: number;
  steps?: string[];
};

export function Dashboard() {
  const [active, setActive] = React.useState<(typeof nav)[number]["id"]>(
    "overview",
  );
  const [optimizing, setOptimizing] = React.useState(false);
  const [progress, setProgress] = React.useState(0);
  const [catalog, setCatalog] = React.useState<CatalogItem[]>([]);
  const [dlLoading, setDlLoading] = React.useState(false);
  const [dlError, setDlError] = React.useState<string | null>(null);
  const [planOpen, setPlanOpen] = React.useState(false);
  const [planLoading, setPlanLoading] = React.useState(false);
  const [planData, setPlanData] = React.useState<PlanPayload | null>(null);
  const [resultOpen, setResultOpen] = React.useState(false);
  const [optimizeResult, setOptimizeResult] =
    React.useState<OptimizePayload | null>(null);
  const [downloadBusyId, setDownloadBusyId] = React.useState<string | null>(
    null,
  );
  const [downloadLastMessage, setDownloadLastMessage] = React.useState<
    string | null
  >(null);

  React.useEffect(() => {
    if (active !== "downloads") return;
    let cancelled = false;
    setDlLoading(true);
    setDlError(null);
    fetch("/api/downloads")
      .then(async (r) => {
        if (!r.ok) throw new Error(`HTTP ${r.status}`);
        return r.json() as Promise<{ items?: CatalogItem[] }>;
      })
      .then((data) => {
        if (!cancelled) setCatalog(data.items ?? []);
      })
      .catch((e: Error) => {
        if (!cancelled) setDlError(e.message);
      })
      .finally(() => {
        if (!cancelled) setDlLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, [active]);

  const loadPlan = React.useCallback(async () => {
    setPlanLoading(true);
    setPlanData(null);
    try {
      const r = await fetch("/api/optimize/plan");
      const j = (await r.json()) as PlanPayload;
      setPlanData(j);
    } catch {
      setPlanData({ notes: "Failed to load plan." });
    } finally {
      setPlanLoading(false);
    }
  }, []);

  React.useEffect(() => {
    if (planOpen) void loadPlan();
  }, [planOpen, loadPlan]);

  const runOptimize = React.useCallback(async () => {
    setOptimizing(true);
    setProgress(0);
    setOptimizeResult(null);
    const tick = window.setInterval(() => {
      setProgress((p) => (p >= 92 ? p : p + 4));
    }, 140);
    try {
      const r = await fetch("/api/optimize", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({}),
      });
      const data = (await r.json()) as OptimizePayload;
      setOptimizeResult(data);
      setProgress(100);
      setResultOpen(true);
    } catch (e) {
      setOptimizeResult({
        ok: false,
        message: e instanceof Error ? e.message : "Request failed",
      });
      setResultOpen(true);
    } finally {
      window.clearInterval(tick);
      setOptimizing(false);
    }
  }, []);

  const startDownload = React.useCallback(async (id: string) => {
    setDownloadBusyId(id);
    setDownloadLastMessage(null);
    try {
      const r = await fetch("/api/downloads", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ id }),
      });
      const data = (await r.json()) as {
        ok?: boolean;
        message?: string;
        url?: string | null;
        direct?: boolean;
      };
      setDownloadLastMessage(data.message ?? (data.ok ? "OK" : "Failed"));
      if (data.url && typeof window !== "undefined") {
        window.open(data.url, "_blank", "noopener,noreferrer");
      }
    } catch (e) {
      setDownloadLastMessage(e instanceof Error ? e.message : "Request failed");
    } finally {
      setDownloadBusyId(null);
    }
  }, []);

  const copyApiBase = React.useCallback(() => {
    if (typeof window === "undefined") return;
    void navigator.clipboard.writeText(`${window.location.origin}/api`);
  }, []);

  return (
    <div className="relative min-h-screen overflow-hidden">
      <div
        className="pointer-events-none fixed inset-0 bg-[size:48px_48px] bg-grid-fade opacity-[0.65]"
        aria-hidden
      />
      <div
        className="pointer-events-none fixed -left-1/4 top-0 h-[min(70vh,640px)] w-[min(70vw,640px)] rounded-full bg-glow-conic blur-3xl opacity-90"
        aria-hidden
      />
      <div
        className="pointer-events-none fixed bottom-0 right-0 h-[min(50vh,480px)] w-[min(50vw,480px)] rounded-full bg-primary/10 blur-[100px]"
        aria-hidden
      />

      <div className="relative z-10 mx-auto flex min-h-screen max-w-[1400px] flex-col gap-8 px-4 pb-16 pt-10 sm:px-6 lg:flex-row lg:gap-12 lg:px-10 lg:pt-14">
        <aside className="flex shrink-0 flex-col gap-6 lg:w-56">
          <div className="flex items-center gap-3 px-1">
            <div className="flex h-10 w-10 items-center justify-center rounded-xl border border-border/60 bg-card/60 shadow-glass backdrop-blur-xl">
              <Sparkles className="h-5 w-5 text-primary" aria-hidden />
            </div>
            <div>
              <p className="text-sm font-semibold tracking-tight">ErixOpti</p>
              <p className="text-xs text-muted-foreground">Control surface</p>
            </div>
          </div>

          <nav className="flex flex-row gap-2 lg:flex-col lg:gap-1">
            {nav.map((item) => {
              const Icon = item.icon;
              const isActive = active === item.id;
              return (
                <button
                  key={item.id}
                  type="button"
                  onClick={() => setActive(item.id)}
                  className={cn(
                    "flex items-center gap-3 rounded-lg border px-3 py-2.5 text-left text-sm font-medium transition-all",
                    isActive
                      ? "border-primary/35 bg-primary/10 text-foreground shadow-lift"
                      : "border-transparent text-muted-foreground hover:border-border/60 hover:bg-card/40 hover:text-foreground",
                  )}
                >
                  <Icon className="h-4 w-4 shrink-0 opacity-80" />
                  {item.label}
                </button>
              );
            })}
          </nav>

          <Card className="hidden border-primary/20 bg-gradient-to-b from-primary/5 to-transparent lg:block">
            <CardHeader className="pb-2">
              <div className="flex items-center gap-2">
                <ShieldCheck className="h-4 w-4 text-primary" />
                <CardTitle className="text-sm">Safety first</CardTitle>
              </div>
              <CardDescription className="text-xs leading-relaxed">
                Restore point, targeted registry export, and BCD backup run
                before system changes.
              </CardDescription>
            </CardHeader>
          </Card>
        </aside>

        <main className="min-w-0 flex-1 space-y-10">
          <header className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between [animation-delay:0.05s] opacity-0 animate-fade-up">
            <div className="space-y-3">
              <div className="flex flex-wrap items-center gap-2">
                <Badge variant="secondary" className="font-normal">
                  Windows 11 · Desktop
                </Badge>
                <Badge
                  variant="outline"
                  className="border-primary/30 text-primary"
                >
                  Gaming profile
                </Badge>
              </div>
              <h1 className="text-balance text-4xl font-semibold tracking-tight sm:text-5xl">
                Precision tuning,
                <span className="bg-gradient-to-r from-primary via-sky-400 to-cyan-300 bg-clip-text text-transparent">
                  {" "}
                  pristine control
                </span>
              </h1>
              <p className="max-w-2xl text-pretty text-base text-muted-foreground sm:text-lg">
                Next.js API routes proxy to your WinUI host when{" "}
                <span className="font-mono text-xs text-primary/90">
                  ERIXOTI_HOST
                </span>{" "}
                is set; otherwise you get structured mock payloads for UI work.
              </p>
            </div>

            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="glass" size="sm" className="gap-2 shrink-0">
                  <MoreHorizontal className="h-4 w-4" />
                  API
                  <ChevronDown className="h-3.5 w-3.5 opacity-60" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-56">
                <DropdownMenuLabel>Integration</DropdownMenuLabel>
                <DropdownMenuItem onSelect={copyApiBase}>
                  Copy API base URL
                </DropdownMenuItem>
                <DropdownMenuSeparator />
                <DropdownMenuItem
                  onSelect={() =>
                    window.open(
                      "https://ui.shadcn.com/docs/components",
                      "_blank",
                      "noopener,noreferrer",
                    )
                  }
                >
                  shadcn docs
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          </header>

          <Tabs
            value={active}
            onValueChange={(v) => setActive(v as typeof active)}
            className="w-full"
          >
            <TabsList className="hidden w-full max-w-md sm:inline-flex">
              {nav.map((item) => (
                <TabsTrigger key={item.id} value={item.id} className="flex-1">
                  {item.label}
                </TabsTrigger>
              ))}
            </TabsList>

            <TabsContent value="overview" className="outline-none">
              <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
                {stats.map((s, i) => {
                  const Icon = s.icon;
                  return (
                    <div
                      key={s.label}
                      className="opacity-0 animate-fade-up"
                      style={{ animationDelay: `${i * 70}ms` }}
                    >
                      <Card className="group overflow-hidden transition-all duration-300 hover:border-primary/25 hover:shadow-lift">
                        <CardHeader className="flex flex-row items-start justify-between space-y-0 pb-2">
                          <div>
                            <CardDescription>{s.label}</CardDescription>
                            <CardTitle className="mt-1 text-lg font-semibold">
                              {s.value}
                            </CardTitle>
                          </div>
                          <div className="rounded-lg border border-border/50 bg-muted/20 p-2 text-primary transition-colors group-hover:border-primary/30">
                            <Icon className="h-4 w-4" />
                          </div>
                        </CardHeader>
                        <CardContent>
                          <p className="font-mono text-xs text-muted-foreground">
                            {s.sub}
                          </p>
                        </CardContent>
                      </Card>
                    </div>
                  );
                })}
              </div>

              <div className="mt-8 grid gap-6 lg:grid-cols-5">
                <Card className="lg:col-span-3">
                  <CardHeader>
                    <div className="flex items-center gap-2">
                      <Gauge className="h-4 w-4 text-primary" />
                      <CardTitle>System health</CardTitle>
                    </div>
                    <CardDescription>
                      Wire overview tiles to GET /api/hardware when you add it on
                      the host.
                    </CardDescription>
                  </CardHeader>
                  <CardContent className="space-y-6">
                    <div className="space-y-2">
                      <div className="flex justify-between text-sm">
                        <span className="text-muted-foreground">
                          Optimization coverage
                        </span>
                        <span className="font-mono text-xs text-primary">
                          72 tweaks eligible
                        </span>
                      </div>
                      <Progress value={78} />
                    </div>
                    <Separator />
                    <div className="grid gap-4 sm:grid-cols-3">
                      {[
                        { k: "Monitors", v: "2" },
                        { k: "USB devices", v: "14" },
                        { k: "Audio endpoints", v: "3" },
                      ].map((row) => (
                        <div
                          key={row.k}
                          className="rounded-lg border border-border/40 bg-muted/10 px-4 py-3"
                        >
                          <p className="text-xs text-muted-foreground">
                            {row.k}
                          </p>
                          <p className="mt-1 text-xl font-semibold tracking-tight">
                            {row.v}
                          </p>
                        </div>
                      ))}
                    </div>
                  </CardContent>
                </Card>

                <Card className="lg:col-span-2">
                  <CardHeader>
                    <CardTitle>Peripherals</CardTitle>
                    <CardDescription>
                      Dense list — pair with WMI from the desktop shell.
                    </CardDescription>
                  </CardHeader>
                  <CardContent>
                    <ScrollArea className="h-[220px] rounded-lg border border-border/40">
                      <ul className="space-y-2 p-4 pr-6 text-sm">
                        {[
                          "USB — Logitech G Pro X Superlight",
                          "USB — Elgato Stream Deck MK.2",
                          "Audio — Realtek USB Audio",
                          "Monitor — LG 27GP950-B (DP)",
                          "Monitor — Dell U2723QE (USB-C)",
                          "Network — Intel Ethernet I225-V",
                        ].map((line) => (
                          <li
                            key={line}
                            className="rounded-md border border-transparent px-2 py-1.5 hover:border-border/50 hover:bg-muted/20"
                          >
                            {line}
                          </li>
                        ))}
                      </ul>
                    </ScrollArea>
                  </CardContent>
                </Card>
              </div>
            </TabsContent>

            <TabsContent value="optimize" className="outline-none">
              <Card className="max-w-2xl border-primary/20 shadow-lift">
                <CardHeader>
                  <CardTitle>Auto optimize</CardTitle>
                  <CardDescription>
                    Calls{" "}
                    <span className="font-mono text-xs">POST /api/optimize</span>{" "}
                    (proxies to the host when configured).
                  </CardDescription>
                </CardHeader>
                <CardContent className="space-y-6">
                  <div className="space-y-2">
                    <div className="flex justify-between text-sm">
                      <span className="text-muted-foreground">Progress</span>
                      <span className="font-mono text-xs">{progress}%</span>
                    </div>
                    <Progress value={progress} />
                  </div>
                  <div className="flex flex-wrap gap-3">
                    <Button
                      size="lg"
                      className="shadow-lift"
                      disabled={optimizing}
                      onClick={() => void runOptimize()}
                    >
                      {optimizing ? (
                        <Loader2 className="h-4 w-4 animate-spin" />
                      ) : (
                        <Zap className="h-4 w-4" />
                      )}
                      Run optimization
                    </Button>
                    <Button
                      size="lg"
                      variant="glass"
                      type="button"
                      onClick={() => setPlanOpen(true)}
                    >
                      View tweak plan
                    </Button>
                  </div>
                  <p className="text-xs text-muted-foreground">
                    Desktop app should expose the same JSON contract on{" "}
                    <span className="font-mono">ERIXOTI_HOST</span> for a single
                    source of truth.
                  </p>
                </CardContent>
              </Card>
            </TabsContent>

            <TabsContent value="downloads" className="outline-none">
              {downloadLastMessage ? (
                <p className="mb-4 text-sm text-muted-foreground">
                  Last action: {downloadLastMessage}
                </p>
              ) : null}
              {dlLoading ? (
                <div className="flex items-center gap-2 text-sm text-muted-foreground">
                  <Loader2 className="h-4 w-4 animate-spin" />
                  Loading catalog from GET /api/downloads…
                </div>
              ) : null}
              {dlError ? (
                <p className="text-sm text-destructive">{dlError}</p>
              ) : null}
              <div className="grid gap-4 md:grid-cols-2">
                {catalog.map((d) => (
                  <Card key={d.id}>
                    <CardHeader className="flex flex-row items-start justify-between space-y-0">
                      <div>
                        <CardTitle className="text-base font-semibold">
                          {d.name}
                        </CardTitle>
                        <CardDescription>{d.category}</CardDescription>
                        <p className="mt-1 text-xs text-muted-foreground">
                          {d.description}
                        </p>
                      </div>
                      <Badge variant={d.direct ? "success" : "secondary"}>
                        {d.direct ? "Direct" : "Web"}
                      </Badge>
                    </CardHeader>
                    <CardContent className="flex gap-2">
                      <Button
                        size="sm"
                        className="flex-1"
                        disabled={downloadBusyId === d.id}
                        onClick={() => void startDownload(d.id)}
                      >
                        {downloadBusyId === d.id ? (
                          <Loader2 className="h-4 w-4 animate-spin" />
                        ) : (
                          "Download"
                        )}
                      </Button>
                      <Button size="sm" variant="outline" type="button">
                        Details
                      </Button>
                    </CardContent>
                  </Card>
                ))}
              </div>
            </TabsContent>

            <TabsContent value="tools" className="outline-none">
              <Card className="max-w-xl">
                <CardHeader>
                  <CardTitle>Maintenance</CardTitle>
                  <CardDescription>
                    Map these to POST /api/tools/* on the host when you add
                    routes.
                  </CardDescription>
                </CardHeader>
                <CardContent className="flex flex-wrap gap-3">
                  <Button variant="secondary">Deep clean</Button>
                  <Button variant="secondary">Create restore point</Button>
                  <Button variant="outline">Open Disk Management</Button>
                </CardContent>
              </Card>
            </TabsContent>
          </Tabs>
        </main>
      </div>

      <Dialog open={planOpen} onOpenChange={setPlanOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Tweak plan</DialogTitle>
            <DialogDescription>
              Loaded from{" "}
              <span className="font-mono text-xs">GET /api/optimize/plan</span>.
            </DialogDescription>
          </DialogHeader>
          {planLoading ? (
            <div className="flex items-center gap-2 py-6 text-sm text-muted-foreground">
              <Loader2 className="h-4 w-4 animate-spin" />
              Fetching…
            </div>
          ) : planData ? (
            <div className="space-y-4 text-sm">
              {planData.source ? (
                <Badge variant="outline">Source: {planData.source}</Badge>
              ) : null}
              {typeof planData.tweakCount === "number" ? (
                <p>
                  <span className="text-muted-foreground">Total tweaks:</span>{" "}
                  <span className="font-mono">{planData.tweakCount}</span>
                </p>
              ) : null}
              {planData.categories?.length ? (
                <ul className="space-y-2">
                  {planData.categories.map((c) => (
                    <li
                      key={c.name}
                      className="flex justify-between rounded-md border border-border/40 px-3 py-2"
                    >
                      <span>{c.name}</span>
                      <span className="font-mono text-muted-foreground">
                        {c.count}
                      </span>
                    </li>
                  ))}
                </ul>
              ) : null}
              {planData.notes ? (
                <p className="text-muted-foreground">{planData.notes}</p>
              ) : null}
            </div>
          ) : null}
          <DialogFooter>
            <Button type="button" variant="secondary" onClick={loadPlan}>
              Refresh
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog open={resultOpen} onOpenChange={setResultOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Optimization result</DialogTitle>
            <DialogDescription>
              Response from <span className="font-mono text-xs">POST</span>{" "}
              <span className="font-mono text-xs">/api/optimize</span>.
            </DialogDescription>
          </DialogHeader>
          {optimizeResult ? (
            <div className="space-y-3 text-sm">
              <div className="flex flex-wrap gap-2">
                {optimizeResult.source ? (
                  <Badge variant="outline">{optimizeResult.source}</Badge>
                ) : null}
                {optimizeResult.ok !== undefined ? (
                  <Badge variant={optimizeResult.ok ? "success" : "warning"}>
                    {optimizeResult.ok ? "OK" : "Issue"}
                  </Badge>
                ) : null}
              </div>
              {optimizeResult.message ? (
                <p className="text-muted-foreground">{optimizeResult.message}</p>
              ) : null}
              {optimizeResult.succeeded !== undefined ||
              optimizeResult.failed !== undefined ? (
                <p className="font-mono text-xs">
                  Succeeded: {optimizeResult.succeeded ?? "—"} · Failed:{" "}
                  {optimizeResult.failed ?? "—"}
                </p>
              ) : null}
              {optimizeResult.steps?.length ? (
                <ul className="list-inside list-disc text-muted-foreground">
                  {optimizeResult.steps.map((s) => (
                    <li key={s}>{s}</li>
                  ))}
                </ul>
              ) : null}
            </div>
          ) : null}
          <DialogFooter>
            <Button type="button" onClick={() => setResultOpen(false)}>
              Close
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

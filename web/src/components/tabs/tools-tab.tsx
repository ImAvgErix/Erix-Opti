"use client";

import * as React from "react";
import {
  AlertCircle,
  CheckCircle2,
  ClipboardList,
  FileX2,
  Loader2,
  ScrollText,
  Shield,
  Trash2,
  Wrench,
} from "lucide-react";

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
import type { ToolResult } from "@/lib/types";

const tools = [
  {
    id: "deep-clean",
    label: "Deep clean",
    description: "TEMP, Prefetch, Windows Update cache",
    icon: Trash2,
    variant: "default" as const,
  },
  {
    id: "restore-point",
    label: "Create restore point",
    description: "Safety checkpoint before manual changes",
    icon: Shield,
    variant: "secondary" as const,
  },
  {
    id: "clear-logs",
    label: "Clear event logs",
    description: "Wipe all Windows event log channels",
    icon: ScrollText,
    variant: "secondary" as const,
  },
  {
    id: "dism",
    label: "DISM cleanup",
    description: "Component store cleanup — frees stale WinSxS data",
    icon: FileX2,
    variant: "secondary" as const,
  },
] as const;

const shortcuts = [
  { label: "Programs & Features", cmd: "appwiz.cpl" },
  { label: "Power Options", cmd: "powercfg.cpl" },
  { label: "Device Manager", cmd: "devmgmt.msc" },
  { label: "Services", cmd: "services.msc" },
  { label: "Disk Management", cmd: "diskmgmt.msc" },
  { label: "Event Viewer", cmd: "eventvwr.msc" },
  { label: "Firewall", cmd: "firewall.cpl" },
  { label: "System Properties", cmd: "sysdm.cpl" },
] as const;

export function ToolsTab() {
  const [busyId, setBusyId] = React.useState<string | null>(null);
  const [resultOpen, setResultOpen] = React.useState(false);
  const [result, setResult] = React.useState<ToolResult | null>(null);

  const runTool = React.useCallback(async (action: string) => {
    setBusyId(action);
    setResult(null);
    try {
      const r = await fetch("/api/tools", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ action }),
      });
      const data = (await r.json()) as ToolResult;
      setResult(data);
      setResultOpen(true);
    } catch (e) {
      setResult({
        ok: false,
        message: e instanceof Error ? e.message : "Network error",
      });
      setResultOpen(true);
    } finally {
      setBusyId(null);
    }
  }, []);

  return (
    <>
      <div className="grid gap-6 lg:grid-cols-5">
        <div className="lg:col-span-3 space-y-6">
          <section>
            <h3 className="mb-3 flex items-center gap-2 text-[11px] font-semibold uppercase tracking-wider text-muted-foreground">
              <Wrench className="h-3.5 w-3.5" />
              Maintenance
            </h3>
            <div className="grid gap-3 sm:grid-cols-2">
              {tools.map((t) => {
                const Icon = t.icon;
                const isBusy = busyId === t.id;
                return (
                  <Card
                    key={t.id}
                    className="transition-all duration-200 hover:border-border/80"
                  >
                    <CardContent className="flex items-start gap-3 p-4">
                      <div className="mt-0.5 rounded-lg border border-border/40 bg-muted/15 p-2 text-primary">
                        <Icon className="h-4 w-4" />
                      </div>
                      <div className="min-w-0 flex-1">
                        <p className="text-sm font-medium">{t.label}</p>
                        <p className="mt-0.5 text-xs text-muted-foreground">
                          {t.description}
                        </p>
                        <Button
                          size="sm"
                          variant={t.variant}
                          className="mt-3 w-full"
                          disabled={isBusy}
                          onClick={() => void runTool(t.id)}
                        >
                          {isBusy ? (
                            <Loader2 className="h-3.5 w-3.5 animate-spin" />
                          ) : null}
                          {isBusy ? "Running…" : "Run"}
                        </Button>
                      </div>
                    </CardContent>
                  </Card>
                );
              })}
            </div>
          </section>
        </div>

        <div className="lg:col-span-2">
          <h3 className="mb-3 flex items-center gap-2 text-[11px] font-semibold uppercase tracking-wider text-muted-foreground">
            <ClipboardList className="h-3.5 w-3.5" />
            Control panel shortcuts
          </h3>
          <Card>
            <CardContent className="grid grid-cols-2 gap-2 p-4">
              {shortcuts.map((s) => (
                <Button
                  key={s.cmd}
                  variant="ghost"
                  size="sm"
                  className="justify-start text-xs font-normal text-muted-foreground hover:text-foreground"
                  title={s.cmd}
                >
                  {s.label}
                </Button>
              ))}
            </CardContent>
          </Card>
          <p className="mt-3 text-[11px] text-muted-foreground">
            Shortcuts open on the desktop host — map these to{" "}
            <span className="font-mono">POST /api/tools</span> with{" "}
            <span className="font-mono">{`{ action: "launch", cmd: "..." }`}</span>.
          </p>
        </div>
      </div>

      <Dialog open={resultOpen} onOpenChange={setResultOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2">
              {result?.ok ? (
                <CheckCircle2 className="h-5 w-5 text-emerald-400" />
              ) : (
                <AlertCircle className="h-5 w-5 text-amber-400" />
              )}
              {result?.ok ? "Done" : "Finished with warnings"}
            </DialogTitle>
            <DialogDescription>
              {result?.message}
            </DialogDescription>
          </DialogHeader>
          {result?.details?.length ? (
            <ul className="space-y-1 text-sm text-muted-foreground">
              {result.details.map((d) => (
                <li key={d} className="flex gap-2">
                  <span className="text-primary">›</span>
                  {d}
                </li>
              ))}
            </ul>
          ) : null}
          <DialogFooter>
            <Button onClick={() => setResultOpen(false)}>Close</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}

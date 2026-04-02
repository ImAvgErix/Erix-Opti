"use client";

import * as React from "react";
import { CheckCircle2, Loader2, XCircle, Zap } from "lucide-react";

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
import { Separator } from "@/components/ui/separator";
import type { OptimizePayload, PlanPayload } from "@/lib/types";

export function OptimizeTab() {
  const [optimizing, setOptimizing] = React.useState(false);
  const [progress, setProgress] = React.useState(0);
  const [planOpen, setPlanOpen] = React.useState(false);
  const [planLoading, setPlanLoading] = React.useState(false);
  const [planData, setPlanData] = React.useState<PlanPayload | null>(null);
  const [resultOpen, setResultOpen] = React.useState(false);
  const [result, setResult] = React.useState<OptimizePayload | null>(null);

  const loadPlan = React.useCallback(async () => {
    setPlanLoading(true);
    setPlanData(null);
    try {
      const r = await fetch("/api/optimize/plan");
      setPlanData((await r.json()) as PlanPayload);
    } catch {
      setPlanData({ notes: "Could not load tweak plan." });
    } finally {
      setPlanLoading(false);
    }
  }, []);

  React.useEffect(() => {
    if (planOpen) void loadPlan();
  }, [planOpen, loadPlan]);

  const run = React.useCallback(async () => {
    setOptimizing(true);
    setProgress(0);
    setResult(null);
    const tick = window.setInterval(() => {
      setProgress((p) => (p >= 92 ? p : p + 3));
    }, 140);
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
    <>
      <div className="grid gap-6 lg:grid-cols-5">
        <Card className="lg:col-span-3 border-primary/15 shadow-lift">
          <CardHeader>
            <CardTitle className="text-xl">Auto optimize</CardTitle>
            <CardDescription>
              One-click hardware-aware optimization — applies tweaks matched to
              your CPU, GPU, RAM, and storage configuration.
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-6">
            <div className="space-y-2">
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">Progress</span>
                <span className="font-mono text-xs tabular-nums">
                  {progress}%
                </span>
              </div>
              <Progress value={progress} />
            </div>
            <div className="flex flex-wrap gap-3">
              <Button
                size="lg"
                className="shadow-lift"
                disabled={optimizing}
                onClick={() => void run()}
              >
                {optimizing ? (
                  <Loader2 className="h-4 w-4 animate-spin" />
                ) : (
                  <Zap className="h-4 w-4" />
                )}
                {optimizing ? "Optimizing…" : "Run optimization"}
              </Button>
              <Button
                size="lg"
                variant="glass"
                onClick={() => setPlanOpen(true)}
              >
                View tweak plan
              </Button>
            </div>
          </CardContent>
        </Card>

        <Card className="lg:col-span-2">
          <CardHeader>
            <CardTitle>What happens</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4 text-sm text-muted-foreground">
            <ul className="space-y-2.5">
              {[
                "System restore point is created",
                "Targeted registry keys are backed up",
                "BCD store is exported",
                "72 hardware-matched tweaks are applied",
                "Results summary is displayed",
              ].map((step, i) => (
                <li key={step} className="flex gap-2.5">
                  <span className="mt-px flex h-5 w-5 shrink-0 items-center justify-center rounded-full border border-border/50 bg-muted/20 text-[10px] font-semibold tabular-nums text-muted-foreground">
                    {i + 1}
                  </span>
                  {step}
                </li>
              ))}
            </ul>
          </CardContent>
        </Card>
      </div>

      {/* Plan dialog */}
      <Dialog open={planOpen} onOpenChange={setPlanOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Tweak plan</DialogTitle>
            <DialogDescription>
              Categories and counts for the current hardware profile.
            </DialogDescription>
          </DialogHeader>
          {planLoading ? (
            <div className="flex items-center gap-2 py-8 text-sm text-muted-foreground">
              <Loader2 className="h-4 w-4 animate-spin" />
              Loading plan…
            </div>
          ) : planData ? (
            <div className="space-y-4 text-sm">
              {planData.source ? (
                <Badge variant="outline">Source: {planData.source}</Badge>
              ) : null}
              {typeof planData.tweakCount === "number" ? (
                <div className="flex items-baseline gap-2">
                  <span className="text-3xl font-semibold tabular-nums text-primary">
                    {planData.tweakCount}
                  </span>
                  <span className="text-muted-foreground">tweaks eligible</span>
                </div>
              ) : null}
              {planData.categories?.length ? (
                <ul className="space-y-1.5">
                  {planData.categories.map((c) => (
                    <li
                      key={c.name}
                      className="flex justify-between rounded-md border border-border/30 bg-muted/[0.06] px-3 py-2"
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
                <p className="text-xs text-muted-foreground">{planData.notes}</p>
              ) : null}
            </div>
          ) : null}
          <DialogFooter>
            <Button variant="secondary" onClick={loadPlan}>
              Refresh
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Result dialog */}
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
          </DialogHeader>
          {result ? (
            <div className="space-y-4 text-sm">
              {result.message ? (
                <p className="text-muted-foreground">{result.message}</p>
              ) : null}
              {(result.succeeded !== undefined ||
                result.failed !== undefined) && (
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
          ) : null}
          <DialogFooter>
            <Button onClick={() => setResultOpen(false)}>Done</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}

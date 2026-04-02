"use client";

import * as React from "react";
import {
  ArrowUpRight,
  CheckCircle2,
  Download,
  ExternalLink,
  Loader2,
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
import type { CatalogItem } from "@/lib/types";

function groupByCategory(items: CatalogItem[]) {
  const groups: Record<string, CatalogItem[]> = {};
  for (const item of items) {
    (groups[item.category] ??= []).push(item);
  }
  return Object.entries(groups);
}

export function DownloadsTab() {
  const [catalog, setCatalog] = React.useState<CatalogItem[]>([]);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState<string | null>(null);
  const [busyId, setBusyId] = React.useState<string | null>(null);
  const [doneIds, setDoneIds] = React.useState<Set<string>>(new Set());
  const [toast, setToast] = React.useState<string | null>(null);

  React.useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError(null);
    fetch("/api/downloads")
      .then(async (r) => {
        if (!r.ok) throw new Error(`HTTP ${r.status}`);
        return r.json() as Promise<{ items?: CatalogItem[] }>;
      })
      .then((data) => {
        if (!cancelled) setCatalog(data.items ?? []);
      })
      .catch((e: Error) => {
        if (!cancelled) setError(e.message);
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, []);

  const startDownload = React.useCallback(
    async (id: string) => {
      setBusyId(id);
      setToast(null);
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
        };
        if (data.url) {
          window.open(data.url, "_blank", "noopener,noreferrer");
        }
        setToast(data.message ?? (data.ok ? "Initiated" : "Failed"));
        if (data.ok) setDoneIds((prev) => new Set(prev).add(id));
      } catch (e) {
        setToast(e instanceof Error ? e.message : "Request failed");
      } finally {
        setBusyId(null);
      }
    },
    [],
  );

  if (loading) {
    return (
      <div className="flex items-center gap-2 py-16 text-sm text-muted-foreground justify-center">
        <Loader2 className="h-4 w-4 animate-spin" />
        Loading download catalog…
      </div>
    );
  }

  if (error) {
    return (
      <Card className="max-w-md mx-auto">
        <CardContent className="p-6 text-center">
          <p className="text-sm text-destructive">
            Failed to load catalog: {error}
          </p>
          <Button
            size="sm"
            variant="secondary"
            className="mt-4"
            onClick={() => window.location.reload()}
          >
            Retry
          </Button>
        </CardContent>
      </Card>
    );
  }

  const groups = groupByCategory(catalog);

  return (
    <div className="space-y-8">
      {toast && (
        <div className="rounded-lg border border-border/50 bg-card/80 px-4 py-2.5 text-sm text-muted-foreground backdrop-blur-sm animate-fade-up">
          {toast}
        </div>
      )}

      {groups.map(([category, items]) => (
        <section key={category}>
          <h3 className="mb-3 text-[11px] font-semibold uppercase tracking-wider text-muted-foreground">
            {category}
          </h3>
          <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
            {items.map((d) => {
              const isDone = doneIds.has(d.id);
              return (
                <Card
                  key={d.id}
                  className="transition-all duration-200 hover:border-border/80"
                >
                  <CardHeader className="flex flex-row items-start justify-between space-y-0 pb-2">
                    <div className="min-w-0 flex-1">
                      <CardTitle className="truncate text-[15px] font-semibold">
                        {d.name}
                      </CardTitle>
                      <CardDescription className="mt-0.5 text-xs">
                        {d.description}
                      </CardDescription>
                    </div>
                    <Badge
                      variant={d.direct ? "success" : "secondary"}
                      className="ml-2 shrink-0"
                    >
                      {d.direct ? "Direct" : "Web"}
                    </Badge>
                  </CardHeader>
                  <CardContent className="pt-0">
                    <Button
                      size="sm"
                      variant={isDone ? "secondary" : "default"}
                      className="w-full"
                      disabled={busyId === d.id}
                      onClick={() => void startDownload(d.id)}
                    >
                      {busyId === d.id ? (
                        <Loader2 className="h-3.5 w-3.5 animate-spin" />
                      ) : isDone ? (
                        <CheckCircle2 className="h-3.5 w-3.5 text-emerald-400" />
                      ) : d.direct ? (
                        <Download className="h-3.5 w-3.5" />
                      ) : (
                        <ExternalLink className="h-3.5 w-3.5" />
                      )}
                      {busyId === d.id
                        ? "Working…"
                        : isDone
                          ? "Done"
                          : d.direct
                            ? "Download & install"
                            : "Open page"}
                      {!d.direct && busyId !== d.id && !isDone && (
                        <ArrowUpRight className="h-3 w-3 ml-auto opacity-40" />
                      )}
                    </Button>
                  </CardContent>
                </Card>
              );
            })}
          </div>
        </section>
      ))}
    </div>
  );
}

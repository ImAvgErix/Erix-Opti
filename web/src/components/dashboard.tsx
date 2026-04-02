"use client";

import * as React from "react";
import { ChevronDown, Copy, ExternalLink, MoreHorizontal } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { TooltipProvider } from "@/components/ui/tooltip";
import type { NavId } from "@/lib/types";

import { NavRail } from "@/components/nav-rail";
import { OverviewTab } from "@/components/tabs/overview-tab";
import { OptimizeTab } from "@/components/tabs/optimize-tab";
import { DownloadsTab } from "@/components/tabs/downloads-tab";
import { ToolsTab } from "@/components/tabs/tools-tab";

const tabMeta: { id: NavId; label: string }[] = [
  { id: "overview", label: "Overview" },
  { id: "optimize", label: "Optimize" },
  { id: "downloads", label: "Downloads" },
  { id: "tools", label: "Tools" },
];

export function Dashboard() {
  const [active, setActive] = React.useState<NavId>("overview");

  const copyApiBase = React.useCallback(() => {
    if (typeof window === "undefined") return;
    void navigator.clipboard.writeText(`${window.location.origin}/api`);
  }, []);

  return (
    <TooltipProvider delayDuration={300}>
      <div className="relative min-h-screen overflow-hidden">
        {/* Ambient background layers */}
        <div
          className="pointer-events-none fixed inset-0 bg-[size:48px_48px] bg-grid-fade opacity-60"
          aria-hidden
        />
        <div
          className="pointer-events-none fixed -left-1/4 top-0 h-[min(70vh,640px)] w-[min(70vw,640px)] rounded-full bg-glow-conic blur-3xl opacity-80"
          aria-hidden
        />
        <div
          className="pointer-events-none fixed bottom-0 right-0 h-[min(50vh,480px)] w-[min(50vw,480px)] rounded-full bg-primary/8 blur-[120px]"
          aria-hidden
        />

        <div className="relative z-10 mx-auto flex min-h-screen max-w-[1440px] flex-col gap-8 px-4 pb-16 pt-10 sm:px-6 lg:flex-row lg:gap-12 lg:px-10 lg:pt-14">
          <NavRail active={active} onNavigate={setActive} />

          <main className="min-w-0 flex-1 space-y-10">
            {/* Header */}
            <header className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between opacity-0 animate-fade-up [animation-delay:40ms]">
              <div className="space-y-3">
                <div className="flex flex-wrap items-center gap-2">
                  <Badge variant="secondary" className="font-normal">
                    Windows 11 · Desktop
                  </Badge>
                  <Badge
                    variant="outline"
                    className="border-primary/25 text-primary"
                  >
                    Gaming profile
                  </Badge>
                </div>
                <h1 className="text-balance text-4xl font-semibold tracking-tight sm:text-5xl">
                  Precision tuning,
                  <span className="bg-gradient-to-r from-primary via-sky-400 to-cyan-300 bg-clip-text text-transparent">
                    {" "}pristine control
                  </span>
                </h1>
                <p className="max-w-2xl text-pretty text-[15px] leading-relaxed text-muted-foreground">
                  Hardware-aware optimization, in-app downloads, and system
                  maintenance — all from a single surface.
                </p>
              </div>

              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button
                    variant="glass"
                    size="sm"
                    className="gap-1.5 shrink-0"
                  >
                    <MoreHorizontal className="h-4 w-4" />
                    API
                    <ChevronDown className="h-3 w-3 opacity-50" />
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end" className="w-52">
                  <DropdownMenuLabel>Integration</DropdownMenuLabel>
                  <DropdownMenuItem onSelect={copyApiBase}>
                    <Copy className="h-3.5 w-3.5" />
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
                    <ExternalLink className="h-3.5 w-3.5" />
                    shadcn/ui docs
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
            </header>

            {/* Tabs */}
            <Tabs
              value={active}
              onValueChange={(v) => setActive(v as NavId)}
              className="w-full"
            >
              <TabsList className="hidden w-full max-w-md sm:inline-flex">
                {tabMeta.map((t) => (
                  <TabsTrigger key={t.id} value={t.id} className="flex-1">
                    {t.label}
                  </TabsTrigger>
                ))}
              </TabsList>

              <TabsContent value="overview" className="outline-none">
                <OverviewTab />
              </TabsContent>

              <TabsContent value="optimize" className="outline-none">
                <OptimizeTab />
              </TabsContent>

              <TabsContent value="downloads" className="outline-none">
                <DownloadsTab />
              </TabsContent>

              <TabsContent value="tools" className="outline-none">
                <ToolsTab />
              </TabsContent>
            </Tabs>
          </main>
        </div>
      </div>
    </TooltipProvider>
  );
}

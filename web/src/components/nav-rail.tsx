"use client";

import {
  Download,
  LayoutDashboard,
  ShieldCheck,
  Sparkles,
  Wrench,
  Zap,
} from "lucide-react";

import { Card, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { cn } from "@/lib/utils";
import type { NavId } from "@/lib/types";

const items = [
  { id: "overview" as NavId, label: "Overview", icon: LayoutDashboard },
  { id: "optimize" as NavId, label: "Optimize", icon: Zap },
  { id: "downloads" as NavId, label: "Downloads", icon: Download },
  { id: "tools" as NavId, label: "Tools", icon: Wrench },
];

export function NavRail({
  active,
  onNavigate,
}: {
  active: NavId;
  onNavigate: (id: NavId) => void;
}) {
  return (
    <aside className="flex shrink-0 flex-col gap-6 lg:w-56">
      <div className="flex items-center gap-3 px-1">
        <div className="flex h-10 w-10 items-center justify-center rounded-xl border border-border/60 bg-card/60 shadow-glass backdrop-blur-xl">
          <Sparkles className="h-5 w-5 text-primary" aria-hidden />
        </div>
        <div>
          <p className="text-sm font-semibold tracking-tight">ErixOpti</p>
          <p className="text-[11px] text-muted-foreground">v0.1 · Windows 11</p>
        </div>
      </div>

      <nav className="flex flex-row gap-2 lg:flex-col lg:gap-1">
        {items.map((item) => {
          const Icon = item.icon;
          const isActive = active === item.id;
          return (
            <button
              key={item.id}
              type="button"
              onClick={() => onNavigate(item.id)}
              className={cn(
                "flex items-center gap-3 rounded-lg border px-3 py-2.5 text-left text-sm font-medium transition-all duration-200",
                isActive
                  ? "border-primary/30 bg-primary/[0.08] text-foreground shadow-lift"
                  : "border-transparent text-muted-foreground hover:border-border/50 hover:bg-card/40 hover:text-foreground",
              )}
            >
              <Icon
                className={cn(
                  "h-4 w-4 shrink-0 transition-colors",
                  isActive ? "text-primary" : "opacity-60",
                )}
              />
              {item.label}
            </button>
          );
        })}
      </nav>

      <Card className="hidden border-primary/15 bg-gradient-to-b from-primary/[0.04] to-transparent lg:block">
        <CardHeader className="pb-3 pt-4">
          <div className="flex items-center gap-2">
            <ShieldCheck className="h-4 w-4 text-primary" />
            <CardTitle className="text-[13px]">Safety first</CardTitle>
          </div>
          <CardDescription className="text-xs leading-relaxed">
            Automatic restore point + targeted registry backup before any
            changes.
          </CardDescription>
        </CardHeader>
      </Card>
    </aside>
  );
}

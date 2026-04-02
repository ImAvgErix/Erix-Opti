"use client";

import {
  Activity,
  Cpu,
  Gauge,
  HardDrive,
  MemoryStick,
  Monitor,
  Network,
  Usb,
  Volume2,
} from "lucide-react";

import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Progress } from "@/components/ui/progress";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Separator } from "@/components/ui/separator";

const stats = [
  { label: "CPU", value: "Ryzen 9 7950X", sub: "16C / 32T · 5.7 GHz", icon: Cpu },
  { label: "GPU", value: "RTX 4080 Super", sub: "16 GB GDDR6X", icon: Monitor },
  { label: "Memory", value: "64 GB", sub: "DDR5-6000 · 2 × 32 GB", icon: MemoryStick },
  { label: "Storage", value: "2 TB NVMe", sub: "Samsung 990 PRO · SSD", icon: HardDrive },
];

const peripherals = [
  { type: "usb", icon: Usb, name: "Logitech G Pro X Superlight 2" },
  { type: "usb", icon: Usb, name: "Elgato Stream Deck MK.2" },
  { type: "usb", icon: Usb, name: "SteelSeries Arctis Nova Pro" },
  { type: "audio", icon: Volume2, name: "Realtek USB Audio" },
  { type: "monitor", icon: Monitor, name: "LG 27GP950-B (DP) — 3840×2160 @ 144 Hz" },
  { type: "monitor", icon: Monitor, name: "Dell U2723QE (USB-C) — 3840×2160 @ 60 Hz" },
  { type: "network", icon: Network, name: "Intel Ethernet I225-V — 2.5 GbE" },
];

const counters = [
  { label: "Monitors", value: "2" },
  { label: "USB devices", value: "14" },
  { label: "Audio endpoints", value: "3" },
];

export function OverviewTab() {
  return (
    <div className="space-y-8">
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        {stats.map((s, i) => {
          const Icon = s.icon;
          return (
            <div
              key={s.label}
              className="opacity-0 animate-fade-up"
              style={{ animationDelay: `${i * 60}ms` }}
            >
              <Card className="group overflow-hidden transition-all duration-300 hover:border-primary/20 hover:shadow-lift">
                <CardHeader className="flex flex-row items-start justify-between space-y-0 pb-2">
                  <div className="min-w-0">
                    <CardDescription className="text-xs uppercase tracking-wider">
                      {s.label}
                    </CardDescription>
                    <CardTitle className="mt-1.5 truncate text-[17px] font-semibold">
                      {s.value}
                    </CardTitle>
                  </div>
                  <div className="rounded-lg border border-border/40 bg-muted/15 p-2 text-primary transition-colors group-hover:border-primary/25 group-hover:bg-primary/[0.06]">
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

      <div className="grid gap-6 lg:grid-cols-5">
        <Card className="lg:col-span-3">
          <CardHeader>
            <div className="flex items-center gap-2">
              <Gauge className="h-4 w-4 text-primary" />
              <CardTitle>System readiness</CardTitle>
            </div>
            <CardDescription>
              Hardware-aware analysis determines eligible optimizations.
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-6">
            <div className="space-y-2">
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">
                  Optimization coverage
                </span>
                <span className="font-mono text-xs text-primary">
                  72 of 84 tweaks eligible
                </span>
              </div>
              <Progress value={86} />
            </div>
            <Separator />
            <div className="grid gap-3 sm:grid-cols-3">
              {counters.map((row) => (
                <div
                  key={row.label}
                  className="rounded-lg border border-border/30 bg-muted/[0.06] px-4 py-3 transition-colors hover:border-border/50"
                >
                  <p className="text-[11px] uppercase tracking-wider text-muted-foreground">
                    {row.label}
                  </p>
                  <p className="mt-1 text-2xl font-semibold tabular-nums tracking-tight">
                    {row.value}
                  </p>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        <Card className="lg:col-span-2">
          <CardHeader>
            <div className="flex items-center gap-2">
              <Activity className="h-4 w-4 text-primary" />
              <CardTitle>Peripherals</CardTitle>
            </div>
            <CardDescription>
              Detected devices from WMI PnP enumeration.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <ScrollArea className="h-[248px] rounded-lg border border-border/30">
              <ul className="space-y-0.5 p-3 text-sm">
                {peripherals.map((p) => {
                  const Icon = p.icon;
                  return (
                    <li
                      key={p.name}
                      className="flex items-center gap-2.5 rounded-md px-2 py-2 transition-colors hover:bg-muted/15"
                    >
                      <Icon className="h-3.5 w-3.5 shrink-0 text-muted-foreground" />
                      <span className="truncate">{p.name}</span>
                    </li>
                  );
                })}
              </ul>
            </ScrollArea>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

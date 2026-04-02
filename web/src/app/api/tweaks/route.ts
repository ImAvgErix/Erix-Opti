import { NextResponse } from "next/server";

import { forwardToErixOptiHost } from "@/lib/erixopti-proxy";
import { tweakCatalog } from "@/lib/tweak-catalog";
import type { TweakStatus } from "@/lib/types";

export async function GET() {
  const hostRes = await forwardToErixOptiHost("/api/tweaks", {
    method: "GET",
  });

  if (hostRes) {
    const text = await hostRes.text();
    return new NextResponse(text, {
      status: hostRes.status,
      headers: {
        "Content-Type":
          hostRes.headers.get("content-type") ?? "application/json",
      },
    });
  }

  const statuses: TweakStatus[] = ["applied", "not_applied"];
  const tweaks = tweakCatalog.map((t) => ({
    ...t,
    status: statuses[Math.random() > 0.55 ? 0 : 1],
  }));

  return NextResponse.json({ source: "mock", tweaks });
}

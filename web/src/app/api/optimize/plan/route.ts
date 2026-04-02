import { NextResponse } from "next/server";

import { forwardToErixOptiHost } from "@/lib/erixopti-proxy";

export async function GET() {
  const hostRes = await forwardToErixOptiHost("/api/optimize/plan", {
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

  return NextResponse.json({
    source: "mock",
    tweakCount: 72,
    categories: [
      { name: "Registry", count: 32 },
      { name: "Services", count: 18 },
      { name: "Power & GPU", count: 12 },
      { name: "Network & cleanup", count: 10 },
    ],
    notes:
      "Replace this payload by serving GET /api/optimize/plan from the desktop host when ERIXOTI_HOST is set.",
  });
}

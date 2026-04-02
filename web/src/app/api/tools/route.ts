import { NextResponse } from "next/server";

import { forwardToErixOptiHost } from "@/lib/erixopti-proxy";

type PostBody = { action?: string };

export async function POST(req: Request) {
  const body = (await req.json().catch(() => ({}))) as PostBody;
  const action = body.action?.trim();

  if (!action) {
    return NextResponse.json(
      { ok: false, message: "Missing action" },
      { status: 400 },
    );
  }

  const hostRes = await forwardToErixOptiHost("/api/tools", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ action }),
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

  await new Promise((r) => setTimeout(r, 600));

  const mocks: Record<string, object> = {
    "deep-clean": {
      source: "mock",
      ok: true,
      message: "Cleaned 1,247 files — freed 2.3 GB.",
      details: [
        "TEMP: 842 files, 1.1 GB",
        "Prefetch: 128 files, 340 MB",
        "WU cache: 277 files, 860 MB",
      ],
    },
    "restore-point": {
      source: "mock",
      ok: true,
      message: "System restore point created.",
    },
    "clear-logs": {
      source: "mock",
      ok: true,
      message: "Cleared 312 event logs.",
    },
    dism: {
      source: "mock",
      ok: true,
      message: "DISM component store cleanup complete.",
    },
  };

  const result = mocks[action];
  if (!result) {
    return NextResponse.json(
      { source: "mock", ok: false, message: `Unknown action: ${action}` },
      { status: 400 },
    );
  }

  return NextResponse.json(result);
}

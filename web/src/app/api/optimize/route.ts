import { NextResponse } from "next/server";

import { forwardToErixOptiHost } from "@/lib/erixopti-proxy";

export async function POST(req: Request) {
  const bodyText = await req.text();

  const hostRes = await forwardToErixOptiHost("/api/optimize", {
    method: "POST",
    body: bodyText.length ? bodyText : undefined,
    headers: {
      "Content-Type": req.headers.get("content-type") ?? "application/json",
    },
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

  await new Promise((r) => setTimeout(r, 450));

  return NextResponse.json({
    source: "mock",
    ok: true,
    message:
      "Demo response from Next.js. Set ERIXOTI_HOST in web/.env.local to forward POST /api/optimize to your WinUI / .NET listener.",
    succeeded: 48,
    failed: 2,
    steps: [
      "Restore point + targeted registry + BCD",
      "Registry, gaming, privacy",
      "Services & maintenance",
      "Power, GPU, network, cleanup",
    ],
  });
}

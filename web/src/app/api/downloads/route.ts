import { NextResponse } from "next/server";

import { defaultDownloadCatalog } from "@/lib/downloads-catalog";
import { forwardToErixOptiHost } from "@/lib/erixopti-proxy";

export async function GET() {
  const hostRes = await forwardToErixOptiHost("/api/downloads", {
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
    items: defaultDownloadCatalog.map((e) => ({
      id: e.id,
      name: e.name,
      description: e.description,
      category: e.category,
      direct: e.direct,
    })),
  });
}

type PostBody = { id?: string };

export async function POST(req: Request) {
  const body = (await req.json().catch(() => ({}))) as PostBody;
  const id = body.id?.trim();
  if (!id) {
    return NextResponse.json({ ok: false, message: "Missing id" }, { status: 400 });
  }

  const hostRes = await forwardToErixOptiHost("/api/downloads", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ id }),
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

  const entry = defaultDownloadCatalog.find((e) => e.id === id);
  if (!entry) {
    return NextResponse.json(
      { source: "mock", ok: false, message: "Unknown download id" },
      { status: 404 },
    );
  }

  return NextResponse.json({
    source: "mock",
    ok: true,
    id: entry.id,
    direct: entry.direct,
    url: entry.url,
    message: entry.direct
      ? `Ready to download ${entry.name}.`
      : `Opening ${entry.name} page.`,
  });
}

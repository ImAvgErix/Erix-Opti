/**
 * Server-only: forward requests to the ErixOpti desktop host when ERIXOTI_HOST is set.
 * Expected .NET routes (adjust paths to match your listener): /api/optimize, /api/optimize/plan, /api/downloads
 */
export function getErixOptiHost(): string | undefined {
  const raw = process.env.ERIXOTI_HOST?.trim();
  if (!raw) return undefined;
  return raw.replace(/\/$/, "");
}

export async function forwardToErixOptiHost(
  path: string,
  init?: RequestInit,
): Promise<Response | null> {
  const base = getErixOptiHost();
  if (!base) return null;
  const normalized = path.startsWith("/") ? path : `/${path}`;
  return fetch(`${base}${normalized}`, {
    ...init,
    cache: "no-store",
  });
}

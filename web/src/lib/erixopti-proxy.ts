const PROXY_TIMEOUT_MS = 8_000;

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
  const controller = new AbortController();
  const timer = setTimeout(() => controller.abort(), PROXY_TIMEOUT_MS);

  try {
    return await fetch(`${base}${normalized}`, {
      ...init,
      signal: controller.signal,
      cache: "no-store",
    });
  } catch {
    return null;
  } finally {
    clearTimeout(timer);
  }
}

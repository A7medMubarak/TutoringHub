export function decodePayload(token) {
  try {
    const base64 = token.split('.')[1];
    const json = atob(base64.replace(/-/g, '+').replace(/_/g, '/'));
    return JSON.parse(json);
  } catch {
    return null;
  }
}

export function isTokenExpired(token) {
  const payload = decodePayload(token);
  if (!payload?.exp) return true;
  return payload.exp * 1000 < Date.now();
}

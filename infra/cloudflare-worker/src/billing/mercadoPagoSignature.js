const HEX_SHA256 = /^[0-9a-f]{64}$/i;
const ALPHANUMERIC = /^[0-9a-z]+$/i;

function parseSignatureHeader(signatureHeader) {
  if (typeof signatureHeader !== 'string') {
    return null;
  }

  const parts = signatureHeader.split(',');
  if (parts.length !== 2) {
    return null;
  }

  let timestamp = null;
  let signature = null;
  for (const part of parts) {
    const match = /^[\t ]*(ts|v1)=([^\s,=]+)[\t ]*$/.exec(part);
    if (!match) {
      return null;
    }

    if (match[1] === 'ts') {
      if (timestamp !== null) {
        return null;
      }
      timestamp = match[2];
    } else {
      if (signature !== null) {
        return null;
      }
      signature = match[2];
    }
  }

  if (timestamp === null || !/^\d+$/.test(timestamp) || signature === null || !HEX_SHA256.test(signature)) {
    return null;
  }

  const signatureBytes = new Uint8Array(32);
  for (let i = 0; i < signatureBytes.length; i++) {
    signatureBytes[i] = Number.parseInt(signature.slice(i * 2, (i * 2) + 2), 16);
  }

  return { timestamp, signatureBytes };
}

/**
 * Validates a Mercado Pago webhook signature without reading or trusting its body.
 * The caller supplies the secret from a Worker secret binding.
 */
export async function verifyMercadoPagoSignature({ requestUrl, signatureHeader, requestId, secret } = {}) {
  const parsed = parseSignatureHeader(signatureHeader);
  if (!parsed || typeof secret !== 'string' || secret.length === 0) {
    return false;
  }

  if (requestId !== null && requestId !== undefined && typeof requestId !== 'string') {
    return false;
  }

  let url;
  try {
    url = new URL(requestUrl);
  } catch {
    return false;
  }

  const dataIds = url.searchParams.getAll('data.id');
  if (dataIds.length > 1) {
    return false;
  }

  const parts = [];
  const dataId = dataIds[0];
  if (dataId) {
    parts.push(`id:${ALPHANUMERIC.test(dataId) ? dataId.toLowerCase() : dataId};`);
  }
  if (requestId) {
    parts.push(`request-id:${requestId};`);
  }
  parts.push(`ts:${parsed.timestamp};`);

  try {
    const encoder = new TextEncoder();
    const key = await crypto.subtle.importKey(
      'raw',
      encoder.encode(secret),
      { name: 'HMAC', hash: 'SHA-256' },
      false,
      ['verify'],
    );

    // Web Crypto verifies the fixed 32-byte HMAC without a JavaScript string comparison.
    return await crypto.subtle.verify(
      'HMAC',
      key,
      parsed.signatureBytes,
      encoder.encode(parts.join('')),
    );
  } catch {
    return false;
  }
}

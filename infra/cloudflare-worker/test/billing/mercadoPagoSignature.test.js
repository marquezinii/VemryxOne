import { test } from 'node:test';
import assert from 'node:assert/strict';
import { verifyMercadoPagoSignature } from '../../src/billing/mercadoPagoSignature.js';

const SECRET = 'test-only-webhook-secret';

async function sign(manifest, secret = SECRET) {
  const encoder = new TextEncoder();
  const key = await crypto.subtle.importKey(
    'raw',
    encoder.encode(secret),
    { name: 'HMAC', hash: 'SHA-256' },
    false,
    ['sign'],
  );
  const signature = new Uint8Array(await crypto.subtle.sign('HMAC', key, encoder.encode(manifest)));
  return Array.from(signature, (byte) => byte.toString(16).padStart(2, '0')).join('');
}

test('accepts a valid signature and lowercases an alphanumeric data.id from the query', async () => {
  const timestamp = '1742505638683';
  const signature = await sign(`id:abc123;request-id:request-123;ts:${timestamp};`);

  assert.equal(await verifyMercadoPagoSignature({
    requestUrl: 'https://example.test/webhooks/mercado-pago?data.id=ABC123',
    signatureHeader: `ts=${timestamp},v1=${signature}`,
    requestId: 'request-123',
    secret: SECRET,
  }), true);
});

test('preserves a non-alphanumeric data.id and omits absent segments', async () => {
  const timestamp = '1';
  const withDataId = await sign(`id:AbC-123;ts:${timestamp};`);
  const withoutOptionalValues = await sign(`ts:${timestamp};`);

  assert.equal(await verifyMercadoPagoSignature({
    requestUrl: 'https://example.test/webhooks/mercado-pago?data.id=AbC-123',
    signatureHeader: `ts=${timestamp},v1=${withDataId}`,
    requestId: null,
    secret: SECRET,
  }), true);
  assert.equal(await verifyMercadoPagoSignature({
    requestUrl: 'https://example.test/webhooks/mercado-pago',
    signatureHeader: `v1=${withoutOptionalValues}, ts=${timestamp}`,
    secret: SECRET,
  }), true);
});

test('rejects malformed, incomplete, duplicate, and unknown signature fields', async () => {
  const validSignature = await sign('ts:1742505638683;');
  const invalidHeaders = [
    `v1=${validSignature}`,
    'ts=1742505638683',
    `ts=not-a-number,v1=${validSignature}`,
    `ts=1742505638683,v1=${validSignature.slice(2)}`,
    `ts=1742505638683,v1=${'g'.repeat(64)}`,
    `ts=1742505638683,ts=1742505638683,v1=${validSignature}`,
    `ts=1742505638683,v1=${validSignature},v2=${validSignature}`,
  ];

  for (const signatureHeader of invalidHeaders) {
    assert.equal(await verifyMercadoPagoSignature({
      requestUrl: 'https://example.test/webhooks/mercado-pago',
      signatureHeader,
      secret: SECRET,
    }), false, signatureHeader);
  }
});

test('rejects a wrong signature, wrong secret, and ambiguous duplicate data.id query values', async () => {
  const timestamp = '1742505638683';
  const signature = await sign(`id:123;ts:${timestamp};`);
  const input = {
    requestUrl: 'https://example.test/webhooks/mercado-pago?data.id=123',
    signatureHeader: `ts=${timestamp},v1=${signature}`,
    secret: SECRET,
  };

  assert.equal(await verifyMercadoPagoSignature({ ...input, secret: 'wrong-secret' }), false);
  assert.equal(await verifyMercadoPagoSignature({
    ...input,
    signatureHeader: `ts=${timestamp},v1=${'0'.repeat(64)}`,
  }), false);
  assert.equal(await verifyMercadoPagoSignature({
    ...input,
    requestUrl: `${input.requestUrl}&data.id=456`,
  }), false);
});

test('fails closed for invalid inputs instead of throwing', async () => {
  const invalidInputs = [
    undefined,
    {},
    { requestUrl: 'not a URL', signatureHeader: `ts=1,v1=${'0'.repeat(64)}`, secret: SECRET },
    { requestUrl: 'https://example.test', signatureHeader: `ts=1,v1=${'0'.repeat(64)}`, secret: '' },
    { requestUrl: 'https://example.test', signatureHeader: `ts=1,v1=${'0'.repeat(64)}`, requestId: 123, secret: SECRET },
  ];

  for (const input of invalidInputs) {
    assert.equal(await verifyMercadoPagoSignature(input), false);
  }
});

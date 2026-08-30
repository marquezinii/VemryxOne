import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { DatabaseSync } from 'node:sqlite';
import worker from '../../src/index.js';
import { deleteAccountProfile } from '../../src/auth/accountProfile.js';
import { handleMercadoPagoWebhook } from '../../src/billing/mercadoPagoWebhook.js';

const SECRET = 'test-only-webhook-secret';
const ACCESS_TOKEN = 'test-only-access-token';
const RESOURCE_ID = '2c938084726fca480172750000000001';
const EXTERNAL_REFERENCE = 'opaque-checkout-reference';
const PROVIDER_UPDATED_AT = '2026-08-29T11:59:00.000Z';
const MIGRATION = readFileSync(
  new URL('../../migrations/0007_billing_foundation.sql', import.meta.url),
  'utf8',
);

class SqliteD1 {
  constructor(sqlite) {
    this.sqlite = sqlite;
    this.beforeBatch = null;
    this.throwAfterCommit = false;
  }

  prepare(sql) {
    return {
      bind: (...params) => ({
        first: async () => this.sqlite.prepare(sql).get(...params) ?? null,
        run: async () => {
          const result = this.sqlite.prepare(sql).run(...params);
          return { success: true, meta: { changes: Number(result.changes) } };
        },
      }),
    };
  }

  async batch(statements) {
    this.beforeBatch?.();
    this.beforeBatch = null;
    this.sqlite.exec('BEGIN IMMEDIATE');
    let committed = false;
    try {
      const results = [];
      for (const statement of statements) results.push(await statement.run());
      this.sqlite.exec('COMMIT');
      committed = true;
      if (this.throwAfterCommit) {
        this.throwAfterCommit = false;
        throw new Error('simulated D1 timeout after commit');
      }
      return results;
    } catch (error) {
      if (!committed) this.sqlite.exec('ROLLBACK');
      throw error;
    }
  }
}

function createDatabase() {
  const sqlite = new DatabaseSync(':memory:');
  sqlite.exec(`
    PRAGMA foreign_keys = ON;
    CREATE TABLE account_profiles (uid TEXT PRIMARY KEY NOT NULL);
  `);
  sqlite.exec(MIGRATION);
  return { sqlite, db: new SqliteD1(sqlite) };
}

function seedIntent(sqlite, {
  id = 'intent-1',
  accountUid = 'firebase-uid-123',
  externalReference = EXTERNAL_REFERENCE,
  providerCheckoutId = null,
  state = 'created',
} = {}) {
  sqlite.prepare('INSERT OR IGNORE INTO account_profiles (uid) VALUES (?)').run(accountUid);
  sqlite.prepare(
    `INSERT INTO billing_checkout_intents
       (id, account_uid, provider, external_reference, offer_key, amount_cents,
        currency, provider_checkout_id, state, created_at, updated_at)
     VALUES (?, ?, 'mercado_pago', ?, 'ralven_pro_monthly', 2990,
             'BRL', ?, ?, '2026-08-29T11:00:00.000Z', '2026-08-29T11:00:00.000Z')`,
  ).run(id, accountUid, externalReference, providerCheckoutId, state);
}

function preapproval(overrides = {}) {
  return {
    id: RESOURCE_ID,
    external_reference: EXTERNAL_REFERENCE,
    status: 'authorized',
    last_modified: PROVIDER_UPDATED_AT,
    auto_recurring: { transaction_amount: 29.9, currency_id: 'BRL' },
    ...overrides,
  };
}

async function sign(manifest, secret = SECRET) {
  const encoder = new TextEncoder();
  const key = await crypto.subtle.importKey(
    'raw',
    encoder.encode(secret),
    { name: 'HMAC', hash: 'SHA-256' },
    false,
    ['sign'],
  );
  const bytes = new Uint8Array(await crypto.subtle.sign('HMAC', key, encoder.encode(manifest)));
  return Array.from(bytes, byte => byte.toString(16).padStart(2, '0')).join('');
}

async function webhookRequest({
  resourceId = RESOURCE_ID,
  requestId = 'request-123',
  signingSecret = SECRET,
} = {}) {
  const timestamp = '1788004800000';
  const signature = await sign(
    `id:${resourceId.toLowerCase()};request-id:${requestId};ts:${timestamp};`,
    signingSecret,
  );
  return new Request(`https://worker.test/billing/mercado-pago/webhook?data.id=${resourceId}`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'x-request-id': requestId,
      'x-signature': `ts=${timestamp},v1=${signature}`,
    },
    body: JSON.stringify({ data: { id: 'untrusted' }, status: 'cancelled' }),
  });
}

function environment(db) {
  return {
    TELEMETRY_DB: db,
    MERCADO_PAGO_ACCESS_TOKEN: ACCESS_TOKEN,
    MERCADO_PAGO_WEBHOOK_SECRET: SECRET,
  };
}

function providerFetch(payload) {
  const calls = [];
  return {
    calls,
    fetchImpl: async (url, init) => {
      calls.push({ url, init });
      return new Response(JSON.stringify(payload), {
        status: 200,
        headers: { 'Content-Type': 'application/json' },
      });
    },
  };
}

test('reconciles and replays a canonical preapproval against real SQLite', async (t) => {
  const { sqlite, db } = createDatabase();
  t.after(() => sqlite.close());
  seedIntent(sqlite);
  const provider = providerFetch(preapproval());

  const response = await handleMercadoPagoWebhook(
    await webhookRequest(),
    environment(db),
    { fetchImpl: provider.fetchImpl },
  );

  assert.equal(response.status, 200);
  assert.deepEqual(await response.json(), { accepted: true });
  assert.equal(provider.calls[0].url, `https://api.mercadopago.com/preapproval/${RESOURCE_ID}`);
  assert.equal(provider.calls[0].init.headers.Authorization, `Bearer ${ACCESS_TOKEN}`);
  assert.deepEqual(
    { ...sqlite.prepare(
      'SELECT account_uid, checkout_intent_id, state, provider_updated_at FROM billing_subscriptions',
    ).get() },
    {
      account_uid: 'firebase-uid-123',
      checkout_intent_id: 'intent-1',
      state: 'authorized',
      provider_updated_at: PROVIDER_UPDATED_AT,
    },
  );
  assert.deepEqual(
    { ...sqlite.prepare('SELECT provider_checkout_id, state FROM billing_checkout_intents').get() },
    { provider_checkout_id: RESOURCE_ID, state: 'completed' },
  );
  assert.equal(
    sqlite.prepare('SELECT processing_outcome FROM billing_webhook_events').get().processing_outcome,
    'processed',
  );
  assert.equal(sqlite.prepare('SELECT COUNT(*) AS count FROM account_entitlements').get().count, 0);

  const replay = await handleMercadoPagoWebhook(
    await webhookRequest(),
    environment(db),
    { fetchImpl: provider.fetchImpl },
  );
  assert.equal(replay.status, 200);
  assert.equal(provider.calls.length, 1);
  assert.equal(sqlite.prepare('SELECT COUNT(*) AS count FROM billing_subscriptions').get().count, 1);
});

test('equal or older provider timestamps cannot diverge checkout and subscription state', async (t) => {
  const { sqlite, db } = createDatabase();
  t.after(() => sqlite.close());
  seedIntent(sqlite);

  await handleMercadoPagoWebhook(
    await webhookRequest(),
    environment(db),
    { fetchImpl: providerFetch(preapproval()).fetchImpl },
  );
  await handleMercadoPagoWebhook(
    await webhookRequest({ requestId: 'request-equal' }),
    environment(db),
    { fetchImpl: providerFetch(preapproval({ status: 'cancelled' })).fetchImpl },
  );
  assert.equal(sqlite.prepare('SELECT state FROM billing_subscriptions').get().state, 'authorized');
  assert.equal(sqlite.prepare('SELECT state FROM billing_checkout_intents').get().state, 'completed');

  await handleMercadoPagoWebhook(
    await webhookRequest({ requestId: 'request-newer' }),
    environment(db),
    {
      fetchImpl: providerFetch(preapproval({
        status: 'paused',
        last_modified: '2026-08-29T12:00:00.000Z',
      })).fetchImpl,
    },
  );
  assert.equal(sqlite.prepare('SELECT state FROM billing_subscriptions').get().state, 'paused');
  assert.equal(sqlite.prepare('SELECT state FROM billing_checkout_intents').get().state, 'completed');

  await handleMercadoPagoWebhook(
    await webhookRequest({ requestId: 'request-older' }),
    environment(db),
    {
      fetchImpl: providerFetch(preapproval({
        status: 'cancelled',
        last_modified: '2026-08-29T11:58:00.000Z',
      })).fetchImpl,
    },
  );
  assert.equal(sqlite.prepare('SELECT state FROM billing_subscriptions').get().state, 'paused');
  assert.equal(sqlite.prepare('SELECT state FROM billing_checkout_intents').get().state, 'completed');
});

test('rejects a bad HMAC before provider or database writes', async (t) => {
  const { sqlite, db } = createDatabase();
  t.after(() => sqlite.close());
  seedIntent(sqlite);
  let fetchCount = 0;

  const response = await handleMercadoPagoWebhook(
    await webhookRequest({ signingSecret: 'wrong-secret' }),
    environment(db),
    { fetchImpl: async () => { fetchCount += 1; } },
  );

  assert.equal(response.status, 401);
  assert.equal(fetchCount, 0);
  assert.equal(sqlite.prepare('SELECT COUNT(*) AS count FROM billing_webhook_events').get().count, 0);
});

test('account deletion is blocked while a checkout exists without a subscription', async (t) => {
  const { sqlite, db } = createDatabase();
  t.after(() => sqlite.close());
  seedIntent(sqlite);

  assert.equal(await deleteAccountProfile(db, 'firebase-uid-123'), false);
  assert.equal(sqlite.prepare('SELECT COUNT(*) AS count FROM account_profiles').get().count, 1);
  assert.equal(sqlite.prepare('SELECT COUNT(*) AS count FROM billing_checkout_intents').get().count, 1);
});

test('records canonical reference and price mismatches as ignored', async (t) => {
  const { sqlite, db } = createDatabase();
  t.after(() => sqlite.close());
  seedIntent(sqlite);
  const cases = [
    ['request-reference', preapproval({ external_reference: 'unknown-reference' })],
    ['request-price', preapproval({ auto_recurring: { transaction_amount: 30, currency_id: 'BRL' } })],
  ];

  for (const [requestId, payload] of cases) {
    const response = await handleMercadoPagoWebhook(
      await webhookRequest({ requestId }),
      environment(db),
      { fetchImpl: providerFetch(payload).fetchImpl },
    );
    assert.equal(response.status, 200);
  }

  assert.equal(sqlite.prepare('SELECT COUNT(*) AS count FROM billing_subscriptions').get().count, 0);
  assert.deepEqual(
    sqlite.prepare('SELECT processing_outcome FROM billing_webhook_events ORDER BY id')
      .all().map(row => ({ ...row })),
    [{ processing_outcome: 'ignored' }, { processing_outcome: 'ignored' }],
  );
});

test('a timeout after commit replays without a second provider fetch', async (t) => {
  const { sqlite, db } = createDatabase();
  t.after(() => sqlite.close());
  seedIntent(sqlite);
  db.throwAfterCommit = true;
  const provider = providerFetch(preapproval());

  const first = await handleMercadoPagoWebhook(
    await webhookRequest(),
    environment(db),
    { fetchImpl: provider.fetchImpl },
  );
  const replay = await handleMercadoPagoWebhook(
    await webhookRequest(),
    environment(db),
    { fetchImpl: provider.fetchImpl },
  );

  assert.equal(first.status, 503);
  assert.equal(replay.status, 200);
  assert.equal(provider.calls.length, 1);
  assert.equal(sqlite.prepare('SELECT COUNT(*) AS count FROM billing_subscriptions').get().count, 1);
});

test('a concurrent ownership conflict leaves the event pending and asks for retry', async (t) => {
  const { sqlite, db } = createDatabase();
  t.after(() => sqlite.close());
  seedIntent(sqlite);
  db.beforeBatch = () => {
    seedIntent(sqlite, {
      id: 'intent-other',
      accountUid: 'firebase-uid-other',
      externalReference: 'opaque-other-reference',
      providerCheckoutId: RESOURCE_ID,
      state: 'completed',
    });
    sqlite.prepare(
      `INSERT INTO billing_subscriptions
         (id, account_uid, checkout_intent_id, provider, provider_subscription_id,
          offer_key, state, provider_updated_at, created_at, updated_at)
       VALUES (?, 'firebase-uid-other', 'intent-other', 'mercado_pago', ?,
               'ralven_pro_monthly', 'authorized', '2026-08-29T12:01:00.000Z',
               '2026-08-29T12:01:00.000Z', '2026-08-29T12:01:00.000Z')`,
    ).run(`mercado_pago:${RESOURCE_ID}`, RESOURCE_ID);
  };

  const response = await handleMercadoPagoWebhook(
    await webhookRequest(),
    environment(db),
    { fetchImpl: providerFetch(preapproval()).fetchImpl },
  );

  assert.equal(response.status, 503);
  assert.equal(
    sqlite.prepare('SELECT processing_outcome FROM billing_webhook_events').get().processing_outcome,
    'pending',
  );
  assert.deepEqual(
    {
      ...sqlite.prepare('SELECT provider_checkout_id, state FROM billing_checkout_intents WHERE id = ?')
        .get('intent-1'),
    },
    { provider_checkout_id: null, state: 'created' },
  );
  assert.equal(
    sqlite.prepare('SELECT account_uid FROM billing_subscriptions WHERE provider_subscription_id = ?')
      .get(RESOURCE_ID).account_uid,
    'firebase-uid-other',
  );
});

test('worker wires the entitlement and Mercado Pago routes', async () => {
  const entitlements = await worker.fetch(
    new Request('https://worker.test/account/entitlements'),
    {},
  );
  const webhook = await worker.fetch(
    new Request('https://worker.test/billing/mercado-pago/webhook', { method: 'POST' }),
    {},
  );

  assert.equal(entitlements.status, 401);
  assert.equal(webhook.status, 400);
});

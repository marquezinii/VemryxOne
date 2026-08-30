import { test } from 'node:test';
import assert from 'node:assert/strict';
import { fetchAccountEntitlements } from '../../src/billing/entitlements.js';

const NOW = '2026-08-29T12:00:00.000Z';

function fakeDb(rows = []) {
  const calls = [];
  return {
    calls,
    prepare(sql) {
      return {
        bind(...params) {
          calls.push({ sql, params });
          return {
            async first() {
              const [uid, entitlementKey, validFromCutoff, validUntilCutoff] = params;
              const row = rows.find(candidate => candidate.account_uid === uid
                && candidate.entitlement_key === entitlementKey
                && ['active', 'grace_period'].includes(candidate.state)
                && candidate.valid_from <= validFromCutoff
                && candidate.valid_until > validUntilCutoff);
              return row === undefined
                ? null
                : { entitlement_key: row.entitlement_key, valid_until: row.valid_until };
            },
          };
        },
      };
    },
  };
}

function entitlement(overrides = {}) {
  return {
    account_uid: 'firebase-uid-123',
    entitlement_key: 'ralven_pro',
    state: 'active',
    valid_from: '2026-08-01T00:00:00.000Z',
    valid_until: '2026-09-01T00:00:00.000Z',
    provider_subscription_id: 'must-never-leak',
    ...overrides,
  };
}

test('fetchAccountEntitlements grants only current active or grace-period Ralven Pro access', async () => {
  for (const state of ['active', 'grace_period']) {
    const db = fakeDb([entitlement({ state })]);
    assert.deepEqual(await fetchAccountEntitlements(db, 'firebase-uid-123', NOW), {
      tier: 'pro',
      entitlements: ['ralven_pro'],
      validUntil: '2026-09-01T00:00:00.000Z',
    });

    const [{ sql, params }] = db.calls;
    assert.match(sql, /entitlement_key = \?/);
    assert.match(sql, /state IN \('active', 'grace_period'\)/);
    assert.match(sql, /valid_from <= \?/);
    assert.match(sql, /valid_until > \?/);
    assert.doesNotMatch(sql, /provider|subscription/i);
    assert.deepEqual(params, ['firebase-uid-123', 'ralven_pro', NOW, NOW]);
  }
});

test('fetchAccountEntitlements returns free for absent, revoked, expired, or not-yet-valid access', async () => {
  const cases = [
    [],
    [entitlement({ state: 'revoked' })],
    [entitlement({ state: 'expired' })],
    [entitlement({ valid_until: NOW })],
    [entitlement({ valid_from: '2026-08-29T12:00:00.001Z' })],
    [entitlement({ entitlement_key: 'another_product' })],
    [entitlement({ account_uid: 'another-user' })],
  ];

  for (const rows of cases) {
    assert.deepEqual(await fetchAccountEntitlements(fakeDb(rows), 'firebase-uid-123', NOW), {
      tier: 'free',
      entitlements: [],
      validUntil: null,
    });
  }
});

test('fetchAccountEntitlements treats valid_from equal to now as active and exposes no provider data', async () => {
  const result = await fetchAccountEntitlements(
    fakeDb([entitlement({ valid_from: NOW })]),
    'firebase-uid-123',
    NOW,
  );

  assert.deepEqual(Object.keys(result).sort(), ['entitlements', 'tier', 'validUntil']);
  assert.equal(JSON.stringify(result).includes('must-never-leak'), false);
});

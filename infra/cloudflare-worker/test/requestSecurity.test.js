import { test } from 'node:test';
import assert from 'node:assert/strict';
import worker from '../src/index.js';
import { createCsrfToken, hashSessionId } from '../src/auth/crypto.js';
import { SESSION_COOKIE_NAME } from '../src/auth/sessionStore.js';
import { hasExactJsonContentType, readBoundedJson } from '../src/requestSecurity.js';

const DASHBOARD_ORIGIN = 'https://dashboard.example';
const CSRF_SECRET = 'csrf-secret-used-only-by-the-route-tests';

function createLiveAlertDatabase(session) {
  let liveAlertWrites = 0;
  let lastRead;
  return {
    prepare(sql) {
      return {
        bind(..._parameters) {
          return {
            first: async () => (sql.includes('FROM admin_sessions') ? session : null),
            run: async () => {
              if (sql.includes('live_alert')) liveAlertWrites += 1;
              return {};
            },
            all: async () => {
              lastRead = { sql, parameters: _parameters };
              return { results: [] };
            },
          };
        },
      };
    },
    get liveAlertWrites() {
      return liveAlertWrites;
    },
    get lastRead() {
      return lastRead;
    },
  };
}

async function createAuthenticatedAlertEnvironment() {
  const sessionId = 'session-id-used-only-by-the-route-tests';
  const session = {
    id: await hashSessionId(sessionId),
    created_at: '2026-01-01T00:00:00.000Z',
    expires_at: '2099-01-01T00:00:00.000Z',
    revoked_at: null,
  };
  const db = createLiveAlertDatabase(session);
  return {
    sessionId,
    csrfToken: await createCsrfToken(sessionId, CSRF_SECRET),
    db,
    env: {
      DASHBOARD_ORIGIN,
      ADMIN_CSRF_SECRET: CSRF_SECRET,
      TELEMETRY_DB: db,
    },
  };
}

test('readBoundedJson parses a body within the byte limit', async () => {
  const request = new Request('https://worker.example/test', {
    method: 'POST',
    body: JSON.stringify({ value: 'ok' }),
  });

  assert.deepEqual(await readBoundedJson(request, 64), { value: 'ok' });
});

test('readBoundedJson rejects streamed bodies above the byte limit', async () => {
  const request = new Request('https://worker.example/test', {
    method: 'POST',
    body: JSON.stringify({ value: 'x'.repeat(128) }),
  });

  assert.equal(await readBoundedJson(request, 64), null);
});

test('readBoundedJson rejects a declared oversized body without reading it', async () => {
  const request = new Request('https://worker.example/test', {
    method: 'POST',
    headers: { 'Content-Length': '1000' },
    body: '{}',
  });

  assert.equal(await readBoundedJson(request, 64), null);
});

test('hasExactJsonContentType only accepts application/json without parameters', () => {
  assert.equal(hasExactJsonContentType(new Request('https://worker.example', {
    headers: { 'Content-Type': 'application/json' },
  })), true);
  assert.equal(hasExactJsonContentType(new Request('https://worker.example', {
    headers: { 'Content-Type': 'application/json; charset=utf-8' },
  })), false);
});

test('admin mutations reject requests outside the configured dashboard origin', async () => {
  for (const path of ['/admin/login', '/admin/logout', '/admin/live-alert']) {
    const response = await worker.fetch(new Request(`https://worker.example${path}`, {
      method: 'POST',
      headers: { Origin: 'https://evil.example' },
      body: path.endsWith('login') ? JSON.stringify({ password: 'irrelevant' }) : undefined,
    }), { DASHBOARD_ORIGIN: 'https://dashboard.example' });

    assert.equal(response.status, 403);
  }
});

test('live-alert route requires exact JSON before it reads an authenticated cookie request', async () => {
  const { sessionId, env, db } = await createAuthenticatedAlertEnvironment();
  const response = await worker.fetch(new Request('https://worker.example/admin/live-alert', {
    method: 'POST',
    headers: {
      Origin: DASHBOARD_ORIGIN,
      Cookie: `${SESSION_COOKIE_NAME}=${sessionId}`,
      'Content-Type': 'application/x-www-form-urlencoded',
    },
    body: 'active=true',
  }), env);

  assert.equal(response.status, 415);
  assert.equal(db.liveAlertWrites, 0);
});

test('live-alert route rejects a session cookie without the custom CSRF header', async () => {
  const { sessionId, env, db } = await createAuthenticatedAlertEnvironment();
  const response = await worker.fetch(new Request('https://worker.example/admin/live-alert', {
    method: 'POST',
    headers: {
      Origin: DASHBOARD_ORIGIN,
      Cookie: `${SESSION_COOKIE_NAME}=${sessionId}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ message: 'official-looking phishing', active: true }),
  }), env);

  assert.equal(response.status, 403);
  assert.equal(db.liveAlertWrites, 0);
});

test('live-alert route accepts only a session-bound CSRF token from the exact dashboard origin', async () => {
  const { sessionId, csrfToken, env, db } = await createAuthenticatedAlertEnvironment();
  const response = await worker.fetch(new Request('https://worker.example/admin/live-alert', {
    method: 'POST',
    headers: {
      Origin: DASHBOARD_ORIGIN,
      Cookie: `${SESSION_COOKIE_NAME}=${sessionId}`,
      'Content-Type': 'application/json',
      'X-Ralven-Csrf-Token': csrfToken,
    },
    body: JSON.stringify({ message: 'Atualização oficial', active: true }),
  }), env);

  assert.equal(response.status, 200);
  assert.deepEqual(await response.json(), { success: true });
  assert.equal(db.liveAlertWrites, 1);
});

test('CSRF token route is session-protected and origin-locked', async () => {
  const { sessionId, csrfToken, env } = await createAuthenticatedAlertEnvironment();
  const valid = await worker.fetch(new Request('https://worker.example/admin/csrf', {
    headers: { Origin: DASHBOARD_ORIGIN, Cookie: `${SESSION_COOKIE_NAME}=${sessionId}` },
  }), env);
  assert.equal(valid.status, 200);
  assert.deepEqual(await valid.json(), { csrfToken });

  const invalidOrigin = await worker.fetch(new Request('https://worker.example/admin/csrf', {
    headers: { Origin: 'https://evil.example', Cookie: `${SESSION_COOKIE_NAME}=${sessionId}` },
  }), env);
  assert.equal(invalidOrigin.status, 403);
});

test('bug report CSV applies the version filter and the documented export limit', async () => {
  const { sessionId, env, db } = await createAuthenticatedAlertEnvironment();
  const response = await worker.fetch(new Request('https://worker.example/api/bugs.csv?version=1.0.4', {
    headers: { Cookie: `${SESSION_COOKIE_NAME}=${sessionId}` },
  }), env);

  assert.equal(response.status, 200);
  assert.match(db.lastRead.sql, /app_version = \?/);
  assert.deepEqual(db.lastRead.parameters, ['1.0.4', 200]);
});

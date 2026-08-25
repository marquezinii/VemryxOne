import { test } from 'node:test';
import assert from 'node:assert/strict';
import worker from '../src/index.js';
import { MAX_BATCH_SIZE, MAX_ACTION_IDS } from '../src/validateEvent.js';

function event(eventId, actionIds = ['fivem.legacy.cache.repair']) {
  return {
    eventId,
    eventName: 'optimization-completed',
    executionTimeMs: 100,
    appVersion: '1.5.1',
    actionIds,
  };
}

function request(events) {
  return new Request('https://telemetry.example.workers.dev/telemetry', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(events),
  });
}

function environment(db) {
  return {
    TELEMETRY_DB: db,
    TELEMETRY_LIMITER: { limit: async () => ({ success: true }) },
  };
}

class TransactionalD1 {
  events = new Map();
  actions = new Set();
  batches = [];
  failAtStatement = null;
  timeoutAfterCommit = false;
  nextId = 1;

  prepare(sql) {
    return { bind: (...values) => ({ sql, values }) };
  }

  async batch(statements) {
    this.batches.push(statements);
    const events = new Map(this.events);
    const actions = new Set(this.actions);
    let nextId = this.nextId;

    for (const [index, statement] of statements.entries()) {
      if (this.failAtStatement === index) throw new Error('simulated statement failure');
      if (statement.sql.startsWith('INSERT INTO telemetry_events')) {
        const [eventId] = statement.values;
        if (!events.has(eventId)) events.set(eventId, { id: nextId++, eventId });
      } else {
        const [actionId, eventId] = statement.values;
        const stored = events.get(eventId);
        if (!stored) throw new Error('action points at a missing event');
        actions.add(`${stored.id}:${actionId}`);
      }
    }

    this.events = events;
    this.actions = actions;
    this.nextId = nextId;
    if (this.timeoutAfterCommit) {
      this.timeoutAfterCommit = false;
      throw new Error('simulated timeout after commit');
    }
    return [];
  }
}

test('a failed statement rolls back every event and action in the request', async () => {
  const db = new TransactionalD1();
  db.failAtStatement = 1;

  const response = await worker.fetch(request([
    event('11111111-1111-4111-8111-111111111111', ['action.one']),
    event('22222222-2222-4222-8222-222222222222', ['action.two']),
  ]), environment(db));

  assert.equal(response.status, 500);
  assert.equal(db.events.size, 0);
  assert.equal(db.actions.size, 0);
});

test('a post-commit timeout and ten retries keep one logical copy and UUID action mapping', async () => {
  const db = new TransactionalD1();
  db.timeoutAfterCommit = true;
  const first = event('11111111-1111-4111-8111-111111111111', ['action.one', 'action.two']);
  const second = event('22222222-2222-4222-8222-222222222222', ['action.three']);
  const payload = [first, second];

  assert.equal((await worker.fetch(request(payload), environment(db))).status, 500);
  for (let retry = 0; retry < 10; retry++) {
    assert.equal((await worker.fetch(request(payload), environment(db))).status, 202);
  }

  assert.equal(db.events.size, 2);
  assert.equal(db.actions.size, 3);
  const firstId = db.events.get(first.eventId).id;
  const secondId = db.events.get(second.eventId).id;
  assert.deepEqual(db.actions, new Set([`${firstId}:action.one`, `${firstId}:action.two`, `${secondId}:action.three`]));
});

test('the maximum request stays inside one D1 transaction', async () => {
  const db = new TransactionalD1();
  const actionIds = Array.from({ length: MAX_ACTION_IDS }, (_, index) => `action.${index}`);
  const payload = Array.from({ length: MAX_BATCH_SIZE }, (_, index) => event(
    `${String(index + 1).padStart(8, '0')}-1111-4111-8111-111111111111`, actionIds,
  ));

  assert.equal((await worker.fetch(request(payload), environment(db))).status, 202);
  assert.equal(db.batches.length, 1);
  assert.equal(db.batches[0].length, MAX_BATCH_SIZE * (MAX_ACTION_IDS + 1));
  assert.ok(db.batches[0].length <= 500);
});

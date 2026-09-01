import assert from 'node:assert/strict';

export function selectReleaseObjectsToDelete(keys, currentTag, keep = 7) {
  const versions = new Map();
  for (const key of keys) {
    const match = key.match(/^releases\/(v\d+\.\d+\.\d+)\//);
    if (!match) continue;
    const list = versions.get(match[1]) ?? [];
    list.push(key);
    versions.set(match[1], list);
  }

  if (!versions.has(currentTag)) throw new Error(`Current release ${currentTag} was not found in R2.`);
  const newest = [...versions.keys()]
    .sort((left, right) => {
      const a = left.slice(1).split('.').map(Number);
      const b = right.slice(1).split('.').map(Number);
      return b[0] - a[0] || b[1] - a[1] || b[2] - a[2];
    })
    .slice(0, keep);
  if (!newest.includes(currentTag)) throw new Error(`Current release ${currentTag} is outside the retention set.`);
  const retained = new Set(newest);
  return [...versions].flatMap(([version, objects]) => (retained.has(version) ? [] : objects));
}

async function cloudflare(path, init = {}) {
  const response = await fetch(`https://api.cloudflare.com/client/v4${path}`, {
    ...init,
    headers: { Authorization: `Bearer ${process.env.CLOUDFLARE_API_TOKEN}` },
  });
  const body = await response.json();
  if (!response.ok || body.success === false) throw new Error(JSON.stringify(body.errors ?? body));
  return body;
}

async function main() {
  const account = process.env.CLOUDFLARE_ACCOUNT_ID;
  const token = process.env.CLOUDFLARE_API_TOKEN;
  const currentTag = process.env.RELEASE_TAG;
  if (!account || !token || !/^v\d+\.\d+\.\d+$/.test(currentTag ?? '')) {
    throw new Error('CLOUDFLARE_ACCOUNT_ID, CLOUDFLARE_API_TOKEN and numeric RELEASE_TAG are required.');
  }

  const bucket = 'ralven-releases';
  const keys = [];
  let cursor;
  do {
    const query = new URLSearchParams({ prefix: 'releases/' });
    if (cursor) query.set('cursor', cursor);
    const page = await cloudflare(`/accounts/${account}/r2/buckets/${bucket}/objects?${query}`);
    keys.push(...page.result.map((object) => object.key));
    cursor = page.result_info?.cursor;
  } while (cursor);

  const stale = selectReleaseObjectsToDelete(keys, currentTag);
  for (const key of stale) {
    const encodedKey = key.split('/').map(encodeURIComponent).join('/');
    await cloudflare(`/accounts/${account}/r2/buckets/${bucket}/objects/${encodedKey}`, { method: 'DELETE' });
    console.log(`Deleted stale R2 object: ${key}`);
  }
  console.log(`R2 retention complete: kept the 7 newest versions; deleted ${stale.length} objects.`);
}

if (process.argv.includes('--self-test')) {
  const keys = Array.from({ length: 9 }, (_, index) => `releases/v1.0.${index}/file.zip`);
  assert.deepEqual(selectReleaseObjectsToDelete(keys, 'v1.0.8'), [
    'releases/v1.0.0/file.zip',
    'releases/v1.0.1/file.zip',
  ]);
  assert.throws(() => selectReleaseObjectsToDelete(keys, 'v2.0.0'));
  console.log('R2 retention self-test passed.');
} else {
  await main();
}

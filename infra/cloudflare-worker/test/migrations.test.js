import assert from 'node:assert/strict';
import { spawnSync } from 'node:child_process';
import { mkdir, mkdtemp, readFile, readdir, rm, writeFile } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { dirname, join, resolve } from 'node:path';
import test from 'node:test';
import { fileURLToPath } from 'node:url';

const workerRoot = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const migrationSource = join(workerRoot, 'migrations');
const wrangler = join(workerRoot, 'node_modules', 'wrangler', 'bin', 'wrangler.js');
const migrationNames = (await readdir(migrationSource))
  .filter((name) => /^\d{4}_.+\.sql$/.test(name))
  .sort();
const historicalBootstrapMigrations = migrationNames.filter((name) => /^000[0-5]_/.test(name));

function run(args, { expectSuccess = true } = {}) {
  const result = spawnSync(process.execPath, [wrangler, ...args], { cwd: workerRoot, encoding: 'utf8' });
  if (expectSuccess !== (result.status === 0)) {
    throw new Error(`wrangler ${args.join(' ')}\n${result.stdout}\n${result.stderr}`);
  }
  return result.stdout;
}

async function createFixture(root, name, migrations) {
  const directory = join(root, name);
  const migrationsDirectory = join(directory, 'migrations');
  await mkdir(migrationsDirectory, { recursive: true });
  await Promise.all(migrations.map(async (migration) => {
    const source = migration === '0006_atomic_failure.sql'
      ? 'CREATE TABLE migration_atomicity_probe (id INTEGER);\nTHIS IS NOT SQL;\n'
      : await readFile(join(migrationSource, migration), 'utf8');
    await writeFile(join(migrationsDirectory, migration), source);
  }));

  const config = join(directory, 'wrangler.toml');
  await writeFile(config, [
    'name = "d1-migration-test"',
    'main = "src/index.js"',
    'compatibility_date = "2026-08-11"',
    '',
    '[[d1_databases]]',
    'binding = "TELEMETRY_DB"',
    'database_name = "d1-migration-test"',
    'database_id = "00000000-0000-0000-0000-000000000000"',
    `migrations_dir = ${JSON.stringify(migrationsDirectory.replaceAll('\\', '/'))}`,
  ].join('\n'));

  return config;
}

function apply(config, stateDirectory, expectSuccess = true) {
  return run([
    'd1', 'migrations', 'apply', 'TELEMETRY_DB', '--local',
    '--persist-to', stateDirectory, '--config', config,
  ], { expectSuccess });
}

function execute(config, stateDirectory, command) {
  return JSON.parse(run([
    'd1', 'execute', 'TELEMETRY_DB', '--local',
    '--persist-to', stateDirectory, '--config', config,
    '--command', command, '--json',
  ]));
}

function adoptHistoricalBootstrap(config, stateDirectory) {
  const state = execute(config, stateDirectory, `
    SELECT
      EXISTS(SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = 'telemetry_events') AS has_schema,
      EXISTS(SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = 'd1_migrations') AS has_ledger,
      EXISTS(SELECT 1 FROM pragma_table_info('telemetry_events') WHERE name = 'process_count_at_start') AS has_v5_telemetry,
      EXISTS(SELECT 1 FROM pragma_table_info('account_profiles') WHERE name = 'terms_version') AS has_profile_terms;
  `)[0].results[0];

  if (!state.has_schema || state.has_ledger) {
    return;
  }

  assert.equal(state.has_v5_telemetry, 1, 'legacy bootstrap must have the v5 telemetry schema before adoption');
  assert.equal(state.has_profile_terms, 1, 'legacy bootstrap must have the profile terms schema before adoption');
  execute(config, stateDirectory, `
    CREATE TABLE IF NOT EXISTS d1_migrations (
      id INTEGER PRIMARY KEY AUTOINCREMENT,
      name TEXT UNIQUE,
      applied_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL
    );
    INSERT OR IGNORE INTO d1_migrations (name) VALUES
      ${historicalBootstrapMigrations.map((name) => `('${name}')`).join(',\n      ')};
  `);
}

const workerSchemaSmoke = `
  INSERT INTO telemetry_events
    (event_name, execution_time_ms, app_version, environment, received_at,
     five_m_install_detected, gta_edition, optimization_target_count,
     windows_build, disk_type, free_space_gib_bucket, run_timestamp,
     days_since_last_run_bucket, backup_created, backup_restored,
     elevation_used, process_count_at_start)
  VALUES
    ('OptimizationCompleted', 1, 'test', 'Production', '2026-01-01T00:00:00.000Z',
     1, 'Legacy', 1, 26100, 'SSD', 1, '2026-01-01T00:00:00.000Z', 1, 1, 0, 0, 1);
  INSERT INTO account_profiles
    (uid, username, username_normalized, first_name, last_name, terms_version, terms_accepted_at, created_at)
  VALUES ('test-user', 'TestUser', 'testuser', 'Test', 'User', 'v1', '2026-01-01T00:00:00.000Z', '2026-01-01T00:00:00.000Z');
  SELECT message, active, updated_at FROM live_alert WHERE id = 1;
  SELECT username, first_name, last_name, terms_version FROM account_profiles WHERE uid = 'test-user';
`;

async function verifyUpgradeFrom(priorCount, t) {
  const root = await mkdtemp(join(tmpdir(), 'vemryx-d1-migrations-'));
  t.after(() => rm(root, { recursive: true, force: true }));

  const currentConfig = await createFixture(root, 'current', migrationNames);
  const stateDirectory = join(root, 'state');
  if (priorCount > 0) {
    const oldConfig = await createFixture(root, 'prior', migrationNames.slice(0, priorCount));
    apply(oldConfig, stateDirectory);
  }

  apply(currentConfig, stateDirectory);
  execute(currentConfig, stateDirectory, workerSchemaSmoke);
}

for (let priorCount = 0; priorCount < migrationNames.length; priorCount += 1) {
  const source = priorCount === 0 ? 'an empty database' : `the ${migrationNames[priorCount - 1]} schema`;
  test(`D1 migrations upgrade ${source} to the current Worker contract`, (t) => verifyUpgradeFrom(priorCount, t));
}

test('D1 migrations adopt the historical schema.sql bootstrap before applying newer migrations', async (t) => {
  const root = await mkdtemp(join(tmpdir(), 'vemryx-d1-legacy-bootstrap-'));
  t.after(() => rm(root, { recursive: true, force: true }));

  const stateDirectory = join(root, 'state');
  const legacyConfig = await createFixture(root, 'legacy', historicalBootstrapMigrations);
  apply(legacyConfig, stateDirectory);
  execute(legacyConfig, stateDirectory, 'DROP TABLE d1_migrations;');

  adoptHistoricalBootstrap(legacyConfig, stateDirectory);
  const currentConfig = await createFixture(root, 'current', migrationNames);
  apply(currentConfig, stateDirectory);
  execute(currentConfig, stateDirectory, workerSchemaSmoke);
});

test('a failed D1 migration is atomic and is not recorded as applied', async (t) => {
  const root = await mkdtemp(join(tmpdir(), 'vemryx-d1-atomicity-'));
  t.after(() => rm(root, { recursive: true, force: true }));

  const stateDirectory = join(root, 'state');
  const currentConfig = await createFixture(root, 'current', migrationNames);
  apply(currentConfig, stateDirectory);
  const config = await createFixture(root, 'atomicity', [...migrationNames, '0006_atomic_failure.sql']);
  apply(config, stateDirectory, false);

  const result = execute(config, stateDirectory, `
    SELECT name FROM sqlite_master WHERE type = 'table' AND name = 'migration_atomicity_probe';
    SELECT name FROM d1_migrations WHERE name = '0006_atomic_failure.sql';
  `);
  assert.deepEqual(result.flatMap((statement) => statement.results), []);
});

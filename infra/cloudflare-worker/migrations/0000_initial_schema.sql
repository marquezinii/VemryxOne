-- Historical D1 baseline before the incremental migrations below. Keep this
-- migration so an empty D1 database reaches the same schema through Wrangler
-- as an already deployed database that has recorded later migrations.

CREATE TABLE IF NOT EXISTS telemetry_events (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    event_name TEXT NOT NULL,
    execution_time_ms INTEGER NOT NULL,
    app_version TEXT NOT NULL,
    error_category TEXT,
    os_version TEXT,
    system_architecture TEXT,
    cpu_model TEXT,
    gpu_model TEXT,
    ram_bucket_gib INTEGER,
    profile TEXT,
    environment TEXT NOT NULL,
    received_at TEXT NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_telemetry_events_received_at
    ON telemetry_events (received_at);
CREATE INDEX IF NOT EXISTS idx_telemetry_events_environment
    ON telemetry_events (environment);
CREATE INDEX IF NOT EXISTS idx_telemetry_events_app_version
    ON telemetry_events (app_version);

CREATE TABLE IF NOT EXISTS telemetry_event_actions (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    telemetry_event_id INTEGER NOT NULL REFERENCES telemetry_events (id),
    action_id TEXT NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_telemetry_event_actions_action_id
    ON telemetry_event_actions (action_id);
CREATE INDEX IF NOT EXISTS idx_telemetry_event_actions_event_id
    ON telemetry_event_actions (telemetry_event_id);

CREATE TABLE IF NOT EXISTS login_attempts (
    ip_hash TEXT PRIMARY KEY,
    failed_count INTEGER NOT NULL DEFAULT 0,
    first_failed_at TEXT NOT NULL,
    locked_until TEXT
);

CREATE TABLE IF NOT EXISTS admin_sessions (
    id TEXT PRIMARY KEY,
    created_at TEXT NOT NULL,
    expires_at TEXT NOT NULL,
    revoked_at TEXT
);

CREATE INDEX IF NOT EXISTS idx_admin_sessions_expires_at
    ON admin_sessions (expires_at);

CREATE TABLE IF NOT EXISTS user_accounts (
    id TEXT PRIMARY KEY,
    first_name TEXT NOT NULL,
    last_name TEXT NOT NULL,
    email_normalized TEXT NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    created_at TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS user_sessions (
    id TEXT PRIMARY KEY,
    user_id TEXT NOT NULL REFERENCES user_accounts (id),
    created_at TEXT NOT NULL,
    expires_at TEXT NOT NULL,
    revoked_at TEXT
);

CREATE INDEX IF NOT EXISTS idx_user_sessions_expires_at ON user_sessions (expires_at);

CREATE TABLE IF NOT EXISTS user_login_attempts (
    ip_hash TEXT PRIMARY KEY,
    failed_count INTEGER NOT NULL DEFAULT 0,
    first_failed_at TEXT NOT NULL,
    locked_until TEXT
);

CREATE TABLE IF NOT EXISTS bug_reports (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    report_id TEXT NOT NULL UNIQUE,
    category TEXT NOT NULL,
    summary TEXT NOT NULL,
    description TEXT NOT NULL,
    app_version TEXT NOT NULL,
    profile TEXT NOT NULL,
    technical_summary TEXT,
    email TEXT,
    log_text TEXT,
    environment TEXT NOT NULL,
    received_at TEXT NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_bug_reports_received_at ON bug_reports (received_at);
CREATE INDEX IF NOT EXISTS idx_bug_reports_environment ON bug_reports (environment);

CREATE TABLE IF NOT EXISTS updater_events (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    event_id TEXT NOT NULL UNIQUE,
    stage TEXT NOT NULL,
    outcome TEXT NOT NULL,
    error_code TEXT NOT NULL,
    previous_version TEXT,
    candidate_version TEXT NOT NULL,
    environment TEXT NOT NULL,
    received_at TEXT NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_updater_events_received_at ON updater_events (received_at);
CREATE INDEX IF NOT EXISTS idx_updater_events_candidate_version ON updater_events (candidate_version);

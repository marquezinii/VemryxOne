ALTER TABLE bug_reports ADD COLUMN bug_code TEXT;
ALTER TABLE telemetry_events ADD COLUMN bug_code TEXT;

-- Historical rows predate the typed contract. Keep them distinguishable from
-- new allowlisted reports without inventing a cause.
UPDATE bug_reports SET bug_code = 'LEGACY_UNCLASSIFIED' WHERE bug_code IS NULL;

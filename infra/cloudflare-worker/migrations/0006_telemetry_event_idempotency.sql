-- Existing telemetry is anonymous and remains readable without an event ID.
-- New client events carry a UUID that makes a retried delivery idempotent.
ALTER TABLE telemetry_events ADD COLUMN event_id TEXT;
CREATE UNIQUE INDEX idx_telemetry_events_event_id ON telemetry_events (event_id);
CREATE UNIQUE INDEX idx_telemetry_event_actions_event_action
    ON telemetry_event_actions (telemetry_event_id, action_id);

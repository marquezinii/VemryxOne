// Pure SQL+params builder for listing bug reports in the dashboard's "Bugs
// reportados" tab. Kept separate from stats/queries.js since bug reports are
// a different domain (individual records, not aggregates).

import { appendEnvironmentClause, appendDateRangeClauses } from '../filters.js';

const DEFAULT_LIMIT = 50;
export const MAX_BUG_REPORT_LIMIT = 200;

/**
 * Lists recent bug reports, optionally filtered by environment/category/
 * date range, newest first.
 */
export function recentBugReports({ environment, version, category, from, to } = {}, limit = DEFAULT_LIMIT) {
  const clauses = [];
  const params = [];

  appendEnvironmentClause(clauses, params, environment);

  if (version) {
    clauses.push('app_version = ?');
    params.push(version);
  }

  if (category) {
    clauses.push('category = ?');
    params.push(category);
  }

  appendDateRangeClauses(clauses, params, { from, to });

  const whereSql = clauses.length > 0 ? `WHERE ${clauses.join(' AND ')}` : '';
  const boundedLimit = Math.min(Math.max(1, Math.trunc(limit) || DEFAULT_LIMIT), MAX_BUG_REPORT_LIMIT);

  return {
    sql: `SELECT id, report_id, category, bug_code, summary, description, app_version, profile,
                 technical_summary, email, log_text, environment, received_at
          FROM bug_reports
          ${whereSql}
          ORDER BY received_at DESC
          LIMIT ?`,
    params: [...params, boundedLimit],
  };
}

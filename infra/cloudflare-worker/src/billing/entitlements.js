const PRO_ENTITLEMENT = 'ralven_pro';

/**
 * Reads the server-authoritative access snapshot for the verified Firebase UID.
 * Provider and subscription identifiers are deliberately neither selected nor
 * returned to the client.
 *
 * @param {D1Database} db
 * @param {string} uid
 * @param {string} [nowIso]
 * @returns {Promise<{ tier: 'free' | 'pro', entitlements: string[], validUntil: string | null }>}
 */
export async function fetchAccountEntitlements(db, uid, nowIso = new Date().toISOString()) {
  const row = await db
    .prepare(
      `SELECT entitlement_key, valid_until
       FROM account_entitlements
       WHERE account_uid = ?
         AND entitlement_key = ?
         AND state IN ('active', 'grace_period')
         AND valid_from <= ?
         AND valid_until > ?
       LIMIT 1`,
    )
    .bind(uid, PRO_ENTITLEMENT, nowIso, nowIso)
    .first();

  return row === null
    ? { tier: 'free', entitlements: [], validUntil: null }
    : { tier: 'pro', entitlements: [PRO_ENTITLEMENT], validUntil: row.valid_until };
}

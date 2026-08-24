import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

test("keeps the dashboard on the Vemryx One public brand", async () => {
  const html = await readFile(new URL("../index.html", import.meta.url), "utf8");

  assert.match(html, /Vemryx One/);
  assert.doesNotMatch(html, /FiveMCleaner/);
});

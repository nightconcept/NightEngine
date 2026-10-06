// Browser checks for any nightengine game, with Playwright. A game's specs import these helpers.
// The page is a build from `python -m nightengine.web build --offline`, served over HTTP (see config.mjs).
import { expect } from '@playwright/test';

// Open a page of the build and start the runtime. Pyxel waits for a tap ("touch to start"), and a tap before the
// runtime has loaded does nothing, so tap until the launcher reports "Running". Returns the list of page errors.
export async function open(page, query = '') {
  const errors = [];
  page.on('pageerror', error => errors.push(error.message));
  await page.goto('/' + query);
  const status = page.locator('#status');
  await expect(async () => {
    expect(await status.textContent()).not.toContain('Could not start');
    if (!(await status.textContent()).includes('Running')) {
      await page.locator('#canvas').tap({timeout: 2_000});
      await expect(status).toContainText('Running', {timeout: 2_000});
    }
  }).toPass({timeout: 120_000});
  return errors;
}

// Open the game and wait until it has run a few frames.
export async function launch(page, query = '') {
  const errors = await open(page, query);
  await expect.poll(() => python(page, 'app.game.frame'), {timeout: 30_000}).toBeGreaterThan(2);
  await expect(page.locator('#pyxel-error-overlay')).toHaveCount(0);
  return errors;
}

// Run Python in the page and return the value of its last line. `app` is the running nightengine App.
export async function python(page, code) {
  const prelude = 'from nightengine.host import app as _host\napp = _host.current\n';
  return page.evaluate(source => {
    const value = window.pyxelContext.pyodide.runPython(source);
    return value?.toJs ? value.toJs() : value;  // Lists and dicts come back as plain JS values.
  }, prelude + code);
}

// Page coordinates of a logical pixel of the game screen.
export async function point(page, x, y) {
  const box = await page.locator('#canvas').boundingBox();
  const {width, height} = await page.evaluate(() => document.body.dataset);
  return {x: box.x + (x + 0.5) * box.width / Number(width), y: box.y + (y + 0.5) * box.height / Number(height)};
}

export async function tap(page, x, y) {
  const at = await point(page, x, y);
  await page.touchscreen.tap(at.x, at.y);
}

// Raw multitouch through the DevTools protocol: `points` are [{x, y, id}] in page coordinates.
export async function touch(cdp, type, points) {
  await cdp.send('Input.dispatchTouchEvent', {type, touchPoints: points.map(p => ({...p, radiusX: 3, radiusY: 3}))});
}

// Checks that hold for every nightengine game in a browser.
import { test, expect } from '@playwright/test';
import { launch, open, python, tap } from '../harness.mjs';

test('the game runs in the browser without errors', async ({page}) => {
  const errors = await launch(page);
  const frame = await python(page, 'app.game.frame');
  await expect.poll(() => python(page, 'app.game.frame')).toBeGreaterThan(frame + 10);
  expect(errors).toEqual([]);
});

test('a tap reaches the game as a recorded pointer', async ({page}) => {
  await launch(page);
  const {width, height} = await page.evaluate(() => document.body.dataset);
  await tap(page, Math.floor(Number(width) / 2), Math.floor(Number(height) - 4));
  await expect.poll(() => python(page, 'sorted({p.phase.value for ps in app.recording.pointers.values() for p in ps})'))
    .toEqual(expect.arrayContaining(['PRESSED', 'RELEASED']));
});

test('the pointer check page runs', async ({page}) => {
  const errors = await open(page, '?app=debug');
  await page.waitForTimeout(500);
  expect(errors).toEqual([]);
});

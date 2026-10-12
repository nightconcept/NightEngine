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

test('a screen smaller than the game shows all of it', async ({page}) => {
  await page.setViewportSize({width: 600, height: 340});  // A phone in landscape, in CSS pixels.
  await launch(page, '?shell=app');
  const fit = await page.evaluate(() => {
    const canvas = document.getElementById('canvas');
    const box = Element.prototype.getBoundingClientRect.call(canvas);  // As shown (the launcher patches the method).
    return {
      // Pyxel never draws below 1x, so the canvas needs at least the game's pixels or the game is cropped.
      enoughPixels: canvas.width >= Number(document.body.dataset.width) && canvas.height >= Number(document.body.dataset.height),
      inside: box.left >= 0 && box.top >= 0 && box.right <= innerWidth + 0.5 && box.bottom <= innerHeight + 0.5,
    };
  });
  expect(fit).toEqual({enoughPixels: true, inside: true});
});

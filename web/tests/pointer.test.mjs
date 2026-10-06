import test from 'node:test';
import assert from 'node:assert/strict';
import { logicalPoint, PointerTracker, installBridge } from '../static/pointer.mjs';

const box = {left: 100, top: 50, width: 800, height: 450, right: 900, bottom: 500};
test('coordinates use displayed canvas, including edges and captured drags', () => {
  assert.deepEqual(logicalPoint(100, 50, box, 256, 144), {x: 0, y: 0});
  assert.deepEqual(logicalPoint(500, 275, box, 256, 144), {x: 128, y: 72});
  assert.deepEqual(logicalPoint(900, 500, box, 256, 144), {x: 255, y: 143});
  assert.deepEqual(logicalPoint(-1, 900, box, 256, 144), {x: 0, y: 143});
  assert.throws(() => logicalPoint(1, 2, {...box, width: 0}, 256, 144), RangeError);
});

test('two pointers retain IDs and initial coordinates through phases', () => {
  const tracker = new PointerTracker();
  tracker.event(7, 'PRESSED', 20, 30);
  tracker.event(9, 'PRESSED', 80, 90);
  assert.deepEqual(tracker.sample().map(t => [t.id, t.phase]), [[7, 'PRESSED'], [9, 'PRESSED']]);
  assert.deepEqual(tracker.sample().map(t => t.phase), ['HELD', 'HELD']);
  tracker.event(7, 'MOVED', 40, 50);
  tracker.event(7, 'MOVED', 60, 70);
  const moved = tracker.sample()[0];
  assert.deepEqual([moved.x, moved.y, moved.start_x, moved.start_y, moved.phase], [60, 70, 20, 30, 'MOVED']);
  tracker.event(7, 'RELEASED', 60, 70);
  assert.equal(tracker.sample()[0].phase, 'RELEASED');
  assert.deepEqual(tracker.sample().map(t => t.id), [9]);
  tracker.cancel();
  assert.equal(tracker.sample()[0].phase, 'CANCELLED');
  assert.deepEqual(tracker.sample(), []);
});

test('tap between frames exposes start then release, cancellation never becomes release', () => {
  const tracker = new PointerTracker();
  tracker.event(1, 'PRESSED', 2, 3);
  tracker.event(1, 'RELEASED', 4, 5);
  assert.equal(tracker.sample()[0].phase, 'PRESSED');
  assert.equal(tracker.sample()[0].phase, 'RELEASED');
  assert.deepEqual(tracker.sample(), []);
  tracker.event(1, 'PRESSED', 2, 3);
  tracker.event(1, 'MOVED', 4, 5);
  tracker.cancel();
  assert.equal(tracker.sample()[0].phase, 'PRESSED');
  assert.equal(tracker.sample()[0].phase, 'CANCELLED');
});

class Target extends EventTarget {
  emit(type, props = {}) {
    const event = new Event(type, {cancelable: true});
    Object.assign(event, props);
    this.dispatchEvent(event);
  }
}
function environment() {
  const canvas = new Target();
  canvas.getBoundingClientRect = () => box;
  const captures = new Set();
  canvas.setPointerCapture = id => captures.add(id);
  canvas.hasPointerCapture = id => captures.has(id);
  canvas.releasePointerCapture = id => captures.delete(id);
  const env = new Target();
  env.document = new Target();
  env.document.createElement = () => ({style: {}, remove() {}});
  env.document.body = {appendChild() {}};
  env.navigator = {maxTouchPoints: 5, userAgent: 'Android'};
  env.matchMedia = () => ({matches: true});
  env.getComputedStyle = () => ({paddingTop: '10px', paddingRight: '0', paddingBottom: '0', paddingLeft: '0'});
  env.innerWidth = 1000;
  env.innerHeight = 600;
  env.localStorage = {setItem() {throw new Error('quota');}, getItem() {throw new Error('disabled');}};
  return {env, canvas, captures};
}

test('bridge captures pointers, ignores margins and cancels on lifecycle changes', () => {
  for (const lifecycle of ['blur', 'resize', 'visibilitychange']) {
    const {env, canvas, captures} = environment();
    const bridge = installBridge(canvas, 256, 144, env);
    canvas.emit('pointerdown', {pointerId: 1, pointerType: 'touch', clientX: 10, clientY: 20});
    assert.deepEqual(JSON.parse(bridge.sample()), []);
    canvas.emit('pointerdown', {pointerId: 2, pointerType: 'touch', clientX: 500, clientY: 275});
    assert(captures.has(2));
    assert.equal(JSON.parse(bridge.sample())[0].x, 128);
    if (lifecycle === 'visibilitychange') {
      env.document.hidden = true;
      env.document.emit(lifecycle);
    } else env.emit(lifecycle);
    assert.equal(JSON.parse(bridge.sample())[0].phase, 'CANCELLED');
    assert.equal(captures.size, 0);
    assert.deepEqual(JSON.parse(bridge.sample()), []);
    assert.equal(JSON.parse(bridge.info()).safe_area.top, 3.2);
    assert.equal(bridge.save('k', 'data'), false);
    assert.throws(() => bridge.load('k'), /Allow site storage/);
    bridge.destroy();
  }
});

test('capture loss cancels but capture loss following release cannot overwrite release', () => {
  const {env, canvas} = environment();
  const bridge = installBridge(canvas, 256, 144, env);
  const props = {pointerId: 2, pointerType: 'touch', clientX: 500, clientY: 275};
  canvas.emit('pointerdown', props);
  bridge.sample();
  canvas.emit('lostpointercapture', {pointerId: 2});
  assert.equal(JSON.parse(bridge.sample())[0].phase, 'CANCELLED');
  canvas.emit('pointerdown', props);
  bridge.sample();
  canvas.emit('pointerup', props);
  canvas.emit('lostpointercapture', {pointerId: 2});
  assert.equal(JSON.parse(bridge.sample())[0].phase, 'RELEASED');
  bridge.destroy();
});

test('rapid taps with a reused ID preserve every gesture before frame sampling', () => {
  const tracker = new PointerTracker();
  for (let i = 0; i < 3; i++) {
    tracker.event(1, 'PRESSED', i * 10, 20);
    tracker.event(1, 'RELEASED', i * 10 + 1, 21);
  }
  for (let i = 0; i < 3; i++) {
    assert.deepEqual(tracker.sample().map(t => [t.phase, t.start_x]), [['PRESSED', i * 10]]);
    assert.deepEqual(tracker.sample().map(t => [t.phase, t.start_x]), [['RELEASED', i * 10]]);
  }
  assert.deepEqual(tracker.sample(), []);
  tracker.event(1, 'PRESSED', 10, 20);
  tracker.cancel();
  tracker.event(1, 'PRESSED', 30, 40);
  assert.equal(tracker.sample()[0].phase, 'PRESSED');
  assert.equal(tracker.sample()[0].phase, 'CANCELLED');
  assert.equal(tracker.sample()[0].start_x, 30);
  assert.equal(tracker.sample()[0].phase, 'HELD');
});

test('startup interaction gate excludes its contact from game input', () => {
  const {env, canvas} = environment();
  let enabled = false;
  const bridge = installBridge(canvas, 256, 144, env, () => enabled);
  const props = {pointerId: 1, pointerType: 'touch', clientX: 500, clientY: 275};
  canvas.emit('pointerdown', props);
  enabled = true;
  canvas.emit('pointerup', props);
  assert.deepEqual(JSON.parse(bridge.sample()), []);
  canvas.emit('pointerdown', props);
  assert.equal(JSON.parse(bridge.sample())[0].phase, 'PRESSED');
  bridge.destroy();
});

// The nightengine browser bridge: touch, pen, and mouse as one pointer stream, sampled once per game frame.
// host/platform.py reads it as window.nightBridge. Coordinates are logical game pixels.
export const TERMINAL = new Set(['RELEASED', 'CANCELLED']);

export function logicalPoint(x, y, rect, width, height) {
  if (!(rect.width > 0 && rect.height > 0 && width > 0 && height > 0)) {
    throw new RangeError('Pointer mapping requires positive dimensions');
  }
  return {
    x: Math.max(0, Math.min(width - 1, (x - rect.left) * width / rect.width)),
    y: Math.max(0, Math.min(height - 1, (y - rect.top) * height / rect.height)),
  };
}

export class PointerTracker {
  constructor() { this.points = new Map(); }

  event(id, phase, x, y) {
    let point = this.points.get(id);
    if (phase === 'PRESSED') {
      if (point && !point.ended) return;
      point = { id, x, y, start_x: x, start_y: y, queue: point?.queue ?? [], ended: false };
      this.points.set(id, point);
    }
    if (!point || point.ended) return;
    point.x = x;
    point.y = y;
    const event = { id, x, y, start_x: point.start_x, start_y: point.start_y, phase };
    if (phase === 'MOVED' && point.queue.at(-1)?.phase === 'MOVED') point.queue.pop();
    point.queue.push(event);
    point.ended = TERMINAL.has(phase);
  }

  cancel() {
    for (const point of this.points.values()) {
      if (!point.ended) {
        // Keep a not-yet-observed start, but discard stale motion on cancellation.
        point.queue = point.queue.filter(event => event.phase !== 'MOVED');
        this.event(point.id, 'CANCELLED', point.x, point.y);
      }
    }
  }

  sample() {
    const result = [];
    for (const point of this.points.values()) {
      const event = point.queue.shift() ?? {
        id: point.id, x: point.x, y: point.y,
        start_x: point.start_x, start_y: point.start_y, phase: 'HELD',
      };
      result.push(event);
      if (TERMINAL.has(event.phase) && !point.queue.length) this.points.delete(point.id);
    }
    return result;
  }
}

export function installBridge(canvas, width, height, env = window, enabled = () => true) {
  const tracker = new PointerTracker();
  const document = env.document;
  const listeners = [];
  const listen = (target, type, handler) => {
    target.addEventListener(type, handler, { passive: false });
    listeners.push(() => target.removeEventListener(type, handler));
  };
  const rect = () => canvas.getBoundingClientRect();
  const release = id => {
    if (canvas.hasPointerCapture(id)) canvas.releasePointerCapture(id);
  };
  const cancel = () => {
    tracker.cancel();
    for (const id of tracker.points.keys()) release(id);
  };
  for (const [type, phase] of Object.entries({
    pointerdown: 'PRESSED', pointermove: 'MOVED', pointerup: 'RELEASED',
    pointercancel: 'CANCELLED', lostpointercapture: 'CANCELLED',
  })) {
    listen(canvas, type, event => {
      if (phase === 'PRESSED') {
        if (!enabled()) return;
        if (event.pointerType === 'mouse' && event.button !== 0) return;
        const box = rect();
        if (event.clientX < box.left || event.clientY < box.top ||
            event.clientX >= box.right || event.clientY >= box.bottom) return;
        canvas.setPointerCapture(event.pointerId);
      } else if (!tracker.points.has(event.pointerId)) return;
      event.preventDefault();
      const point = type === 'lostpointercapture'
        ? tracker.points.get(event.pointerId)
        : logicalPoint(event.clientX, event.clientY, rect(), width, height);
      tracker.event(event.pointerId, phase, point.x, point.y);
      if (TERMINAL.has(phase)) release(event.pointerId);
    });
  }
  listen(canvas, 'contextmenu', event => event.preventDefault());
  listen(env, 'blur', cancel);
  listen(env, 'resize', cancel);
  listen(document, 'visibilitychange', () => { if (document.hidden) cancel(); });
  const probe = document.createElement('div');
  probe.style.cssText = 'position:fixed;visibility:hidden;pointer-events:none;padding:env(safe-area-inset-top) env(safe-area-inset-right) env(safe-area-inset-bottom) env(safe-area-inset-left)';
  document.body.appendChild(probe);
  const bridge = {
    sample: () => JSON.stringify(tracker.sample()),
    info: () => {
      const box = rect();
      const style = env.getComputedStyle(probe);
      const touch = env.navigator.maxTouchPoints > 0 || env.matchMedia('(any-pointer: coarse)').matches;
      return JSON.stringify({
        touch, platform: /Android/i.test(env.navigator.userAgent) ? 'android' : 'desktop',
        viewport_width: env.innerWidth, viewport_height: env.innerHeight,
        orientation: env.innerWidth >= env.innerHeight ? 'LANDSCAPE' : 'PORTRAIT',
        safe_area: {
          top: (parseFloat(style.paddingTop) || 0) * height / box.height,
          right: (parseFloat(style.paddingRight) || 0) * width / box.width,
          bottom: (parseFloat(style.paddingBottom) || 0) * height / box.height,
          left: (parseFloat(style.paddingLeft) || 0) * width / box.width,
        },
      });
    },
    save: (key, data) => {
      try { env.localStorage.setItem(`nightengine:${key}`, data); return true; }
      catch {
        const status = document.getElementById?.('status');
        if (status) status.textContent = 'Not saved: browser storage is unavailable. Allow site storage to save.';
        return false;
      }
    },
    load: key => {
      try { return env.localStorage.getItem(`nightengine:${key}`); }
      catch { throw new Error('Browser storage is unavailable. Allow site storage to read saved data.'); }
    },
    destroy: () => { cancel(); listeners.forEach(remove => remove()); probe.remove(); },
  };
  env.nightBridge = bridge;
  return bridge;
}

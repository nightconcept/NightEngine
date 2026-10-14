// Typed text for the game: a hidden text field that opens the on-screen keyboard on a phone, and a queue that the
// game drains once per frame (window.nightBridge.takeText, read by host/platform.py take_text). Enter is "\n",
// Backspace "\b" and Escape "\x1b". The field keeps one invisible character, so Backspace always has something to
// delete and every keyboard reports it.
export const SENTINEL = '​';
export const KEYS = {Enter: '\n', Escape: '\x1b'};

export class TextQueue {
  constructor() { this.parts = []; }
  push(text) { if (text) this.parts.push(text); }
  take() { const text = this.parts.join(''); this.parts = []; return text; }
}

// What one event from the field adds to the queue. `event` is a beforeinput, compositionend, input or keydown
// event; `value` is the field's value after an input event. Returns {text, prevent, reset}.
export function handle(type, event, value = SENTINEL) {
  if (type === 'keydown') {
    const ch = KEYS[event.key];
    return ch ? {text: ch, prevent: true, reset: false} : {text: '', prevent: false, reset: false};
  }
  if (type === 'beforeinput') {
    if (event.isComposing) return {text: '', prevent: false, reset: false};
    if (event.inputType === 'insertText' || event.inputType === 'insertReplacementText') {
      return {text: (event.data ?? '').replace(/[\r\n]/g, ''), prevent: true, reset: false};
    }
    if (event.inputType === 'deleteContentBackward' || event.inputType === 'deleteWordBackward') {
      return {text: '\b', prevent: true, reset: false};
    }
    if (event.inputType === 'insertLineBreak' || event.inputType === 'insertParagraph') {
      return {text: '\n', prevent: true, reset: false};
    }
    return {text: '', prevent: false, reset: false};
  }
  if (type === 'compositionend') return {text: event.data ?? '', prevent: false, reset: true};
  if (type === 'input') {
    if (event.isComposing) return {text: '', prevent: false, reset: false};
    if (value === SENTINEL) return {text: '', prevent: false, reset: false};
    if (!value.includes(SENTINEL)) {  // The sentinel was deleted: a Backspace that beforeinput did not report.
      return {text: '\b' + value, prevent: false, reset: true};
    }
    return {text: value.split(SENTINEL).join(''), prevent: false, reset: true};
  }
  return {text: '', prevent: false, reset: false};
}

// Install the field on `document` and return {open(on), take()}. Keyboard events in the field stop there, so the
// game's own key handlers do not see the typing.
export function installText(document, canvas) {
  const queue = new TextQueue();
  const field = document.createElement('input');
  Object.assign(field, {type: 'text', id: 'night-text', autocomplete: 'off', spellcheck: false, value: SENTINEL});
  field.setAttribute('autocapitalize', 'sentences');
  field.setAttribute('enterkeyhint', 'send');
  field.setAttribute('aria-label', 'Type to the game');
  // Inside the viewport (a field off screen may not open the keyboard), but invisible and not clickable.
  field.style.cssText = 'position:fixed;left:0;bottom:0;width:2px;height:2px;opacity:0;border:0;padding:0;' +
    'font-size:16px;pointer-events:none;z-index:-1';
  document.body.appendChild(field);
  let wanted = false;
  const apply = (type, event) => {
    const result = handle(type, event, field.value);
    queue.push(result.text);
    if (result.prevent) event.preventDefault();
    if (result.reset) field.value = SENTINEL;
  };
  for (const type of ['beforeinput', 'input', 'compositionend']) {
    field.addEventListener(type, event => apply(type, event));
  }
  field.addEventListener('keydown', event => { event.stopPropagation(); apply('keydown', event); });
  field.addEventListener('keyup', event => event.stopPropagation());
  field.addEventListener('keypress', event => event.stopPropagation());
  const focus = () => {
    if (!wanted) return;
    if (document.activeElement !== field) field.focus({preventScroll: true});
  };
  // A phone opens its keyboard only for a focus that a touch caused. Focus again on each touch of the game.
  canvas?.addEventListener('pointerup', focus);
  field.addEventListener('blur', () => { if (wanted) setTimeout(focus, 0); });
  return {
    open(on) {
      wanted = Boolean(on);
      field.value = SENTINEL;
      if (wanted) focus();
      else field.blur();
      return wanted;
    },
    take: () => queue.take(),
    field,
  };
}

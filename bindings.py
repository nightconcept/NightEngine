"""Button bindings that change while the game runs (a rebind screen). No pyxel: names, not key codes.

A `Binding` names the keys and pad buttons of one button bit, without the `KEY_` or `GAMEPADn_BUTTON_` prefix
("SHIFT", "RIGHTSHOULDER"). The first name of each kind is the **primary**, the one a player can rebind. The rest are
fixed aliases (Enter beside Z, so menus always confirm). `host.keys.table(bindings)` makes the host's key table.

`Bindings.assign` moves one primary. If another action has that name as its primary, the two trade, so no action is
ever left without a binding. `locked` entries never move, and `reserved` names no action may take.
"""

import re
from collections.abc import Iterable, Mapping
from dataclasses import dataclass, field, replace

KINDS = ("key", "pad")
NAME = re.compile(r"[A-Z0-9_]+")


@dataclass(frozen=True)
class Binding:
    keys: tuple[str, ...] = ()
    pad: tuple[str, ...] = ()

    def names(self, kind: str) -> tuple[str, ...]:
        return self.keys if kind == "key" else self.pad

    def with_primary(self, kind: str, name: str | None) -> "Binding":
        """This binding with `name` as the primary of `kind` (None: no primary), keeping the aliases."""
        aliases = self.names(kind)[1:]
        names = ((name,) if name else ()) + tuple(a for a in aliases if a != name)
        return replace(self, **{"keys" if kind == "key" else "pad": names})


@dataclass(frozen=True, eq=False)
class Bindings:
    """`table` maps a button bit to its Binding. `locked` holds `(bit, kind, name)` entries that never move.
    `reserved` holds `(kind, name)` names that no action may take."""

    table: Mapping[int, Binding]
    locked: frozenset = field(default_factory=frozenset)
    reserved: frozenset = field(default_factory=frozenset)

    def __post_init__(self):
        object.__setattr__(self, "table", dict(self.table))
        object.__setattr__(self, "locked", frozenset(self.locked))
        object.__setattr__(self, "reserved", frozenset(self.reserved))

    def __eq__(self, other):
        if not isinstance(other, Bindings):
            return NotImplemented
        return (self.table, self.locked, self.reserved) == (other.table, other.locked, other.reserved)

    __hash__ = None

    def primary(self, bit: int, kind: str) -> str | None:
        names = self.table[bit].names(kind) if bit in self.table else ()
        return names[0] if names else None

    def owner(self, kind: str, name: str) -> int | None:
        """The bit whose primary of `kind` is `name`, or None."""
        return next((bit for bit in self.table if self.primary(bit, kind) == name), None)

    def is_alias(self, kind: str, name: str) -> bool:
        return any(name in b.names(kind)[1:] for b in self.table.values())

    def assign(self, bit: int, kind: str, name: str) -> "Bindings | None":
        """A new table with `name` as the primary of `bit`, or None when the name is reserved, is an alias, or a
        locked entry would move. Another action with `name` as its primary takes the old primary of `bit`."""
        if kind not in KINDS or bit not in self.table or (kind, name) in self.reserved or self.is_alias(kind, name):
            return None
        old = self.primary(bit, kind)
        if old == name:
            return self
        if (bit, kind, old) in self.locked:
            return None
        other = self.owner(kind, name)
        table = dict(self.table)
        if other is not None:
            if old is None or (other, kind, name) in self.locked:
                return None  # The other action would lose its binding, or a locked entry would move.
            table[other] = table[other].with_primary(kind, old)
        table[bit] = table[bit].with_primary(kind, name)
        return replace(self, table=table)

    def with_binding(self, bit: int, binding: Binding) -> "Bindings":
        """A new table with `bit` bound to `binding` (a set of direction keys, for example)."""
        return replace(self, table={**self.table, bit: binding})

    def with_reserved(self, reserved: Iterable[tuple[str, str]]) -> "Bindings":
        """The same table with another set of reserved names."""
        return replace(self, reserved=frozenset(reserved))

    def valid(self) -> bool:
        """No name is the primary of two actions, and every locked entry is in place."""
        for kind in KINDS:
            primaries = [p for bit in self.table if (p := self.primary(bit, kind))]
            if len(primaries) != len(set(primaries)):
                return False
        return all(self.primary(bit, kind) == name for bit, kind, name in self.locked)

    def to_json(self) -> dict:
        """The primaries only: `{"<bit>": {"key": name, "pad": name}}`. The aliases come from the defaults."""
        return {str(bit): {kind: self.primary(bit, kind) for kind in KINDS} for bit in sorted(self.table)}

    @classmethod
    def from_json(cls, data: object, defaults: "Bindings") -> "Bindings":
        """The primaries in `data` over `defaults`. Unknown bits and bad names are dropped, and missing ones come
        from the defaults. If a kind's result breaks a rule (two actions on one name, a reserved name, a locked entry
        moved), that whole kind falls back to the defaults. A reserved name is dropped unless its action holds it in
        the defaults."""
        out = defaults
        if not isinstance(data, Mapping):
            return out
        for kind in KINDS:
            table = dict(out.table)
            for bit in defaults.table:
                row = data.get(str(bit))
                name = row.get(kind) if isinstance(row, Mapping) else None
                if not (isinstance(name, str) and NAME.fullmatch(name)) or defaults.is_alias(kind, name):
                    continue
                if (kind, name) in defaults.reserved and name != defaults.primary(bit, kind):
                    continue  # No action may take a reserved name, except the one that holds it by default.
                table[bit] = table[bit].with_primary(kind, name)
            candidate = replace(out, table=table)
            if candidate.valid():
                out = candidate
        return out

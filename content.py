"""Helpers to load JSON content into typed records, and to check that records point at things that exist.

Adding a record is a data change. See the "Adding content" section of the README.
"""

import json
from collections.abc import Callable, Container
from pathlib import Path
from typing import Any, TypeVar

T = TypeVar("T")


class ContentError(Exception):
    """Content that cannot be loaded or does not match its record type. The message names the record."""


def read_json(path: Path | str) -> dict:
    """Read a JSON file. The top-level "_doc" key is a comment for people and is dropped."""
    data = json.loads(Path(path).read_text())
    if isinstance(data, dict):
        data.pop("_doc", None)
    return data


def read_dir(directory: Path | str, pattern: str = "*.json") -> dict[str, dict]:
    """Read every matching file in a directory into {file stem: data}, in name order."""
    return {p.stem: read_json(p) for p in sorted(Path(directory).glob(pattern))}


def records(
    data: dict[str, dict],
    cls: type[T],
    convert: dict[str, Callable[[Any], Any]] | None = None,
    key: str = "id",
    rename: dict[str, str] | None = None,
) -> dict[str, T]:
    """Build one `cls` per entry of a JSON table, passing the table key as the `key` field.

    `convert` maps a JSON field name to a function that turns its value into a tuple or a sub-record.
    `rename` maps a JSON field name to the record field name (after `convert`), for names Python cannot use.
    """
    convert, rename = convert or {}, rename or {}
    out: dict[str, T] = {}
    for name, fields in data.items():
        values = {rename.get(f, f): convert[f](v) if f in convert else v for f, v in fields.items()}
        try:
            out[name] = cls(**{key: name}, **values)
        except TypeError as e:
            raise ContentError(f"{cls.__name__} {name!r}: {e}") from e
    return out


def check_refs(table: dict, field: str, valid: Container, label: str) -> list[str]:
    """List every record whose `field` names something not in `valid`.

    `field` holds a string, or a list or tuple of strings. Records can be dataclasses or dicts.
    Each problem is one line, so a test can fail with `"\\n".join(problems)`.
    """
    problems = []
    for name, record in table.items():
        value = record.get(field) if isinstance(record, dict) else getattr(record, field, None)
        refs = [value] if isinstance(value, str) else value if isinstance(value, list | tuple) else []
        for ref in refs:
            if isinstance(ref, str) and ref not in valid:
                problems.append(f"{label}: {name}.{field} names missing {ref!r}")
    return problems

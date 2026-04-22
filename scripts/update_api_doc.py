import argparse
import os
import re
import tempfile
import urllib.error
import urllib.request
from collections import defaultdict
from dataclasses import dataclass
from pathlib import Path

LOVE_API_URL = "https://raw.githubusercontent.com/love2d-community/love-api/master/love_api.lua"
OUTPUT_API_MD = Path("project/api.md")
OUTPUT_COVERAGE_MD = Path(".agents/love-api.md")
SOURCE_ROOT = Path("src/NightFrame")

MODULE_NAME_OVERRIDES = {
    "Filesystem": "filesystem",
    "Framework": "",
    "Graphics": "graphics",
    "Joysticks": "joystick",
    "Keyboard": "keyboard",
    "Mouse": "mouse",
    "System": "system",
    "Timer": "timer",
    "VersionInfo": "",
    "Window": "window",
}

EXCLUDED_APIS = {
    "love.touchpressed",
}

MANUAL_IMPLEMENTED_APIS = {
    "love.conf",
}

MANUAL_MAPPINGS = {
    "Night.Framework.GetVersion": "love.getVersion",
    "Night.Framework.Run": "love.run",
    "Night.System.GetOS": "love.system.getOS",
    "Night.System.GetPowerInfo": "love.system.getPowerInfo",
    "Night.System.GetProcessorCount": "love.system.getProcessorCount",
    "Night.VersionInfo.GetVersion": "love.getVersion",
}

CALLBACK_MAPPINGS = {
    "Draw": "love.draw",
    "FileDropped": "love.filedropped",
    "GamepadAxis": "love.gamepadaxis",
    "GamepadPressed": "love.gamepadpressed",
    "GamepadReleased": "love.gamepadreleased",
    "JoystickAdded": "love.joystickadded",
    "JoystickAxis": "love.joystickaxis",
    "JoystickHat": "love.joystickhat",
    "JoystickPressed": "love.joystickpressed",
    "JoystickReleased": "love.joystickreleased",
    "JoystickRemoved": "love.joystickremoved",
    "KeyPressed": "love.keypressed",
    "KeyReleased": "love.keyreleased",
    "Load": "love.load",
    "MousePressed": "love.mousepressed",
    "MouseReleased": "love.mousereleased",
    "Quit": "love.quit",
    "Run": "love.run",
    "Update": "love.update",
}

TYPE_PATTERN = re.compile(
    r"public\s+(?:(?:static|abstract|sealed|partial)\s+)*"
    r"(class|interface|struct)\s+(\w+)"
)
METHOD_PATTERN = re.compile(
    r"public\s+"
    r"(?:(?:static|virtual|override|abstract|async|sealed|new|partial)\s+)*"
    r"[\w<>,\.\?\[\]\(\)\s]+\s+(\w+)\s*\(([^)]*)\)",
    re.MULTILINE,
)
INTERFACE_METHOD_PATTERN = re.compile(
    r"\b[\w<>,\.\?\[\]\(\)\s]+\s+(\w+)\s*\(([^)]*)\)\s*;",
    re.MULTILINE,
)
NAMESPACE_PATTERN = re.compile(r"namespace\s+([\w\.]+)")


@dataclass(frozen=True)
class DiscoveredMethod:
    namespace: str
    type_name: str
    method_name: str
    signature: str
    file_path: str

    @property
    def symbol_id(self):
        return f"{self.namespace}.{self.type_name}.{self.method_name}"


class LuaTableParser:
    def __init__(self, text):
        self.text = text
        self.length = len(text)
        self.index = 0

    def parse(self):
        start = self.text.find("return")
        if start == -1:
            raise ValueError("Could not find return statement in upstream Love API payload.")

        self.index = start + len("return")
        self._skip_ws()
        return self._parse_value()

    def _current(self):
        if self.index >= self.length:
            return ""
        return self.text[self.index]

    def _skip_ws(self):
        while self.index < self.length:
            if self.text.startswith("--", self.index):
                while self.index < self.length and self.text[self.index] != "\n":
                    self.index += 1
                continue

            if self.text[self.index].isspace():
                self.index += 1
                continue

            break

    def _parse_value(self):
        self._skip_ws()
        char = self._current()

        if char == "{":
            return self._parse_table()
        if char in {"'", '"'}:
            return self._parse_string()
        if char.isdigit() or char == "-":
            return self._parse_number()
        if char.isalpha() or char == "_":
            identifier = self._parse_identifier()
            if identifier == "true":
                return True
            if identifier == "false":
                return False
            if identifier == "nil":
                return None
            return identifier

        raise ValueError(f"Unexpected token while parsing Love API payload: {char!r}")

    def _parse_table(self):
        self.index += 1
        array_values = []
        keyed_values = {}

        while True:
            self._skip_ws()
            char = self._current()

            if char == "}":
                self.index += 1
                break

            if char == "":
                raise ValueError("Unterminated table while parsing Love API payload.")

            if char.isalpha() or char == "_":
                checkpoint = self.index
                key = self._parse_identifier()
                self._skip_ws()
                if self._current() == "=":
                    self.index += 1
                    keyed_values[key] = self._parse_value()
                else:
                    self.index = checkpoint
                    array_values.append(self._parse_value())
            else:
                array_values.append(self._parse_value())

            self._skip_ws()
            if self._current() == ",":
                self.index += 1

        if keyed_values and array_values:
            keyed_values["_array"] = array_values
            return keyed_values
        if keyed_values:
            return keyed_values
        return array_values

    def _parse_string(self):
        quote = self._current()
        self.index += 1
        chars = []

        while self.index < self.length:
            char = self.text[self.index]
            self.index += 1

            if char == "\\":
                if self.index >= self.length:
                    break
                escaped = self.text[self.index]
                self.index += 1
                escape_map = {
                    "n": "\n",
                    "r": "\r",
                    "t": "\t",
                    "\\": "\\",
                    "'": "'",
                    '"': '"',
                }
                chars.append(escape_map.get(escaped, escaped))
                continue

            if char == quote:
                return "".join(chars)

            chars.append(char)

        raise ValueError("Unterminated string while parsing Love API payload.")

    def _parse_number(self):
        start = self.index
        if self._current() == "-":
            self.index += 1

        while self.index < self.length and self.text[self.index].isdigit():
            self.index += 1

        if self.index < self.length and self.text[self.index] == ".":
            self.index += 1
            while self.index < self.length and self.text[self.index].isdigit():
                self.index += 1

        number_text = self.text[start:self.index]
        return float(number_text) if "." in number_text else int(number_text)

    def _parse_identifier(self):
        start = self.index
        while self.index < self.length and re.match(r"[\w]", self.text[self.index]):
            self.index += 1
        return self.text[start:self.index]


def get_array(value):
    if isinstance(value, list):
        return value
    if isinstance(value, dict):
        return value.get("_array", [])
    return []


def get_cache_root():
    xdg_cache_home = os.environ.get("XDG_CACHE_HOME")
    if xdg_cache_home:
        return Path(xdg_cache_home) / "NightEngine"

    return Path(tempfile.gettempdir()) / "NightEngine"


def preprocess_lua_source(text):
    return re.sub(
        r"\(\s*require\s*\(\s*path\s*\.\.\s*['\"]([^'\"]+)['\"]\s*\)\s*\)",
        lambda match: f"'__require__:{match.group(1)}'",
        text,
    )


def camel_case(name):
    if not name:
        return name
    return name[0].lower() + name[1:]


def derive_love_api_call(type_name, method_name):
    module_name = MODULE_NAME_OVERRIDES.get(type_name)
    if module_name is None:
        module_name = type_name.lower()

    if not module_name:
        return f"love.{camel_case(method_name)}"

    return f"love.{module_name}.{camel_case(method_name)}"


def fetch_love_api_source(refresh=False):
    cache_path = get_cache_root() / "love_api.lua"
    if cache_path.exists() and not refresh:
        return cache_path.read_text(encoding="utf-8")

    cache_path.parent.mkdir(parents=True, exist_ok=True)

    try:
        with urllib.request.urlopen(LOVE_API_URL) as response:
            payload = response.read().decode("utf-8")
    except urllib.error.URLError as error:
        if cache_path.exists():
            print(f"Warning: failed to refresh Love API data, using cache. {error}")
            return cache_path.read_text(encoding="utf-8")
        raise RuntimeError(f"Unable to fetch Love API data from {LOVE_API_URL}: {error}") from error

    cache_path.write_text(payload, encoding="utf-8")
    return payload


def fetch_love_module_source(module_ref, refresh=False):
    relative_path = Path(*module_ref.split(".")).with_suffix(".lua")
    cache_path = get_cache_root() / "love_api_modules" / relative_path
    module_url = (
        "https://raw.githubusercontent.com/love2d-community/love-api/master/"
        f"{relative_path.as_posix()}"
    )

    if cache_path.exists() and not refresh:
        return cache_path.read_text(encoding="utf-8")

    cache_path.parent.mkdir(parents=True, exist_ok=True)

    try:
        with urllib.request.urlopen(module_url) as response:
            payload = response.read().decode("utf-8")
    except urllib.error.URLError as error:
        if cache_path.exists():
            print(f"Warning: failed to refresh module {module_ref}, using cache. {error}")
            return cache_path.read_text(encoding="utf-8")
        raise RuntimeError(f"Unable to fetch Love API module {module_ref}: {error}") from error

    cache_path.write_text(payload, encoding="utf-8")
    return payload


def parse_lua_table(text):
    return LuaTableParser(preprocess_lua_source(text)).parse()


def flatten_love_api(api_root, refresh=False):
    tracked = defaultdict(set)
    version = str(api_root.get("version", "unknown"))

    for function in get_array(api_root.get("functions")):
        name = function.get("name")
        if name:
            tracked["love"].add(f"love.{name}")

    for callback in get_array(api_root.get("callbacks")):
        name = callback.get("name")
        if name:
            tracked["love"].add(f"love.{name}")

    for module in get_array(api_root.get("modules")):
        if isinstance(module, str) and module.startswith("__require__:"):
            module_ref = module.split(":", 1)[1]
            module = parse_lua_table(fetch_love_module_source(module_ref, refresh=refresh))

        module_name = module.get("name")
        if not module_name:
            continue

        module_key = f"love.{module_name}"
        for function in get_array(module.get("functions")):
            name = function.get("name")
            if name:
                tracked[module_key].add(f"{module_key}.{name}")

    tracked = {key: sorted(values) for key, values in sorted(tracked.items())}
    return version, tracked


def find_matching_brace(content, open_brace_index):
    depth = 0
    in_string = None
    escaped = False
    in_line_comment = False
    in_block_comment = False

    for index in range(open_brace_index, len(content)):
        char = content[index]
        next_char = content[index + 1] if index + 1 < len(content) else ""

        if in_line_comment:
            if char == "\n":
                in_line_comment = False
            continue

        if in_block_comment:
            if char == "*" and next_char == "/":
                in_block_comment = False
            continue

        if in_string:
            if escaped:
                escaped = False
                continue

            if char == "\\":
                escaped = True
                continue

            if char == in_string:
                in_string = None
            continue

        if char == "/" and next_char == "/":
            in_line_comment = True
            continue

        if char == "/" and next_char == "*":
            in_block_comment = True
            continue

        if char in {"'", '"'}:
            in_string = char
            continue

        if char == "{":
            depth += 1
            continue

        if char == "}":
            depth -= 1
            if depth == 0:
                return index

    return -1


def normalize_parameter_list(params_str):
    params = []
    for raw_param in params_str.split(","):
        cleaned = raw_param.strip()
        if not cleaned:
            continue
        cleaned = re.sub(r"\s*=\s*.*", "", cleaned)
        params.append(cleaned)
    return params


def parse_cs_file(filepath):
    content = filepath.read_text(encoding="utf-8")
    namespace_match = NAMESPACE_PATTERN.search(content)
    namespace = namespace_match.group(1) if namespace_match else "Night"
    discovered_methods = []
    type_matches = list(TYPE_PATTERN.finditer(content))

    for index, type_match in enumerate(type_matches):
        type_kind = type_match.group(1)
        type_name = type_match.group(2)
        body_open = content.find("{", type_match.end())
        if body_open == -1:
            continue

        body_end = find_matching_brace(content, body_open)
        if body_end == -1:
            next_type_start = type_matches[index + 1].start() if index + 1 < len(type_matches) else len(content)
            body_end = next_type_start

        body = content[body_open + 1:body_end]
        method_pattern = INTERFACE_METHOD_PATTERN if type_kind == "interface" else METHOD_PATTERN
        for method_match in method_pattern.finditer(body):
            method_name = method_match.group(1)
            if method_name == type_name:
                continue

            params = normalize_parameter_list(method_match.group(2))
            signature = f"{method_name}({', '.join(params)})"
            discovered_methods.append(
                DiscoveredMethod(
                    namespace=namespace,
                    type_name=type_name,
                    method_name=method_name,
                    signature=signature,
                    file_path=str(filepath),
                )
            )

    return discovered_methods


def discover_night_methods():
    methods = []

    for filepath in sorted(SOURCE_ROOT.rglob("*.cs")):
        if any(part in {"bin", "obj"} for part in filepath.parts):
            continue
        methods.extend(parse_cs_file(filepath))

    return methods


def map_method_to_love_api(method):
    if method.symbol_id in MANUAL_MAPPINGS:
        return MANUAL_MAPPINGS[method.symbol_id]

    if method.type_name in {"Game", "IGame"} and method.method_name in CALLBACK_MAPPINGS:
        return CALLBACK_MAPPINGS[method.method_name]

    if method.namespace != "Night":
        return None

    if method.type_name not in MODULE_NAME_OVERRIDES:
        return None

    return derive_love_api_call(method.type_name, method.method_name)


def collect_implemented_apis(methods):
    implemented = defaultdict(list)

    for api_name in MANUAL_IMPLEMENTED_APIS:
        implemented[api_name].append("manual")

    for method in methods:
        api_name = map_method_to_love_api(method)
        if not api_name:
            continue
        implemented[api_name].append(method.symbol_id)

    return implemented


def generate_api_inventory_markdown(methods):
    grouped = defaultdict(lambda: defaultdict(list))

    for method in methods:
        if method.namespace != "Night":
            continue
        grouped[method.type_name][method.method_name].append(method.signature)

    lines = ["# Night / Love2D API", ""]
    for type_name in sorted(grouped.keys()):
        lines.append(f"## {type_name}")
        lines.append("")
        for method_name in sorted(grouped[type_name].keys()):
            signatures = sorted(set(grouped[type_name][method_name]))
            love_call = derive_love_api_call(type_name, method_name)
            lines.append(f"- {method_name}() - {love_call}")
            if len(signatures) > 1 or signatures[0] != f"{method_name}()":
                for signature in signatures:
                    lines.append(f"  - {signature}")
        lines.append("")

    return "\n".join(lines).rstrip() + "\n"


def generate_coverage_markdown(version, tracked_apis, implemented_apis):
    tracked_count = 0
    implemented_count = 0
    excluded_count = 0
    header_lines = [
        "# Love2D API Coverage",
        "",
        "Current implementation status vs. the latest tracked Love2D API surface for NightEngine.",
        "",
        f"Source: `{LOVE_API_URL}`",
        f"Upstream version: `{version}`",
        "",
    ]
    module_sections = []

    for module_name in sorted(tracked_apis.keys()):
        module_lines = []
        for api_name in tracked_apis[module_name]:
            tracked_count += 1
            if api_name in EXCLUDED_APIS:
                excluded_count += 1
                module_lines.append(f"- [~] `{api_name}` - excluded in script")
                continue

            if api_name in implemented_apis:
                implemented_count += 1
                module_lines.append(f"- [x] `{api_name}`")
            else:
                module_lines.append(f"- [ ] `{api_name}`")

        module_sections.append(f"## {module_name}")
        module_sections.append("")
        module_sections.extend(module_lines)
        module_sections.append("")

    remaining_count = tracked_count - implemented_count - excluded_count
    coverage_ratio = 0.0
    active_total = tracked_count - excluded_count
    if active_total > 0:
        coverage_ratio = implemented_count / active_total * 100.0

    summary_lines = [
        "Summary:",
        f"- Tracked: {tracked_count}",
        f"- Implemented: {implemented_count}",
        f"- Excluded: {excluded_count}",
        f"- Remaining: {remaining_count}",
        f"- Coverage: {coverage_ratio:.1f}%",
        "",
    ]

    return "\n".join(header_lines + summary_lines + module_sections).rstrip() + "\n"


def write_text_file(path, contents):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(contents, encoding="utf-8")
    print(f"Wrote {path}")


def main():
    parser = argparse.ArgumentParser(description="Generate Night API inventory and Love2D parity report.")
    parser.add_argument(
        "--refresh-upstream",
        action="store_true",
        help="Force-refresh the upstream Love API payload before generating reports.",
    )
    args = parser.parse_args()

    if not SOURCE_ROOT.is_dir():
        raise RuntimeError(f"Source root not found: {SOURCE_ROOT}")

    love_api_payload = fetch_love_api_source(refresh=args.refresh_upstream)
    love_api = parse_lua_table(love_api_payload)
    version, tracked_apis = flatten_love_api(love_api, refresh=args.refresh_upstream)
    methods = discover_night_methods()
    implemented_apis = collect_implemented_apis(methods)

    write_text_file(OUTPUT_API_MD, generate_api_inventory_markdown(methods))
    write_text_file(
        OUTPUT_COVERAGE_MD,
        generate_coverage_markdown(version, tracked_apis, implemented_apis),
    )

    print(f"Tracked {sum(len(values) for values in tracked_apis.values())} Love APIs.")
    print(f"Matched {len(implemented_apis)} implemented Love APIs.")


if __name__ == "__main__":
    main()

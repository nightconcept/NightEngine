import os
import re
from collections import defaultdict

def derive_love2d_api(class_name, method_name):
    """
    Attempts to derive a Love2D-style API call.
    Example: Filesystem, GetInfo -> love.filesystem.getInfo
    """
    if not class_name or not method_name:
        return ""

    module_name = class_name.lower()

    # Convert PascalCase or camelCase method_name to camelCase (starting lowercase)
    # If it's already camelCase (like 'getInfo'), it should remain as is.
    # If it's PascalCase (like 'GetInfo'), it becomes 'getInfo'.
    love_method_name = method_name[0].lower() + method_name[1:]

    return f"love.{module_name}.{love_method_name}"

def parse_cs_file(filepath):
    """
    Parses a C# file to extract public static classes and their public static methods.
    """
    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()
    except Exception as e:
        print(f"Error reading file {filepath}: {e}")
        return None

    class_data = {}

    # Regex to find public static classes
    class_match = re.search(r"public\s+static\s+class\s+(\w+)", content)

    if class_match:
        class_name = class_match.group(1)
        class_content_start = class_match.end()

        # Rough way to get class content, assuming reasonably formatted code
        # This might fail with complex nested structures or preprocessor directives
        open_braces = 0
        class_body_end = class_content_start
        found_first_brace = False

        for i in range(class_content_start, len(content)):
            if content[i] == '{':
                if not found_first_brace:
                    found_first_brace = True
                open_braces += 1
            elif content[i] == '}':
                open_braces -= 1
                if found_first_brace and open_braces == 0:
                    class_body_end = i
                    break

        class_body = content[class_match.end():class_body_end]

        methods = defaultdict(list)
        # Regex to find public static methods, including parameters
        # This regex aims to capture [return_type] MethodName([params])
        # Group 1: return_type (non-greedy)
        # Group 2: method_name
        # Group 3: params_str
        method_pattern = re.compile(
            r"public\s+static\s+(?:async\s+)?(.*?)\s+(\w+)\s*\(([^)]*)\)"
        )

        for method_match in method_pattern.finditer(class_body):
            # Return type is group 1, method_name is group 2, params_str is group 3
            method_name = method_match.group(2)
            params_str = method_match.group(3).strip()

            # Clean up parameter string: remove default values, extra spaces
            params_list = []
            if params_str:
                raw_params = params_str.split(',')
                for p in raw_params:
                    p_cleaned = p.strip()
                    # Remove default initializers like "= null" or "= 12"
                    p_cleaned = re.sub(r"\s*=\s*.*", "", p_cleaned)
                    params_list.append(p_cleaned)

            full_signature = f"{method_name}({', '.join(params_list)})"
            methods[method_name].append(full_signature)

        if methods:
            class_data[class_name] = dict(methods)

    return class_data

def parse_enums_cs_file(filepath):
    """
    Parses a C# file to extract public enums.
    """
    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()
    except FileNotFoundError:
        return [] # Silently return empty if file not found
    except Exception as e:
        print(f"Error reading enum file {filepath}: {e}")
        return []

    enums = []
    # Regex to find public enums
    enum_pattern = re.compile(r"public\s+enum\s+(\w+)")
    for match in enum_pattern.finditer(content):
        enums.append(match.group(1))
    return sorted(list(set(enums)))

def parse_types_cs_file(filepath):
    """
    Parses a C# file to extract public class names.
    """
    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()
    except FileNotFoundError:
        return [] # Silently return empty if file not found
    except Exception as e:
        print(f"Error reading types file {filepath}: {e}")
        return []

    types = []
    # Regex to find public classes (can be extended for structs, interfaces if needed)
    # e.g., r"public\s+(?:class|struct|interface)\s+(\w+)"
    class_pattern = re.compile(r"public\s+(?:class|struct)\s+(\w+)")
    for match in class_pattern.finditer(content):
        types.append(match.group(1))
    return sorted(list(set(types)))

def generate_markdown(all_module_data, output_file):
    """
    Generates a markdown file from the parsed API data.
    """
    markdown_lines = []
    markdown_lines.append(f"# Night / Love2D API\n")

    sorted_module_names = sorted(all_module_data.keys())

    for module_name_key in sorted_module_names:
        module_data = all_module_data[module_name_key]
        markdown_lines.append(f"## {module_name_key}\n")

        module_had_content = False

        # --- Types ---
        if module_data.get("types"):
            if module_had_content: markdown_lines.append("") # Separator from previous section
            markdown_lines.append(f"### Types ({module_name_key})\n")
            module_had_content = True
            for type_name in module_data["types"]: # Already sorted from parsing function
                markdown_lines.append(f"- {type_name}")

        # --- Functions ---
        if module_data.get("functions"):
            if module_had_content: markdown_lines.append("") # Separator from previous section
            markdown_lines.append(f"### Functions ({module_name_key})\n")
            module_had_content = True
            # The "functions" key holds a dict like: {"ClassName": {"methodName": [signatures]}}
            for class_name, methods in module_data["functions"].items():
                sorted_method_names = sorted(methods.keys())
                for method_name in sorted_method_names:
                    signatures = methods[method_name]
                    love2d_call = derive_love2d_api(class_name, method_name)

                    if love2d_call:
                        markdown_lines.append(f"- {method_name}() - {love2d_call}")
                    else:
                        markdown_lines.append(f"- {method_name}()")

                    if len(signatures) > 1 or (len(signatures) == 1 and signatures[0] != f"{method_name}()"):
                        for sig in sorted(signatures):
                            markdown_lines.append(f"  - {sig}")

        # --- Enums ---
        if module_data.get("enums"):
            if module_had_content: markdown_lines.append("")
            markdown_lines.append(f"### Enums ({module_name_key})\n")
            module_had_content = True
            for enum_name in module_data["enums"]:
                markdown_lines.append(f"- {enum_name}")

        # If not the last module, add a separating blank line.
        if module_name_key != sorted_module_names[-1]:
            markdown_lines.append("")

    # At end of document, add ONE blank line
    markdown_lines.append("")

    try:
        with open(output_file, 'w', encoding='utf-8') as f:
            f.write("\n".join(markdown_lines))
        print(f"Markdown documentation generated at {output_file}")
    except Exception as e:
        print(f"Error writing markdown file {output_file}: {e}")

def main():
    framework_dir = os.path.join("src", "Night")
    output_md_file = os.path.join("project", "API.md")

    all_module_data = defaultdict(lambda: {"functions": {}, "enums": [], "types": []})

    if not os.path.isdir(framework_dir):
        print(f"Error: Directory not found - {framework_dir}")
        return

    # Iterate through subdirectories in framework_dir (each is a module)
    for module_name in sorted(os.listdir(framework_dir)): # Sort for consistent processing order
        module_path = os.path.join(framework_dir, module_name)
        if os.path.isdir(module_path):
            if module_name.startswith('.'): # Skip hidden directories like .git
                continue

            print(f"Processing module: {module_name}...")

            current_module_enums = set()
            current_module_types = set()

            # Define the expected main module file for functions
            main_module_cs_file_identifier = f"{module_name}.cs"
            main_module_cs_file_path_expected = os.path.join(module_path, main_module_cs_file_identifier)

            # 1. Parse main module file for functions if it exists
            if os.path.exists(main_module_cs_file_path_expected):
                print(f"  Parsing functions from main module file: {main_module_cs_file_path_expected}...")
                parsed_functions_data = parse_cs_file(main_module_cs_file_path_expected)
                if parsed_functions_data:
                    for class_name_func, methods in parsed_functions_data.items():
                        # Ensure the functions dict for this class_name_func exists
                        if class_name_func not in all_module_data[module_name]["functions"]:
                            all_module_data[module_name]["functions"][class_name_func] = defaultdict(list)

                        for method_name, signatures in methods.items():
                            all_module_data[module_name]["functions"][class_name_func][method_name].extend(signatures)
                            all_module_data[module_name]["functions"][class_name_func][method_name] = \
                                sorted(list(set(all_module_data[module_name]["functions"][class_name_func][method_name])))
                else:
                    print(f"    No functions found or error parsing in {main_module_cs_file_identifier}.")
            else:
                print(f"  Skipping functions: Main module file {main_module_cs_file_path_expected} not found.")

            # 2. Iterate through ALL .cs files in the module directory for enums and types
            print(f"  Scanning all .cs files in {module_path} for enums and types/structs...")
            for item_name in sorted(os.listdir(module_path)):
                item_path = os.path.join(module_path, item_name)
                if os.path.isfile(item_path) and item_name.endswith(".cs"):
                    # Parse for enums
                    parsed_enums = parse_enums_cs_file(item_path)
                    if parsed_enums:
                        current_module_enums.update(parsed_enums)

                    # Parse for types (classes/structs)
                    parsed_types = parse_types_cs_file(item_path)
                    if parsed_types:
                        current_module_types.update(parsed_types)

            # Store aggregated enums and types
            if current_module_enums:
                all_module_data[module_name]["enums"] = sorted(list(current_module_enums))
                print(f"  Found {len(all_module_data[module_name]['enums'])} enums in module {module_name}.")
            else:
                print(f"  No enums found in module {module_name}.")

            if current_module_types:
                all_module_data[module_name]["types"] = sorted(list(current_module_types))
                print(f"  Found {len(all_module_data[module_name]['types'])} types/structs in module {module_name}.")
            else:
                print(f"  No types/structs found in module {module_name}.")

            print("") # Blank line after processing a module's files for readability in console

    if all_module_data:
        os.makedirs(os.path.dirname(output_md_file), exist_ok=True)
        generate_markdown(all_module_data, output_md_file)
    else:
        print("No API data parsed.")

if __name__ == "__main__":
    main()

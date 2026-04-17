import requests
import zipfile
import os
import shutil
import tempfile
import xml.etree.ElementTree as ET

OWNER = "nightconcept"
REPO = "build-sdl"
PREBUILT_DIR = os.path.join(os.path.dirname(__file__), "..", "lib", "SDL3-Prebuilt")
VERSION_FILE = os.path.join(PREBUILT_DIR, "version.txt")

LIBRARIES_CONFIG = {
    "sdl3-core": {
        "tag_prefix": "sdl3-core-release-",
        "asset_lib_name": "SDL3",
        "lib_files": {
            "windows": "SDL3.dll",
            "macos": "libSDL3.0.dylib",
            "linux": "libSDL3.so.0",
        },
        "extract_subfolder": None,
        "csproj_path": os.path.join(os.path.dirname(__file__), "..", "lib", "SDL3-CS", "SDL3-CS.Native", "SDL3-CS.Native.csproj")
    },
    "sdl2_mixer": {
        "tag_prefix": "sdl3_mixer-release-",
        "asset_lib_name": "SDL3_mixer",
        "lib_files": {
            "windows": "SDL3_mixer.dll",
            "macos": "libSDL3_mixer.0.dylib",
            "linux": "libSDL3_mixer.so.0",
        },
        "extract_subfolder": None,
        "csproj_path": os.path.join(os.path.dirname(__file__), "..", "lib", "SDL3-CS", "SDL3-CS.Native.Mixer", "SDL3-CS.Native.Mixer.csproj")
    },
    "sdl3_ttf": {
        "tag_prefix": "sdl3_ttf-release-",
        "asset_lib_name": "SDL3_ttf",
        "lib_files": {
            "windows": "SDL3_ttf.dll",
            "macos": "libSDL3_ttf.0.dylib",
            "linux": "libSDL3_ttf.so.0",
        },
        "extract_subfolder": None,
        "csproj_path": os.path.join(os.path.dirname(__file__), "..", "lib", "SDL3-CS", "SDL3-CS.Native.TTF", "SDL3-CS.Native.TTF.csproj")
    },
    "sdl3_image": {
        "tag_prefix": "sdl3_image-release-",
        "asset_lib_name": "SDL3_image",
        "lib_files": {
            "windows": "SDL3_image.dll",
            "macos": "libSDL3_image.0.dylib",
            "linux": "libSDL3_image.so.0",
        },
        "extract_subfolder": None,
        "csproj_path": os.path.join(os.path.dirname(__file__), "..", "lib", "SDL3-CS", "SDL3-CS.Native.Image", "SDL3-CS.Native.Image.csproj")
    },
}

PLATFORM_TAGS = {
    "windows": "win32-x64",
    "macos": "macos-universal", # Default for macOS, overridden for SDL_image arm64
    "linux": "linux-x86_64",
}

# Ensure PREBUILT_DIR subdirectories exist
for platform in PLATFORM_TAGS.keys():
    os.makedirs(os.path.join(PREBUILT_DIR, platform), exist_ok=True)

def get_all_releases():
    """Fetches all release information from GitHub."""
    api_url = f"https://api.github.com/repos/{OWNER}/{REPO}/releases"
    print(f"Fetching all releases from {api_url}...")
    response = requests.get(api_url)
    response.raise_for_status()
    return response.json()

def get_version_from_csproj(csproj_path):
    """Extracts and formats the version from a .csproj file."""
    try:
        tree = ET.parse(csproj_path)
        root = tree.getroot()

        def normalize_version(raw_version):
            if not raw_version:
                return None

            clean_version = raw_version.strip().split("-", 1)[0]
            parts = clean_version.split(".")
            if len(parts) >= 3:
                return ".".join(parts[:3])

            print(f"Warning: Version '{raw_version}' in {csproj_path} is not in an expected semantic version format.")
            return None

        property_groups = root.findall("./PropertyGroup")
        nuget_property_groups = [group for group in property_groups if group.get("Label") == "NuGet"]
        search_groups = nuget_property_groups if nuget_property_groups else property_groups

        for property_group in search_groups:
            for version_tag in ("Version", "VersionPrefix"):
                version_element = property_group.find(version_tag)
                if version_element is not None and version_element.text:
                    normalized_version = normalize_version(version_element.text)
                    if normalized_version:
                        return normalized_version

        print(f"Warning: Could not find <Version> or <VersionPrefix> in {csproj_path}.")
        return None
    except ET.ParseError:
        print(f"Error: Could not parse XML from {csproj_path}.")
        return None
    except FileNotFoundError:
        print(f"Error: .csproj file not found at {csproj_path}.")
        return None

def get_specific_release_by_version_tag(releases, tag_prefix, target_version_str):
    """Finds a specific release matching a given tag prefix and version string."""
    expected_tag_name = tag_prefix + target_version_str
    print(f"Searching for release with exact tag: {expected_tag_name}")
    for release in releases:
        if release.get("tag_name", "") == expected_tag_name:
            release["parsed_version"] = target_version_str  # Store the clean version string
            print(f"Found specific release: {release['tag_name']}")
            return release
    print(f"Release with tag '{expected_tag_name}' not found.")
    return None

def find_asset_url(release_data, expected_asset_name):
    """Finds the download URL for a specific asset in the release data."""
    for asset in release_data.get("assets", []):
        if asset["name"] == expected_asset_name:
            # Reduced verbosity: print(f"Found asset: {asset['browser_download_url']}")
            return asset["browser_download_url"]
    print(f"Warning: Asset '{expected_asset_name}' not found in release {release_data.get('tag_name')}")
    return None

def download_file(url, dest_path):
    """Downloads a file from a URL to a destination path."""
    print(f"Downloading {os.path.basename(dest_path)}...")
    response = requests.get(url, stream=True)
    response.raise_for_status()
    with open(dest_path, "wb") as f:
        for chunk in response.iter_content(chunk_size=8192):
            f.write(chunk)
    # Reduced verbosity: print("Download complete.")

def extract_zip(zip_path, extract_to_path):
    """Extracts a zip file to a specified directory."""
    # Reduced verbosity: print(f"Extracting {zip_path} to {extract_to_path}...")
    with zipfile.ZipFile(zip_path, "r") as zip_ref:
        zip_ref.extractall(extract_to_path)
    # Reduced verbosity: print("Extraction complete.")

def copy_library_file(extract_path, lib_name, platform, lib_config):
    """Copies the specific library file from the extracted path to the prebuilt directory."""
    lib_filename = lib_config["lib_files"][platform]

    # Determine source path, considering a potential subfolder in the zip
    src_file_path = extract_path # Default if logic below doesn't find a better path
    if lib_config.get("extract_subfolder"):
        # This logic might need adjustment if the subfolder name is dynamic (e.g., versioned)
        # For now, assumes a fixed subfolder name if provided.
        # Example: if zip extracts to "sdl3-core-3.0.0/", then extract_subfolder could be "sdl3-core-3.0.0"
        # However, the current LIBRARIES_CONFIG has it as None.

        # Simplified: try root, then try common patterns like 'lib', 'bin'
        possible_src_paths = [
            os.path.join(extract_path, lib_config["extract_subfolder"], lib_filename),
            os.path.join(extract_path, lib_config["extract_subfolder"], "lib", lib_filename),
            os.path.join(extract_path, lib_config["extract_subfolder"], "bin", lib_filename),
        ]
        # Also check if the subfolder itself is the direct parent of the lib_filename
        possible_src_paths.insert(0, os.path.join(extract_path, lib_filename)) # Check root of extract_path first

        found_src_file = None
        for p_path in possible_src_paths:
            if os.path.exists(p_path):
                found_src_file = p_path
                break

        if not found_src_file:
            print(f"Warning: Library file {lib_filename} not found in standard subfolder paths for {lib_name} on {platform} with extract_subfolder '{lib_config.get('extract_subfolder')}'. Searching recursively in {extract_path}...")
            for root, _, files in os.walk(extract_path):
                if lib_filename in files:
                    found_src_file = os.path.join(root, lib_filename)
                    print(f"Found {lib_filename} at {found_src_file}")
                    break
            if not found_src_file:
                print(f"Error: Library file {lib_filename} not found in {extract_path} for {lib_name} on {platform}.")
                return False
        src_file_path = found_src_file

    else: # No extract_subfolder specified, assume lib is at root of extracted files or in a single dir.
        src_file_path_direct = os.path.join(extract_path, lib_filename)
        if os.path.exists(src_file_path_direct):
            src_file_path = src_file_path_direct
        else:
            extracted_items = os.listdir(extract_path)
            if len(extracted_items) == 1 and os.path.isdir(os.path.join(extract_path, extracted_items[0])):
                # Zip extracted into a single top-level directory
                base_extracted_dir = os.path.join(extract_path, extracted_items[0])

                # Check for lib directly in this subdir
                src_file_path_subdir_direct = os.path.join(base_extracted_dir, lib_filename)
                if os.path.exists(src_file_path_subdir_direct):
                    src_file_path = src_file_path_subdir_direct
                else:
                    # Try common subdirs like lib/ or bin/ within that single extracted directory
                    common_subdirs_to_check = ["lib", "bin"]
                    found_in_common_subdir = False
                    for common_s_dir in common_subdirs_to_check:
                        path_in_common_subdir = os.path.join(base_extracted_dir, common_s_dir, lib_filename)
                        if os.path.exists(path_in_common_subdir):
                            src_file_path = path_in_common_subdir
                            found_in_common_subdir = True
                            break
                    if not found_in_common_subdir:
                        # Fallback: search recursively within the single extracted directory
                        print(f"Warning: Library file {lib_filename} not found in standard paths within {base_extracted_dir}. Searching recursively...")
                        found_recursively = False
                        for root, _, files in os.walk(base_extracted_dir):
                            if lib_filename in files:
                                src_file_path = os.path.join(root, lib_filename)
                                print(f"Found {lib_filename} at {src_file_path}")
                                found_recursively = True
                                break
                        if not found_recursively:
                            print(f"Error: Library file {lib_filename} not found in {extract_path} or its single subdirectory '{base_extracted_dir}' for {lib_name} on {platform}.")
                            return False
            else: # Multiple items at root of extraction, or not a directory
                # Fallback: search recursively from the root of extract_path
                print(f"Warning: Library file {lib_filename} not found directly in {extract_path} and not a single subdirectory structure. Searching recursively in {extract_path}...")
                found_recursively_at_root = False
                for root, _, files in os.walk(extract_path):
                    if lib_filename in files:
                        src_file_path = os.path.join(root, lib_filename)
                        print(f"Found {lib_filename} at {src_file_path}")
                        found_recursively_at_root = True
                        break
                if not found_recursively_at_root:
                    print(f"Error: Library file {lib_filename} not found directly in {extract_path} for {lib_name} on {platform}.")
                    return False


    dest_dir = os.path.join(PREBUILT_DIR, platform)
    dest_file_path = os.path.join(dest_dir, lib_filename)

    os.makedirs(dest_dir, exist_ok=True)
    shutil.copy2(src_file_path, dest_file_path)
    print(f"  Successfully copied {lib_filename} for {lib_name} ({platform})")
    return True

def update_version_file(library_versions):
    """Updates the version.txt file with all successfully fetched library versions."""
    if library_versions:
        print(f"\nUpdating {VERSION_FILE} with library versions...")
        with open(VERSION_FILE, "w") as f:
            for lib_key, version_str in sorted(library_versions.items()):
                f.write(f"{lib_key}={version_str}\n")
        print("Version file updated.")
    else:
        print("\nSkipping version file update as no library versions were determined.")

def main():
    library_versions = {} # To store successfully fetched versions
    total_expected_files = 0
    successfully_copied_files = 0
    failed_downloads_or_copies = [] # Stores tuples of (lib_key, platform_key, reason)

    try:
        all_releases = get_all_releases()
        if not all_releases:
            print("No releases found. Exiting.")
            return

        for lib_key, lib_config in LIBRARIES_CONFIG.items():
            print(f"\nProcessing library: {lib_key}...")

            csproj_path = lib_config.get("csproj_path")
            if not csproj_path:
                print(f"  Error: csproj_path not defined for {lib_key}. Skipping.")
                for platform_key in PLATFORM_TAGS.keys():
                    total_expected_files += 1
                    failed_downloads_or_copies.append((lib_key, platform_key, "csproj_path not defined"))
                continue

            target_version_str = get_version_from_csproj(csproj_path)
            if not target_version_str:
                print(f"  Could not determine version for {lib_key} from {csproj_path}. Skipping all platforms for this library.")
                for platform_key in PLATFORM_TAGS.keys():
                    total_expected_files += 1
                    failed_downloads_or_copies.append((lib_key, platform_key, f"Version not found in {os.path.basename(csproj_path)}"))
                continue

            print(f"  Target version from {os.path.basename(csproj_path)}: {target_version_str}")
            specific_lib_release = get_specific_release_by_version_tag(all_releases, lib_config["tag_prefix"], target_version_str)

            if not specific_lib_release:
                print(f"  Could not find release for {lib_key} version {target_version_str}. Skipping all platforms for this library.")
                for platform_key in PLATFORM_TAGS.keys():
                    total_expected_files += 1
                    failed_downloads_or_copies.append((lib_key, platform_key, f"Release for version {target_version_str} not found"))
                continue

            lib_version = specific_lib_release["parsed_version"] # Should be target_version_str
            # Store version if release was found, even if some assets fail later
            library_versions[lib_key] = lib_version

            for platform_key, platform_tag_value in PLATFORM_TAGS.items():
                total_expected_files += 1
                asset_to_log_base = f"{lib_key} v{lib_version} ({platform_key})"

                current_platform_tag = platform_tag_value

                # Determine asset_lib_name for constructing the filename
                asset_lib_name_config_value = lib_config["asset_lib_name"]
                actual_asset_lib_name_for_file = ""

                if isinstance(asset_lib_name_config_value, dict):
                    actual_asset_lib_name_for_file = asset_lib_name_config_value.get(platform_key)
                    if not actual_asset_lib_name_for_file:
                        print(f"    Error: Platform-specific asset_lib_name for '{platform_key}' not found in config for '{lib_key}'. Skipping.")
                        failed_downloads_or_copies.append((lib_key, platform_key, f"asset_lib_name for {platform_key} missing"))
                        continue
                else:
                    actual_asset_lib_name_for_file = asset_lib_name_config_value

                if lib_key == "sdl3_image" and platform_key == "macos":
                    # SDL3_image on macOS has a special asset name for arm64
                    expected_asset_name = f"SDL3_image-{lib_version}-macos-arm64.zip"
                else:
                    expected_asset_name = f"{actual_asset_lib_name_for_file}-{lib_version}-{current_platform_tag}.zip"

                print(f"  Looking for asset: {expected_asset_name}")
                asset_url = find_asset_url(specific_lib_release, expected_asset_name)

                if not asset_url:
                    print(f"    Asset not found. Skipping.")
                    failed_downloads_or_copies.append((lib_key, platform_key, "Asset not found in release"))
                    continue

                try:
                    with tempfile.TemporaryDirectory() as tmpdir:
                        zip_filename = expected_asset_name
                        zip_path = os.path.join(tmpdir, zip_filename)

                        download_file(asset_url, zip_path)

                        extract_target_path = os.path.join(tmpdir, f"extracted_{lib_key}_{platform_key}_{lib_version}")
                        os.makedirs(extract_target_path, exist_ok=True)
                        extract_zip(zip_path, extract_target_path)

                        if copy_library_file(extract_target_path, lib_key, platform_key, lib_config):
                            successfully_copied_files +=1
                        else:
                            # Error already printed in copy_library_file
                            failed_downloads_or_copies.append((lib_key, platform_key, "Copy failed"))
                except Exception as e_inner:
                    print(f"    Error processing {asset_to_log_base}: {e_inner}")
                    failed_downloads_or_copies.append((lib_key, platform_key, f"Exception: {e_inner}"))

        update_version_file(library_versions)

    except requests.exceptions.RequestException as e:
        print(f"\nNetwork error: {e}")
    except zipfile.BadZipFile as e:
        print(f"\nError: Downloaded file is not a valid zip file or is corrupted: {e}")
    except Exception as e:
        print(f"\nAn unexpected error occurred: {e}")
        import traceback
        traceback.print_exc()
    finally:
        print("\n--- Update Summary ---")
        print(f"Total library files expected: {total_expected_files}")
        print(f"Successfully copied:        {successfully_copied_files}")
        failures = total_expected_files - successfully_copied_files
        print(f"Failed to retrieve/copy:  {failures}")
        if failed_downloads_or_copies:
            print("\nDetails of failures/skipped files:")
            for lib, plat, reason in failed_downloads_or_copies:
                print(f"  - {lib} ({plat}): {reason}")
        print("----------------------")

if __name__ == "__main__":
    main()

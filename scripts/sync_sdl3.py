import requests
import zipfile
import os
import shutil
import tempfile
import json

OWNER = "nightconcept"
REPO = "build-sdl"
PREBUILT_DIR = os.path.join(os.path.dirname(__file__), "..", "lib", "SDL3-Prebuilt")
MANIFEST_FILE = os.path.join(os.path.dirname(__file__), "..", "lib", "sdl3-manifest.json")

LIBRARIES_CONFIG = {
    "sdl3-core": {
        "tag_prefix": "sdl3-core-release-",
        "asset_lib_name": "SDL3",
        "lib_files": {
            "windows": "SDL3.dll",
            "macos": "libSDL3.0.dylib",
            "linux": "libSDL3.so.0",
        },
    },
    "sdl3_mixer": {
        "tag_prefix": "sdl3_mixer-release-",
        "asset_lib_name": "SDL3_mixer",
        "lib_files": {
            "windows": "SDL3_mixer.dll",
            "macos": "libSDL3_mixer.0.dylib",
            "linux": "libSDL3_mixer.so.0",
        },
    },
    "sdl3_ttf": {
        "tag_prefix": "sdl3_ttf-release-",
        "asset_lib_name": "SDL3_ttf",
        "lib_files": {
            "windows": "SDL3_ttf.dll",
            "macos": "libSDL3_ttf.0.dylib",
            "linux": "libSDL3_ttf.so.0",
        },
    },
    "sdl3_image": {
        "tag_prefix": "sdl3_image-release-",
        "asset_lib_name": "SDL3_image",
        "lib_files": {
            "windows": "SDL3_image.dll",
            "macos": "libSDL3_image.0.dylib",
            "linux": "libSDL3_image.so.0",
        },
    },
}

PLATFORM_TAGS = {
    "windows": "win32-x64",
    "macos": "macos-universal",
    "linux": "linux-x86_64",
}

for platform in PLATFORM_TAGS.keys():
    os.makedirs(os.path.join(PREBUILT_DIR, platform), exist_ok=True)


def load_manifest():
    """Loads library versions from sdl3-manifest.json."""
    try:
        with open(MANIFEST_FILE, "r") as f:
            return json.load(f)
    except FileNotFoundError:
        print(f"Error: Manifest file not found at {MANIFEST_FILE}.")
        return None
    except json.JSONDecodeError as e:
        print(f"Error: Could not parse {MANIFEST_FILE}: {e}")
        return None


def get_all_releases():
    """Fetches all release information from GitHub."""
    api_url = f"https://api.github.com/repos/{OWNER}/{REPO}/releases"
    print(f"Fetching all releases from {api_url}...")
    response = requests.get(api_url)
    response.raise_for_status()
    return response.json()


def get_specific_release_by_version_tag(releases, tag_prefix, target_version_str):
    """Finds a specific release matching a given tag prefix and version string."""
    expected_tag_name = tag_prefix + target_version_str
    print(f"Searching for release with exact tag: {expected_tag_name}")
    for release in releases:
        if release.get("tag_name", "") == expected_tag_name:
            release["parsed_version"] = target_version_str
            print(f"Found specific release: {release['tag_name']}")
            return release
    print(f"Release with tag '{expected_tag_name}' not found.")
    return None


def find_asset_url(release_data, expected_asset_name):
    """Finds the download URL for a specific asset in the release data."""
    for asset in release_data.get("assets", []):
        if asset["name"] == expected_asset_name:
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


def extract_zip(zip_path, extract_to_path):
    """Extracts a zip file to a specified directory."""
    with zipfile.ZipFile(zip_path, "r") as zip_ref:
        zip_ref.extractall(extract_to_path)


def copy_library_file(extract_path, lib_name, platform, lib_config):
    """Copies the specific library file from the extracted path to the prebuilt directory."""
    lib_filename = lib_config["lib_files"][platform]

    src_file_path_direct = os.path.join(extract_path, lib_filename)
    if os.path.exists(src_file_path_direct):
        src_file_path = src_file_path_direct
    else:
        extracted_items = os.listdir(extract_path)
        if len(extracted_items) == 1 and os.path.isdir(os.path.join(extract_path, extracted_items[0])):
            base_extracted_dir = os.path.join(extract_path, extracted_items[0])
            src_file_path_subdir = os.path.join(base_extracted_dir, lib_filename)
            if os.path.exists(src_file_path_subdir):
                src_file_path = src_file_path_subdir
            else:
                for common_s_dir in ["lib", "bin"]:
                    candidate = os.path.join(base_extracted_dir, common_s_dir, lib_filename)
                    if os.path.exists(candidate):
                        src_file_path = candidate
                        break
                else:
                    src_file_path = None
        else:
            src_file_path = None

    if not src_file_path:
        print(f"Warning: {lib_filename} not found in standard paths, searching recursively in {extract_path}...")
        for root, _, files in os.walk(extract_path):
            if lib_filename in files:
                src_file_path = os.path.join(root, lib_filename)
                print(f"Found {lib_filename} at {src_file_path}")
                break

    if not src_file_path:
        print(f"Error: Library file {lib_filename} not found in {extract_path} for {lib_name} on {platform}.")
        return False

    dest_dir = os.path.join(PREBUILT_DIR, platform)
    dest_file_path = os.path.join(dest_dir, lib_filename)
    os.makedirs(dest_dir, exist_ok=True)
    shutil.copy2(src_file_path, dest_file_path)
    print(f"  Successfully copied {lib_filename} for {lib_name} ({platform})")
    return True


def main():
    manifest = load_manifest()
    if not manifest:
        return

    total_expected_files = 0
    successfully_copied_files = 0
    failed_downloads_or_copies = []

    try:
        all_releases = get_all_releases()
        if not all_releases:
            print("No releases found. Exiting.")
            return

        for lib_key, lib_config in LIBRARIES_CONFIG.items():
            print(f"\nProcessing library: {lib_key}...")

            target_version_str = manifest.get(lib_key)
            if not target_version_str:
                print(f"  Warning: '{lib_key}' not found in manifest. Skipping.")
                for platform_key in PLATFORM_TAGS.keys():
                    total_expected_files += 1
                    failed_downloads_or_copies.append((lib_key, platform_key, "Not in manifest"))
                continue

            print(f"  Target version from manifest: {target_version_str}")
            specific_lib_release = get_specific_release_by_version_tag(all_releases, lib_config["tag_prefix"], target_version_str)

            if not specific_lib_release:
                print(f"  Could not find release for {lib_key} version {target_version_str}. Skipping.")
                for platform_key in PLATFORM_TAGS.keys():
                    total_expected_files += 1
                    failed_downloads_or_copies.append((lib_key, platform_key, f"Release for version {target_version_str} not found"))
                continue

            lib_version = specific_lib_release["parsed_version"]

            for platform_key, platform_tag_value in PLATFORM_TAGS.items():
                total_expected_files += 1

                if lib_key == "sdl3_image" and platform_key == "macos":
                    expected_asset_name = f"SDL3_image-{lib_version}-macos-arm64.zip"
                else:
                    expected_asset_name = f"{lib_config['asset_lib_name']}-{lib_version}-{platform_tag_value}.zip"

                print(f"  Looking for asset: {expected_asset_name}")
                asset_url = find_asset_url(specific_lib_release, expected_asset_name)

                if not asset_url:
                    print(f"    Asset not found. Skipping.")
                    failed_downloads_or_copies.append((lib_key, platform_key, "Asset not found in release"))
                    continue

                try:
                    with tempfile.TemporaryDirectory() as tmpdir:
                        zip_path = os.path.join(tmpdir, expected_asset_name)
                        download_file(asset_url, zip_path)

                        extract_target_path = os.path.join(tmpdir, f"extracted_{lib_key}_{platform_key}_{lib_version}")
                        os.makedirs(extract_target_path, exist_ok=True)
                        extract_zip(zip_path, extract_target_path)

                        if copy_library_file(extract_target_path, lib_key, platform_key, lib_config):
                            successfully_copied_files += 1
                        else:
                            failed_downloads_or_copies.append((lib_key, platform_key, "Copy failed"))
                except Exception as e_inner:
                    print(f"    Error processing {lib_key} v{lib_version} ({platform_key}): {e_inner}")
                    failed_downloads_or_copies.append((lib_key, platform_key, f"Exception: {e_inner}"))

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
        print(f"Successfully copied:          {successfully_copied_files}")
        print(f"Failed to retrieve/copy:      {total_expected_files - successfully_copied_files}")
        if failed_downloads_or_copies:
            print("\nDetails of failures/skipped files:")
            for lib, plat, reason in failed_downloads_or_copies:
                print(f"  - {lib} ({plat}): {reason}")
        print("----------------------")


if __name__ == "__main__":
    main()

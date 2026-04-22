import platform
import subprocess
import sys

# The required version of the .NET SDK.
REQUIRED_DOTNET_VERSION = "9.0"


def get_dotnet_version():
    """Gets the version of the .NET SDK."""
    try:
        # The 'dotnet --version' command outputs the version of the SDK.
        result = subprocess.run(
            ["dotnet", "--version"],
            capture_output=True,
            text=True,
            check=True,
        )
        return result.stdout.strip()
    except FileNotFoundError:
        return None
    except subprocess.CalledProcessError as e:
        print(f"Error checking dotnet version: {e}", file=sys.stderr)
        return None


def main():
    """Verifies the dotnet environment."""
    installed_version = get_dotnet_version()

    if installed_version is None:
        print("Error: dotnet CLI not found.", file=sys.stderr)
        print(
            "Please install the .NET SDK and ensure 'dotnet' is in your PATH.",
            file=sys.stderr,
        )
        sys.exit(1)

    # We are only interested in the major and minor version numbers.
    # A version string might be '9.0.100-preview.5.24307.3'.
    # We want to check if it starts with '9.0'.
    if not installed_version.startswith(REQUIRED_DOTNET_VERSION):
        print(
            f"Error: Invalid dotnet version.",
            file=sys.stderr,
        )
        print(
            f"Expected: {REQUIRED_DOTNET_VERSION}.*, Found: {installed_version}",
            file=sys.stderr,
        )
        sys.exit(1)

    print(f"Dotnet version check passed ({installed_version}).")
    sys.exit(0)


if __name__ == "__main__":
    main()
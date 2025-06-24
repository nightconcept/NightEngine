{ pkgs, ... }:

let
  # Import nixpkgs-unstable to get access to the latest packages.
  unstable = import (pkgs.fetchTarball "https://github.com/NixOS/nixpkgs/archive/nixpkgs-unstable.tar.gz") {};
in
{
  # Enable C# language support.
  # This helps devenv integrate with tools like OmniSharp.
  languages.csharp = {
    enable = true;
    package = unstable.dotnet-sdk_9;
  };

  # Set environment variables required by .NET tools.
  env.DOTNET_ROOT = "${unstable.dotnet-sdk_9}";
}
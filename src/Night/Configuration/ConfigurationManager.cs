// <copyright file="ConfigurationManager.cs" company="Night Circle">
// zlib license
//
// Copyright (c) 2025 Danny Solivan, Night Circle
//
// This software is provided 'as-is', without any express or implied
// warranty. In no event will the authors be held liable for any damages
// arising from the use of this software.
//
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:
//
// 1. The origin of this software must not be misrepresented; you must not
//    claim that you wrote the original software. If you use this software
//    in a product, an acknowledgment in the product documentation would be
//    appreciated but is not required.
// 2. Altered source versions must be plainly marked as such, and must not be
//    misrepresented as being the original software.
// 3. This notice may not be removed or altered from any source distribution.
// </copyright>

using System;
using System.IO;
using System.Text.Json;

using Night;
using Night.Log;

namespace Night
{
  /// <summary>
  /// Manages the loading and accessing of game configuration settings.
  /// </summary>
  public static class ConfigurationManager
  {
    private static readonly ILogger Logger = LogManager.GetLogger("Night.Configuration.ConfigurationManager");
    private static readonly string ConfigFileName = "config.json";
    private static GameConfig currentConfig = new GameConfig();
    private static bool isLoaded = false;

    /// <summary>
    /// Gets the currently loaded game configuration. If no configuration has been loaded, returns the default configuration.
    /// </summary>
    public static GameConfig CurrentConfig => currentConfig;

    /// <summary>
    /// Gets a value indicating whether the configuration has been loaded.
    /// </summary>
    public static bool IsLoaded => isLoaded;

    /// <summary>
    /// Loads the game configuration from a 'config.json' file.
    /// If the file is not found, or if an error occurs during loading or parsing, default settings are used.
    /// The configuration is loaded only once; subsequent calls will not reload the configuration.
    /// </summary>
    /// <param name="gameDirectory">The directory to search for 'config.json'. If null, the application's base directory is used.</param>
    public static void LoadConfig(string? gameDirectory = null)
    {
      if (isLoaded)
      {
        return;
      }

      string effectiveGameDirectory = gameDirectory ?? AppContext.BaseDirectory;
      string configFilePath = Path.Combine(effectiveGameDirectory, ConfigFileName);

      if (File.Exists(configFilePath))
      {
        try
        {
          string jsonContent = File.ReadAllText(configFilePath);
          if (!string.IsNullOrEmpty(jsonContent))
          {
            var options = new JsonSerializerOptions
            {
              PropertyNameCaseInsensitive = true,
              AllowTrailingCommas = true,
              ReadCommentHandling = JsonCommentHandling.Skip,
            };
            GameConfig? loadedConfig = JsonSerializer.Deserialize<GameConfig>(jsonContent, options);
            if (loadedConfig != null)
            {
              currentConfig = loadedConfig;
            }
            else
            {
              Logger.Warn($"Could not parse '{ConfigFileName}' from '{configFilePath}'. Using default configuration.");
            }
          }
          else
          {
            Logger.Warn($"'{ConfigFileName}' found at '{configFilePath}' is empty. Using default configuration.");
          }
        }
        catch (JsonException jsonEx)
        {
          Logger.Error($"Error deserializing '{ConfigFileName}' from '{configFilePath}'. Using default configuration.", jsonEx);
        }

        // Catch-all for other potential issues
        catch (Exception ex)
        {
          Logger.Error($"Error loading or deserializing config.json. Using default configuration.", ex);
        }
      }
      else
      {
        Logger.Info($"'{ConfigFileName}' not found at '{configFilePath}'. Using default configuration.");
      }

      isLoaded = true;
    }
  }
}

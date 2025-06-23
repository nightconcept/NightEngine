// <copyright file="FileData.cs" company="Night Circle">
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
using System.Text;

namespace Night;

/// <summary>
/// Data representing the contents of a file. It can be created from a string or byte array
/// and is used to pass around file contents in memory. This is analogous to Love2D's
/// <c>love.filesystem.FileData</c>.
/// </summary>
public class FileData
{
  private readonly byte[] data;
  private readonly string filenameHint;

  /// <summary>
  /// Initializes a new instance of the <see cref="FileData"/> class from a byte array.
  /// </summary>
  /// <param name="data">The byte array containing the file data.</param>
  /// <param name="filenameHint">A hint for the original filename, used for context (e.g., determining file extension).</param>
  /// <exception cref="ArgumentNullException">Thrown if data is null.</exception>
  public FileData(byte[] data, string filenameHint = "data")
  {
    this.data = data ?? throw new ArgumentNullException(nameof(data));
    this.filenameHint = filenameHint;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="FileData"/> class from a string.
  /// The string will be encoded using UTF-8.
  /// </summary>
  /// <param name="content">The string content of the file.</param>
  /// <param name="filenameHint">A hint for the original filename, used for context (e.g., determining file extension).</param>
  /// <exception cref="ArgumentNullException">Thrown if content is null.</exception>
  public FileData(string content, string filenameHint = "data.txt")
  {
    if (content == null)
    {
      throw new ArgumentNullException(nameof(content));
    }

    this.data = Encoding.UTF8.GetBytes(content);
    this.filenameHint = filenameHint;
  }

  /// <summary>
  /// Gets the contents of the FileData as a byte array.
  /// </summary>
  /// <returns>A new byte array containing the file's data.</returns>
  public byte[] GetBytes() => (byte[])this.data.Clone();

  /// <summary>
  /// Gets the contents of the FileData as a string, decoded using UTF-8.
  /// </summary>
  /// <returns>The file's data as a string.</returns>
  public string GetString() => Encoding.UTF8.GetString(this.data);

  /// <summary>
  /// Gets the size of the FileData in bytes.
  /// </summary>
  /// <returns>The size of the data in bytes.</returns>
  public long GetSize() => this.data.Length;

  /// <summary>
  /// Gets the filename hint associated with this FileData.
  /// </summary>
  /// <returns>The filename hint.</returns>
  public string GetFilenameHint() => this.filenameHint;

  /// <summary>
  /// Gets the file extension from the filename hint.
  /// </summary>
  /// <returns>The file extension (including the period), or an empty string if there is no extension.</returns>
  public string GetExtension() => Path.GetExtension(this.filenameHint);
}

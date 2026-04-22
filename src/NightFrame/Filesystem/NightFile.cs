// <copyright file="NightFile.cs" company="Night Circle">
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

using SysIO = System.IO;

namespace Night
{
  /// <summary>
  /// Represents a file in the Night framework, providing methods for file operations.
  /// This class is analogous to LÖVE's File object.
  /// </summary>
  public class NightFile : IDisposable
  {
    private readonly string filename;
    private FileStream? fileStream;
    private Night.FileMode? currentMode;
    private bool disposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="NightFile"/> class.
    /// The file is not opened by this constructor. It must be opened with <see cref="Open(Night.FileMode)"/> or <see cref="Open(string)"/>.
    /// </summary>
    /// <param name="filename">The name (and path) of the file.</param>
    /// <exception cref="ArgumentNullException">Thrown if filename is null or empty.</exception>
    public NightFile(string filename)
    {
      if (string.IsNullOrEmpty(filename))
      {
        throw new ArgumentNullException(nameof(filename), "Filename cannot be null or empty.");
      }

      this.filename = filename;
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="NightFile"/> class.
    /// </summary>
    ~NightFile()
    {
      this.Dispose(false);
    }

    /// <summary>
    /// Gets the filename of the file.
    /// </summary>
    public string Filename => this.filename;

    /// <summary>
    /// Gets a value indicating whether the file is currently open.
    /// </summary>
    public bool IsOpen => this.fileStream != null && this.CanRead_Workaround(); // CanRead can throw if closed

    /// <summary>
    /// Opens the file in the specified mode.
    /// </summary>
    /// <param name="mode">The mode to open the file in.</param>
    /// <returns>A tuple containing a boolean indicating success and an error message if an error occurred.</returns>
    public (bool Success, string? Error) Open(Night.FileMode mode)
    {
      if (this.disposed)
      {
        return (false, "Cannot open a disposed file.");
      }

      if (this.IsOpen)
      {
        return (false, "File is already open.");
      }

      try
      {
        SysIO.FileMode sysFileMode;
        SysIO.FileAccess sysFileAccess;

        switch (mode)
        {
          case Night.FileMode.Read:
            sysFileMode = SysIO.FileMode.Open;
            sysFileAccess = SysIO.FileAccess.Read;
            break;
          case Night.FileMode.Write:
            sysFileMode = SysIO.FileMode.Create; // Creates a new file or overwrites an existing file.
            sysFileAccess = SysIO.FileAccess.Write;
            break;
          case Night.FileMode.Append:
            sysFileMode = SysIO.FileMode.Append; // Opens the file if it exists and seeks to the end, or creates a new file.
            sysFileAccess = SysIO.FileAccess.Write;
            break;
          default:
            return (false, "Invalid file mode specified.");
        }

        this.fileStream = new SysIO.FileStream(this.filename, sysFileMode, sysFileAccess);
        this.currentMode = mode;
        return (true, null);
      }
      catch (Exception ex)
      {
        return (false, ex.Message);
      }
    }

    /// <summary>
    /// Opens the file in the specified mode string (LÖVE-style).
    /// </summary>
    /// <param name="modeString">The mode string ("r", "w", "a", "rb", "wb", "ab").</param>
    /// <returns>A tuple containing a boolean indicating success and an error message if an error occurred.</returns>
    public (bool Success, string? Error) Open(string modeString)
    {
      FileMode? mode;
      switch (modeString)
      {
        case "r":
        case "rb": // Binary distinction is handled by read methods in .NET
          mode = Night.FileMode.Read;
          break;
        case "w":
        case "wb":
          mode = Night.FileMode.Write;
          break;
        case "a":
        case "ab":
          mode = Night.FileMode.Append;
          break;
        default:
          return (false, $"Invalid file mode string: {modeString}");
      }

      return this.Open(mode.Value);
    }

    /// <summary>
    /// Reads the entire content of the file as a string (UTF-8 encoded).
    /// </summary>
    /// <returns>A tuple containing the file content as a string and an error message if an error occurred.</returns>
    public (string? Data, string? Error) Read()
    {
      if (!this.IsOpen || this.currentMode != Night.FileMode.Read)
      {
        return (null, "File is not open for reading.");
      }

      if (this.fileStream == null || !this.fileStream.CanRead)
      {
        return (null, "File stream cannot be read.");
      }

      try
      {
        // LÖVE's file:read() reads from current position to end if no size specified.
        // Ensure stream is at the beginning if it's a fresh read, or respect current position.
        // For simplicity here, we read from current position.
        // If a full re-read is desired after partial reads, Seek(0, SeekOrigin.Begin) would be needed.
        using (var reader = new StreamReader(this.fileStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: -1, leaveOpen: true))
        {
          string content = reader.ReadToEnd();
          return (content, null);
        }
      }
      catch (Exception ex)
      {
        return (null, ex.Message);
      }
    }

    /// <summary>
    /// Reads a specified number of bytes from the file.
    /// </summary>
    /// <param name="bytesToRead">The number of bytes to read.</param>
    /// <returns>A tuple containing the byte array and an error message if an error occurred.</returns>
    public (byte[]? Data, string? Error) ReadBytes(long bytesToRead)
    {
      if (!this.IsOpen || this.currentMode != Night.FileMode.Read)
      {
        return (null, "File is not open for reading.");
      }

      if (this.fileStream == null || !this.fileStream.CanRead)
      {
        return (null, "File stream cannot be read.");
      }

      if (bytesToRead <= 0)
      {
        return (Array.Empty<byte>(), null); // LÖVE returns empty string for 0 or negative
      }

      try
      {
        // Determine how many bytes can actually be read (up to bytesToRead or end of stream)
        long remainingBytes = this.fileStream.Length - this.fileStream.Position;
        int actualBytesToRead = (int)Math.Min(bytesToRead, remainingBytes);
        if (actualBytesToRead <= 0)
        {
          return (Array.Empty<byte>(), null);
        }

        byte[] buffer = new byte[actualBytesToRead];
        int bytesRead = this.fileStream.Read(buffer, 0, actualBytesToRead);

        // If bytesRead is less than actualBytesToRead, it means end of stream was reached earlier than expected.
        // This is fine, just return what was read. If bytesRead is 0, it means we are at EOF.
        if (bytesRead < actualBytesToRead)
        {
          Array.Resize(ref buffer, bytesRead);
        }

        return (buffer, null);
      }
      catch (Exception ex)
      {
        return (null, ex.Message);
      }
    }

    /// <summary>
    /// Reads all remaining bytes from the current position in the file.
    /// </summary>
    /// <returns>A tuple containing the byte array and an error message if an error occurred.</returns>
    public (byte[]? Data, string? Error) ReadBytes()
    {
      if (!this.IsOpen || this.currentMode != Night.FileMode.Read)
      {
        return (null, "File is not open for reading.");
      }

      if (this.fileStream == null || !this.fileStream.CanRead)
      {
        return (null, "File stream cannot be read.");
      }

      try
      {
        long remainingBytes = this.fileStream.Length - this.fileStream.Position;
        if (remainingBytes <= 0)
        {
          return (Array.Empty<byte>(), null);
        }

        // Ensure remainingBytes fits into an int for the byte array size
        if (remainingBytes > int.MaxValue)
        {
          return (null, "Cannot read remaining bytes: file size exceeds maximum array length.");
        }

        byte[] buffer = new byte[(int)remainingBytes];
        int offset = 0;
        int count = (int)remainingBytes;
        int bytesReadTotal = 0;

        while (count > 0)
        {
          int bytesRead = this.fileStream.Read(buffer, offset, count);
          if (bytesRead == 0)
          {
            break; // End of file reached
          }

          bytesReadTotal += bytesRead;
          offset += bytesRead;
          count -= bytesRead;
        }

        if (bytesReadTotal < (int)remainingBytes)
        {
          Array.Resize(ref buffer, bytesReadTotal);
        }

        return (buffer, null);
      }
      catch (Exception ex)
      {
        return (null, ex.Message);
      }
    }

    /// <summary>
    /// Closes the file.
    /// </summary>
    /// <returns>A tuple containing a boolean indicating success and an error message if an error occurred.</returns>
    public (bool Success, string? Error) Close()
    {
      if (this.disposed)
      {
        // LÖVE allows closing a closed file without error.
        // However, if it's disposed, it's a more terminal state.
        // For consistency with LÖVE, perhaps just return true if already closed/disposed.
        // Let's stick to returning true if not open.
        return (true, null);
      }

      if (!this.IsOpen)
      {
        return (true, null); // Already closed or never opened
      }

      string? errorMessage = null;
      bool success = true;
      try
      {
        // If IsOpen is true, fileStream should not be null due to the IsOpen check.
        this.fileStream!.Flush(); // Explicitly flush before closing.
        this.fileStream!.Close(); // Close also disposes the FileStream.
      }
      catch (Exception ex)
      {
        errorMessage = ex.Message;
        success = false;
      }
      finally
      {
        this.fileStream = null;
        this.currentMode = null;
      }

      return (success, errorMessage);
    }

    /// <summary>
    /// Releases all resources used by the <see cref="NightFile"/> object.
    /// </summary>
    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (!this.disposed)
      {
        if (disposing)
        {
          // Dispose managed state (managed objects).
          if (this.fileStream != null)
          {
            this.fileStream.Dispose();
            this.fileStream = null;
          }
        }

        // Free unmanaged resources (unmanaged objects) and override a finalizer below.
        // Set large fields to null.
        this.disposed = true;
      }
    }

    // Workaround for FileStream.CanRead throwing ObjectDisposedException
    private bool CanRead_Workaround()
    {
      try
      {
        return this.fileStream?.CanRead ?? false;
      }
      catch (ObjectDisposedException)
      {
        return false;
      }
    }
  }
}

// <copyright file="Graphics.cs" company="Night Circle">
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
using System.Runtime.InteropServices;

using Night;
using Night.Log;

using SDL3;

namespace Night
{
  /// <summary>
  /// Provides 2D graphics rendering functionality.
  /// </summary>
  public static partial class Graphics
  {
    private static readonly ILogger Logger = LogManager.GetLogger("Night.Graphics.Graphics");

    /// <summary>Loads an image file and creates a new Sprite.</summary>
    /// <param name="filePath">Path to the image file.</param>
    /// <returns>A new Sprite or null if loading fails.</returns>
    public static Sprite? NewImage(string filePath)
    {
      IntPtr rendererPtr = Window.RendererPtr;
      if (rendererPtr == IntPtr.Zero)
      {
        Logger.Error("Renderer pointer is null. Was Window.SetMode called successfully?");
        return null;
      }

      if (!File.Exists(filePath))
      {
        Logger.Error($"Image file not found at '{filePath}'.");
        return null;
      }

      IntPtr surfacePtr = SDL3.Image.Load(filePath);

      if (surfacePtr == IntPtr.Zero)
      {
        string sdlError = SDL.GetError();
        Logger.Error($"Failed to load image into surface from '{filePath}'. SDL_image Error: {sdlError}");
        return null;
      }

      SDL.Surface surface = Marshal.PtrToStructure<SDL.Surface>(surfacePtr);
      int width = surface.Width;
      int height = surface.Height;

      if (width <= 0 || height <= 0)
      {
        Logger.Error($"Invalid surface dimensions ({width}x{height}) for '{filePath}'.");
        SDL.DestroySurface(surfacePtr);
        return null;
      }

      IntPtr texturePtr = SDL.CreateTextureFromSurface(rendererPtr, surfacePtr);
      SDL.DestroySurface(surfacePtr);

      if (texturePtr == IntPtr.Zero)
      {
        string sdlError = SDL.GetError();
        Logger.Error($"Failed to create texture from surface for '{filePath}'. SDL Error: {sdlError}");
        return null;
      }

      return new Sprite(texturePtr, width, height);
    }

    /// <summary>Sets the current drawing color.</summary>
    /// <param name="color">The color to use for subsequent drawing operations.</param>
    public static void SetColor(Color color)
    {
      IntPtr rendererPtr = Window.RendererPtr;
      if (rendererPtr == IntPtr.Zero)
      {
        Logger.Error("Renderer pointer is null. Was Window.SetMode called successfully?");
        return;
      }

      if (!SDL.SetRenderDrawColor(rendererPtr, color.R, color.G, color.B, color.A))
      {
        string sdlError = SDL.GetError();
        Logger.Error($"SetRenderDrawColor failed: {sdlError}");
      }
    }

    /// <summary>Sets the current drawing color using RGBA components.</summary>
    /// <param name="r">Red component (0-255).</param>
    /// <param name="g">Green component (0-255).</param>
    /// <param name="b">Blue component (0-255).</param>
    /// <param name="a">Alpha/transparency (0-255, default 255 = opaque).</param>
    public static void SetColor(byte r, byte g, byte b, byte a = 255)
    {
      SetColor(new Color(r, g, b, a));
    }

    /// <summary>Draws a rectangle.</summary>
    /// <param name="mode">Fill or outline the rectangle.</param>
    /// <param name="x">Left position.</param>
    /// <param name="y">Top position.</param>
    /// <param name="width">Rectangle width.</param>
    /// <param name="height">Rectangle height.</param>
    public static void Rectangle(DrawMode mode, float x, float y, float width, float height)
    {
      IntPtr rendererPtr = Window.RendererPtr;
      if (rendererPtr == IntPtr.Zero)
      {
        Logger.Error("Renderer pointer is null. Was Window.SetMode called successfully?");
        return;
      }

      if (width <= 0 || height <= 0)
      {
        // No-op for zero or negative dimensions
        return;
      }

      bool success;
      if (mode == DrawMode.Fill)
      {
        // Define the 4 vertices of the rectangle
        float[] xy = new float[]
        {
            x, y,                        // Top-left
            x + width, y,                // Top-right
            x + width, y + height,       // Bottom-right
            x, y + height, // Bottom-left
        };

        SDL.FColor[] vertexColors = new SDL.FColor[4];
        byte r_byte, g_byte, b_byte, a_byte;
        _ = SDL.GetRenderDrawColor(rendererPtr, out r_byte, out g_byte, out b_byte, out a_byte);
        SDL.FColor drawColor = new SDL.FColor { R = r_byte / 255f, G = g_byte / 255f, B = b_byte / 255f, A = a_byte / 255f };
        for (int i = 0; i < 4; i++)
        {
          vertexColors[i] = drawColor;
        }

        // Indices for two triangles forming the rectangle (0,1,2 and 0,2,3)
        // SDL.RenderGeometryRaw expects indices, typically byte or short.
        // Using byte here as polygon fill does, but ensure it's appropriate for SDL3's expectations.
        // For a quad, it's 2 triangles, 6 indices.
        byte[] indices = new byte[] { 0, 1, 2, 0, 2, 3 }; // Triangle 1: (v0,v1,v2), Triangle 2: (v0,v2,v3)

        GCHandle xyHandle = default;
        GCHandle colorsHandle = default;
        GCHandle indicesHandle = default;

        try
        {
          xyHandle = GCHandle.Alloc(xy, GCHandleType.Pinned);
          colorsHandle = GCHandle.Alloc(vertexColors, GCHandleType.Pinned);
          indicesHandle = GCHandle.Alloc(indices, GCHandleType.Pinned);

          IntPtr xyPtr = xyHandle.AddrOfPinnedObject();
          IntPtr colorsPtr = colorsHandle.AddrOfPinnedObject();
          IntPtr indicesPtr = indicesHandle.AddrOfPinnedObject();

          success = SDL.RenderGeometryRaw(
                                     rendererPtr,
                                     IntPtr.Zero, // No texture
                                     xyPtr,
                                     sizeof(float) * 2, // Stride for xy
                                     colorsPtr,
                                     Marshal.SizeOf<SDL.FColor>(), // Stride for colors
                                     IntPtr.Zero, // No UVs
                                     0,           // Stride for UVs
                                     4,           // Number of vertices
                                     indicesPtr,
                                     indices.Length, // Number of indices
                                     sizeof(byte));  // Size of each index
        }
        finally
        {
          if (xyHandle.IsAllocated)
          {
            xyHandle.Free();
          }

          if (colorsHandle.IsAllocated)
          {
            colorsHandle.Free();
          }

          if (indicesHandle.IsAllocated)
          {
            indicesHandle.Free();
          }
        }
      }

      // DrawMode.Line
      else
      {
        SDL.FRect rect = new SDL.FRect { X = x, Y = y, W = width, H = height };
        success = SDL.RenderRect(rendererPtr, rect);
      }

      if (!success)
      {
        string sdlError = SDL.GetError();
        Logger.Error($"Rectangle rendering failed (Mode: {mode}): {sdlError}");
      }
    }

    /// <summary>
    /// Draws a line segment between two points.
    /// </summary>
    /// <param name="x1">The x-coordinate of the first point.</param>
    /// <param name="y1">The y-coordinate of the first point.</param>
    /// <param name="x2">The x-coordinate of the second point.</param>
    /// <param name="y2">The y-coordinate of the second point.</param>
    public static void Line(float x1, float y1, float x2, float y2)
    {
      IntPtr rendererPtr = Window.RendererPtr;
      if (rendererPtr == IntPtr.Zero)
      {
        Logger.Error("Renderer pointer is null. Was Window.SetMode called successfully?");
        return;
      }

      if (!SDL.RenderLine(rendererPtr, x1, y1, x2, y2))
      {
        string sdlError = SDL.GetError();
        Logger.Error($"Line rendering failed: {sdlError}");
      }
    }

    /// <summary>
    /// Draws a sequence of connected line segments.
    /// </summary>
    /// <param name="points">An array of points to connect.</param>
    public static void Line(PointF[] points)
    {
      IntPtr rendererPtr = Window.RendererPtr;
      if (rendererPtr == IntPtr.Zero)
      {
        Logger.Error("Renderer pointer is null. Was Window.SetMode called successfully?");
        return;
      }

      if (points == null || points.Length < 2)
      {
        Logger.Error("At least two points are required to draw lines.");
        return;
      }

      SDL.FPoint[] sdlPoints = new SDL.FPoint[points.Length];
      for (int i = 0; i < points.Length; i++)
      {
        sdlPoints[i] = new SDL.FPoint { X = points[i].X, Y = points[i].Y };
      }

      if (!SDL.RenderLines(rendererPtr, sdlPoints, sdlPoints.Length))
      {
        string sdlError = SDL.GetError();
        Logger.Error($"Multiple points line rendering failed: {sdlError}");
      }
    }

    /// <summary>
    /// Draws a polygon.
    /// </summary>
    /// <param name="mode">The drawing mode (Fill or Line).</param>
    /// <param name="vertices">An array of points representing the polygon's vertices.</param>
    public static void Polygon(DrawMode mode, PointF[] vertices)
    {
      IntPtr rendererPtr = Window.RendererPtr;
      if (rendererPtr == IntPtr.Zero)
      {
        Logger.Error("Renderer pointer is null. Was Window.SetMode called successfully?");
        return;
      }

      if (vertices == null || vertices.Length < 3)
      {
        Logger.Error("At least three vertices are required to draw a polygon.");
        return;
      }

      if (mode == DrawMode.Line)
      {
        SDL.FPoint[] lineVertices = new SDL.FPoint[vertices.Length + 1];
        for (int i = 0; i < vertices.Length; i++)
        {
          lineVertices[i] = new SDL.FPoint { X = vertices[i].X, Y = vertices[i].Y };
        }

        // Close the polygon
        lineVertices[vertices.Length] = new SDL.FPoint { X = vertices[0].X, Y = vertices[0].Y };

        if (!SDL.RenderLines(rendererPtr, lineVertices, lineVertices.Length))
        {
          string sdlError = SDL.GetError();
          Logger.Error($"Polygon rendering failed (Line Mode): {sdlError}");
        }
      }
      else
      {
        if (vertices.Length < 3)
        {
          return;
        }

        float[] xy = new float[vertices.Length * 2];
        SDL.FColor[] vertexColors = new SDL.FColor[vertices.Length];

        byte r, g, b, a;
        _ = SDL.GetRenderDrawColor(rendererPtr, out r, out g, out b, out a);
        SDL.FColor drawColor = new SDL.FColor { R = r / 255f, G = g / 255f, B = b / 255f, A = a / 255f };

        for (int i = 0; i < vertices.Length; i++)
        {
          xy[i * 2] = vertices[i].X;
          xy[(i * 2) + 1] = vertices[i].Y;
          vertexColors[i] = drawColor;
        }

        byte[] indices = new byte[(vertices.Length - 2) * 3];
        for (int i = 0; i < vertices.Length - 2; i++)
        {
          indices[i * 3] = 0;
          indices[(i * 3) + 1] = (byte)(i + 1);
          indices[(i * 3) + 2] = (byte)(i + 2);
        }

        GCHandle xyHandle = default;
        GCHandle colorsHandle = default;
        GCHandle indicesHandle = default;

        try
        {
          xyHandle = GCHandle.Alloc(xy, GCHandleType.Pinned);
          colorsHandle = GCHandle.Alloc(vertexColors, GCHandleType.Pinned);
          indicesHandle = GCHandle.Alloc(indices, GCHandleType.Pinned);

          IntPtr xyPtr = xyHandle.AddrOfPinnedObject();
          IntPtr colorsPtr = colorsHandle.AddrOfPinnedObject();
          IntPtr indicesPtr = indicesHandle.AddrOfPinnedObject();

          if (!SDL.RenderGeometryRaw(
                                     rendererPtr,
                                     IntPtr.Zero,
                                     xyPtr,
                                     sizeof(float) * 2,
                                     colorsPtr,
                                     Marshal.SizeOf<SDL.FColor>(),
                                     IntPtr.Zero,
                                     0,
                                     vertices.Length,
                                     indicesPtr,
                                     indices.Length,
                                     sizeof(byte)))
          {
            string sdlError = SDL.GetError();
            Logger.Error($"Polygon rendering failed (Fill Mode - RenderGeometryRaw): {sdlError}");
          }
        }
        finally
        {
          if (xyHandle.IsAllocated)
          {
            xyHandle.Free();
          }

          if (colorsHandle.IsAllocated)
          {
            colorsHandle.Free();
          }

          if (indicesHandle.IsAllocated)
          {
            indicesHandle.Free();
          }
        }
      }
    }

    /// <summary>
    /// Draws a circle.
    /// </summary>
    /// <param name="mode">The drawing mode (Fill or Line).</param>
    /// <param name="x">The x-coordinate of the circle's center.</param>
    /// <param name="y">The y-coordinate of the circle's center.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="segments">The number of segments used to draw the circle (more segments means a smoother circle).</param>
    public static void Circle(
      DrawMode mode,
      float x,
      float y,
      float radius,
      int segments = 12)
    {
      IntPtr rendererPtr = Window.RendererPtr;
      if (rendererPtr == IntPtr.Zero)
      {
        Logger.Error("Renderer pointer is null. Was Window.SetMode called successfully?");
        return;
      }

      if (segments <= 0)
      {
        // Default to 12 segments if an invalid number is provided.
        segments = 12;
      }

      if (radius < 0)
      {
        radius = 0;
      }

      // An array of FPoints to hold the vertices of the circle
      SDL.FPoint[] points = new SDL.FPoint[segments + 1];

      for (int i = 0; i <= segments; i++)
      {
        double angle = (Math.PI * 2.0 * i) / segments;
        points[i].X = x + (float)(Math.Cos(angle) * radius);
        points[i].Y = y + (float)(Math.Sin(angle) * radius);
      }

      bool success;
      if (mode == DrawMode.Fill)
      {
        // For filling, we need to convert points to vertices and provide indices if using RenderGeometry.
        // SDL_RenderGeometry does not directly support filled circles with a simple point list.
        // A common approach is to create a triangle fan.
        // However, SDL_gfx or a custom triangle rasterizer would be better for perfect filled circles.
        // For simplicity, we will draw many lines for a "filled" effect if segments is high enough,
        // or just outline if not DrawMode.Line.
        // This is not a true fill for convex polygons but works for circles.
        // A more robust solution would involve SDL_gfx.filledCircleRGBA or custom geometry rendering.
        // SDL3 provides SDL_RenderFillPolygon, which is EXPERIMENTAL for now.
        // As a fallback, let's draw it as connected lines (outline) even for Fill mode for now,
        // as a true fill is complex without SDL_gfx or a geometry helper.
        // TODO: Implement a proper fill for circles.
        success = SDL.RenderLines(rendererPtr, points, segments + 1);
      }
      else
      {
        success = SDL.RenderLines(rendererPtr, points, segments + 1);
      }

      if (!success)
      {
        string sdlError = SDL.GetError();
        Logger.Error($"Circle rendering failed (Mode: {mode}): {sdlError}");
      }
    }

    /// <summary>
    /// Draws a sprite to the screen.
    /// </summary>
    /// <param name="sprite">The sprite to draw.</param>
    /// <param name="x">The x-coordinate to draw the sprite at.</param>
    /// <param name="y">The y-coordinate to draw the sprite at.</param>
    /// <param name="rotation">The rotation of the sprite (in radians).</param>
    /// <param name="scaleX">The horizontal scale factor.</param>
    /// <param name="scaleY">The vertical scale factor.</param>
    /// <param name="offsetX">The x-offset for the sprite's origin.</param>
    /// <param name="offsetY">The y-offset for the sprite's origin.</param>
    public static void Draw(
        Sprite sprite,
        float x,
        float y,
        float rotation = 0,
        float scaleX = 1,
        float scaleY = 1,
        float offsetX = 0,
        float offsetY = 0)
    {
      // Check if sprite is null
      if (sprite == null)
      {
        return;
      }

      if (sprite.Texture == IntPtr.Zero)
      {
        Logger.Error("Sprite or sprite texture is null.");
        return;
      }

      IntPtr rendererPtr = Window.RendererPtr;
      if (rendererPtr == IntPtr.Zero)
      {
        Logger.Error("Renderer pointer is null. Was Window.SetMode called successfully?");
        return;
      }

      SDL.FRect dstRectStruct = new SDL.FRect
      {
        X = x,
        Y = y,
        W = sprite.Width * scaleX,
        H = sprite.Height * scaleY,
      };

      SDL.FPoint centerPointStruct = new SDL.FPoint
      {
        X = offsetX * scaleX,
        Y = offsetY * scaleY,
      };

      double angleInDegrees = rotation * (180.0 / Math.PI);

      IntPtr dstRectPtr = IntPtr.Zero;
      IntPtr centerPointPtr = IntPtr.Zero;

      try
      {
        dstRectPtr = Marshal.AllocHGlobal(Marshal.SizeOf(dstRectStruct));
        Marshal.StructureToPtr(dstRectStruct, dstRectPtr, false);

        centerPointPtr = Marshal.AllocHGlobal(Marshal.SizeOf(centerPointStruct));
        Marshal.StructureToPtr(centerPointStruct, centerPointPtr, false);

        if (!SDL.RenderTextureRotated(rendererPtr, sprite.Texture, IntPtr.Zero, dstRectPtr, angleInDegrees, centerPointPtr, SDL.FlipMode.None))
        {
          string sdlError = SDL.GetError();
          Logger.Error($"RenderTextureRotated failed: {sdlError}");
        }
      }
      finally
      {
        if (dstRectPtr != IntPtr.Zero)
        {
          Marshal.FreeHGlobal(dstRectPtr);
        }

        if (centerPointPtr != IntPtr.Zero)
        {
          Marshal.FreeHGlobal(centerPointPtr);
        }
      }
    }

    /// <summary>
    /// Clears the screen to a specific color.
    /// </summary>
    /// <param name="color">The color to clear the screen with.</param>
    public static void Clear(Color color)
    {
      IntPtr rendererPtr = Window.RendererPtr;
      if (rendererPtr == IntPtr.Zero)
      {
        Logger.Error("Renderer pointer is null. Was Window.SetMode called successfully?");
        return;
      }

      backgroundColor = color; // Store the new background color

      // Set color for clearing
      if (!SDL.SetRenderDrawColor(rendererPtr, color.R, color.G, color.B, color.A))
      {
        string sdlError = SDL.GetError();
        Logger.Error($"SetRenderDrawColor failed: {sdlError}");
        return; // Return if color setting fails, to avoid clearing with wrong color
      }

      if (!SDL.RenderClear(rendererPtr))
      {
        string sdlError = SDL.GetError();
        Logger.Error($"RenderClear failed: {sdlError}");
      }
    }

    /// <summary>
    /// Presents the drawn graphics to the screen (swaps buffers).
    /// </summary>
    public static void Present()
    {
      IntPtr rendererPtr = Window.RendererPtr;
      if (rendererPtr == IntPtr.Zero)
      {
        Logger.Error("Renderer pointer is null. Was Window.SetMode called successfully?");
        return;
      }

      _ = SDL.RenderPresent(Window.RendererPtr);
    }
  }
}

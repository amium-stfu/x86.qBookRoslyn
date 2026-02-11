using System;
using System.Buffers;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace qbookCode.Controls
{

    internal static class DwmTitleBar
    {
        // DWMWINDOWATTRIBUTE Werte (Windows 11+)
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20; // (19 bei älteren Win10-Insider Builds)
        private const int DWMWA_BORDER_COLOR = 34;
        private const int DWMWA_CAPTION_COLOR = 35;
        private const int DWMWA_TEXT_COLOR = 36;


        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);


        public static bool IsWindows11OrNewer()
        {
            // Windows 11: Build >= 22000
            try { return Environment.OSVersion.Version.Build >= 22000; } catch { return false; }
        }


        public static bool SetImmersiveDarkMode(IntPtr hwnd, bool enabled)
        {
            int v = enabled ? 1 : 0;
            int hr = DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref v, sizeof(int));
            return hr >= 0; // S_OK = 0; negative = Fehler
        }


        public static bool SetCaptionColor(IntPtr hwnd, Color color)
        {
            // COLORREF (BGR) – ColorTranslator.ToWin32 liefert korrektes Format
            int colorRef = ColorTranslator.ToWin32(color);
            int hr = DwmSetWindowAttribute(hwnd, DWMWA_CAPTION_COLOR, ref colorRef, sizeof(int));
            return hr >= 0;
        }


        public static bool SetTextColor(IntPtr hwnd, Color color)
        {
            int colorRef = ColorTranslator.ToWin32(color);
            int hr = DwmSetWindowAttribute(hwnd, DWMWA_TEXT_COLOR, ref colorRef, sizeof(int));
            return hr >= 0;
        }


        public static bool SetBorderColor(IntPtr hwnd, Color color)
        {
            int colorRef = ColorTranslator.ToWin32(color);
            int hr = DwmSetWindowAttribute(hwnd, DWMWA_BORDER_COLOR, ref colorRef, sizeof(int));
            return hr >= 0;
        }
    }

    public static class BitmapTools
    {
        public static int Clamp(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public static float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }



        // =============== Public API ===============

        // 1) Farbe ersetzen (mit Toleranz in RGB-Euklidisch), Alpha bleibt erhalten
        public static Bitmap ReplaceColor(Bitmap input, Color from, Color to, int tolerance = 16)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            tolerance = Clamp(tolerance, 0, 255);
            int tol2 = tolerance * tolerance; // wir prüfen je Kanal separat kumuliert
            return Process(input, (ref byte r, ref byte g, ref byte b, ref byte a) =>
            {
                if (a == 0) return; // voll transparent ignorieren
                int dr = r - from.R, dg = g - from.G, db = b - from.B;
                if (dr * dr + dg * dg + db * db <= tol2)
                {
                    r = to.R; g = to.G; b = to.B;
                }
            });
        }

        // Komfort-Overload für Image (vermeidet Cast im Aufrufer)
        public static Bitmap ReplaceColor(Image input, Color from, Color to, int tolerance = 16)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            using var bmp = new Bitmap(input);
            // DPI des Originalbildes übernehmen
            try { bmp.SetResolution(input.HorizontalResolution, input.VerticalResolution); } catch { }
            return ReplaceColor(bmp, from, to, tolerance);
        }

        // 2) Tönung (Tint). amount 0..1 (0=Original, 1=volle Tönung)
        public static Bitmap Tint(Bitmap input, Color tint, float amount)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            amount = Clamp(amount, 0f, 1f);
            float tr = tint.R / 255f, tg = tint.G / 255f, tb = tint.B / 255f;
            return Process(input, (ref byte r, ref byte g, ref byte b, ref byte a) =>
            {
                if (a == 0) return;
                r = (byte)Math.Round(r * (1f - amount) + (r * tr) * amount);
                g = (byte)Math.Round(g * (1f - amount) + (g * tg) * amount);
                b = (byte)Math.Round(b * (1f - amount) + (b * tb) * amount);
            });
        }

        // 3) Recolor für (nahezu) monochrome Icons – Helligkeit bewahren
        public static Bitmap RecolorSolid(Bitmap input, Color target)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            return Process(input, (ref byte r, ref byte g, ref byte b, ref byte a) =>
            {
                if (a == 0) return;
                float lum = (0.2126f * r + 0.7152f * g + 0.0722f * b) / 255f;
                r = (byte)Math.Round(target.R * lum);
                g = (byte)Math.Round(target.G * lum);
                b = (byte)Math.Round(target.B * lum);
            });
        }

        // 4) Hue-Rotation in HSL (degrees: -180..+180)
        public static Bitmap HueRotate(Bitmap input, float degrees)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            return Process(input, (ref byte r, ref byte g, ref byte b, ref byte a) =>
            {
                if (a == 0) return;
                float h, s, l; RgbToHsl(r, g, b, out h, out s, out l);
                h = (h + degrees) % 360f; if (h < 0) h += 360f;
                HslToRgb(h, s, l, out r, out g, out b);
            });
        }

        // 5) Resize / Scale
        public static Bitmap ResizeExact(Bitmap input, int width, int height)
            => ResizeImpl(input ?? throw new ArgumentNullException(nameof(input)), width, height);

        public static Bitmap ResizeFit(Bitmap input, int maxWidth, int maxHeight)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (maxWidth <= 0 || maxHeight <= 0) throw new ArgumentOutOfRangeException();
            float sx = (float)maxWidth / input.Width;
            float sy = (float)maxHeight / input.Height;
            float s = Math.Min(sx, sy);
            if (s <= 0f) s = 1f;
            int w = Math.Max(1, (int)Math.Round(input.Width * s));
            int h = Math.Max(1, (int)Math.Round(input.Height * s));
            return ResizeImpl(input, w, h);
        }

        public static Bitmap ResizeScale(Bitmap input, float scale)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (scale <= 0f) throw new ArgumentOutOfRangeException(nameof(scale));
            int w = Math.Max(1, (int)Math.Round(input.Width * scale));
            int h = Math.Max(1, (int)Math.Round(input.Height * scale));
            return ResizeImpl(input, w, h);
        }

        // Füllend (Cover) mit optionalem Fokus (0..1) in X/Y (0=Start, 0.5=Mitte, 1=Ende)
        public static Bitmap ResizeCover(Bitmap input, int width, int height, float focusX = 0.5f, float focusY = 0.5f)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (width <= 0 || height <= 0) throw new ArgumentOutOfRangeException();
            focusX = Clamp(focusX, 0f, 1f);
            focusY = Clamp(focusY, 0f, 1f);

            using var src = Ensure32bppArgb(input);
            var dst = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
            try { dst.SetResolution(input.HorizontalResolution, input.VerticalResolution); } catch { }

            float s = Math.Max((float)width / src.Width, (float)height / src.Height); // cover = größerer Faktor
            int sw = (int)Math.Round(width / s);
            int sh = (int)Math.Round(height / s);
            int sx = (int)Math.Round((src.Width - sw) * focusX);
            int sy = (int)Math.Round((src.Height - sh) * focusY);
            sx = Clamp(sx, 0, Math.Max(0, src.Width - sw));
            sy = Clamp(sy, 0, Math.Max(0, src.Height - sh));

            using (var g = Graphics.FromImage(dst))
            using (var ia = new ImageAttributes())
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                ia.SetWrapMode(WrapMode.TileFlipXY);
                g.DrawImage(src,
                    new Rectangle(0, 0, width, height),
                    sx, sy, sw, sh,
                    GraphicsUnit.Pixel, ia);
            }
            return dst;
        }

        // 5.1) Gemeinsame Resize-Implementierung (HighQuality + Alpha + DPI)
        private static Bitmap ResizeImpl(Bitmap input, int width, int height)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (width <= 0 || height <= 0) throw new ArgumentOutOfRangeException();

            using var src = Ensure32bppArgb(input);
            var dst = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
            // DPI des Originals übernehmen, damit die sichtbare Größe stabil bleibt
            try
            {
                dst.SetResolution(input.HorizontalResolution, input.VerticalResolution);
            }
            catch { }

            using (var g = Graphics.FromImage(dst))
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(src, new Rectangle(0, 0, width, height));
            }
            return dst;
        }

        private static Bitmap Process(Bitmap input, PixelOp op)
        {
            using var src = Ensure32bppArgb(input);
            var bmp = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);
            // DPI beibehalten → sonst wirkt das Bild im UI skaliert
            try { bmp.SetResolution(src.HorizontalResolution, src.VerticalResolution); } catch { }

            using (var g = Graphics.FromImage(bmp))
                g.DrawImageUnscaled(src, 0, 0);

            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var data = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int length = Math.Abs(data.Stride) * data.Height;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(length);

            try
            {
                Marshal.Copy(data.Scan0, buffer, 0, length);
                var span = buffer.AsSpan(0, length);

                for (int y = 0; y < data.Height; y++)
                {
                    int row = y * Math.Abs(data.Stride);
                    for (int x = 0; x < data.Width; x++)
                    {
                        int i = row + x * 4;
                        ref byte b = ref span[i + 0];
                        ref byte gch = ref span[i + 1];
                        ref byte r = ref span[i + 2];
                        ref byte a = ref span[i + 3];
                        op(ref r, ref gch, ref b, ref a);
                    }
                }

                Marshal.Copy(buffer, 0, data.Scan0, length);
            }
            finally
            {
                bmp.UnlockBits(data);
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return bmp;
        }

        private delegate void PixelOp(ref byte r, ref byte g, ref byte b, ref byte a);

        private static Bitmap Ensure32bppArgb(Bitmap input)
        {
            if (input.PixelFormat == PixelFormat.Format32bppArgb)
            {
                var same = (Bitmap)input.Clone();
                try { same.SetResolution(input.HorizontalResolution, input.VerticalResolution); } catch { }
                return same;
            }

            var clone = new Bitmap(input.Width, input.Height, PixelFormat.Format32bppArgb);
            try { clone.SetResolution(input.HorizontalResolution, input.VerticalResolution); } catch { }
            using (var g = Graphics.FromImage(clone))
                g.DrawImage(input, new Rectangle(0, 0, input.Width, input.Height));
            return clone;
        }

        private static void RgbToHsl(byte r8, byte g8, byte b8, out float h, out float s, out float l)
        {
            float r = r8 / 255f, g = g8 / 255f, b = b8 / 255f;
            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            l = (max + min) / 2f;
            if (Math.Abs(max - min) < 1e-6f) { h = 0f; s = 0f; return; }
            float d = max - min;
            s = l > 0.5f ? d / (2f - max - min) : d / (max + min);
            if (max == r) h = (g - b) / d + (g < b ? 6f : 0f);
            else if (max == g) h = (b - r) / d + 2f;
            else h = (r - g) / d + 4f;
            h *= 60f;
        }

        private static void HslToRgb(float h, float s, float l, out byte r8, out byte g8, out byte b8)
        {
            float r, g, b;
            if (s <= 0f) { r = g = b = l; }
            else
            {
                float q = l < 0.5f ? l * (1f + s) : l + s - l * s;
                float p = 2f * l - q;
                float hk = h / 360f;
                float[] t = { hk + 1f / 3f, hk, hk - 1f / 3f };
                for (int i = 0; i < 3; i++) { if (t[i] < 0f) t[i] += 1f; if (t[i] > 1f) t[i] -= 1f; }
                r = HueToRgb(p, q, t[0]); g = HueToRgb(p, q, t[1]); b = HueToRgb(p, q, t[2]);
            }
            r8 = (byte)Math.Round(r * 255f); g8 = (byte)Math.Round(g * 255f); b8 = (byte)Math.Round(b * 255f);
        }

        private static float HueToRgb(float p, float q, float t)
        {
            if (t < 1f / 6f) return p + (q - p) * 6f * t;
            if (t < 1f / 2f) return q;
            if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
            return p;
        }
    }
}

// ===== Beispiel: Buttons neu einfärben (DPI stabil + Dispose) =====
/*
foreach (Button b in panelFunctions.Controls)
{
    if (b.Image is Image img)
    {
        var old = b.Image;
        b.Image = AmiumCodeCSharp.Tools.BitmapTools.ReplaceColor(img, ButtonForeColor, Color.Black, tolerance: 12);
        old?.Dispose(); // GDI-Handles freigeben
    }
}
ButtonForeColor = Color.Black;
*/

using Android.Content;
using Android.Graphics;
using Android.Views;
using RecompOne.Runtime.Host;

namespace RecompOne.SoTN.Android;

/// <summary>Draws artwork only outside the game viewport, beneath the touch controls.</summary>
public sealed class BorderOverlayView : View
{
    readonly Paint _paint = new(PaintFlags.FilterBitmap);
    Bitmap? _bitmap;
    BorderStyle _style;
    BorderStyle _loadedStyle = (BorderStyle)(-1);

    public BorderOverlayView(Context context) : base(context)
    {
        Clickable = false;
        Focusable = false;
        ImportantForAccessibility = ImportantForAccessibility.No;
        SetWillNotDraw(false);
    }

    public void SetStyle(BorderStyle style)
    {
        _style = style;
        if (_style == BorderStyle.Off)
        {
            _bitmap?.Dispose();
            _bitmap = null;
            _loadedStyle = BorderStyle.Off;
        }
        Invalidate();
    }

    protected override void OnAttachedToWindow()
    {
        base.OnAttachedToWindow();
        HostWindow.GameViewportChanged += OnGameViewportChanged;
    }

    protected override void OnDetachedFromWindow()
    {
        HostWindow.GameViewportChanged -= OnGameViewportChanged;
        _bitmap?.Dispose();
        _bitmap = null;
        _loadedStyle = (BorderStyle)(-1);
        base.OnDetachedFromWindow();
    }

    void OnGameViewportChanged() => PostInvalidate();

    void EnsureBitmap()
    {
        if (_loadedStyle == _style) return;
        _bitmap?.Dispose();
        _bitmap = null;
        _loadedStyle = _style;
        string? file = _style switch
        {
            BorderStyle.GothicCastle => "gothic-castle.png",
            BorderStyle.AlucardAndDracula => "alucard-dracula.png",
            BorderStyle.NightStainedGlass => "night-stained-glass.png",
            _ => null
        };
        if (file == null) return;
        try
        {
            using var stream = Context!.Assets!.Open($"borders/{file}");
            _bitmap = BitmapFactory.DecodeStream(stream);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Borders] Could not load {file}: {ex.Message}");
        }
    }

    protected override void OnDraw(Canvas canvas)
    {
        base.OnDraw(canvas);
        if (_style == BorderStyle.Off) return;
        var viewport = HostWindow.CurrentGameViewport;
        if (viewport == null || viewport.ScreenWidth <= 0 || viewport.ScreenHeight <= 0 ||
            viewport.Width <= 0 || viewport.Height <= 0) return;

        float sx = Width / (float)viewport.ScreenWidth;
        float sy = Height / (float)viewport.ScreenHeight;
        float left = Math.Clamp(viewport.X * sx, 0f, Width);
        float right = Math.Clamp((viewport.X + viewport.Width) * sx, 0f, Width);
        // OpenGL's viewport origin is bottom-left; Android Canvas starts at top-left.
        float top = Math.Clamp((viewport.ScreenHeight - viewport.Y - viewport.Height) * sy, 0f, Height);
        float bottom = Math.Clamp((viewport.ScreenHeight - viewport.Y) * sy, 0f, Height);
        if (left <= 0f && right >= Width && top <= 0f && bottom >= Height) return;

        EnsureBitmap();
        if (_bitmap == null) return;
        DrawPanel(canvas, false, 0f, 0f, left, Height);
        DrawPanel(canvas, true, right, 0f, Width, Height);
        DrawPanel(canvas, false, left, 0f, right, top);
        DrawPanel(canvas, true, left, bottom, right, Height);
    }

    void DrawPanel(Canvas canvas, bool rightPanel, float left, float top, float right, float bottom)
    {
        if (_bitmap == null || right - left < 1f || bottom - top < 1f) return;
        int half = _bitmap.Width / 2;
        int origin = rightPanel ? half : 0;
        int panelWidth = rightPanel ? _bitmap.Width - half : half;
        float sourceWidth = panelWidth;
        float sourceHeight = _bitmap.Height;
        float targetAspect = (right - left) / (bottom - top);
        if (sourceWidth / sourceHeight > targetAspect)
            sourceWidth = sourceHeight * targetAspect;
        else
            sourceHeight = sourceWidth / targetAspect;

        int sourceLeft = origin + (int)((panelWidth - sourceWidth) / 2f);
        int sourceTop = (int)((_bitmap.Height - sourceHeight) / 2f);
        using var source = new Rect(sourceLeft, sourceTop,
            Math.Min(origin + panelWidth, sourceLeft + Math.Max(1, (int)sourceWidth)),
            Math.Min(_bitmap.Height, sourceTop + Math.Max(1, (int)sourceHeight)));
        using var target = new RectF(left, top, right, bottom);
        int save = canvas.Save();
        canvas.ClipRect(target);
        canvas.DrawColor(Color.Black);
        canvas.DrawBitmap(_bitmap, source, target, _paint);
        canvas.RestoreToCount(save);
    }
}

using System.Drawing.Drawing2D;

namespace AlTayerERP.Desktop.Common;

/// <summary>
/// رأس الهوية الموحد لنظام الطائر السعيد. يرسم الشعار متجهياً لضمان وضوحه
/// في جميع دقات العرض، ويمكن استبداله لاحقاً بصورة الشعار الرسمية من نقطة واحدة.
/// </summary>
public sealed class BrandHeaderControl : Control
{
    public string ScreenTitle { get; set; } = string.Empty;
    public string SectionTitle { get; set; } = "التهيئة والبيانات الأساسية";

    public BrandHeaderControl(string screenTitle, string? sectionTitle = null)
    {
        ScreenTitle = screenTitle;
        if (!string.IsNullOrWhiteSpace(sectionTitle)) SectionTitle = sectionTitle;
        Dock = DockStyle.Fill;
        Height = 72;
        MinimumSize = new Size(360, 64);
        DoubleBuffered = true;
        RightToLeft = RightToLeft.Yes;
        Font = new Font("Segoe UI", 10F);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        using var background = new LinearGradientBrush(ClientRectangle,
            Color.FromArgb(7, 45, 91), Color.FromArgb(10, 78, 145), LinearGradientMode.Horizontal);
        g.FillRectangle(background, ClientRectangle);

        var logoBox = new Rectangle(Width - 76, 8, 58, 56);
        DrawBirdMark(g, logoBox);

        var textRight = logoBox.Left - 14;
        using var titleFont = new Font("Segoe UI", 14.5F, FontStyle.Bold);
        using var brandFont = new Font("Segoe UI", 8.8F, FontStyle.Bold);
        using var sectionFont = new Font("Segoe UI", 8.5F, FontStyle.Regular);
        using var white = new SolidBrush(Color.White);
        using var soft = new SolidBrush(Color.FromArgb(205, 225, 245));
        using var accent = new SolidBrush(Color.FromArgb(255, 188, 64));

        var titleRect = new Rectangle(24, 10, Math.Max(100, textRight - 24), 28);
        var subtitleRect = new Rectangle(24, 39, Math.Max(100, textRight - 24), 20);
        var format = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center, FormatFlags = StringFormatFlags.DirectionRightToLeft };
        g.DrawString(ScreenTitle, titleFont, white, titleRect, format);
        g.DrawString($"نظام الطائر السعيد  •  {SectionTitle}", sectionFont, soft, subtitleRect, format);

        g.FillRectangle(accent, new Rectangle(0, Height - 3, Width, 3));
    }

    private static void DrawBirdMark(Graphics g, Rectangle box)
    {
        using var circleBrush = new SolidBrush(Color.FromArgb(245, 249, 255));
        using var borderPen = new Pen(Color.FromArgb(255, 188, 64), 2F);
        g.FillEllipse(circleBrush, box);
        g.DrawEllipse(borderPen, box);

        var cx = box.Left + box.Width / 2F;
        var cy = box.Top + box.Height / 2F;
        using var wingBrush = new SolidBrush(Color.FromArgb(10, 78, 145));
        using var goldBrush = new SolidBrush(Color.FromArgb(245, 166, 35));

        using var leftWing = new GraphicsPath();
        leftWing.AddBezier(cx - 2, cy + 2, cx - 15, cy - 17, cx - 27, cy - 12, cx - 14, cy + 6);
        leftWing.AddBezier(cx - 14, cy + 6, cx - 8, cy + 13, cx - 3, cy + 12, cx - 2, cy + 2);
        g.FillPath(wingBrush, leftWing);

        using var rightWing = new GraphicsPath();
        rightWing.AddBezier(cx + 1, cy + 2, cx + 15, cy - 18, cx + 28, cy - 10, cx + 13, cy + 7);
        rightWing.AddBezier(cx + 13, cy + 7, cx + 8, cy + 13, cx + 3, cy + 12, cx + 1, cy + 2);
        g.FillPath(wingBrush, rightWing);

        using var body = new GraphicsPath();
        body.AddBezier(cx - 3, cy - 7, cx + 6, cy - 13, cx + 11, cy - 4, cx + 5, cy + 13);
        body.AddBezier(cx + 5, cy + 13, cx + 1, cy + 18, cx - 4, cy + 9, cx - 3, cy - 7);
        g.FillPath(goldBrush, body);

        g.FillPolygon(goldBrush, new[]
        {
            new PointF(cx + 8, cy - 7), new PointF(cx + 18, cy - 4), new PointF(cx + 9, cy - 1)
        });
        g.FillEllipse(Brushes.White, cx + 5, cy - 8, 2.6F, 2.6F);
    }
}

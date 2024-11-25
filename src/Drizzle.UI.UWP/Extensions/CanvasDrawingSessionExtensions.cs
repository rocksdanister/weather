using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Windows.Foundation;

namespace Drizzle.UI.UWP.Extensions;

public static class CanvasDrawingSessionExtensions
{
    public static void DrawOverlayWithText(this CanvasDrawingSession drawingSession,
        string text,
        float fontSize,
        float overlayX,
        float overlayY,
        float overlayPadding,
        Windows.UI.Color overlayColor,
        Windows.UI.Color textColor)
    {
        var textFormat = new CanvasTextFormat
        {
            FontSize = fontSize,
            WordWrapping = CanvasWordWrapping.NoWrap,
            VerticalAlignment = CanvasVerticalAlignment.Center
        };

        // Create a text layout to measure the text
        using var textLayout = new CanvasTextLayout(drawingSession, text, textFormat, 0.0f, 0.0f);
        float textWidth = (float)textLayout.DrawBounds.Width;
        float textHeight = (float)textLayout.DrawBounds.Height;

        float overlayWidth = textWidth + overlayPadding * 2;
        float overlayHeight = textHeight + overlayPadding * 2;

        drawingSession.FillRectangle(
            overlayX, overlayY, overlayWidth, overlayHeight, overlayColor);

        drawingSession.DrawText(
            text,
            new Rect(overlayX + overlayPadding, overlayY, overlayWidth - overlayPadding * 2, overlayHeight),
            textColor,
            textFormat);
    }
}

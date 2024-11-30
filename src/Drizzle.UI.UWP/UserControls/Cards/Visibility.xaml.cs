using CommunityToolkit.WinUI.Media;
using Drizzle.Models.Weather;
using Drizzle.Weather.Helpers;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Drizzle.UI.UWP.UserControls.Cards;

public sealed partial class Visibility : UserControl
{
    public float? Value
    {
        get { return (float?)GetValue(ValueProperty); }
        set { SetValue(ValueProperty, value); }
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(float?), typeof(Visibility), new PropertyMetadata(null, OnPropertyChanged));

    public VisibilityUnits Unit
    {
        get { return (VisibilityUnits)GetValue(UnitProperty); }
        set { SetValue(UnitProperty, value); }
    }

    public static readonly DependencyProperty UnitProperty =
        DependencyProperty.Register("Unit", typeof(VisibilityUnits), typeof(Visibility), new PropertyMetadata(VisibilityUnits.km, OnPropertyChanged));

    public string UnitString
    {
        get { return (string)GetValue(UnitStringProperty); }
        private set { SetValue(UnitStringProperty, value); }
    }

    public static readonly DependencyProperty UnitStringProperty =
        DependencyProperty.Register("UnitString", typeof(string), typeof(Visibility), new PropertyMetadata(string.Empty));

    public Brush BlurBrush
    {
        get { return (Brush)GetValue(BlurBrushProperty); }
        set { SetValue(BlurBrushProperty, value); }
    }

    public static readonly DependencyProperty BlurBrushProperty =
        DependencyProperty.Register("BlurBrush", typeof(Brush), typeof(Visibility), new PropertyMetadata(null, OnPropertyChanged));

    public Brush BlurBrushClone
    {
        get { return (Brush)GetValue(BlurBrushCloneProperty); }
        private set { SetValue(BlurBrushCloneProperty, value); }
    }

    public static readonly DependencyProperty BlurBrushCloneProperty =
        DependencyProperty.Register("BlurBrushClone", typeof(Brush), typeof(Visibility), new PropertyMetadata(new CommunityToolkit.WinUI.Media.BackdropBlurBrush()));

    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var obj = d as Cards.Visibility;
        if (e.Property == BlurBrushProperty)
        {
            // Clone the brush (in case its global resource) to avoid value change.
            obj.BlurBrushClone = e.NewValue switch
            {
                CommunityToolkit.WinUI.Media.AcrylicBrush acrylicBrush => new CommunityToolkit.WinUI.Media.AcrylicBrush
                {
                    BlurAmount = acrylicBrush.BlurAmount,
                    TintColor = acrylicBrush.TintColor,
                    TextureUri = acrylicBrush.TextureUri,
                    TintOpacity = acrylicBrush.TintOpacity
                },
                BackdropBlurBrush backdropBrush => new BackdropBlurBrush
                {
                    Amount = backdropBrush.Amount
                },
                _ => throw new NotImplementedException(),
            };
        }
        else if (e.Property == ValueProperty)
        {
            obj.UpdateAnimation();
        }
        else if (e.Property == UnitProperty)
        {
            obj.UnitString = ((VisibilityUnits)e.NewValue).GetUnitString();
        }
    }

    public Visibility()
    {
        this.InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (Value is null)
            return;

        var minVisibility = 0f;
        var maxVisibility = 25f;
        var minVisualBlur = 0f;
        var maxVisualBlur = 5f;
        var minTextBlur = 0f;
        var maxTextBlur = 1.5f;

        // Normalizer unit
        float visibility = Unit switch
        {
            VisibilityUnits.km => (float)Value,
            VisibilityUnits.mi => (float)WeatherUtil.MiToKm(Value),
            _ => (float)Value,
        };
        // Clamp to range
        visibility = Math.Max(minVisibility, Math.Min(visibility, maxVisibility));
        // Map to blur
        var normalizedVisibility = (visibility - minVisibility) / (maxVisibility - minVisibility);
        var visualBlurAmount = (1f - normalizedVisibility) * (maxVisualBlur - minVisualBlur);
        var textBlurAmount = (1f - normalizedVisibility) * (maxTextBlur - minTextBlur);

        UpdateBrush(AnimationBlurControl, visualBlurAmount);
        UpdateBrush(TextBlurControl, textBlurAmount);
    }

    private static void UpdateBrush(Border border, double amount)
    {
        if (border.Background is null)
            return;

        switch (border.Background)
        {
            case CommunityToolkit.WinUI.Media.AcrylicBrush acrylicBrush:
                acrylicBrush.BlurAmount = amount;
                break;
            case CommunityToolkit.WinUI.Media.BackdropBlurBrush backdropBrush:
                backdropBrush.Amount = amount;
                break;
            default:
                throw new NotImplementedException();
        }
    }
}

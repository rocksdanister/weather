﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//       LottieGen version:
//           7.1.1+g046e9eb0a2
//       
//       Command:
//           LottieGen -Language CSharp -Public -WinUIVersion 2.4 -InputFile haze.json
//       
//       Input file:
//           haze.json (4065 bytes created 22:35+05:30 Dec 23 2023)
//       
//       LottieGen source:
//           http://aka.ms/Lottie
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
// ____________________________________
// |       Object stats       | Count |
// |__________________________|_______|
// | All CompositionObjects   |    54 |
// |--------------------------+-------|
// | Expression animators     |     5 |
// | KeyFrame animators       |     3 |
// | Reference parameters     |     6 |
// | Expression operations    |     0 |
// |--------------------------+-------|
// | Animated brushes         |     - |
// | Animated gradient stops  |     - |
// | ExpressionAnimations     |     1 |
// | PathKeyFrameAnimations   |     - |
// |--------------------------+-------|
// | ContainerVisuals         |     1 |
// | ShapeVisuals             |     1 |
// |--------------------------+-------|
// | ContainerShapes          |     - |
// | CompositionSpriteShapes  |     3 |
// |--------------------------+-------|
// | Brushes                  |     4 |
// | Gradient stops           |     6 |
// | CompositionVisualSurface |     - |
// ------------------------------------
using Microsoft.Graphics.Canvas.Geometry;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.Graphics;
using Windows.UI;
using Windows.UI.Composition;

namespace Drizzle.UI.UWP.AnimatedIcons
{
    // Name:        haze
    // Frame rate:  60 fps
    // Frame count: 360
    // Duration:    6000.0 mS
    sealed class Haze
        : Microsoft.UI.Xaml.Controls.IAnimatedVisualSource
    {
        // Animation duration: 6.000 seconds.
        internal const long c_durationTicks = 60000000;

        public Microsoft.UI.Xaml.Controls.IAnimatedVisual TryCreateAnimatedVisual(Compositor compositor)
        {
            object ignored = null;
            return TryCreateAnimatedVisual(compositor, out ignored);
        }

        public Microsoft.UI.Xaml.Controls.IAnimatedVisual TryCreateAnimatedVisual(Compositor compositor, out object diagnostics)
        {
            diagnostics = null;

            if (Haze_AnimatedVisual.IsRuntimeCompatible())
            {
                var res = 
                    new Haze_AnimatedVisual(
                        compositor
                        );
                    return res;
            }

            return null;
        }

        /// <summary>
        /// Gets the number of frames in the animation.
        /// </summary>
        public double FrameCount => 360d;

        /// <summary>
        /// Gets the frame rate of the animation.
        /// </summary>
        public double Framerate => 60d;

        /// <summary>
        /// Gets the duration of the animation.
        /// </summary>
        public TimeSpan Duration => TimeSpan.FromTicks(c_durationTicks);

        /// <summary>
        /// Converts a zero-based frame number to the corresponding progress value denoting the
        /// start of the frame.
        /// </summary>
        public double FrameToProgress(double frameNumber)
        {
            return frameNumber / 360d;
        }

        /// <summary>
        /// Returns a map from marker names to corresponding progress values.
        /// </summary>
        public IReadOnlyDictionary<string, double> Markers =>
            new Dictionary<string, double>
            {
            };

        /// <summary>
        /// Sets the color property with the given name, or does nothing if no such property
        /// exists.
        /// </summary>
        public void SetColorProperty(string propertyName, Color value)
        {
        }

        /// <summary>
        /// Sets the scalar property with the given name, or does nothing if no such property
        /// exists.
        /// </summary>
        public void SetScalarProperty(string propertyName, double value)
        {
        }

        sealed class Haze_AnimatedVisual : Microsoft.UI.Xaml.Controls.IAnimatedVisual
        {
            const long c_durationTicks = 60000000;
            readonly Compositor _c;
            readonly ExpressionAnimation _reusableExpressionAnimation;
            CompositionColorGradientStop _gradientStop_0_AlmostLightGray_FFD3D6DD;
            CompositionColorGradientStop _gradientStop_0p45_AlmostLightGray_FFD3D6DD;
            CompositionColorGradientStop _gradientStop_1_AlmostSilver_FFBDC1C5;
            CompositionPathGeometry _pathGeometry_1;
            ContainerVisual _root;
            CubicBezierEasingFunction _cubicBezierEasingFunction_0;
            ExpressionAnimation _rootProgress;
            StepEasingFunction _holdThenStepEasingFunction;
            StepEasingFunction _stepThenHoldEasingFunction;

            static void StartProgressBoundAnimation(
                CompositionObject target,
                string animatedPropertyName,
                CompositionAnimation animation,
                ExpressionAnimation controllerProgressExpression)
            {
                target.StartAnimation(animatedPropertyName, animation);
                var controller = target.TryGetAnimationController(animatedPropertyName);
                controller.Pause();
                controller.StartAnimation("Progress", controllerProgressExpression);
            }

            ScalarKeyFrameAnimation CreateScalarKeyFrameAnimation(float initialProgress, float initialValue, CompositionEasingFunction initialEasingFunction)
            {
                var result = _c.CreateScalarKeyFrameAnimation();
                result.Duration = TimeSpan.FromTicks(c_durationTicks);
                result.InsertKeyFrame(initialProgress, initialValue, initialEasingFunction);
                return result;
            }

            Vector2KeyFrameAnimation CreateVector2KeyFrameAnimation(float initialProgress, Vector2 initialValue, CompositionEasingFunction initialEasingFunction)
            {
                var result = _c.CreateVector2KeyFrameAnimation();
                result.Duration = TimeSpan.FromTicks(c_durationTicks);
                result.InsertKeyFrame(initialProgress, initialValue, initialEasingFunction);
                return result;
            }

            CompositionSpriteShape CreateSpriteShape(CompositionGeometry geometry, Matrix3x2 transformMatrix, CompositionBrush fillBrush)
            {
                var result = _c.CreateSpriteShape(geometry);
                result.TransformMatrix = transformMatrix;
                result.FillBrush = fillBrush;
                return result;
            }

            // - - - Layer aggregator
            // - -  Offset:<256, 256>
            CanvasGeometry Geometry_0()
            {
                CanvasGeometry result;
                using (var builder = new CanvasPathBuilder(null))
                {
                    builder.SetFilledRegionDetermination(CanvasFilledRegionDetermination.Winding);
                    builder.BeginFigure(new Vector2(116F, -4F));
                    builder.AddCubicBezier(new Vector2(115.146004F, -4F), new Vector2(114.315002F, -3.91100001F), new Vector2(113.471001F, -3.87299991F));
                    builder.AddCubicBezier(new Vector2(115.063004F, -10.3319998F), new Vector2(116F, -17.0489998F), new Vector2(116F, -24F));
                    builder.AddCubicBezier(new Vector2(116F, -70.3919983F), new Vector2(78.3919983F, -108F), new Vector2(32F, -108F));
                    builder.AddCubicBezier(new Vector2(1.79900002F, -108F), new Vector2(-24.6009998F, -92.0090027F), new Vector2(-39.4039993F, -68.0849991F));
                    builder.AddCubicBezier(new Vector2(-47.7820015F, -73.0790024F), new Vector2(-57.5390015F, -76F), new Vector2(-68F, -76F));
                    builder.AddCubicBezier(new Vector2(-98.9280014F, -76F), new Vector2(-124F, -50.9280014F), new Vector2(-124F, -20F));
                    builder.AddCubicBezier(new Vector2(-124F, -16.8999996F), new Vector2(-123.682999F, -13.8800001F), new Vector2(-123.198997F, -10.9169998F));
                    builder.AddCubicBezier(new Vector2(-150.977997F, -5.66699982F), new Vector2(-172F, 18.6940002F), new Vector2(-172F, 48F));
                    builder.AddCubicBezier(new Vector2(-172F, 81.137001F), new Vector2(-145.136993F, 108F), new Vector2(-112F, 108F));
                    builder.AddCubicBezier(new Vector2(-110.649002F, 108F), new Vector2(-109.328003F, 107.886002F), new Vector2(-108F, 107.797997F));
                    builder.AddLine(new Vector2(-108F, 108F));
                    builder.AddLine(new Vector2(116F, 108F));
                    builder.AddCubicBezier(new Vector2(146.927994F, 108F), new Vector2(172F, 82.9280014F), new Vector2(172F, 52F));
                    builder.AddCubicBezier(new Vector2(172F, 21.0720005F), new Vector2(146.927994F, -4F), new Vector2(116F, -4F));
                    builder.EndFigure(CanvasFigureLoop.Closed);
                    result = CanvasGeometry.CreatePath(builder);
                }
                return result;
            }

            CanvasGeometry Geometry_1()
            {
                CanvasGeometry result;
                using (var builder = new CanvasPathBuilder(null))
                {
                    builder.BeginFigure(new Vector2(-120F, 0F));
                    builder.AddLine(new Vector2(120F, 0F));
                    builder.EndFigure(CanvasFigureLoop.Open);
                    result = CanvasGeometry.CreatePath(builder);
                }
                return result;
            }

            // - Layer aggregator
            // Offset:<256, 256>
            CompositionColorBrush ColorBrush_AlmostLavender_FFE6EFFC()
            {
                return _c.CreateColorBrush(Color.FromArgb(0xFF, 0xE6, 0xEF, 0xFC));
            }

            // - - Layer aggregator
            // -  Offset:<256, 256>
            // Stop 0
            CompositionColorGradientStop GradientStop_0_AlmostAliceBlue_FFF3F7FD()
            {
                return _c.CreateColorGradientStop(0F, Color.FromArgb(0xFF, 0xF3, 0xF7, 0xFD));
            }

            // Stop 0
            CompositionColorGradientStop GradientStop_0_AlmostLightGray_FFD3D6DD()
            {
                return _gradientStop_0_AlmostLightGray_FFD3D6DD = _c.CreateColorGradientStop(0F, Color.FromArgb(0xFF, 0xD3, 0xD6, 0xDD));
            }

            // - - Layer aggregator
            // -  Offset:<256, 256>
            // Stop 1
            CompositionColorGradientStop GradientStop_0p45_AlmostAliceBlue_FFF3F7FD()
            {
                return _c.CreateColorGradientStop(0.449999988F, Color.FromArgb(0xFF, 0xF3, 0xF7, 0xFD));
            }

            // Stop 1
            CompositionColorGradientStop GradientStop_0p45_AlmostLightGray_FFD3D6DD()
            {
                return _gradientStop_0p45_AlmostLightGray_FFD3D6DD = _c.CreateColorGradientStop(0.449999988F, Color.FromArgb(0xFF, 0xD3, 0xD6, 0xDD));
            }

            // - - Layer aggregator
            // -  Offset:<256, 256>
            // Stop 2
            CompositionColorGradientStop GradientStop_1_AlmostLavender_FFDEEAFA()
            {
                return _c.CreateColorGradientStop(1F, Color.FromArgb(0xFF, 0xDE, 0xEA, 0xFA));
            }

            // Stop 2
            CompositionColorGradientStop GradientStop_1_AlmostSilver_FFBDC1C5()
            {
                return _gradientStop_1_AlmostSilver_FFBDC1C5 = _c.CreateColorGradientStop(1F, Color.FromArgb(0xFF, 0xBD, 0xC1, 0xC5));
            }

            // - Layer aggregator
            // Offset:<256, 256>
            CompositionLinearGradientBrush LinearGradientBrush_0()
            {
                var result = _c.CreateLinearGradientBrush();
                var colorStops = result.ColorStops;
                colorStops.Add(GradientStop_0_AlmostAliceBlue_FFF3F7FD());
                colorStops.Add(GradientStop_0p45_AlmostAliceBlue_FFF3F7FD());
                colorStops.Add(GradientStop_1_AlmostLavender_FFDEEAFA());
                result.MappingMode = CompositionMappingMode.Absolute;
                result.StartPoint = new Vector2(-76F, -81F);
                result.EndPoint = new Vector2(57.1860008F, 149.684998F);
                return result;
            }

            // - Layer aggregator
            // Path 1
            CompositionLinearGradientBrush LinearGradientBrush_1()
            {
                var result = _c.CreateLinearGradientBrush();
                var colorStops = result.ColorStops;
                colorStops.Add(GradientStop_0_AlmostLightGray_FFD3D6DD());
                colorStops.Add(GradientStop_0p45_AlmostLightGray_FFD3D6DD());
                colorStops.Add(GradientStop_1_AlmostSilver_FFBDC1C5());
                result.MappingMode = CompositionMappingMode.Absolute;
                result.StartPoint = new Vector2(-36F, -63F);
                result.EndPoint = new Vector2(36F, 61.7080002F);
                return result;
            }

            // - Layer aggregator
            // Path 1
            CompositionLinearGradientBrush LinearGradientBrush_2()
            {
                var result = _c.CreateLinearGradientBrush();
                var colorStops = result.ColorStops;
                colorStops.Add(_gradientStop_0_AlmostLightGray_FFD3D6DD);
                colorStops.Add(_gradientStop_0p45_AlmostLightGray_FFD3D6DD);
                colorStops.Add(_gradientStop_1_AlmostSilver_FFBDC1C5);
                result.MappingMode = CompositionMappingMode.Absolute;
                result.StartPoint = new Vector2(-37F, -63F);
                result.EndPoint = new Vector2(35F, 61.7080002F);
                return result;
            }

            // - Layer aggregator
            // Offset:<256, 256>
            CompositionPathGeometry PathGeometry_0()
            {
                return _c.CreatePathGeometry(new CompositionPath(Geometry_0()));
            }

            CompositionPathGeometry PathGeometry_1()
            {
                return _pathGeometry_1 = _c.CreatePathGeometry(new CompositionPath(Geometry_1()));
            }

            // Layer aggregator
            // cloud-path
            CompositionSpriteShape SpriteShape_0()
            {
                // Offset:<256, 256>
                var geometry = PathGeometry_0();
                var result = CreateSpriteShape(geometry, new Matrix3x2(1F, 0F, 0F, 1F, 256F, 256F), LinearGradientBrush_0());;
                result.StrokeBrush = ColorBrush_AlmostLavender_FFE6EFFC();
                result.StrokeMiterLimit = 5F;
                result.StrokeThickness = 6F;
                return result;
            }

            // Layer aggregator
            // Path 1
            CompositionSpriteShape SpriteShape_1()
            {
                var result = _c.CreateSpriteShape(PathGeometry_1());
                result.StrokeBrush = LinearGradientBrush_1();
                result.StrokeDashCap = CompositionStrokeCap.Round;
                result.StrokeStartCap = CompositionStrokeCap.Round;
                result.StrokeEndCap = CompositionStrokeCap.Round;
                result.StrokeMiterLimit = 5F;
                result.StrokeThickness = 24F;
                StartProgressBoundAnimation(result, "Offset", OffsetVector2Animation_0(), RootProgress());
                return result;
            }

            // Layer aggregator
            // Path 1
            CompositionSpriteShape SpriteShape_2()
            {
                var result = _c.CreateSpriteShape(_pathGeometry_1);
                result.StrokeBrush = LinearGradientBrush_2();
                result.StrokeDashCap = CompositionStrokeCap.Round;
                result.StrokeStartCap = CompositionStrokeCap.Round;
                result.StrokeEndCap = CompositionStrokeCap.Round;
                result.StrokeMiterLimit = 5F;
                result.StrokeThickness = 24F;
                StartProgressBoundAnimation(result, "Offset", OffsetVector2Animation_1(), _rootProgress);
                return result;
            }

            // The root of the composition.
            ContainerVisual Root()
            {
                var result = _root = _c.CreateContainerVisual();
                var propertySet = result.Properties;
                propertySet.InsertScalar("Progress", 0F);
                propertySet.InsertScalar("t0", 0F);
                // Layer aggregator
                result.Children.InsertAtTop(ShapeVisual_0());
                StartProgressBoundAnimation(result.Properties, "t0", t0ScalarAnimation_0_to_1(), _rootProgress);
                return result;
            }

            CubicBezierEasingFunction CubicBezierEasingFunction_0()
            {
                return _cubicBezierEasingFunction_0 = _c.CreateCubicBezierEasingFunction(new Vector2(0.166999996F, 0.166999996F), new Vector2(0.833000004F, 0.833000004F));
            }

            ExpressionAnimation RootProgress()
            {
                var result = _rootProgress = _c.CreateExpressionAnimation("_.Progress");
                result.SetReferenceParameter("_", _root);
                return result;
            }

            ScalarKeyFrameAnimation t0ScalarAnimation_0_to_1()
            {
                // Frame 180.
                var result = CreateScalarKeyFrameAnimation(0.50000006F, 0F, _stepThenHoldEasingFunction);
                result.SetReferenceParameter("_", _root);
                // Frame 359.
                result.InsertKeyFrame(0.997222185F, 1F, _cubicBezierEasingFunction_0);
                return result;
            }

            // Layer aggregator
            ShapeVisual ShapeVisual_0()
            {
                var result = _c.CreateShapeVisual();
                result.Size = new Vector2(512F, 512F);
                var shapes = result.Shapes;
                // Offset:<256, 256>
                shapes.Add(SpriteShape_0());
                // Path 1
                shapes.Add(SpriteShape_1());
                // Path 1
                shapes.Add(SpriteShape_2());
                return result;
            }

            StepEasingFunction HoldThenStepEasingFunction()
            {
                var result = _holdThenStepEasingFunction = _c.CreateStepEasingFunction();
                result.IsFinalStepSingleFrame = true;
                return result;
            }

            StepEasingFunction StepThenHoldEasingFunction()
            {
                var result = _stepThenHoldEasingFunction = _c.CreateStepEasingFunction();
                result.IsInitialStepSingleFrame = true;
                return result;
            }

            // - Layer aggregator
            // Path 1
            // Offset
            Vector2KeyFrameAnimation OffsetVector2Animation_0()
            {
                // Frame 0.
                var result = CreateVector2KeyFrameAnimation(0F, new Vector2(280F, 462F), HoldThenStepEasingFunction());
                result.SetReferenceParameter("_", _root);
                // Frame 180.
                result.InsertKeyFrame(0.5F, new Vector2(232F, 462F), CubicBezierEasingFunction_0());
                // Frame 359.
                result.InsertExpressionKeyFrame(0.997222185F, "Pow(1-_.t0,3)*Vector2(232,462)+(3*Square(1-_.t0)*_.t0*Vector2(232,462))+(3*(1-_.t0)*Square(_.t0)*Vector2(272,462))+(Pow(_.t0,3)*Vector2(280,462))", StepThenHoldEasingFunction());
                // Frame 359.
                result.InsertKeyFrame(0.997222245F, new Vector2(280F, 462F), _stepThenHoldEasingFunction);
                return result;
            }

            // - Layer aggregator
            // Path 1
            // Offset
            Vector2KeyFrameAnimation OffsetVector2Animation_1()
            {
                // Frame 0.
                var result = CreateVector2KeyFrameAnimation(0F, new Vector2(232F, 414F), _holdThenStepEasingFunction);
                result.SetReferenceParameter("_", _root);
                // Frame 180.
                result.InsertKeyFrame(0.5F, new Vector2(280F, 414F), _cubicBezierEasingFunction_0);
                // Frame 359.
                result.InsertExpressionKeyFrame(0.997222185F, "Pow(1-_.t0,3)*Vector2(280,414)+(3*Square(1-_.t0)*_.t0*Vector2(280,414))+(3*(1-_.t0)*Square(_.t0)*Vector2(240,414))+(Pow(_.t0,3)*Vector2(232,414))", _stepThenHoldEasingFunction);
                // Frame 359.
                result.InsertKeyFrame(0.997222245F, new Vector2(232F, 414F), _stepThenHoldEasingFunction);
                return result;
            }

            internal Haze_AnimatedVisual(
                Compositor compositor
                )
            {
                _c = compositor;
                _reusableExpressionAnimation = compositor.CreateExpressionAnimation();
                Root();
            }

            public Visual RootVisual => _root;
            public TimeSpan Duration => TimeSpan.FromTicks(c_durationTicks);
            public Vector2 Size => new Vector2(512F, 512F);
            void IDisposable.Dispose() => _root?.Dispose();

            internal static bool IsRuntimeCompatible()
            {
                return Windows.Foundation.Metadata.ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7);
            }
        }
    }
}
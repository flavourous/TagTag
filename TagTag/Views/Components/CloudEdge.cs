using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaGraphControl;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Drawing;

namespace TagTag.Views.Components;

public class CloudEdge : Connection
{
    private static readonly Func<AvaloniaGraphControl.Edge, Microsoft.Msagl.Drawing.Edge> MsAglEdgeGetter;
    private static readonly SolidColorBrush Wb;

    static CloudEdge()
    {
        var prop = typeof(AvaloniaGraphControl.Edge).GetProperty("DEdge", BindingFlags.NonPublic | BindingFlags.Instance);
        var getter = prop.GetGetMethod(true);
        MsAglEdgeGetter = (Func<AvaloniaGraphControl.Edge, Microsoft.Msagl.Drawing.Edge>)Delegate.CreateDelegate(
            typeof(Func<AvaloniaGraphControl.Edge, Microsoft.Msagl.Drawing.Edge>), 
            getter
        );
        Wb = new SolidColorBrush(Avalonia.Media.Color.FromRgb(0x46, 0x4a, 0x59));
    }

    private Microsoft.Msagl.Drawing.Edge MsAglEdge => MsAglEdgeGetter(DataContext as AvaloniaGraphControl.Edge);
    
    private readonly List<Drawing> Drawings = [];

    
    protected override Size ArrangeOverride(Size finalSize)
    {
        Drawings.Clear();
        var obj = DataContext as AvaloniaGraphControl.Edge;
        Microsoft.Msagl.Drawing.Edge dEdge = MsAglEdge;
        Rectangle boundingBox = dEdge.BoundingBox;
        AglToAvalonia a2a = new AglToAvalonia(boundingBox.LeftTop);
        Avalonia.Point arrowStart = new();
        if (obj.HeadSymbol == AvaloniaGraphControl.Edge.Symbol.Arrow)
        {
            arrowStart = a2a.Convert(dEdge.ArrowAtTargetPosition);
            Drawings.Add(FigureToDrawing(CreateArrowHeadFigure(dEdge.EdgeCurve.End, dEdge.ArrowAtTargetPosition, a2a), Wb, Brushes.Transparent));
        }
        if (obj.TailSymbol == AvaloniaGraphControl.Edge.Symbol.Arrow)
        {
            arrowStart = a2a.Convert(dEdge.ArrowAtSourcePosition);
            Drawings.Add(FigureToDrawing(CreateArrowHeadFigure(dEdge.EdgeCurve.Start, dEdge.ArrowAtSourcePosition, a2a), Wb, Brushes.Transparent));
        }
        Drawings.Add(FigureToDrawing(CreateEdgePathFigure(dEdge, a2a, arrowStart), Wb, Brushes.Transparent));

        return AglToAvalonia.Convert(boundingBox.Size);
    }

    public override void Render(DrawingContext context)
    {
        foreach (Drawing drawing in Drawings)
        {
            drawing.Draw(context);
        }
    }

    private static Drawing FigureToDrawing(PathFigure figure, IBrush strokeBrush, IBrush fillBrush)
    {
        return new GeometryDrawing
        {
            Pen = new Pen(strokeBrush, 1.5),
            Brush = fillBrush,
            Geometry = new PathGeometry
            {
                Figures = new PathFigures { figure }
            }
        };
    }

    private static PathFigure CreateEdgePathFigure(Microsoft.Msagl.Drawing.Edge edge, AglToAvalonia a2a, Avalonia.Point arrowStart)
    {
        return a2a.Convert(arrowStart, edge.EdgeCurve);
    }

    private static PathFigure CreateArrowHeadFigure(Microsoft.Msagl.Core.Geometry.Point origin, Microsoft.Msagl.Core.Geometry.Point target, AglToAvalonia a2a)
    {
        List<Microsoft.Msagl.Core.Geometry.Point> source = ComputeArrowHead(origin, target, 5.0, 7.0).ToList();
        PathSegments pathSegments = new PathSegments();
        pathSegments.Add(new Avalonia.Media.LineSegment { Point = a2a.Convert(source[0]) });
        pathSegments.Add(new Avalonia.Media.LineSegment { Point = a2a.Convert(source[2]) });
        return new PathFigure
        {
            IsFilled = false,
            IsClosed = false,
            StartPoint = a2a.Convert(source[1]),
            Segments = pathSegments
        };
    }

    private static IEnumerable<Microsoft.Msagl.Core.Geometry.Point> ComputeArrowHead(
        Microsoft.Msagl.Core.Geometry.Point origin, 
        Microsoft.Msagl.Core.Geometry.Point target, 
        double width, 
        double length)
    {
        yield return target;

        Microsoft.Msagl.Core.Geometry.Point dir = target - origin;
        double len = Math.Sqrt(dir.X * dir.X + dir.Y * dir.Y);

        if (len == 0.0) yield break;

        Microsoft.Msagl.Core.Geometry.Point unitDir = dir / len;
        Microsoft.Msagl.Core.Geometry.Point perp = new(-unitDir.Y, unitDir.X);
        Microsoft.Msagl.Core.Geometry.Point baseCenter = target - (unitDir * length);

        yield return baseCenter + (perp * (width / 2.0));
        yield return baseCenter - (perp * (width / 2.0));
    }

    class AglToAvalonia
    {
        private readonly Microsoft.Msagl.Core.Geometry.Point origin;

        public AglToAvalonia(Microsoft.Msagl.Core.Geometry.Point origin)
        {
            this.origin = origin;
        }

        public Avalonia.Point Convert(Microsoft.Msagl.Core.Geometry.Point pt)
        {
            return new Avalonia.Point(pt.X - origin.X, origin.Y - pt.Y);
        }

        public static Avalonia.Size Convert(Microsoft.Msagl.Core.DataStructures.Size size)
        {
            return new Avalonia.Size(size.Width, size.Height);
        }

        public Rect Convert(Rectangle rect)
        {
            return new Rect(Convert(rect.LeftTop), Convert(rect.RightBottom));
        }

        public PathFigure Convert(Avalonia.Point arrowStart, ICurve curve)
        {
            PathSegments pathSegments = new PathSegments();
            pathSegments.AddRange(Flatten(curve).Select(TransformSegment));
            pathSegments.Add(new Avalonia.Media.LineSegment
            {
                Point = arrowStart
            });

            Microsoft.Msagl.Core.Geometry.Point startDirection = curve.Derivative(curve.ParStart);
            double startDirectionLength = Math.Sqrt(startDirection.X * startDirection.X + startDirection.Y * startDirection.Y);
            Microsoft.Msagl.Core.Geometry.Point start = startDirectionLength == 0.0
                ? curve.Start
                : curve.Start - startDirection * (5.0 / startDirectionLength);

            return new PathFigure
            {
                StartPoint = Convert(start),
                Segments = pathSegments,
                IsClosed = false,
                IsFilled = false
            };
        }

        private IEnumerable<ICurve> Flatten(ICurve curve)
        {
            if (curve is Curve curve2)
            {
                return curve2.Segments.SelectMany((ICurve c) => Flatten(c));
            }

            return Enumerable.Repeat(curve, 1);
        }

        private PathSegment TransformSegment(ICurve curve)
        {
            if (curve is Microsoft.Msagl.Core.Geometry.Curves.LineSegment lineSegment)
            {
                return new Avalonia.Media.LineSegment
                {
                    Point = Convert(lineSegment.End)
                };
            }

            if (curve is CubicBezierSegment cubicBezierSegment)
            {
                return new BezierSegment
                {
                    Point1 = Convert(cubicBezierSegment.B(1)),
                    Point2 = Convert(cubicBezierSegment.B(2)),
                    Point3 = Convert(cubicBezierSegment.End)
                };
            }

            if (curve is Ellipse ellipse)
            {
                return ApproximateEllipticalArcWithBezierCurve_ThisMethodNeedsTesting(ellipse);
            }

            throw new NotImplementedException($"Cannot transform {curve} of type {curve.GetType().FullName}");
        }

        private PathSegment ApproximateEllipticalArcWithBezierCurve_ThisMethodNeedsTesting(Ellipse ellipse)
        {
            double num = ellipse.ParEnd - ellipse.ParStart;
            double num2 = Math.Sin(num) * (Math.Sqrt(4.0 + 3.0 * Math.Pow(Math.Tan(num / 2.0), 2.0)) - 1.0) / 3.0;
            Microsoft.Msagl.Core.Geometry.Point pt = ellipse.Start + num2 * ellipse.Derivative(ellipse.ParStart);
            Microsoft.Msagl.Core.Geometry.Point pt2 = ellipse.End - num2 * ellipse.Derivative(ellipse.ParEnd);
            return new BezierSegment
            {
                Point1 = Convert(pt),
                Point2 = Convert(pt2),
                Point3 = Convert(ellipse.End)
            };
        }
    }
}
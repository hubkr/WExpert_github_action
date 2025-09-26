using System.ComponentModel;
using System.Runtime.InteropServices.WindowsRuntime;
using Clipper2Lib;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Newtonsoft.Json.Linq;
using WExpert.Code;
using WExpert.Models.Dto.Data;
using WExpert.Utils;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using GeometryPoint = Rulyotano.Math.Geometry.Point;
using Path = Microsoft.UI.Xaml.Shapes.Path;

namespace WExpert.Controls;

public sealed partial class USViewerControl : UserControl, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<ScrollViewerViewChangedEventArgs>? ViewChanged; // 사용자 정의 이벤트

    private const float ZoomIncrement = 0.1f; // Zoom 10% 증가 처리

    private readonly DispatcherTimer _debounceFilterTimer = new() { Interval = TimeSpan.FromMilliseconds(100) };

    #region image drag move
    private bool _isDragging = false;
    private Point _lastPosition;
    private readonly InputCursor _handCursor;
    private readonly InputCursor _grabCursor;
    private readonly InputCursor _arrowCursor;

    private const float DragSpeedMultiplier = 1.0f; // 드래그 속도 증가 배수
    #endregion

    public USViewerControl()
    {
        InitializeComponent();
        DataContext = this;

        #region image drag move
        _handCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
        _grabCursor = InputSystemCursor.Create(InputSystemCursorShape.Person); // 실제로는 Hand 커서를 사용합니다
        _arrowCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
        #endregion
    }

    public WriteableBitmap? OriginalSource { get; set; } = null;

    private WriteableBitmap? _displaySource = new(1, 1); // 처음에 NoImage 가 출력 되지 않도록 초기값 설정
    public WriteableBitmap? DisplaySource
    {
        get => _displaySource;
        set
        {
            if (_displaySource != value)
            {
                _displaySource = value;
                //SetZoom(null, null, null, true); // 현재 창에 맞게 이미지 크기 설정 변경
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplaySource)));
            }
        }
    }

    private bool _flipHorizontal = false;
    private bool _flipVertical = false;

    private int _brightness = 0;
    public int Brightness
    {
        get => _brightness;
        set
        {
            if (_brightness != value && DisplaySource != null)
            {
                _brightness = value;
                ApplyAllFilterEffects();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Brightness)));
            }
        }
    }

    private int _contrast = 0;
    public int Contrast
    {
        get => _contrast;
        set
        {
            if (_contrast != value && DisplaySource != null)
            {
                _contrast = value;
                ApplyAllFilterEffects();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Contrast)));
            }
        }
    }

    private int _sharpness = 0;
    public int Sharpness
    {
        get => _sharpness;
        set
        {
            if (_sharpness != value && DisplaySource != null)
            {
                _sharpness = value;
                ApplyAllFilterEffects();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Sharpness)));
            }
        }
    }

    private async void DebounceFilterTimer_Tick(object? sender, object? e)
    {
        _debounceFilterTimer.Stop();

        // 실제 이미지 필터 적용 코드 실행   
        if (OriginalSource == null)
        {
            return;
        }

        //var stopWatch = new Stopwatch();
        //stopWatch.Start();

        var processedImage = OriginalSource;
        processedImage = await ImageFilter.ApplyFilters(processedImage, _brightness, _contrast, _sharpness);

        // Flip 적용  
        if (_flipHorizontal)
        {
            processedImage = DisplayUtils.HorizontalFlip(processedImage);
        }

        if (_flipVertical)
        {
            processedImage = DisplayUtils.VerticalFlip(processedImage);
        }

        //stopWatch.Stop();

        // 경과 시간(TimeSpan) 구하기  
        //var ts = stopWatch.Elapsed;
        //var elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
        //WExpertLogger.Instance.Debug("############## Elapsed Time: " + elapsedTime);

        DisplaySource = processedImage;
    }

    private void ApplyAllFilterEffects()
    {
        // 필터 적용을 위한 타이머 시작(이벤트 빈번 한 발생으로 인항 성능 저하 방지)
        _debounceFilterTimer.Stop();
        _debounceFilterTimer.Tick -= DebounceFilterTimer_Tick;
        _debounceFilterTimer.Tick += DebounceFilterTimer_Tick;
        _debounceFilterTimer.Start();
    }

    private float _zoom = 1.0f;
    public float Zoom
    {
        get => _zoom;
        set
        {
            if (_zoom != value)
            {
                WExpertLogger.Instance.Debug("[USViewerControl]Zoom Property Set : " + value);
                _zoom = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Zoom)));
            }
        }
    }

    private bool _fitToScreen = false;
    public bool FitToScreen
    {
        get => _fitToScreen;
        set
        {
            if (_fitToScreen != value)
            {
                WExpertLogger.Instance.Debug("[USViewerControl]FitToScreen Property Set : " + value);
                _fitToScreen = value;                
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FitToScreen)));
            }
        }
    }

    private bool _realSize = false;
    public bool RealSize
    {
        get => _realSize;
        set
        {
            if (_realSize != value)
            {
                _realSize = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RealSize)));
            }
        }
    }

    private float? GetFitToScreenZoomFactor()
    {
        if (DisplaySource == null)
        {
            return null;
        }

        var scaleX = ScrollViewer.ViewportWidth / DisplaySource.PixelWidth;
        var scaleY = ScrollViewer.ViewportHeight / DisplaySource.PixelHeight;
        var scale = Math.Min(scaleX, scaleY);

        // zoom 값을 반내림
        return (float)CommonUtils.FloorToDecimalPlaces(scale, 2);
    }

    public void SetFitToScreen()
    {
        SetZoom(null, null, null, true);
        FitToScreen = true;
        //RealSize = false;
    }

    //public void SetRealSize()
    //{
    //    SetZoom(null, null, 1.0f, false);
    //    FitToScreen = false;
    //    //RealSize = true;
    //}

    public void SetZoom(double? horizontalOffset, double? verticalOffset, float? zoomFactor, bool fitToScreen)
    {
        if (DisplaySource == null)
        {
            return;
        }

        // 현재 화면에 이미지 크기 맞추어 출력
        if (fitToScreen)
        {
            horizontalOffset = null;
            verticalOffset = null;
            zoomFactor = GetFitToScreenZoomFactor();
        }
        else
        {
            if (horizontalOffset == null && verticalOffset == null)
            {
                // 현재 스크롤 위치를 가져옵니다
                var currentHorizontalOffset = ScrollViewer.HorizontalOffset;
                var currentVerticalOffset = ScrollViewer.VerticalOffset;

                // 줌 팩터가 변경되는 경우, 새로운 컨텐츠 크기를 계산합니다
                var newZoomFactor = zoomFactor ?? ScrollViewer.ZoomFactor;
                var newContentWidth = ScrollViewer.ExtentWidth * (newZoomFactor / ScrollViewer.ZoomFactor);
                var newContentHeight = ScrollViewer.ExtentHeight * (newZoomFactor / ScrollViewer.ZoomFactor);

                // 뷰포트 중앙을 기준으로 새로운 오프셋을 계산합니다
                horizontalOffset = currentHorizontalOffset + (newContentWidth - ScrollViewer.ExtentWidth) / 2;
                verticalOffset = currentVerticalOffset + (newContentHeight - ScrollViewer.ExtentHeight) / 2;
            }
        }

        var result = ScrollViewer.ChangeView(horizontalOffset, verticalOffset, zoomFactor, true);
        WExpertLogger.Instance.Debug(string.Format("[USViewerControl]SetZoom(hOffset:{0}, vOffset:{1}, zoomFactor:{2}, fitToScreen : {3}) = {4}",
            horizontalOffset, verticalOffset, zoomFactor, fitToScreen, result));
    }

    public void SetFlip(bool? vertical)
    {
        if (DisplaySource == null || vertical == null)
        {
            HeatMapCanvas.RenderTransform = null; // 초기화(vertical 이 null 인경우는 초기화 처리)
            _flipHorizontal = false;
            _flipVertical = false;
            return;
        }

        var flipTransform = HeatMapCanvas.RenderTransform as ScaleTransform;        
        if ((bool)vertical) // Vertical flip
        {
            DisplaySource = DisplayUtils.VerticalFlip(DisplaySource);

            if (flipTransform == null)
            {
                flipTransform = new ScaleTransform() { ScaleY = -1, CenterY = HeatMapCanvas.ActualHeight / 2 };
            }
            else
            {
                flipTransform.ScaleY *= -1;
                flipTransform.CenterY = flipTransform.ScaleY == 1 ? 0 : HeatMapCanvas.ActualHeight / 2;
            }

            _flipVertical = flipTransform.ScaleY == -1;
        }
        else // Horizontal flip
        {
            DisplaySource = DisplayUtils.HorizontalFlip(DisplaySource);

            if (flipTransform == null)
            {
                flipTransform = new ScaleTransform() { ScaleX = -1, CenterX = HeatMapCanvas.ActualWidth / 2 };
            }
            else
            {
                flipTransform.ScaleX *= -1;
                flipTransform.CenterX = flipTransform.CenterX == 1 ? 0 : HeatMapCanvas.ActualWidth / 2;
            }

            _flipHorizontal = flipTransform.ScaleX == -1;
        }

        HeatMapCanvas.RenderTransform = flipTransform;
    }

    private Point ConvertToVisualPoint(GeometryPoint p)
    {
        return new Point(p.X, p.Y);
    }

    public void ClearCanvas()
    {
        var removeElements = new List<UIElement>();
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(HeatMapCanvas); i++)
        {
            var child = VisualTreeHelper.GetChild(HeatMapCanvas, i);
            if (child != null && child is Path)
            {
                removeElements.Add((UIElement)child);
            }
        }

        foreach (var element in removeElements)
        {
            HeatMapCanvas.Children.Remove(element);
        }

        removeElements.Clear();
    }

    public void SetROIMarking(int thickness, Color color)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(HeatMapCanvas); i++)
        {
            var child = VisualTreeHelper.GetChild(HeatMapCanvas, i);
            if (child != null && child is Path)
            {
                var childPath = (Path)child;
                var storkBrush = new SolidColorBrush(color); // ROI 표시

                childPath.Stroke = storkBrush;
                childPath.StrokeThickness = thickness;
            }
        }
    }

    private void DrawPath(List<GeometryPoint> points, Color? fillColor, Color borderColor, int borderThickness)
    {
#if DEBUG
        //List<GeometryPoint> points2 = points.DistinctBy(p => (p.X, p.Y)).ToList();
        //var minus = points.Count - points2.Count;
        // 임시
        //points = points.DistinctBy(p => (p.X, p.Y)).ToList();
        //points.Add(points[0]);
#endif
        var IsClosedCurve = true;
        if (points == null || points.Count <= 1 || points[0] == null)
        {
            return;
        }

        var myPathFigure = new PathFigure { StartPoint = ConvertToVisualPoint(points[0]) };

        var myPathSegmentCollection = new PathSegmentCollection();
        var bezierCurve = Rulyotano.Math.Interpolation.Bezier.BezierInterpolation.PointsToBezierCurves(points, IsClosedCurve);
        if (bezierCurve == null || bezierCurve.Segments.Count() < 1)
        {
            //Add a line segment <this is generic for more than one line>
            foreach (var point in points.GetRange(1, points.Count - 1))
            {
                var myLineSegment = new LineSegment { Point = ConvertToVisualPoint(point) };
                myPathSegmentCollection.Add(myLineSegment);
            }
        }
        else
        {
            foreach (var bezierCurveSegment in bezierCurve.Segments)
            {
                var segment = new BezierSegment
                {
                    Point1 = ConvertToVisualPoint(bezierCurveSegment.FirstControlPoint),
                    Point2 = ConvertToVisualPoint(bezierCurveSegment.SecondControlPoint),
                    Point3 = ConvertToVisualPoint(bezierCurveSegment.EndPoint)
                };
                myPathSegmentCollection.Add(segment);
            }
        }

        myPathFigure.Segments = myPathSegmentCollection;
        myPathFigure.IsClosed = IsClosedCurve;

        Brush? fillBrush = null;

        // fillColor 이 null 인 경우 heatmap 색깔 사용
        if (fillColor == null)
        {
            var c1 = Color.FromArgb(255, 255, 0, 0);
            var c2 = Color.FromArgb(189, 255, 242, 0);
            var c3 = Color.FromArgb(160, 181, 230, 29);
            var c4 = Color.FromArgb(63, 153, 217, 234);

            // Create a RadialGradientBrush with four gradient stops.
            var gradient = new RadialGradientBrush
            {
                // Set the GradientOrigin to the center of the area being painted.
                GradientOrigin = new Point(0.5, 0.5),

                // Set the gradient center to the center of the area being painted.
                Center = new Point(0.5, 0.5),

                // Set the radius of the gradient circle so that it extends to
                // the edges of the area being painted.
                RadiusX = 0.5,
                RadiusY = 0.5
            };

            // Create four gradient stops.
            gradient.GradientStops.Add(new GradientStop() { Color = c1, Offset = 0.0 });
            gradient.GradientStops.Add(new GradientStop() { Color = c2, Offset = 0.4 });
            gradient.GradientStops.Add(new GradientStop() { Color = c3, Offset = 0.6 });
            gradient.GradientStops.Add(new GradientStop() { Color = c4, Offset = 1.0 });

            // Freeze the brush (make it unmodifiable) for performance benefits.
            //gradient.Freeze();

            //var myPathFigureCollection = new PathFigureCollection { myPathFigure };
            //var myPathGeometry = new PathGeometry { Figures = myPathFigureCollection };

            fillBrush = gradient;
        }
        else
        {
            fillBrush = new SolidColorBrush((Color)fillColor);
        }

        var myPathFigureCollection = new PathFigureCollection { myPathFigure };
        var myPathGeometry = new PathGeometry { Figures = myPathFigureCollection };

        var path = new Path
        {
            Fill = fillBrush,
            //Stroke = new SolidColorBrush(Colors.Transparent),
            Stroke = new SolidColorBrush(borderColor), // Heatmap contour 표시
            StrokeThickness = borderThickness,
            Data = myPathGeometry
        };

        HeatMapCanvas.Children.Add(path);
    }

#if false
    private void DrawContourPaths(List<GeometryPoint> originalPoints, int numberOfContours, double contourSpacing)
    {
        // 색상 팔레트 정의 (예시)
        var colors = new List<Color>
        {
            Color.FromArgb(255, 0, 0, 255),   // Blue (가장 바깥)
            Color.FromArgb(255, 0, 128, 0),   // Green
            Color.FromArgb(255, 255, 255, 0), // Yellow
            Color.FromArgb(255, 255, 165, 0), // Orange
            Color.FromArgb(255, 255, 0, 0)    // Red (가장 안쪽)

        };

        // 가장 바깥쪽 등고선 (원본 경로)
        DrawSinglePath(originalPoints, 1, colors[0]); // 윤곽선 두께와 색상 필요에 따라 조절

        // 안쪽으로 등고선 생성
        for (var i = 1; i < numberOfContours && i < colors.Count; i++)
        {
            // 원본 경로에서 안쪽으로 'i * contourSpacing' 만큼 떨어진 등고선 포인트 생성
            // 이 함수는 사용자 정의 또는 외부 라이브러리를 통해 구현되어야 합니다.
            var offsetPoints = CreateOffsetPoints(originalPoints, i * contourSpacing, true); // true는 안쪽으로 오프셋
            if (offsetPoints != null && offsetPoints.Count > 1)
            {
                DrawSinglePath(offsetPoints, 1, colors[i]);
            }
        }
    }

    // 개별 Path를 그리는 헬퍼 함수
    private void DrawSinglePath(List<GeometryPoint> points, int outlineThickness, Color fillColor)
    {
        if (points == null || points.Count <= 1 || points[0] == null)
        {
            return;
        }

        var IsClosedCurve = true; // 등고선은 보통 닫힌 곡선일 것입니다.

        var myPathFigure = new PathFigure { StartPoint = ConvertToVisualPoint(points[0]) };
        var myPathSegmentCollection = new PathSegmentCollection();
        var bezierCurve = Rulyotano.Math.Interpolation.Bezier.BezierInterpolation.PointsToBezierCurves(points, IsClosedCurve);

        if (bezierCurve == null || bezierCurve.Segments.Count() < 1)
        {
            foreach (var point in points.GetRange(1, points.Count - 1))
            {
                myPathSegmentCollection.Add(new LineSegment { Point = ConvertToVisualPoint(point) });
            }
        }
        else
        {
            foreach (var bezierCurveSegment in bezierCurve.Segments)
            {
                var segment = new BezierSegment
                {
                    Point1 = ConvertToVisualPoint(bezierCurveSegment.FirstControlPoint),
                    Point2 = ConvertToVisualPoint(bezierCurveSegment.SecondControlPoint),
                    Point3 = ConvertToVisualPoint(bezierCurveSegment.EndPoint)
                };
                myPathSegmentCollection.Add(segment);
            }
        }

        myPathFigure.Segments = myPathSegmentCollection;
        myPathFigure.IsClosed = IsClosedCurve;

        var myPathFigureCollection = new PathFigureCollection { myPathFigure };
        var myPathGeometry = new PathGeometry { Figures = myPathFigureCollection };

        var path = new Path
        {
            Fill = new SolidColorBrush(fillColor), // 각 등고선에 고유한 단색 채우기
                                                   // Stroke는 필요에 따라 등고선 윤곽선을 그리거나 투명하게 설정할 수 있습니다.
                                                   // Stroke = new SolidColorBrush(Colors.Black),
                                                   // StrokeThickness = outlineThickness,
            Data = myPathGeometry
        };

        HeatMapCanvas.Children.Add(path);
    }

    // 이 함수는 등고선 오프셋을 계산하는 로직을 포함해야 합니다.
    // 이는 상당히 복잡하며, 선형 보간 또는 곡선 보간을 사용하여 각 점에서 법선 벡터를 찾아 이동시켜야 합니다.
    // 필요하다면 외부 기하학 라이브러리 (예: NetTopologySuite, Clipper)를 고려할 수 있습니다.
    private List<GeometryPoint> CreateOffsetPoints(List<GeometryPoint> originalPoints, double offsetDistance, bool inward)
    {
        if (originalPoints == null || originalPoints.Count < 3)
            return new List<GeometryPoint>();

        try
        {
            // GeometryPoint를 Clipper의 Point64로 변환
            // Clipper는 정수 좌표를 사용하므로 스케일링 필요
            const double scale = 1000.0; // 정밀도를 위한 스케일링 팩터

            var clipperPath = new Path64();

            foreach (var point in originalPoints)
            {
                clipperPath.Add(new Point64(
                    (long)(point.X * scale),
                    (long)(point.Y * scale)
                ));
            }

            // 안쪽 오프셋의 경우 거리를 음수로, 바깥쪽의 경우 양수로
            var actualOffset = inward ? -Math.Abs(offsetDistance) : Math.Abs(offsetDistance);

            // ClipperOffset 객체 생성
            var co = new ClipperOffset();

            // 조인 타입과 엔드 타입 설정
            // JoinType.Round: 둥근 모서리 (부드러운 등고선에 적합)
            // EndType.Polygon: 닫힌 다각형
            co.AddPath(clipperPath, JoinType.Round, EndType.Polygon);

            // 오프셋 실행
            var solution = new Paths64();
            co.Execute(actualOffset * scale, solution);

            // 결과가 없는 경우 빈 리스트 반환
            if (solution.Count == 0)
                return new List<GeometryPoint>();

            // 가장 큰 면적을 가진 경로를 선택 (보통 메인 경로)
            var largestPath = solution.OrderByDescending(p => Math.Abs(Clipper.Area(p))).FirstOrDefault();

            if (largestPath == null)
                return new List<GeometryPoint>();

            // Point64를 다시 GeometryPoint로 변환
            var offsetPoints = new List<GeometryPoint>();

            foreach (var point in largestPath)
            {
                offsetPoints.Add(new GeometryPoint(point.X / scale, point.Y / scale));

            }

            return offsetPoints;
        }
        catch (Exception ex)
        {
            // 오류 발생 시 로깅하고 빈 리스트 반환
            System.Diagnostics.Debug.WriteLine($"CreateOffsetPoints 오류: {ex.Message}");
            return new List<GeometryPoint>();
        }
    }
#else
    private void DrawContourPaths(List<GeometryPoint> originalPoints, int numberOfContours, double contourSpacing)
    {
        // 색상 팔레트 정의 (예시)
        var colors = new List<Color>
        {
            Color.FromArgb(255, 0, 0, 255),   // Blue (가장 바깥)
            Color.FromArgb(255, 0, 128, 0),   // Green
            Color.FromArgb(255, 255, 255, 0), // Yellow
            Color.FromArgb(255, 255, 165, 0), // Orange
            Color.FromArgb(255, 255, 0, 0)    // Red (가장 안쪽)
        };

        // 가장 바깥쪽 등고선 (원본 경로)
        DrawSinglePath(originalPoints, 1, colors[0]); // 윤곽선 두께와 색상 필요에 따라 조절

        // 안쪽으로 등고선 생성
        for (var i = 1; i < numberOfContours && i < colors.Count; i++)
        {
            // 원본 경로에서 안쪽으로 'i * contourSpacing' 만큼 떨어진 등고선 포인트 생성
            var offsetPoints = CreateOffsetPoints(originalPoints, i * contourSpacing, true); // true는 안쪽으로 오프셋
            if (offsetPoints != null && offsetPoints.Count > 1)
            {
                DrawSinglePath(offsetPoints, 1, colors[i]);
            }
        }
    }

    // 개별 Path를 그리는 헬퍼 함수
    private void DrawSinglePath(List<GeometryPoint> points, int outlineThickness, Color fillColor)
    {
        if (points == null || points.Count <= 1 || points[0] == null)
        {
            return;
        }

        var IsClosedCurve = true; // 등고선은 보통 닫힌 곡선일 것입니다.

        var myPathFigure = new PathFigure { StartPoint = ConvertToVisualPoint(points[0]) };
        var myPathSegmentCollection = new PathSegmentCollection();
        var bezierCurve = Rulyotano.Math.Interpolation.Bezier.BezierInterpolation.PointsToBezierCurves(points, IsClosedCurve);

        if (bezierCurve == null || bezierCurve.Segments.Count() < 1)
        {
            foreach (var point in points.GetRange(1, points.Count - 1))
            {
                myPathSegmentCollection.Add(new LineSegment { Point = ConvertToVisualPoint(point) });
            }
        }
        else
        {
            foreach (var bezierCurveSegment in bezierCurve.Segments)
            {
                var segment = new BezierSegment
                {
                    Point1 = ConvertToVisualPoint(bezierCurveSegment.FirstControlPoint),
                    Point2 = ConvertToVisualPoint(bezierCurveSegment.SecondControlPoint),
                    Point3 = ConvertToVisualPoint(bezierCurveSegment.EndPoint)
                };
                myPathSegmentCollection.Add(segment);
            }
        }

        myPathFigure.Segments = myPathSegmentCollection;
        myPathFigure.IsClosed = IsClosedCurve;

        var myPathFigureCollection = new PathFigureCollection { myPathFigure };
        var myPathGeometry = new PathGeometry { Figures = myPathFigureCollection };

        var path = new Path
        {
            Fill = new SolidColorBrush(fillColor), // 각 등고선에 고유한 단색 채우기
                                                   // Stroke는 필요에 따라 등고선 윤곽선을 그리거나 투명하게 설정할 수 있습니다.
                                                   // Stroke = new SolidColorBrush(Colors.Black),
                                                   // StrokeThickness = outlineThickness,
            Data = myPathGeometry
        };

        HeatMapCanvas.Children.Add(path);
    }



    // 그라데이션을 사용하는 개별 Path 그리기 함수
    /*
        List<GeometryPoint> points: 그릴 점들의 목록
        int outlineThickness: 윤곽선 두께
        Color startColor: 그라데이션 시작 색상
        Color endColor: 그라데이션 끝 색상
        GradientDirection direction: 그라데이션 방향 (기본값: RadialFromCenter)
     */
    private void DrawSinglePathWithGradient(List<GeometryPoint> points, int outlineThickness,
        Color startColor, Color endColor, GradientDirection direction = GradientDirection.RadialFromCenter)
    {
        if (points == null || points.Count <= 1 || points[0] == null)
        {
            return;
        }

        var IsClosedCurve = true;

        var myPathFigure = new PathFigure { StartPoint = ConvertToVisualPoint(points[0]) };
        var myPathSegmentCollection = new PathSegmentCollection();
        var bezierCurve = Rulyotano.Math.Interpolation.Bezier.BezierInterpolation.PointsToBezierCurves(points, IsClosedCurve);

        if (bezierCurve == null || bezierCurve.Segments.Count() < 1)
        {
            foreach (var point in points.GetRange(1, points.Count - 1))
            {
                myPathSegmentCollection.Add(new LineSegment { Point = ConvertToVisualPoint(point) });
            }
        }
        else
        {
            foreach (var bezierCurveSegment in bezierCurve.Segments)
            {
                var segment = new BezierSegment
                {
                    Point1 = ConvertToVisualPoint(bezierCurveSegment.FirstControlPoint),
                    Point2 = ConvertToVisualPoint(bezierCurveSegment.SecondControlPoint),
                    Point3 = ConvertToVisualPoint(bezierCurveSegment.EndPoint)
                };
                myPathSegmentCollection.Add(segment);
            }
        }

        myPathFigure.Segments = myPathSegmentCollection;
        myPathFigure.IsClosed = IsClosedCurve;

        var myPathFigureCollection = new PathFigureCollection { myPathFigure };
        var myPathGeometry = new PathGeometry { Figures = myPathFigureCollection };

        // 그라데이션 브러시 생성
        var gradientBrush = CreateGradientBrush(points, startColor, endColor, direction);

        var path = new Path
        {
            Fill = gradientBrush,
            Data = myPathGeometry
        };

        HeatMapCanvas.Children.Add(path);
    }

    // 그라데이션 방향 열거형
    public enum GradientDirection
    {
        RadialFromCenter,      // 중심에서 방사형
        LinearTopToBottom,     // 상하 선형
        LinearLeftToRight,     // 좌우 선형
        LinearTopLeftToBottomRight, // 대각선
        RadialFromTopLeft      // 왼쪽 상단에서 방사형
    }


    // 그라데이션 브러시 생성 함수
    private Brush CreateGradientBrush(List<GeometryPoint> points, Color startColor, Color endColor, GradientDirection direction)
    {
        // 경계 상자 계산
        var minX = points.Min(p => p.X);
        var maxX = points.Max(p => p.X);
        var minY = points.Min(p => p.Y);
        var maxY = points.Max(p => p.Y);
        var centerX = (minX + maxX) / 2.0;
        var centerY = (minY + maxY) / 2.0;
        var width = maxX - minX;
        var height = maxY - minY;

        switch (direction)
        {
            case GradientDirection.RadialFromCenter: // 중심에서 방사형 그라데이션
                var radialBrush = new RadialGradientBrush();
                radialBrush.GradientStops.Add(new GradientStop { Color = startColor, Offset = 0.0 });
                radialBrush.GradientStops.Add(new GradientStop { Color = endColor, Offset = 1.0 });
                radialBrush.Center = new Windows.Foundation.Point(0.5, 0.5);
                radialBrush.RadiusX = 0.5;
                radialBrush.RadiusY = 0.5;
                return radialBrush;

            case GradientDirection.LinearTopToBottom: // 위에서 아래로 선형 그라데이션
                var linearBrush1 = new LinearGradientBrush();
                linearBrush1.StartPoint = new Windows.Foundation.Point(0, 0);
                linearBrush1.EndPoint = new Windows.Foundation.Point(0, 1);
                linearBrush1.GradientStops.Add(new GradientStop { Color = startColor, Offset = 0.0 });
                linearBrush1.GradientStops.Add(new GradientStop { Color = endColor, Offset = 1.0 });
                return linearBrush1;

            case GradientDirection.LinearLeftToRight: // 왼쪽에서 오른쪽으로 선형 그라데이션
                var linearBrush2 = new LinearGradientBrush();
                linearBrush2.StartPoint = new Windows.Foundation.Point(0, 0);
                linearBrush2.EndPoint = new Windows.Foundation.Point(1, 0);
                linearBrush2.GradientStops.Add(new GradientStop { Color = startColor, Offset = 0.0 });
                linearBrush2.GradientStops.Add(new GradientStop { Color = endColor, Offset = 1.0 });
                return linearBrush2;

            case GradientDirection.LinearTopLeftToBottomRight: // 대각선 그라데이션
                var linearBrush3 = new LinearGradientBrush();
                linearBrush3.StartPoint = new Windows.Foundation.Point(0, 0);
                linearBrush3.EndPoint = new Windows.Foundation.Point(1, 1);
                linearBrush3.GradientStops.Add(new GradientStop { Color = startColor, Offset = 0.0 });
                linearBrush3.GradientStops.Add(new GradientStop { Color = endColor, Offset = 1.0 });
                return linearBrush3;

            case GradientDirection.RadialFromTopLeft: //왼쪽 상단에서 방사형 그라데이션
                var radialBrush2 = new RadialGradientBrush();
                radialBrush2.GradientStops.Add(new GradientStop { Color = startColor, Offset = 0.0 });
                radialBrush2.GradientStops.Add(new GradientStop { Color = endColor, Offset = 1.0 });
                radialBrush2.Center = new Windows.Foundation.Point(0.0, 0.0);
                radialBrush2.RadiusX = 1.0;
                radialBrush2.RadiusY = 1.0;
                return radialBrush2;

            default:
                return new SolidColorBrush(startColor);
        }
    }

    // Clipper Library를 사용한 오프셋 경로 생성 함수
    private List<GeometryPoint> CreateOffsetPoints(List<GeometryPoint> originalPoints, double offsetDistance, bool inward)
    {
        if (originalPoints == null || originalPoints.Count < 3)
        {
            return [];
        }

        try
        {
            // GeometryPoint를 Clipper의 Point64로 변환
            // Clipper는 정수 좌표를 사용하므로 스케일링 필요
            const double scale = 1000.0; // 정밀도를 위한 스케일링 팩터

            var clipperPath = new Path64();

            foreach (var point in originalPoints)
            {
                clipperPath.Add(new Point64(
                    (long)(point.X * scale),
                    (long)(point.Y * scale)
                ));
            }

            // 안쪽 오프셋의 경우 거리를 음수로, 바깥쪽의 경우 양수로
            var actualOffset = inward ? -Math.Abs(offsetDistance) : Math.Abs(offsetDistance);

            // ClipperOffset 객체 생성
            var co = new ClipperOffset();

            // 조인 타입과 엔드 타입 설정
            // JoinType.Round: 둥근 모서리 (부드러운 등고선에 적합)
            // EndType.Polygon: 닫힌 다각형
            co.AddPath(clipperPath, JoinType.Round, EndType.Polygon);

            // 오프셋 실행
            var solution = new Paths64();
            co.Execute(actualOffset * scale, solution);

            // 결과가 없는 경우 빈 리스트 반환
            if (solution.Count == 0)
            {
                return [];
            }

            // 가장 큰 면적을 가진 경로를 선택 (보통 메인 경로)
            var largestPath = solution.OrderByDescending(p => Math.Abs(Clipper.Area(p))).FirstOrDefault();

            if (largestPath == null)
            {
                return new List<GeometryPoint>();
            }

            // Point64를 다시 GeometryPoint로 변환
            var offsetPoints = new List<GeometryPoint>();

            foreach (var point in largestPath)
            {
                offsetPoints.Add(new GeometryPoint(point.X / scale, point.Y / scale));
            }

            return offsetPoints;
        }
        catch (Exception ex)
        {
            // 오류 발생 시 로깅하고 빈 리스트 반환
            System.Diagnostics.Debug.WriteLine($"CreateOffsetPoints 오류: {ex.Message}");
            return [];
        }
    }

    // 선택적: 더 세밀한 제어가 필요한 경우를 위한 오버로드
    // JoinType 선택 가능: JoinType.Round: 둥근 모서리(기본값), JoinType.Square: 직각 모서리, JoinType.Miter: 뾰족한 모서리, miterLimit 설정 가능
    private List<GeometryPoint> CreateOffsetPointsAdvanced(List<GeometryPoint> originalPoints,
        double offsetDistance, bool inward, JoinType joinType = JoinType.Round, double miterLimit = 2.0)
    {
        if (originalPoints == null || originalPoints.Count < 3)
        {
            return [];
        }

        try
        {
            const double scale = 1000.0;

            var clipperPath = new Path64();

            foreach (var point in originalPoints)
            {
                clipperPath.Add(new Point64(
                    (long)(point.X * scale),
                    (long)(point.Y * scale)
                ));
            }

            var actualOffset = inward ? -Math.Abs(offsetDistance) : Math.Abs(offsetDistance);

            var co = new ClipperOffset(miterLimit);
            co.AddPath(clipperPath, joinType, EndType.Polygon);

            var solution = new Paths64();
            co.Execute(actualOffset * scale, solution);

            if (solution.Count == 0)
            {
                return [];
            }

            var largestPath = solution.OrderByDescending(p => Math.Abs(Clipper.Area(p))).FirstOrDefault();

            if (largestPath == null)
            {
                return new List<GeometryPoint>();
            }

            var offsetPoints = new List<GeometryPoint>();

            foreach (var point in largestPath)
            {
                offsetPoints.Add(new GeometryPoint(point.X / scale, point.Y / scale));
            }

            return offsetPoints;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CreateOffsetPointsAdvanced 오류: {ex.Message}");
            return [];
        }
    }

    // 성능 최적화를 위한 다중 오프셋 경로 생성 함수
    private void DrawContourPathsOptimized(List<GeometryPoint> originalPoints, int numberOfContours, double contourSpacing)
    {
        // 색상 팔레트 정의
        var colors = new List<Color>
        {
            Color.FromArgb(255, 0, 0, 255),   // Blue (가장 바깥)
            Color.FromArgb(255, 0, 128, 0),   // Green
            Color.FromArgb(255, 255, 255, 0), // rangeYellow
            Color.FromArgb(255, 255, 165, 0), // O
            Color.FromArgb(255, 255, 0, 0)    // Red (가장 안쪽)
        };

        // 가장 바깥쪽 등고선 (원본 경로)
        DrawSinglePath(originalPoints, 1, colors[0]);

        // 모든 오프셋 거리 배열 생성
        var offsetDistances = new List<double>();
        for (var i = 1; i < numberOfContours && i < colors.Count; i++)
        {
            offsetDistances.Add(i * contourSpacing);
        }

        // 한번에 모든 오프셋 경로 생성
        var offsetPaths = CreateMultipleOffsetPaths(originalPoints, offsetDistances, true);

        // 각 오프셋 경로 그리기
        for (var i = 0; i < offsetPaths.Count && (i + 1) < colors.Count; i++)
        {
            if (offsetPaths[i] != null && offsetPaths[i].Count > 1)
            {
                DrawSinglePath(offsetPaths[i], 1, colors[i + 1]);
            }
        }
    }

    // 다중 오프셋 경로를 한번에 생성하는 함수
    private List<List<GeometryPoint>> CreateMultipleOffsetPaths(List<GeometryPoint> originalPoints,
        List<double> offsetDistances, bool inward)
    {
        var results = new List<List<GeometryPoint>>();

        if (originalPoints == null || originalPoints.Count < 3 || offsetDistances == null)
        {
            return results;
        }

        try
        {
            const double scale = 1000.0;

            var clipperPath = new Path64();

            foreach (var point in originalPoints)
            {
                clipperPath.Add(new Point64(
                    (long)(point.X * scale),
                    (long)(point.Y * scale)
                ));
            }

            foreach (var offsetDistance in offsetDistances)
            {
                var actualOffset = inward ? -Math.Abs(offsetDistance) : Math.Abs(offsetDistance);

                var co = new ClipperOffset();
                co.AddPath(clipperPath, JoinType.Round, EndType.Polygon);

                var solution = new Paths64();
                co.Execute(actualOffset * scale, solution);

                if (solution.Count > 0)
                {
                    var largestPath = solution.OrderByDescending(p => Math.Abs(Clipper.Area(p))).FirstOrDefault();

                    if (largestPath != null)
                    {
                        var offsetPoints = new List<GeometryPoint>();

                        foreach (var point in largestPath)
                        {
                            // GeometryPoint 생성자를 사용 (X, Y 좌표를 매개변수로 전달)
                            offsetPoints.Add(new GeometryPoint(point.X / scale, point.Y / scale));
                        }

                        results.Add(offsetPoints);
                    }
                    else
                    {
                        results.Add([]);
                    }
                }
                else
                {
                    results.Add([]);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CreateMultipleOffsetPaths 오류: {ex.Message}");
        }

        return results;
    }

    // 그라데이션을 사용하는 등고선 그리기 함수
    private void DrawContourPathsWithGradient(List<GeometryPoint> originalPoints, int numberOfContours, double contourSpacing)
    {
        // 그라데이션용 색상 쌍 정의
        var gradientColorPairs = new List<(Color start, Color end)>
        {
            (Color.FromArgb(255, 255, 100, 100), Color.FromArgb(255, 200, 0, 0)),     // 연한 빨강 -> 진한 빨강
            (Color.FromArgb(255, 255, 200, 100), Color.FromArgb(255, 255, 140, 0)),   // 연한 주황 -> 진한 주황
            (Color.FromArgb(255, 255, 255, 150), Color.FromArgb(255, 255, 220, 0)),   // 연한 노랑 -> 진한 노랑
            (Color.FromArgb(255, 100, 200, 100), Color.FromArgb(255, 0, 150, 0)),     // 연한 초록 -> 진한 초록
            (Color.FromArgb(255, 100, 150, 255), Color.FromArgb(255, 0, 50, 200))     // 연한 파랑 -> 진한 파랑
        };

        // 가장 바깥쪽 등고선 (원본 경로) - 그라데이션으로 그리기
        if (gradientColorPairs.Count > 0)
        {
            DrawSinglePathWithGradient(originalPoints, 1,
                gradientColorPairs[0].start, gradientColorPairs[0].end,
                GradientDirection.RadialFromCenter);
        }

        // 안쪽으로 등고선 생성
        for (var i = 1; i < numberOfContours && i < gradientColorPairs.Count; i++)
        {
            var offsetPoints = CreateOffsetPoints(originalPoints, i * contourSpacing, true);
            if (offsetPoints != null && offsetPoints.Count > 1)
            {
                DrawSinglePathWithGradient(offsetPoints, 1,
                    gradientColorPairs[i].start, gradientColorPairs[i].end,
                    GradientDirection.RadialFromCenter);
            }
        }
    }

    // 히트맵 스타일 그라데이션 등고선
    private void DrawContourPathsHeatMapStyle(List<GeometryPoint> originalPoints, int numberOfContours, double contourSpacing)
    {
        // 가장 바깥쪽 등고선 (원본 경로)
        var heatMapColors = GenerateHeatMapColors(numberOfContours);
        DrawSinglePath(originalPoints, 1, heatMapColors[0]);

        // 안쪽으로 등고선 생성
        for (var i = 1; i < numberOfContours && i < heatMapColors.Count; i++)
        {
            var offsetPoints = CreateOffsetPoints(originalPoints, i * contourSpacing, true);
            if (offsetPoints != null && offsetPoints.Count > 1)
            {
                // 각 등고선에 투명도를 적용하여 레이어 효과 생성
                var colorWithAlpha = Color.FromArgb(
                    (byte)(180 - (i * 20)), // 안쪽으로 갈수록 더 투명해짐
                    heatMapColors[i].R,
                    heatMapColors[i].G,
                    heatMapColors[i].B);

                DrawSinglePath(offsetPoints, 1, colorWithAlpha);
            }
        }
    }

    // 히트맵 색상 생성 함수
    private List<Color> GenerateHeatMapColors(int count)
    {
        var colors = new List<Color>();

        for (var i = 0; i < count; i++)
        {
            var ratio = (float)i / (count - 1);

            // 파란색 -> 초록색 -> 노란색 -> 빨간색 그라데이션
            if (ratio <= 0.25f)
            {
                // 파란색에서 시안색으로
                var localRatio = ratio / 0.25f;
                colors.Add(Color.FromArgb(255,
                    (byte)(0),
                    (byte)(localRatio * 255),
                    (byte)(255)));
            }
            else if (ratio <= 0.5f)
            {
                // 시안색에서 초록색으로
                var localRatio = (ratio - 0.25f) / 0.25f;
                colors.Add(Color.FromArgb(255,
                    (byte)(0),
                    (byte)(255),
                    (byte)(255 - localRatio * 255)));
            }
            else if (ratio <= 0.75f)
            {
                // 초록색에서 노란색으로
                var localRatio = (ratio - 0.5f) / 0.25f;
                colors.Add(Color.FromArgb(255,
                    (byte)(localRatio * 255),
                    (byte)(255),
                    (byte)(0)));
            }
            else
            {
                // 노란색에서 빨간색으로
                var localRatio = (ratio - 0.75f) / 0.25f;
                colors.Add(Color.FromArgb(255,
                    (byte)(255),
                    (byte)(255 - localRatio * 255),
                    (byte)(0)));
            }
        }

        return colors;
    }

    // 사용자 정의 그라데이션 색상으로 등고선 그리기
    private void DrawContourPathsCustomGradient(List<GeometryPoint> originalPoints, int numberOfContours,
        double contourSpacing, Color startColor, Color endColor, GradientDirection direction = GradientDirection.RadialFromCenter)
    {
        // 시작색과 끝색 사이의 중간 색상들 생성
        var interpolatedColors = InterpolateColors(startColor, endColor, numberOfContours);

        // 가장 바깥쪽 등고선 (원본 경로)
        DrawSinglePathWithGradient(originalPoints, 1, startColor, interpolatedColors[0], direction);

        // 안쪽으로 등고선 생성
        for (var i = 1; i < numberOfContours && i < interpolatedColors.Count; i++)
        {
            var offsetPoints = CreateOffsetPoints(originalPoints, i * contourSpacing, true);
            if (offsetPoints != null && offsetPoints.Count > 1)
            {
                var currentStart = interpolatedColors[Math.Max(0, i - 1)];
                var currentEnd = interpolatedColors[i];
                DrawSinglePathWithGradient(offsetPoints, 1, currentStart, currentEnd, direction);
            }
        }
    }

    // 두 색상 간의 보간된 색상 목록 생성
    private List<Color> InterpolateColors(Color startColor, Color endColor, int steps)
    {
        var colors = new List<Color>();

        for (var i = 0; i < steps; i++)
        {
            var ratio = (float)i / (steps - 1);

            var r = (byte)(startColor.R + (endColor.R - startColor.R) * ratio);
            var g = (byte)(startColor.G + (endColor.G - startColor.G) * ratio);
            var b = (byte)(startColor.B + (endColor.B - startColor.B) * ratio);
            var a = (byte)(startColor.A + (endColor.A - startColor.A) * ratio);

            colors.Add(Color.FromArgb(a, r, g, b));
        }

        return colors;
    }

    // 중심에서 가장자리로 갈수록 투명해지는 등고선 그리기
    private void DrawContourPathsWithFadeEffect(List<GeometryPoint> originalPoints, int numberOfContours, double contourSpacing)
    {
        // 색상 팔레트 정의 (기본 불투명 색상)
        var colors = new List<Color>
        {
            Color.FromArgb(255, 255, 0, 0),       // Red (가장 바깥)
            Color.FromArgb(255, 255, 165, 0),     // Orange
            Color.FromArgb(255, 255, 255, 0),     // Yellow
            Color.FromArgb(255, 0, 128, 0),       // Green
            Color.FromArgb(255, 0, 0, 255)        // Blue (가장 안쪽)
        };

        // 가장 바깥쪽 등고선 (원본 경로) - 페이드 효과 적용
        DrawSinglePathWithFadeFromCenter(originalPoints, 1, colors[0]);

        // 안쪽으로 등고선 생성
        for (var i = 1; i < numberOfContours && i < colors.Count; i++)
        {
            var offsetPoints = CreateOffsetPoints(originalPoints, i * contourSpacing, true);
            if (offsetPoints != null && offsetPoints.Count > 1)
            {
                DrawSinglePathWithFadeFromCenter(offsetPoints, 1, colors[i]);
            }
        }
    }

    // 중심에서 가장자리로 페이드되는 개별 Path 그리기 함수
    private void DrawSinglePathWithFadeFromCenter(List<GeometryPoint> points, int outlineThickness, Color baseColor)
    {
        if (points == null || points.Count <= 1 || points[0] == null)
        {
            return;
        }

        var IsClosedCurve = true;

        var myPathFigure = new PathFigure { StartPoint = ConvertToVisualPoint(points[0]) };
        var myPathSegmentCollection = new PathSegmentCollection();
        var bezierCurve = Rulyotano.Math.Interpolation.Bezier.BezierInterpolation.PointsToBezierCurves(points, IsClosedCurve);

        if (bezierCurve == null || bezierCurve.Segments.Count() < 1)
        {
            foreach (var point in points.GetRange(1, points.Count - 1))
            {
                myPathSegmentCollection.Add(new LineSegment { Point = ConvertToVisualPoint(point) });
            }
        }
        else
        {
            foreach (var bezierCurveSegment in bezierCurve.Segments)
            {
                var segment = new BezierSegment
                {
                    Point1 = ConvertToVisualPoint(bezierCurveSegment.FirstControlPoint),
                    Point2 = ConvertToVisualPoint(bezierCurveSegment.SecondControlPoint),
                    Point3 = ConvertToVisualPoint(bezierCurveSegment.EndPoint)
                };
                myPathSegmentCollection.Add(segment);
            }
        }

        myPathFigure.Segments = myPathSegmentCollection;
        myPathFigure.IsClosed = IsClosedCurve;

        var myPathFigureCollection = new PathFigureCollection { myPathFigure };
        var myPathGeometry = new PathGeometry { Figures = myPathFigureCollection };

        // 중심에서 가장자리로 페이드되는 그라데이션 브러시 생성
        var fadeFromCenterBrush = CreateFadeFromCenterBrush(baseColor);

        var path = new Path
        {
            Fill = fadeFromCenterBrush,
            Data = myPathGeometry
        };

        HeatMapCanvas.Children.Add(path);
    }

    // 중심에서 가장자리로 페이드되는 브러시 생성
    private Brush CreateFadeFromCenterBrush(Color baseColor)
    {
        var radialBrush = new RadialGradientBrush();

        // 중심: 불투명한 기본 색상
        var centerColor = Color.FromArgb(baseColor.A, baseColor.R, baseColor.G, baseColor.B);

        // 가장자리: 완전히 투명한 같은 색상
        var edgeColor = Color.FromArgb(0, baseColor.R, baseColor.G, baseColor.B);

        // 그라데이션 스톱 추가
        radialBrush.GradientStops.Add(new GradientStop { Color = centerColor, Offset = 0.0 });  // 중심
        radialBrush.GradientStops.Add(new GradientStop { Color = centerColor, Offset = 0.3 });  // 30%까지는 불투명 유지
        radialBrush.GradientStops.Add(new GradientStop { Color = edgeColor, Offset = 1.0 });    // 가장자리에서 완전 투명

        // 방사형 그라데이션 설정
        radialBrush.Center = new Windows.Foundation.Point(0.5, 0.5);  // 중심점
        radialBrush.RadiusX = 0.5;  // X축 반지름
        radialBrush.RadiusY = 0.5;  // Y축 반지름

        return radialBrush;
    }


    // 사용자 정의 페이드 강도로 등고선 그리기
    private void DrawContourPathsWithCustomFade(List<GeometryPoint> originalPoints, int numberOfContours,
        double contourSpacing, double fadeStartPoint = 0.3, double fadeIntensity = 1.0)
    {
        var colors = new List<Color>
        {
            Color.FromArgb(255, 255, 0, 0),       // Red
            Color.FromArgb(255, 255, 165, 0),     // Orange
            Color.FromArgb(255, 255, 255, 0),     // Yellow
            Color.FromArgb(255, 0, 128, 0),       // Green
            Color.FromArgb(255, 0, 0, 255)        // Blue
        };

        // 가장 바깥쪽 등고선
        DrawSinglePathWithCustomFade(originalPoints, 1, colors[0], fadeStartPoint, fadeIntensity);

        // 안쪽 등고선들
        for (var i = 1; i < numberOfContours && i < colors.Count; i++)
        {
            var offsetPoints = CreateOffsetPoints(originalPoints, i * contourSpacing, true);
            if (offsetPoints != null && offsetPoints.Count > 1)
            {
                DrawSinglePathWithCustomFade(offsetPoints, 1, colors[i], fadeStartPoint, fadeIntensity);
            }
        }
    }

    // 사용자 정의 페이드 효과로 Path 그리기
    private void DrawSinglePathWithCustomFade(List<GeometryPoint> points, int outlineThickness,
        Color baseColor, double fadeStartPoint, double fadeIntensity)
    {
        if (points == null || points.Count <= 1 || points[0] == null)
        {
            return;
        }

        var IsClosedCurve = true;

        var myPathFigure = new PathFigure { StartPoint = ConvertToVisualPoint(points[0]) };
        var myPathSegmentCollection = new PathSegmentCollection();
        var bezierCurve = Rulyotano.Math.Interpolation.Bezier.BezierInterpolation.PointsToBezierCurves(points, IsClosedCurve);

        if (bezierCurve == null || bezierCurve.Segments.Count() < 1)
        {
            foreach (var point in points.GetRange(1, points.Count - 1))
            {
                myPathSegmentCollection.Add(new LineSegment { Point = ConvertToVisualPoint(point) });
            }
        }
        else
        {
            foreach (var bezierCurveSegment in bezierCurve.Segments)
            {
                var segment = new BezierSegment
                {
                    Point1 = ConvertToVisualPoint(bezierCurveSegment.FirstControlPoint),
                    Point2 = ConvertToVisualPoint(bezierCurveSegment.SecondControlPoint),
                    Point3 = ConvertToVisualPoint(bezierCurveSegment.EndPoint)
                };
                myPathSegmentCollection.Add(segment);
            }
        }

        myPathFigure.Segments = myPathSegmentCollection;
        myPathFigure.IsClosed = IsClosedCurve;

        var myPathFigureCollection = new PathFigureCollection { myPathFigure };
        var myPathGeometry = new PathGeometry { Figures = myPathFigureCollection };

        // 사용자 정의 페이드 브러시 생성
        var customFadeBrush = CreateCustomFadeBrush(baseColor, fadeStartPoint, fadeIntensity);

        var path = new Path
        {
            Fill = customFadeBrush,
            Data = myPathGeometry
        };

        HeatMapCanvas.Children.Add(path);
    }

    // 사용자 정의 페이드 브러시 생성
    private Brush CreateCustomFadeBrush(Color baseColor, double fadeStartPoint, double fadeIntensity)
    {
        var radialBrush = new RadialGradientBrush();

        // 페이드 강도에 따른 최종 투명도 계산
        var finalAlpha = (byte)(baseColor.A * (1.0 - fadeIntensity));

        var centerColor = Color.FromArgb(baseColor.A, baseColor.R, baseColor.G, baseColor.B);
        var fadeStartColor = Color.FromArgb((byte)(baseColor.A * 0.95), baseColor.R, baseColor.G, baseColor.B);
        var edgeColor = Color.FromArgb(finalAlpha, baseColor.R, baseColor.G, baseColor.B);

        radialBrush.GradientStops.Add(new GradientStop { Color = centerColor, Offset = 0.0 });
        radialBrush.GradientStops.Add(new GradientStop { Color = fadeStartColor, Offset = fadeStartPoint });
        radialBrush.GradientStops.Add(new GradientStop { Color = edgeColor, Offset = 1.0 });

        radialBrush.Center = new Windows.Foundation.Point(0.5, 0.5);
        radialBrush.RadiusX = 0.5;
        radialBrush.RadiusY = 0.5;

        return radialBrush;
    }

    // 부드러운 색상 전환을 위한 최적화된 등고선 그리기
    private void DrawContourPathsOptimizedSmooth(List<GeometryPoint> originalPoints, int numberOfContours, double contourSpacing)
    {
        // 색상 팔레트 정의 (히트맵 스타일)
        var colors = GenerateHeatMapColorsSmooth(numberOfContours);

        // 가장 바깥쪽 등고선 (원본 경로) - 투명도 적용
        DrawSinglePathWithAlpha(originalPoints, 1, colors[0], 0.7); // 70% 불투명도

        // 모든 오프셋 거리 배열 생성
        var offsetDistances = new List<double>();
        for (var i = 1; i < numberOfContours && i < colors.Count; i++)
        {
            offsetDistances.Add(i * contourSpacing);
        }

        // 한번에 모든 오프셋 경로 생성
        var offsetPaths = CreateMultipleOffsetPaths(originalPoints, offsetDistances, true);

        // 각 오프셋 경로를 투명도를 적용해서 그리기 (레이어 블렌딩 효과)
        for (var i = 0; i < offsetPaths.Count && (i + 1) < colors.Count; i++)
        {
            if (offsetPaths[i] != null && offsetPaths[i].Count > 1)
            {
                // 안쪽으로 갈수록 조금 더 불투명하게 (더 진한 색상 효과)
                var alpha = 0.6 + (i * 0.1); // 60%에서 시작해서 점점 증가
                alpha = Math.Min(alpha, 0.9); // 최대 90%까지

                DrawSinglePathWithAlpha(offsetPaths[i], 1, colors[i + 1], alpha);
            }
        }
    }

    // 투명도를 적용한 Path 그리기
    private void DrawSinglePathWithAlpha(List<GeometryPoint> points, int outlineThickness, Color baseColor, double alpha)
    {
        if (points == null || points.Count <= 1 || points[0] == null)
        {
            return;
        }

        var IsClosedCurve = true;

        var myPathFigure = new PathFigure { StartPoint = ConvertToVisualPoint(points[0]) };
        var myPathSegmentCollection = new PathSegmentCollection();
        var bezierCurve = Rulyotano.Math.Interpolation.Bezier.BezierInterpolation.PointsToBezierCurves(points, IsClosedCurve);

        if (bezierCurve == null || bezierCurve.Segments.Count() < 1)
        {
            foreach (var point in points.GetRange(1, points.Count - 1))
            {
                myPathSegmentCollection.Add(new LineSegment { Point = ConvertToVisualPoint(point) });
            }
        }
        else
        {
            foreach (var bezierCurveSegment in bezierCurve.Segments)
            {
                var segment = new BezierSegment
                {
                    Point1 = ConvertToVisualPoint(bezierCurveSegment.FirstControlPoint),
                    Point2 = ConvertToVisualPoint(bezierCurveSegment.SecondControlPoint),
                    Point3 = ConvertToVisualPoint(bezierCurveSegment.EndPoint)
                };
                myPathSegmentCollection.Add(segment);
            }
        }

        myPathFigure.Segments = myPathSegmentCollection;
        myPathFigure.IsClosed = IsClosedCurve;

        var myPathFigureCollection = new PathFigureCollection { myPathFigure };
        var myPathGeometry = new PathGeometry { Figures = myPathFigureCollection };

        // 투명도가 적용된 색상 생성
        var colorWithAlpha = Color.FromArgb(
            (byte)(baseColor.A * alpha),
            baseColor.R,
            baseColor.G,
            baseColor.B
        );

        var path = new Path
        {
            Fill = new SolidColorBrush(colorWithAlpha),
            Data = myPathGeometry
        };

        HeatMapCanvas.Children.Add(path);
    }

    // 부드러운 히트맵 색상 생성 (더 많은 중간 단계 포함)
    private List<Color> GenerateHeatMapColorsSmooth(int count)
    {
        var colors = new List<Color>();

        for (var i = 0; i < count; i++)
        {
            var ratio = (float)i / Math.Max(1, count - 1);

            // 파란색 -> 청록색 -> 초록색 -> 노란색 -> 주황색 -> 빨간색
            if (ratio <= 0.2f)
            {
                // 짙은 파란색에서 파란색으로
                var localRatio = ratio / 0.2f;
                colors.Add(Color.FromArgb(255,
                    (byte)(0),
                    (byte)(localRatio * 100),
                    (byte)(150 + localRatio * 105)));
            }
            else if (ratio <= 0.4f)
            {
                // 파란색에서 청록색으로
                var localRatio = (ratio - 0.2f) / 0.2f;
                colors.Add(Color.FromArgb(255,
                    (byte)(0),
                    (byte)(100 + localRatio * 155),
                    (byte)(255 - localRatio * 100)));
            }
            else if (ratio <= 0.6f)
            {
                // 청록색에서 초록색으로
                var localRatio = (ratio - 0.4f) / 0.2f;
                colors.Add(Color.FromArgb(255,
                    (byte)(localRatio * 100),
                    (byte)(255),
                    (byte)(155 - localRatio * 155)));
            }
            else if (ratio <= 0.8f)
            {
                // 초록색에서 노란색으로
                var localRatio = (ratio - 0.6f) / 0.2f;
                colors.Add(Color.FromArgb(255,
                    (byte)(100 + localRatio * 155),
                    (byte)(255),
                    (byte)(0)));
            }
            else
            {
                // 노란색에서 빨간색으로
                var localRatio = (ratio - 0.8f) / 0.2f;
                colors.Add(Color.FromArgb(255,
                    (byte)(255),
                    (byte)(255 - localRatio * 155),
                    (byte)(0)));
            }
        }

        return colors;
    }

    // 더 부드러운 전환을 위한 그라데이션 오버레이 방식
    private void DrawContourPathsSuperSmooth(List<GeometryPoint> originalPoints, int numberOfContours, double contourSpacing)
    {
        // 더 많은 등고선을 생성하여 부드러운 전환 효과 만들기
        var smoothContours = numberOfContours * 2; // 2배 더 많은 등고선
        var smoothSpacing = contourSpacing / 2.0; // 간격을 절반으로

        var colors = GenerateHeatMapColorsSmooth(smoothContours);

        // 가장 바깥쪽 등고선
        DrawSinglePathWithAlpha(originalPoints, 1, colors[0], 0.5);

        // 부드러운 전환을 위한 촘촘한 등고선들
        for (var i = 1; i < smoothContours && i < colors.Count; i++)
        {
            var offsetPoints = CreateOffsetPoints(originalPoints, i * smoothSpacing, true);
            if (offsetPoints != null && offsetPoints.Count > 1)
            {
                // 각 등고선의 투명도를 조절하여 부드러운 블렌딩 효과
                var alpha = 0.3 + (i * 0.02); // 매우 낮은 투명도로 시작
                alpha = Math.Min(alpha, 0.7); // 최대 70%까지

                DrawSinglePathWithAlpha(offsetPoints, 1, colors[i], alpha);
            }
        }
    }

    // 사용자 정의 부드러운 전환 등고선
    private void DrawContourPathsCustomSmooth(List<GeometryPoint> originalPoints, int numberOfContours,
        double contourSpacing, Color startColor, Color endColor, int smoothnessFactor = 2)
    {
        // 부드러움 정도에 따른 실제 등고선 수
        int actualContours = numberOfContours * smoothnessFactor;
        double actualSpacing = contourSpacing / smoothnessFactor;

        // 시작색에서 끝색까지의 보간된 색상들 생성
        var colors = InterpolateColorsSmooth(startColor, endColor, actualContours);

        // 가장 바깥쪽 등고선
        DrawSinglePathWithAlpha(originalPoints, 1, colors[0], 0.4);

        // 부드러운 전환 등고선들
        for (int i = 1; i < actualContours && i < colors.Count; i++)
        {
            List<GeometryPoint> offsetPoints = CreateOffsetPoints(originalPoints, i * actualSpacing, true);
            if (offsetPoints != null && offsetPoints.Count > 1)
            {
                // 투명도를 점진적으로 증가 (더 부드러운 블렌딩)
                double alpha = 0.2 + (i * 0.8 / actualContours);
                alpha = Math.Min(alpha, 0.8);

                DrawSinglePathWithAlpha(offsetPoints, 1, colors[i], alpha);
            }
        }
    }

    private List<Color> InterpolateColorsSmooth(Color startColor, Color endColor, int steps)
    {
        var colors = new List<Color>();

        for (int i = 0; i < steps; i++)
        {
            float ratio = (float)i / Math.Max(1, steps - 1);

            // 부드러운 곡선 보간 (ease-in-out 효과)
            float smoothRatio = (float)(0.5 * (1 + Math.Sin(Math.PI * (ratio - 0.5))));

            byte r = (byte)(startColor.R + (endColor.R - startColor.R) * smoothRatio);
            byte g = (byte)(startColor.G + (endColor.G - startColor.G) * smoothRatio);
            byte b = (byte)(startColor.B + (endColor.B - startColor.B) * smoothRatio);
            byte a = (byte)(startColor.A + (endColor.A - startColor.A) * smoothRatio);

            colors.Add(Color.FromArgb(a, r, g, b));
        }

        return colors;
    }

    // 완전히 매끄러운 그라데이션 효과 (층 없이)
    private void DrawSmoothHeatMap(List<GeometryPoint> originalPoints, Color centerColor, Color edgeColor)
    {
        if (originalPoints == null || originalPoints.Count <= 1)
            return;

        var IsClosedCurve = true;

        var myPathFigure = new PathFigure { StartPoint = ConvertToVisualPoint(originalPoints[0]) };
        var myPathSegmentCollection = new PathSegmentCollection();
        var bezierCurve = Rulyotano.Math.Interpolation.Bezier.BezierInterpolation.PointsToBezierCurves(originalPoints, IsClosedCurve);

        if (bezierCurve == null || bezierCurve.Segments.Count() < 1)
        {
            foreach (var point in originalPoints.GetRange(1, originalPoints.Count - 1))
            {
                myPathSegmentCollection.Add(new LineSegment { Point = ConvertToVisualPoint(point) });
            }
        }
        else
        {
            foreach (var bezierCurveSegment in bezierCurve.Segments)
            {
                var segment = new BezierSegment
                {
                    Point1 = ConvertToVisualPoint(bezierCurveSegment.FirstControlPoint),
                    Point2 = ConvertToVisualPoint(bezierCurveSegment.SecondControlPoint),
                    Point3 = ConvertToVisualPoint(bezierCurveSegment.EndPoint)
                };
                myPathSegmentCollection.Add(segment);
            }
        }

        myPathFigure.Segments = myPathSegmentCollection;
        myPathFigure.IsClosed = IsClosedCurve;

        var myPathFigureCollection = new PathFigureCollection { myPathFigure };
        var myPathGeometry = new PathGeometry { Figures = myPathFigureCollection };

        // 완전히 부드러운 방사형 그라데이션 생성
        var smoothGradientBrush = CreateUltraSmoothRadialGradient(originalPoints, centerColor, edgeColor);

        var path = new Path
        {
            Fill = smoothGradientBrush,
            Data = myPathGeometry
        };

        HeatMapCanvas.Children.Add(path);
    }

    // 매우 부드러운 방사형 그라데이션 브러시 생성
    private Brush CreateUltraSmoothRadialGradient(List<GeometryPoint> points, Color centerColor, Color edgeColor)
    {
        var radialBrush = new RadialGradientBrush();

        // 경계 상자와 중심점 계산
        var minX = points.Min(p => p.X);
        var maxX = points.Max(p => p.X);
        var minY = points.Min(p => p.Y);
        var maxY = points.Max(p => p.Y);

        // 다중 그라데이션 스톱으로 매끄러운 전환 만들기
        var gradientStops = CreateSmoothGradientStops(centerColor, edgeColor, 20); // 20개 스톱으로 부드러움 극대화

        foreach (var stop in gradientStops)
        {
            radialBrush.GradientStops.Add(stop);
        }

        radialBrush.Center = new Windows.Foundation.Point(0.5, 0.5);
        radialBrush.RadiusX = 0.5;
        radialBrush.RadiusY = 0.5;

        return radialBrush;
    }

    // 매끄러운 그라데이션 스톱 생성
    private List<GradientStop> CreateSmoothGradientStops(Color centerColor, Color edgeColor, int stopCount)
    {
        var stops = new List<GradientStop>();

        for (int i = 0; i < stopCount; i++)
        {
            double offset = (double)i / (stopCount - 1);

            // 부드러운 곡선 보간 (ease-out 효과)
            double smoothOffset = 1.0 - Math.Pow(1.0 - offset, 2.0);

            // 색상 보간
            byte r = (byte)(centerColor.R + (edgeColor.R - centerColor.R) * smoothOffset);
            byte g = (byte)(centerColor.G + (edgeColor.G - centerColor.G) * smoothOffset);
            byte b = (byte)(centerColor.B + (edgeColor.B - centerColor.B) * smoothOffset);
            byte a = (byte)(centerColor.A + (edgeColor.A - centerColor.A) * smoothOffset);

            var interpolatedColor = Color.FromArgb(a, r, g, b);

            stops.Add(new GradientStop
            {
                Color = interpolatedColor,
                Offset = offset
            });
        }

        return stops;
    }

#endif


    private bool IsComplication(WExpertAlgorithmsType type) =>
    type is WExpertAlgorithmsType.RUPTURE
        or WExpertAlgorithmsType.SILICONE_INVASION_TO_LN
        or WExpertAlgorithmsType.SILICONE_INVASION_TO_CAPSULE
        or WExpertAlgorithmsType.FOLDING
        or WExpertAlgorithmsType.FLUID_COLLECTION
        or WExpertAlgorithmsType.THICKENED_CAPSULE
        or WExpertAlgorithmsType.UPSIDE_DOWN_ROTATION
        or WExpertAlgorithmsType.CAPSULAR_MASS
        or WExpertAlgorithmsType.CAPSULAR_CALCIFICATION;

    public void SetPathData(List<AnalysisMenusOut>? analysisMenus, Dictionary<WExpertAlgorithmsType, JArray> algorithmPointsList)
    {
        ClearCanvas(); // 기존 사용 하던 Path 존재시 삭제 처리

        if (algorithmPointsList.Count == 0)
        {
            return;
        }

        foreach (var token in algorithmPointsList.Where(t => t.Value?.Type == JTokenType.Array))
        {
            var polygonArray = token.Value!.ToObject<JArray>()!;

            foreach (var polygon in polygonArray.OfType<JArray>())
            {
                var pointsList = polygon
                    .OfType<JArray>()
                    .Where(p => p.Count == 2)
                    .Select(p => new GeometryPoint((int)p[0], (int)p[1]))
                    .ToList();

                if (pointsList.Count < 3)
                {
                    continue;
                }

                var roi = analysisMenus?.SelectMany(menu => menu.Items).FirstOrDefault(item => item.Id == token.Key);

                Color? fillColor = roi?.roiStatus?.FillColor == "heatmap" ? null : DisplayUtils.HexToColor(roi?.roiStatus?.FillColor);
                var borderColor = DisplayUtils.HexToColor(roi?.roiStatus?.BorderColor);
                var borderThickness = roi?.roiStatus?.BorderThickness ?? 0;

                DrawPath(pointsList, fillColor, borderColor, borderThickness);
#if false
                // 성능 최적화 버전
                //DrawContourPathsOptimized(pointsList, 5, 10.0);

                // 기본 사용 버전
                //DrawContourPaths(pointsList, 5, 10.0);

                // 고급 제어 버전
                //var offsetPoints = CreateOffsetPointsAdvanced(pointsList, 15.0, true, JoinType.Round, 3.0);
                //DrawContourPathsOptimized(offsetPoints, 5, 10.0);


                // 기본 그라데이션 등고선
                //DrawContourPathsWithGradient(pointsList, 5, 10.0);

                // 히트맵 스타일 (파란색→초록색→노란색→빨간색)
                //DrawContourPathsHeatMapStyle(pointsList, 5, 10.0);


                // 사용자 정의 그라데이션
                //var startColor = Color.FromArgb(63, 153, 217, 234); 
                //var endColor = Color.FromArgb(255, 255, 0, 0);   
                //DrawContourPathsCustomGradient(pointsList, 5, 10.0, startColor, endColor, GradientDirection.RadialFromCenter);

                // 기본 페이드 효과
                // 중심에서 가장자리로 갈수록 투명해짐
                //DrawContourPathsWithFadeEffect(pointsList, 5, 10.0);

                // 사용자 정의 페이드 효과
                // fadeStartPoint: 언제부터 투명해지기 시작할지 (0.0~1.0, 0.3이면 30%지점부터)
                // fadeIntensity: 페이드 강도 (0.0~1.0, 1.0이면 가장자리에서 완전투명)
                //DrawContourPathsWithCustomFade(pointsList, 5, 10.0, 0.2, 0.8);

                // 기본 부드러운 등고선(권장)
                //DrawContourPathsOptimizedSmooth(pointsList, 5, 10.0);

                // 매우 부드러운 전환 (더 촘촘한 등고선)
                //DrawContourPathsSuperSmooth(pointsList, 5, 10.0);

                // 사용자 정의 부드러운 전환
                var startColor = Color.FromArgb(255, 0, 128, 0);   // 파란색
                var endColor = Color.FromArgb(255, 255, 0, 0);     // 빨간색

                // smoothnessFactor: 2 = 2배 더 촘촘한 등고선 (더 부드러움)
                DrawContourPathsCustomSmooth(pointsList, 5, 10.0, startColor, endColor, 3);
#endif
            }
        }
    }

#if false // ROI Marking 만 저장 (임상용 기능)
    /// <summary>
    /// 히트맵 결과 이미지 정보 반환
    /// </summary>
    /// <returns>이미지 데이터, width:이미지 넓이, height:이미지 높이</returns>
    public async Task<(byte[]? pixels, int width, int height)> GetHeatmapImageAsync()
    {
        var width = (int)usImageGrd.ActualWidth;
        var height = (int)usImageGrd.ActualHeight;

        // 임시로 배경을 검게 처리
        usImageBackground.Visibility = Visibility.Visible;

        // RenderTargetBitmap을 생성합니다
        var rtb = new RenderTargetBitmap();
        await rtb.RenderAsync(usImageGrd);

        // 픽셀 버퍼를 가져옵니다
        var pixelBuffer = await rtb.GetPixelsAsync();
        var pixels = pixelBuffer.ToArray();


        // 그려진 내용은 흰색으로 변경
        for (var i = 0; i < pixels.Length; i += 4)
        {
            var blue = pixels[i];
            var green = pixels[i + 1];
            var red = pixels[i + 2];
            //var alpha = pixels[i + 3];

            if (/*alpha > 0 ||*/ blue > 0 || green > 0 || red > 0)
            {
                // 모든 픽셀을 흰색으로 변경
                pixels[i] = 255; // Blue
                pixels[i + 1] = 255; // Green
                pixels[i + 2] = 255; // Red
                pixels[i + 3] = 255; // Alpha
            }
            //else
            //{
            //    // 배경은 검정색으로
            //    pixels[i] = 0;   // Blue
            //    pixels[i + 1] = 0;   // Green
            //    pixels[i + 2] = 0;   // Red
            //    pixels[i + 3] = 255; // Alpha
            //}
        }

        // 처리 완료후 원복
        usImageBackground.Visibility = Visibility.Collapsed;

        return (pixels, width, height);
    }
#else
    /// <summary>
    /// 히트맵 결과 이미지 정보 반환
    /// </summary>
    /// <returns>이미지 데이터, width:이미지 넓이, height:이미지 높이</returns>
    public async Task<(byte[]? pixels, int width, int height)> GetHeatmapImageAsync()
    {
        var width = (int)usImageGrd.ActualWidth;
        var height = (int)usImageGrd.ActualHeight;

        // RenderTargetBitmap을 생성합니다
        var rtb = new RenderTargetBitmap();
        await rtb.RenderAsync(usImageGrd);

        // 픽셀 버퍼를 가져옵니다
        var pixelBuffer = await rtb.GetPixelsAsync();
        var pixels = pixelBuffer.ToArray();

        return (pixels, width, height);
    }
#endif

    /* 참고 코드...결과 이미지 전체 저장
    public async Task SaveGridImageAsync()
    {

        // 원본 이미지의 크기를 가져옵니다.
        var originalWidth = usImage.ActualWidth;
        var originalHeight = usImage.ActualHeight;

        // RenderTargetBitmap을 사용하여 Grid의 내용을 캡처합니다.
        var renderTargetBitmap = new RenderTargetBitmap();
        await renderTargetBitmap.RenderAsync(usImageGrd);

        // 픽셀 버퍼를 가져옵니다.
        var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();

        // 저장할 파일을 생성합니다.
        var file = await KnownFolders.PicturesLibrary.CreateFileAsync("SavedImage.png", CreationCollisionOption.ReplaceExisting);

        using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);

        // 원본 크기로 인코더의 크기를 설정합니다.
        encoder.SetPixelData(
            BitmapPixelFormat.Bgra8,
            BitmapAlphaMode.Premultiplied,
            (uint)originalWidth,
            (uint)originalHeight,
            96, // DPI
            96, // DPI
            pixelBuffer.ToArray()
        );

        await encoder.FlushAsync();
    }
    */


    private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
    {
        var scrollViewer = sender as ScrollViewer;

        if (e.IsIntermediate ||
            scrollViewer == null ||
            DisplaySource == null)
        {
            return;
        }

        WExpertLogger.Instance.Debug("[USViewerControl]ScrollViewer_ViewChanged IsIntermediate: " + e.IsIntermediate);      

        Zoom = (float)Math.Round(scrollViewer.ZoomFactor, 2);
        RealSize = Zoom == 1.0f;
        FitToScreen = GetFitToScreenZoomFactor() == Zoom;

        UpdateCursor();
    }

    private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (FitToScreen)
        {
            SetZoom(null, null, null, true);
        }
    }

    /// <summary>
    /// Ctrl + Wheel 시 확대 축소 동작 처리
    /// ZoomMode 를 Enable 로 하면 자동으로 처리가 되나 세세한 튜닝이 불가하여 직접 구현 처리
    /// </summary>
    /// <param name="sender">ScrollViewer object</param>
    /// <param name="e">PointerRoutedEventArgs</param>
    /// <returns></returns>
    private void ScrollViewer_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        var scrollViewer = (ScrollViewer)sender;
        var ctrlPressed = e.KeyModifiers == VirtualKeyModifiers.Control;

        if (ctrlPressed)
        {
            e.Handled = true;

            var delta = e.GetCurrentPoint(scrollViewer).Properties.MouseWheelDelta;
            var currentZoom = scrollViewer.ZoomFactor;
            var zoomChange = delta > 0 ? ZoomIncrement : -ZoomIncrement;
            var newZoom = Math.Clamp(currentZoom + zoomChange, scrollViewer.MinZoomFactor, scrollViewer.MaxZoomFactor);

            // 가장 가까운 10% 단위로 반올림
            newZoom = (float)Math.Round(newZoom / ZoomIncrement) * ZoomIncrement;

            // 마우스 포인터 위치 가져오기 (ScrollViewer 내부 좌표계)
            var pointerPosition = e.GetCurrentPoint(scrollViewer).Position;

            // 현재 스크롤 위치
            var oldOffsetX = scrollViewer.HorizontalOffset;
            var oldOffsetY = scrollViewer.VerticalOffset;

            // 마우스 포인터의 콘텐츠 내 위치 계산
            var contentX = oldOffsetX + pointerPosition.X;
            var contentY = oldOffsetY + pointerPosition.Y;

            // 새로운 스크롤 위치 계산
            var newOffsetX = (contentX * newZoom / currentZoom) - pointerPosition.X;
            var newOffsetY = (contentY * newZoom / currentZoom) - pointerPosition.Y;

            // 뷰 변경 적용
            SetZoom(newOffsetX, newOffsetY, newZoom, false);
        }
    }

    #region image drag move
    private void UpdateCursor()
    {
        if (ScrollViewer.ComputedHorizontalScrollBarVisibility == Visibility.Visible ||
            ScrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible)
        {
            ProtectedCursor = _isDragging ? _grabCursor : _handCursor;
        }
        else
        {
            ProtectedCursor = _arrowCursor;
        }
    }

    private void ScrollViewer_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        UpdateCursor();
    }

    private void ScrollViewer_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        ProtectedCursor = _arrowCursor;
    }

    private void ScrollViewer_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        _isDragging = true;
        _lastPosition = e.GetCurrentPoint(ScrollViewer).Position;
        UpdateCursor();
        ScrollViewer.CapturePointer(e.Pointer);
    }

    private void ScrollViewer_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        _isDragging = false;
        ScrollViewer.ReleasePointerCapture(e.Pointer);
        UpdateCursor();
    }

    private void ScrollViewer_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (_isDragging)
        {
            var currentPosition = e.GetCurrentPoint(ScrollViewer).Position;
            var deltaX = (_lastPosition.X - currentPosition.X) * DragSpeedMultiplier;
            var deltaY = (_lastPosition.Y - currentPosition.Y) * DragSpeedMultiplier;

            SetZoom(ScrollViewer.HorizontalOffset + deltaX, ScrollViewer.VerticalOffset + deltaY, null, false);

            _lastPosition = currentPosition;
        }
    }
    #endregion
}

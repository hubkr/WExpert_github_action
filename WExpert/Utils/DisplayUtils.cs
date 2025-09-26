
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using WExpert.Code;
using Windows.Graphics;
using Windows.UI;

namespace WExpert.Utils;

public class DisplayUtils
{
    /// <summary>
    /// Bitmap 이미지의 Vertical Flip 수행
    /// </summary>
    /// <param name="source">원본 source bitmap 데이터</param>
    /// <returns>수행 결과 bitmap 데이터</returns>
    public static WriteableBitmap? VerticalFlip(WriteableBitmap? source)
    {
        if (source == null)
        {
            return null;
        }
        var width = source.PixelWidth;
        var height = source.PixelHeight;
        var resultBitmap = new WriteableBitmap(width, height);
        var pixelBuffer = new byte[4 * width * height];
        var resultBuffer = new byte[4 * width * height];

        using (var sourceStream = source.PixelBuffer.AsStream())
        {
            sourceStream.Read(pixelBuffer, 0, pixelBuffer.Length);
        }

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var sourceIndex = 4 * (y * width + x);
                var targetIndex = 4 * ((height - 1 - y) * width + x);

                // Copy pixel data from source to flipped position
                resultBuffer[targetIndex] = pixelBuffer[sourceIndex];         // Blue
                resultBuffer[targetIndex + 1] = pixelBuffer[sourceIndex + 1]; // Green
                resultBuffer[targetIndex + 2] = pixelBuffer[sourceIndex + 2]; // Red
                resultBuffer[targetIndex + 3] = pixelBuffer[sourceIndex + 3]; // Alpha
            }
        }

        using (var resultStream = resultBitmap.PixelBuffer.AsStream())
        {
            resultStream.Write(resultBuffer, 0, resultBuffer.Length);
        }

        return resultBitmap;
    }

    /// <summary>
    /// Bitmap 이미지의 Horizontal Flip 수행
    /// </summary>
    /// <param name="source">원본 source bitmap 데이터</param>
    /// <returns>수행 결과 bitmap 데이터</returns>
    public static WriteableBitmap? HorizontalFlip(WriteableBitmap? source)
    {
        if (source == null)
        {
            return null;
        }
        var width = source.PixelWidth;
        var height = source.PixelHeight;
        var resultBitmap = new WriteableBitmap(width, height);
        var pixelBuffer = new byte[4 * width * height];
        var resultBuffer = new byte[4 * width * height];

        using (var sourceStream = source.PixelBuffer.AsStream())
        {
            sourceStream.Read(pixelBuffer, 0, pixelBuffer.Length);
        }

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var sourceIndex = 4 * (y * width + x);
                var targetIndex = 4 * (y * width + (width - 1 - x));

                // Copy pixel data from source to flipped position
                resultBuffer[targetIndex] = pixelBuffer[sourceIndex];         // Blue
                resultBuffer[targetIndex + 1] = pixelBuffer[sourceIndex + 1]; // Green
                resultBuffer[targetIndex + 2] = pixelBuffer[sourceIndex + 2]; // Red
                resultBuffer[targetIndex + 3] = pixelBuffer[sourceIndex + 3]; // Alpha
            }
        }

        using (var resultStream = resultBitmap.PixelBuffer.AsStream())
        {
            resultStream.Write(resultBuffer, 0, resultBuffer.Length);
        }

        return resultBitmap;
    }

    /// <summary>
    /// 입력 X,Y 좌표가 속하는 디스플레이 영역의 존재 유무 확인
    /// </summary>
    /// <param name="x">x 좌표</param>
    /// <param name="y">y 좌표</param>
    /// <returns>존재 유무 반환(bool)</returns>
    public static bool IsInRangeScreen(int x, int y)
    {
        var point = new PointInt32(x, y); // 현재 윈도우의 x,y 좌표를 지정합니다.
        //  포인트가 어떤 디스플레이 영역에도 속하지 않을 경우 false
        var displayArea = DisplayArea.GetFromPoint(point, DisplayAreaFallback.None);
        if (displayArea != null)
        {
            // Outbounds 가 아닌 WorkArea 로 실제 윈도우가 보이는 영역 기준으로 유효 영역인지 계산
            var isInXArea = (displayArea.WorkArea.X <= x) && x < (displayArea.WorkArea.X + displayArea.WorkArea.Width);
            var isInYArea = (displayArea.WorkArea.Y <= y) && y < (displayArea.WorkArea.Y + displayArea.WorkArea.Height);
            return isInXArea && isInYArea;
        }
        
        return false;
    }

    /*
    /// <summary>
    /// ROIColorType 에 맞는 Color 값을 반환
    /// </summary>
    /// <param name="type">ROIColorType</param>
    /// <returns>Color</returns>
    public static Color ROIColorTypeToColor(ROIColorType type)
    {
        return type switch
        {
            ROIColorType.COLOR1 => (Color)Application.Current.Resources["WSegOutline1"],
            ROIColorType.COLOR2 => (Color)Application.Current.Resources["WSegOutline2"],
            ROIColorType.COLOR3 => (Color)Application.Current.Resources["WSegOutline3"],
            ROIColorType.COLOR4 => (Color)Application.Current.Resources["WSegOutline4"],
            ROIColorType.COLOR5 => (Color)Application.Current.Resources["WSegOutline5"],
            ROIColorType.COLOR6 => (Color)Application.Current.Resources["WSegOutline6"],
            ROIColorType.COLOR7 => (Color)Application.Current.Resources["WSegOutline7"],
            ROIColorType.COLOR8 => (Color)Application.Current.Resources["WSegOutline8"],
            _ => Colors.Transparent
        };
    }
    */

    /// <summary>
    /// Hex 컬러 값을 Color 값으로 변환
    /// </summary>
    /// <param name="hex">hex 컬러 값</param>
    /// <returns>Color 변환 결과</returns>
    public static Color HexToColor(string? hex)
    {
        if (string.IsNullOrEmpty(hex))
        {
            return Color.FromArgb(0, 0, 0, 0); // transparent
        }

        if (hex.StartsWith("#"))
        {
            hex = hex.Substring(1);
        }

        byte a, r, g, b;
        if (hex.Length == 8)
        {
            a = Convert.ToByte(hex.Substring(0, 2), 16);
            r = Convert.ToByte(hex.Substring(2, 2), 16);
            g = Convert.ToByte(hex.Substring(4, 2), 16);
            b = Convert.ToByte(hex.Substring(6, 2), 16);
        }
        else if (hex.Length == 6)
        {
            a = 255;
            r = Convert.ToByte(hex.Substring(0, 2), 16);
            g = Convert.ToByte(hex.Substring(2, 2), 16);
            b = Convert.ToByte(hex.Substring(4, 2), 16);
        }
        else
        {
            WExpertLogger.Instance.Error("Color convert error(invalid parameter).");
            return Color.FromArgb(0, 0, 0, 0); // transparent
        }

        return Color.FromArgb(a, r, g, b);
    }
}

using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.DirectX;

namespace WExpert.Utils;

public static class ImageFilter
{
    public static async Task<WriteableBitmap?> ApplyFilters(WriteableBitmap? source, int brightness, int contrast, float sharpness)
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

        // 모든 필터 값이 0인 경우 원본 그대로 반환
        if (brightness == 0 && contrast == 0 && sharpness == 0)
        {
            Buffer.BlockCopy(pixelBuffer, 0, resultBuffer, 0, pixelBuffer.Length);
        }
        else
        {
            // 사전 계산된 lookup table 생성 (brightness + contrast가 둘 다 0이 아닌 경우)
            byte[]? lookupTable = null;
            if (brightness != 0 || contrast != 0)
            {
                lookupTable = new byte[256];
                // 올바른 contrast 계수 계산: contrast가 -100~100 범위라고 가정
                var contrastFactor = contrast != 0 ? (259.0 * (contrast + 255.0)) / (255.0 * (259.0 - contrast)) : 1.0;

                for (var i = 0; i < 256; i++)
                {
                    double value = i;

                    // contrast 먼저 적용 (128을 중심으로)
                    if (contrast != 0)
                    {
                        value = contrastFactor * (value - 128.0) + 128.0;
                    }

                    // brightness 적용
                    if (brightness != 0)
                    {
                        value += brightness;
                    }

                    // 클램핑
                    lookupTable[i] = (byte)Math.Min(255, Math.Max(0, value));
                }
            }

            var totalPixels = width * height;
            Parallel.For(0, height, y =>
            {
                var rowStart = y * width * 4;
                var rowEnd = rowStart + width * 4;

                for (var idx = rowStart; idx < rowEnd; idx += 4)
                {
                    if (lookupTable != null)
                    {
                        // Lookup table 사용으로 계산 최소화
                        resultBuffer[idx] = lookupTable[pixelBuffer[idx]];         // Blue
                        resultBuffer[idx + 1] = lookupTable[pixelBuffer[idx + 1]]; // Green
                        resultBuffer[idx + 2] = lookupTable[pixelBuffer[idx + 2]]; // Red
                    }
                    else
                    {
                        // 필터가 없는 경우 직접 복사
                        resultBuffer[idx] = pixelBuffer[idx];
                        resultBuffer[idx + 1] = pixelBuffer[idx + 1];
                        resultBuffer[idx + 2] = pixelBuffer[idx + 2];
                    }

                    resultBuffer[idx + 3] = pixelBuffer[idx + 3]; // Alpha (항상 복사)
                }
            });
        }

        using (var resultStream = resultBitmap.PixelBuffer.AsStream())
        {
            resultStream.Write(resultBuffer, 0, resultBuffer.Length);
        }

        // Sharpness가 0이 아닌 경우에만 적용
        return sharpness == 0 ? resultBitmap : await Sharpness(resultBitmap, sharpness);
    }




    // 최적화된 Sharpness 구현 (Unsharp Mask 기법)
    private static async Task<WriteableBitmap> Sharpness(WriteableBitmap source, float amount, float radius = 1.0f, float threshold = 0)
    {
        // amount 범위 제한 및 정규화
        amount = Math.Max(-150, Math.Min(150, amount));
        var normalizedAmount = amount / 150.0f;

        var width = source.PixelWidth;
        var height = source.PixelHeight;
        var resultBitmap = new WriteableBitmap(width, height);
        var pixelBuffer = new byte[4 * width * height];
        var resultBuffer = new byte[4 * width * height];

        using (var sourceStream = source.PixelBuffer.AsStream())
        {
            sourceStream.Read(pixelBuffer, 0, pixelBuffer.Length);
        }

        // radius 범위 제한
        radius = Math.Max(0.1f, radius);

        // 가우시안 커널 생성
        var kernelSize = Math.Max(3, (int)Math.Ceiling(radius * 2));
        if (kernelSize % 2 == 0) kernelSize++;
        var kernel = CreateGaussianKernel(kernelSize, radius);

        // 블러된 이미지를 미리 계산 (성능 최적화)
        var blurredBuffer = new float[3 * width * height]; // RGB만 저장
        Parallel.For(0, height, y =>
        {
            for (var x = 0; x < width; x++)
            {
                var blurredIndex = (y * width + x) * 3;
                ApplyKernelOptimized(x, y, width, height, kernelSize, kernel, pixelBuffer, blurredBuffer, blurredIndex);
            }
        });

        // Unsharp mask 적용
        Parallel.For(0, height, y =>
        {
            for (var x = 0; x < width; x++)
            {
                var pixelIndex = (y * width + x) * 4;
                var blurredIndex = (y * width + x) * 3;

                // RGB 채널만 처리 (Alpha는 원본 유지)
                for (var i = 0; i < 3; i++)
                {
                    var original = pixelBuffer[pixelIndex + i];
                    var blurred = blurredBuffer[blurredIndex + i];
                    var diff = original - blurred;

                    // threshold 적용
                    if (Math.Abs(diff) > threshold)
                    {
                        var sharpened = original + normalizedAmount * diff * 5;
                        resultBuffer[pixelIndex + i] = (byte)Math.Max(0, Math.Min(255, sharpened));
                    }
                    else
                    {
                        resultBuffer[pixelIndex + i] = (byte)original;
                    }
                }

                // Alpha 채널은 원본 그대로 복사
                resultBuffer[pixelIndex + 3] = pixelBuffer[pixelIndex + 3];
            }
        });

        using (var resultStream = resultBitmap.PixelBuffer.AsStream())
        {
            resultStream.Write(resultBuffer, 0, resultBuffer.Length);
        }

        return resultBitmap;
    }

    // 최적화된 커널 적용 (RGB만 처리)
    private static void ApplyKernelOptimized(int x, int y, int width, int height, int kernelSize, float[,] kernel, byte[] pixelBuffer, float[] blurredBuffer, int blurredIndex)
    {
        var halfSize = kernelSize / 2;
        float r = 0, g = 0, b = 0;

        for (var ky = -halfSize; ky <= halfSize; ky++)
        {
            for (var kx = -halfSize; kx <= halfSize; kx++)
            {
                var px = Math.Min(Math.Max(x + kx, 0), width - 1);
                var py = Math.Min(Math.Max(y + ky, 0), height - 1);
                var offset = (py * width + px) * 4;
                var weight = kernel[ky + halfSize, kx + halfSize];

                b += pixelBuffer[offset] * weight;     // Blue
                g += pixelBuffer[offset + 1] * weight; // Green
                r += pixelBuffer[offset + 2] * weight; // Red
            }
        }

        blurredBuffer[blurredIndex] = b;
        blurredBuffer[blurredIndex + 1] = g;
        blurredBuffer[blurredIndex + 2] = r;
    }

    // 가우시안 커널 생성 (기존과 동일하지만 캐싱 최적화 가능)
    private static readonly Dictionary<(int size, float sigma), float[,]> _kernelCache = [];

    private static float[,] CreateGaussianKernel(int size, float sigma)
    {
        var key = (size, sigma);
        if (_kernelCache.TryGetValue(key, out var cachedKernel))
        {
            return cachedKernel;
        }

        if (size < 3 || size % 2 == 0)
        {
            throw new ArgumentException("Kernel size must be an odd number greater than or equal to 3", nameof(size));
        }

        var kernel = new float[size, size];
        var sum = 0.0f;
        var halfSize = size / 2;
        var twoSigmaSquared = 2 * sigma * sigma;
        var coefficient = 1.0f / (MathF.PI * twoSigmaSquared);

        for (var x = -halfSize; x <= halfSize; x++)
        {
            for (var y = -halfSize; y <= halfSize; y++)
            {
                var exponent = -(x * x + y * y) / twoSigmaSquared;
                var value = coefficient * MathF.Exp(exponent);
                kernel[x + halfSize, y + halfSize] = value;
                sum += value;
            }
        }

        // 정규화
        if (sum != 0)
        {
            for (var x = 0; x < size; x++)
            {
                for (var y = 0; y < size; y++)
                {
                    kernel[x, y] /= sum;
                }
            }
        }

        // 캐시에 저장 (메모리 사용량 주의)
        if (_kernelCache.Count < 100) // 캐시 크기 제한
        {
            _kernelCache[key] = kernel;
        }

        return kernel;
    }
}
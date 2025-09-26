
using Windows.Storage.Pickers;
using Windows.Storage;
using WinRT.Interop;
using Microsoft.Win32;
using System.Net.Mime;
using WExpert.Models;
using WExpert.Helpers;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml;
using System.Runtime.InteropServices.WindowsRuntime;
using WExpert.Code;
using WExpert.Contracts.Services;

namespace WExpert.Utils;
public class FileUtils
{
    private static string AppDirectory()
    {
        return string.Format("{0}{1}{2}",
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Path.DirectorySeparatorChar, "AppName".GetLocalized());
    }
    
    private static string CreateIdentifierDirectory(string wexpertId)
    {
        var destPath = AppDirectory();
        if (!Directory.Exists(destPath))
        {
            Directory.CreateDirectory(destPath);
        }

        destPath = GetDirectory(DirectoryType.DATA);
        if (!Directory.Exists(destPath))
        {
            Directory.CreateDirectory(destPath);
        }

        destPath = GetDirectory(DirectoryType.DATA_FILES);
        if (!Directory.Exists(destPath))
        {
            Directory.CreateDirectory(destPath);
        }

        destPath = string.Format("{0}{1}{2}", destPath, Path.DirectorySeparatorChar, DateTime.Now.ToString("yyyyMM"));
        if (!Directory.Exists(destPath))
        {
            Directory.CreateDirectory(destPath);
        }

        destPath = string.Format("{0}{1}{2}", destPath, Path.DirectorySeparatorChar, wexpertId);
        if (!Directory.Exists(destPath))
        {
            Directory.CreateDirectory(destPath);
        }

        return destPath;
    }

    public static string GetDirectory(DirectoryType type)
    {
        var strAppDirectory = AppDirectory();
        var strResultDirectory = type switch
        {
            DirectoryType.SETTINGS => string.Format("{0}{1}settings", strAppDirectory, Path.DirectorySeparatorChar),
            DirectoryType.DATA => string.Format("{0}{1}data", strAppDirectory, Path.DirectorySeparatorChar),
            DirectoryType.DATA_FILES => string.Format("{0}{1}data{2}files", strAppDirectory, Path.DirectorySeparatorChar, Path.DirectorySeparatorChar),
            DirectoryType.CACHE => string.Format("{0}{1}cache", strAppDirectory, Path.DirectorySeparatorChar),
            DirectoryType.DUMP => string.Format("{0}{1}dump", strAppDirectory, Path.DirectorySeparatorChar),
            _ => strAppDirectory,
        };
        CreateDirectory(strResultDirectory);

        return strResultDirectory;
    }

    public static void CreateDirectory(string path)
    {
        if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
        {
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception e)
            {
                WExpertLogger.Instance.Error(e.ToString());
            }
        }
    }

    // 초음파 영상 파일 데이터를 App이 관리하는 저장 공간 영역으로 복사
    public static async Task<List<string>?> CopyDataFilesAsync(string patientIdentifier, List<string> sourceFiles)
    {
        try
        {
            // 잘못된 환자 식별 변호 또는 빈 source files
            if (string.IsNullOrEmpty(patientIdentifier) ||
                sourceFiles == null || sourceFiles.Count == 0)
            {
                return null;
            }

            foreach (var sourceFile in sourceFiles)
            {
                if (!File.Exists(sourceFile))
                {
                    // 존재 하지 않는 원본 파일이 존재 하는 경우
                    return null;
                }
            }

            var destPath = CreateIdentifierDirectory(patientIdentifier);

            List<string> destFiles = [];
            // 백그라운드 스레드에서 실행
            await Task.Run(() =>
            {
                foreach (var sourceFile in sourceFiles)
                {
                    // Identifier 폴더 내에 동일 한 파일명이 존재 하는지 Check > 새로 등록할 파일명이 존재시 다름이름으로 자동 변경후 반환
                    var fileName = FileUtils.UniqueFileNameInDir(patientIdentifier, Path.GetFileName(sourceFile));
                    var destFile = string.Format("{0}{1}{2}", destPath, Path.DirectorySeparatorChar, fileName);
                    File.Copy(sourceFile, destFile);
                    destFiles.Add(destFile);
                }          
            });

            return destFiles;
        }
        catch (Exception e)
        {
            WExpertLogger.Instance.Error(e.ToString());
        }

        return null;
    }

    // 초음파 영상 파일 데이터를 App이 관리하는 저장 공간 영역으로 복사
    public static async Task<List<string>?> CopyDataFilesAsync(string patientIdentifier, List<NewRegistrationFileInfo> fileInfos)
    {
        try
        {
            // 잘못된 환자 식별 변호 또는 빈 source files
            if (string.IsNullOrEmpty(patientIdentifier) || fileInfos.Count == 0)
            {
                return null;
            }

            foreach (var fileInfo in fileInfos)
            {
                if (!File.Exists(fileInfo.FilePath))
                {
                    // 존재 하지 않는 원본 파일이 존재 하는 경우
                    return null;
                }
            }

            var destPath = CreateIdentifierDirectory(patientIdentifier);

            List<string> destFiles = new();
            // 백그라운드 스레드에서 실행
            await Task.Run(() =>
            {
                foreach (var fileInfo in fileInfos)
                {
                    var destFile = string.Format("{0}{1}{2}", destPath, Path.DirectorySeparatorChar, fileInfo.DestFileName);
                    File.Copy(fileInfo.FilePath, destFile);
                    destFiles.Add(destFile);
                }
            });

            return destFiles;
        }
        catch (Exception e)
        {
            WExpertLogger.Instance.Error(e.ToString());
        }

        return null;
    }

    /*
    public static void DeleteDataFiles(List<string> files)
    {
        if (files == null || files.Count == 0)
        {
            return;
        }

        files.ForEach(f =>
        {
            DeleteAllDataFile(f);
        });
    }
    */

    // 파일이 속한 상위 디렉토리 전체를 삭제
    public static void DeleteDirectory(string file)
    {
        try
        {
            if (!File.Exists(file))
            {
                return;
            }
        
            var dir = Path.GetDirectoryName(file);
            if (Directory.Exists(dir))
            {
                // id 디렉 토리 자체를 삭제
                Directory.Delete(dir, true);
            }            
        }
        catch (Exception e)
        {
            WExpertLogger.Instance.Error(e.ToString());
        }
    }

    // 해당 파일을 삭제
    public static void DeleteFile(string file)
    {
        try
        {
            if (File.Exists(file))
            {
                // 이미지 파일 삭제
                File.Delete(file);
#if DEBUG
                var name = Path.GetFileNameWithoutExtension(file);
                var jsonFile = string.Format(@"{0}\{1}.json", Path.GetDirectoryName(file), name);
                // json 파일 삭제
                if (File.Exists(jsonFile))
                {                
                    File.Delete(jsonFile);
                }
#endif

            }
        }
        catch (Exception e)
        {
            WExpertLogger.Instance.Error(e.ToString());
        }
    }

    public static async Task<StorageFolder?> ShowFolderPickerAsync(string path = "")
    {
        var initPath = string.IsNullOrEmpty(path) ? path : Path.GetFullPath(path);

        if (string.IsNullOrEmpty(initPath))
        {
            var folderPicker = new FolderPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.Desktop
            };
            folderPicker.FileTypeFilter.Add("*");

            var hwnd = App.MainWindow.GetWindowHandle();
            InitializeWithWindow.Initialize(folderPicker, hwnd);
            var folder = await folderPicker.PickSingleFolderAsync();

            return folder;
        }
        else
        {
            // TODO InitialLocation 지정
            return null;
        }
    }

    /// <summary>
    /// 파일의 mime type 확인
    /// </summary>
    /// <param name="filePath">전체 파일 경로</param>
    /// <returns>입력된 파일의 mime type</returns>
    public static string GetMimeType(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        var mimeType = "";

        if (!string.IsNullOrEmpty(extension))
        {
            try
            {
                var key = Registry.ClassesRoot.OpenSubKey(extension);
                if (key != null && key.GetValue("Content Type") != null)
                {
                    mimeType = key.GetValue("Content Type")?.ToString();
                }
            }
            catch(Exception e)
            {
                WExpertLogger.Instance.Error(e.ToString());
            }
        }

        if (string.IsNullOrEmpty(mimeType))
        {
            // MIME 타입을 찾을 수 없는 경우 기본값을 사용할 수 있습니다.
            mimeType = MediaTypeNames.Application.Octet;
        }

        return mimeType;
    }

    /// <summary>
    /// 파일의 항목 유형 확인
    /// </summary>
    /// <param name="mimeType">파일의 mimeType</param>
    /// <returns>파일의 항목 유형</returns>
    public static string GetItemTypeFromMimeType(string mimeType)
    {
        var mimeToItemType = new Dictionary<string, string>
            {
                { "text/plain", "StringItemTypeText".GetLocalized() },
                { "text/html", "StringItemTypeHtml".GetLocalized() },
                { "text/css", "StringItemTypeCSS".GetLocalized() },
                { "text/javascript", "StringItemTypeJavaScript".GetLocalized() },
                { "image/jpeg", "StringItemTypeJpeg".GetLocalized() },
                { "image/png", "StringItemTypePng".GetLocalized() },
                { "image/gif", "StringItemTypeGif".GetLocalized() },
                { "image/bmp", "StringItemTypeBmp".GetLocalized() },
                { "image/svg+xml", "StringItemTypeSvg".GetLocalized() },
                { "application/pdf", "StringItemTypePdf".GetLocalized() },
                { "application/msword", "StringItemTypeWord".GetLocalized() },
                { "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "StringItemTypeWord".GetLocalized() },
                { "application/vnd.ms-excel", "StringItemTypeExcel".GetLocalized() },
                { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "StringItemTypeExcel".GetLocalized() },
                { "application/vnd.ms-powerpoint", "StringItemTypePowerPoint".GetLocalized() },
                { "application/vnd.openxmlformats-officedocument.presentationml.presentation", "StringItemTypePowerPoint".GetLocalized() },
                { "application/zip", "StringItemTypeZip".GetLocalized() },
                { "application/x-rar-compressed", "StringItemTypeRar".GetLocalized() },
                { "application/x-7z-compressed", "StringItemType7Z".GetLocalized() },
                { "application/x-tar", "StringItemTypeTar".GetLocalized() },
                { "application/json", "StringItemTypeJson".GetLocalized() },
                { "application/xml", "StringItemTypeXml".GetLocalized() },
                { "application/x-shockwave-flash", "StringItemTypeFlash".GetLocalized() },
                { "audio/mpeg", "StringItemTypeMp3".GetLocalized() },
                { "audio/wav", "StringItemTypeWav".GetLocalized() },
                { "audio/ogg", "StringItemTypeOgg".GetLocalized() },
                { "video/mp4", "StringItemTypeMp4".GetLocalized() },
                { "video/x-msvideo", "StringItemTypeAvi".GetLocalized() },
                { "video/x-matroska", "StringItemTypeMkv".GetLocalized() },
                { "video/webm", "StringItemTypeWebM".GetLocalized() },
                { "application/dicom", "StringItemTypeDicom".GetLocalized() },
                // 필요시 MIME 타입 및 항목 유형 매핑을 계속 추가.
            };

        return mimeToItemType.ContainsKey(mimeType) ? mimeToItemType[mimeType] : "StringUnknown".GetLocalized();
    }

    /// <summary>
    /// 입력된 파일명중 파일명으로 사용할수 없는 문자 확인
    /// </summary>
    /// <param name="fileName">파일명 문자열</param>
    /// <returns>파일명으로 사용할수 없는 문자 list</returns>
    public static List<char> GetInvalidFileNameChars(string fileName)
    {
        List<char> result = [];

        var invalidChars = Path.GetInvalidFileNameChars();

        foreach (var c in fileName)
        {
            if (Array.Exists(invalidChars, invalidChar => invalidChar == c))
            {
                result.Add(c);
            }
        }

        return result;
    }


    /// <summary>
    /// 입력된 patientIdentifier 폴더내에 fileName 과 동일한 파일명이 존재 하는지 확인 후 겹치지 않는 이름을 반환 
    /// </summary>
    /// <param name="patientIdentifier">환자 식별 번호</param>
    /// <param name="fileName">신규 생성 하고자 하는 파일명</param>
    /// <returns>patientIdentifier 폴더 내에서 사용해도 되는 유니크한 파일명 반환</returns>
    public static string UniqueFileNameInDir(string patientIdentifier, string fileName)
    {
        var path = CreateIdentifierDirectory(patientIdentifier);
        var fileExtension = Path.GetExtension(fileName);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var pathWithFile = Path.Combine(path, fileName);

        var count = 1;
        while (File.Exists(pathWithFile))
        {
            var newFileName = $"{fileNameWithoutExtension}({count}){fileExtension}";
            pathWithFile = Path.Combine(path, newFileName);
            count++;
        }

        return Path.GetFileName(pathWithFile);
    }



    public static async Task<string?> MakeTmpFromClipboaard(FrameworkElement _visualRoot, DataPackageView dataPackageView)
    {
        try
        {
            // Get the BitmapImage and convert to WriteableBitmap
            var imageStreamReference = await dataPackageView.GetBitmapAsync();
            using IRandomAccessStream imageStream = await imageStreamReference.OpenReadAsync();
            var bitmapImage = new BitmapImage();
            await bitmapImage.SetSourceAsync(imageStream);

            var dialogService = App.GetService<IDialogService>();
            var targetFileName = await dialogService.ShowPasteImageDialogAsync(_visualRoot, bitmapImage);

            // file name 가 없는 경우는 취소 버튼 눌렀을경우..
            if (!string.IsNullOrEmpty(targetFileName))
            {
                // 클립 보드 이미지 데이터를 Writeable Bitmap 로 변환
                var writeableBitmap = new WriteableBitmap(bitmapImage.PixelWidth, bitmapImage.PixelHeight);
                imageStream.Seek(0);
                await writeableBitmap.SetSourceAsync(imageStream);

                // Convert WriteableBitmap to byte array
                var pixelStream = writeableBitmap.PixelBuffer.AsStream();
                var pixels = new byte[pixelStream.Length];
                await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                // Temp 폴더 내에 'WExp' 폴더 생성
                var tempFolderPath = Path.Combine(Path.GetTempPath(), "WExp");
                Directory.CreateDirectory(tempFolderPath);

                // 폴더 내에 파일 경로 설정
                var filePath = Path.Combine(tempFolderPath, targetFileName);

                // 임시 파일에 저장
                var folder = await StorageFolder.GetFolderFromPathAsync(tempFolderPath);
                var file = await folder.CreateFileAsync(targetFileName, CreationCollisionOption.ReplaceExisting);
                if (file != null)
                {
                    using var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite);
                    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, fileStream);
                    encoder.SetPixelData(
                        BitmapPixelFormat.Bgra8,
                        BitmapAlphaMode.Ignore,
                        (uint)writeableBitmap.PixelWidth,
                        (uint)writeableBitmap.PixelHeight,
                        96, // Horizontal DPI
                        96, // Vertical DPI
                        pixels);
                    await encoder.FlushAsync();
                }

                return filePath;
            }
        }
        catch (Exception e)
        {
            WExpertLogger.Instance.Error($"Error during make temp file from clipboard data: {e}");
        }

        return null;
    }
}

using System.Net.Http.Headers;
using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using WExpert.Code;
using WExpert.Helpers;
using WExpert.Helpers.Exceptions;
using WExpert.Helpers.Http;
using WExpert.Models;
using WExpert.Models.Dto.Data;
using WExpert.Utils;
using WExpert.ViewModels;
using WExpert.Views.Base;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace WExpert.Views.ContentDialogs;

public sealed partial class RegistrationPatientContentDialog : WEXBaseContentDialog
{
    private bool isDraggable = false;
    //private CreatePatientOut? createPatientOut = null;
    private APIResponse<CreatePatientOut>? apiResponse = null;
    private readonly FrameworkElement _visualRoot;
    public RegistrationPatientContentViewModel ViewModel { get;}

    public RegistrationPatientContentDialog()
    {
        InitializeComponent();

        _visualRoot = (FrameworkElement)App.MainWindow.Content;
        ViewModel = App.GetService<RegistrationPatientContentViewModel>();
        DataContext = ViewModel;

        PrimaryButtonClick += ContentDialog_PrimaryButtonClick;
        SecondaryButtonClick += ContentDialog_SecondaryButtonClick;
    }

    public async new Task<CreatePatientOut?> ShowAsync()
    {
        await base.ShowAsync();

        // 오류 처리
        if (apiResponse?.Result == APIResultType.SUCCESS && apiResponse.Data is CreatePatientOut createPatientOut)
        {
            return createPatientOut;
        }
        else if (apiResponse?.Result == APIResultType.UNAUTHORIZED)
        {            
            var message = CommonUtils.MakeHTTPErrorMessage("StringUnauthorizedMessage".GetLocalized(), apiResponse.ResultCode);
            throw new UnauthorizedException(message);
        }

        return null;
    }

    private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        try
        {
            args.Cancel = true; // 다이얼로그가 자동으로 닫히는 것을 방지
            apiResponse = null;

            var multipartContent = new MultipartFormDataContent();

            // 환자 정보 추가
            var patientData = new
            {
                name = string.IsNullOrWhiteSpace(ViewModel.Name) ? ResourceExtensions.GetLocalized("StringNoName") : ViewModel.Name,
                type = ViewModel.Type.ToString().ToLower()
            };
            var jsonString = JsonConvert.SerializeObject(patientData);
            var jsonContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            multipartContent.Add(jsonContent, "patient");

            // 초음파 이미지 추가
            var addFileCount = 0;
            foreach (var file in ViewModel.NewRegistrationFiles)
            {
                if (File.Exists(file.FilePath))
                {
                    var fileContent = new ByteArrayContent(File.ReadAllBytes(file.FilePath));
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.MimeType);
                    multipartContent.Add(fileContent, "file", file.DestFileName);
                    addFileCount++;
                }
            }

            if (addFileCount == 0)
            {
                // TODO 오류 메시지 출력
                return;
            }

            ViewModel.EnableRegisterButton = false;
            ViewModel.RegisteringProgressRing = true;

            apiResponse = await ViewModel.RegisterPatient(multipartContent);
            Hide(); // 다이얼로그가 닫히도록 처리
        }
        catch (Exception e)
        {
            // 오류시 오류 메시지 출력        
            ViewModel.RegistrationErrorMessage = e.Message;
        }
        finally
        {
            ViewModel.EnableRegisterButton = true;
            ViewModel.RegisteringProgressRing = false;
        }
    }

    private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        Hide();
    }

    private void Dialog_Loaded(object sender, RoutedEventArgs e)
    {
        //AddFileButtonTeachingTip.IsOpen = true;
    }

    // 클립보드 이미지 추가 기능의 경우 사용
    public void AddSourceImage(string orginalFilePath, string newFileName)
    {
        if (!string.IsNullOrEmpty(orginalFilePath) && !string.IsNullOrEmpty(newFileName))
        {
            var listInput = new List<NewRegistrationFileInfo>
            {
                new(orginalFilePath, newFileName, "image/jpeg")
            };
            ViewModel.AddFileItemCommand.Execute(listInput);
        }
    }

    private void OnClose(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    private async void SourceFile_DragEnterAsync(object sender, DragEventArgs e)
    {
        // WExpertLogger.Instance.Debug("====================== SourceFile_DragEnterAsync1");
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var items = await e.DataView.GetStorageItemsAsync();
            // WExpertLogger.Instance.Debug("====================== SourceFile_DragEnterAsync2");
            var files = items.OfType<StorageFile>();

            var supportedTypes = new[] { ".jpg", ".jpeg", ".png" };
            isDraggable = files.Any(file => supportedTypes.Contains(Path.GetExtension(file.Name).ToLowerInvariant()));
        }
        // WExpertLogger.Instance.Debug("====================== SourceFile_DragEnterAsync3");
    }

    private void SourceFile_DragOver(object sender, DragEventArgs e)
    {
        //WExpertLogger.Instance.Debug(string.Format("====================== SourceFile_DragOver isDragable : {0}", isDragable));
        if (isDraggable)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.Caption = "StringDropFile".GetLocalized();
            e.DragUIOverride.IsCaptionVisible = true; // Sets if the caption is visible
            e.DragUIOverride.IsContentVisible = true; // Sets if the dragged content is visible
            e.DragUIOverride.IsGlyphVisible = false; // Sets if the glyph is visibile  
        }
    }

    private async void SourceFile_Drop(object sender, DragEventArgs e)
    {
        // WExpertLogger.Instance.Debug(string.Format("====================== SourceFile_Drop isDragable : {0}", isDragable));
        if (isDraggable && e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var items = await e.DataView.GetStorageItemsAsync();
            var files = items.OfType<StorageFile>();
            ViewModel.AddFileItemCommand.Execute(files.ToList());
        }
    }

    private void DeleteSourceItemClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            if (button.DataContext is NewRegistrationFileInfo newRegistrationFiles)
            {
                ViewModel.DeleteFileItemCommand.Execute(newRegistrationFiles);
            }
        }
    }

    private void SourceFile_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
#if false // TODO 오류로 인해 일단 비 활성화
        var listView = sender as ListView;
        if (args.Phase == 0)
        {
            // Scroll to the newly added item
            listView?.ScrollIntoView(args.Item);
        }
#endif
    }

    private void PatientType_Checked(object sender, RoutedEventArgs e)
    {
        var radio = sender as RadioButton;
        if (radio == null)
        {
            return;
        }

        if (radio.Name == "TypeAesthetic")
        {
            ViewModel.Type = PatientType.AESTHETIC;
        }
        else if (radio.Name == "TypeReconstructive")
        {
            ViewModel.Type = PatientType.RECONSTRUCTIVE;
        }
        else if (radio.Name == "TypeBoth")
        {
            ViewModel.Type = PatientType.BOTH;
        }
    }

    private void Thumbnail_ImageFailed(object sender, ExceptionRoutedEventArgs e)
    {
        if (sender is Image image)
        {
            ViewModel.ThumbnailOpenFailedCommand.Execute(image);
        }
    }
}

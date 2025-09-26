using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WExpert.Code;
using WExpert.Helpers;
using WExpert.Helpers.Exceptions;
using WExpert.Helpers.Http;
using WExpert.Models.Dto.Data;
using WExpert.Utils;
using WExpert.ViewModels;
using WExpert.Views.Base;

namespace WExpert.Views.ContentDialogs;

public sealed partial class RegisterConsultationContentDialog : WEXBaseContentDialog
{
    private string? SonographyId;
    private APIResponse<CreateConsultationOut>? apiResponse = null;
    public RegisterConsultationContentViewModel ViewModel { get; }
    
    public RegisterConsultationContentDialog()
    {
        InitializeComponent();

        ViewModel = App.GetService<RegisterConsultationContentViewModel>();
        DataContext = ViewModel;
    }

    public void SetInformation(string? sonographyId, int consultationQuota, int consultationUsed)
    {
        SonographyId = sonographyId;
        ViewModel.ConsultationQuota = consultationQuota;
        ViewModel.ConsultationUsed = consultationUsed;
        ViewModel.ExceededCount = Math.Max(0, consultationUsed - consultationQuota);
    }

    public async new Task<CreateConsultationOut?> ShowAsync()
    {
        await base.ShowAsync();

        // 오류 처리
        if (apiResponse?.Result == APIResultType.SUCCESS && apiResponse.Data is CreateConsultationOut createConsultationOut)
        {
            return createConsultationOut;
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
            ErrorTeachingTip.IsOpen = false;
            ViewModel.RegisteringProgressRing = true;
            ViewModel.EnableRegisterButton = false;
            args.Cancel = true; // 다이얼로그가 자동으로 닫히는 것을 방지
            await Task.Delay(500);

            var question = ViewModel.QuestionText.Trim();
            var content = new CreateConsultationIn() { SonographyId = this.SonographyId, Question = question };
            apiResponse = await ViewModel.RegisterQuestion(content);

            Hide(); // 다이얼로그가 닫히도록 처리
        }
        catch (Exception e)
        {
            // 오류시 오류 메시지 출력
            // WExpertLogger.Instance.Error($"[PatientList]Refresh error : {e}");            

            var buttonList = CommonUtils.FindVisualChildren<Button>(sender);
            var primaryButton = buttonList?.FirstOrDefault(b => b.Name == "PrimaryButton");
            if (primaryButton != null)
            {
                args.Cancel = true;
                ErrorMessage.Text = e.Message;
                ErrorTeachingTip.Target = primaryButton;
                ErrorTeachingTip.IsOpen = true;
            }
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
    }

    private void OnClose(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    private void AddQuestion_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button != null)
        {
            if (ViewModel.QuestionText.Length < WExpertDefine.MAX_QUESTION_INPUT)
            {
                StringBuilder sb = new(ViewModel.QuestionText);
                if (sb.Length > 0)
                {
                    sb.Append('\r');
                }
                sb.Append(button.Tag);

                ViewModel.QuestionText = sb.ToString().Length > WExpertDefine.MAX_QUESTION_INPUT ? sb.ToString()[..WExpertDefine.MAX_QUESTION_INPUT] : sb.ToString();
            }

            // 캐럿 위치 조정..마지막 입력 텍스트 위치로
            QuestionTextBox.SelectionStart = ViewModel.QuestionText.Length;
            QuestionTextBox.Focus(FocusState.Programmatic);
        }
    }
}

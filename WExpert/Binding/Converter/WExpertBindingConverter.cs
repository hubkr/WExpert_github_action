using System.Globalization;
using System.Text;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using WExpert.Code;
using WExpert.Helpers;
using WExpert.Models;
using WExpert.Models.Dto.Data;
using WExpert.Utils;
using Windows.UI;

namespace WExpert.Binding.Converter;

public class ShowUploadStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (parameter is not string param)
        {
            throw new NotImplementedException();
        }

        if (param.Equals("grdUploadStatus"))
        {
            var status = (bool)value;
            return status ? Visibility.Visible : Visibility.Collapsed;
        }
        else if (param.Equals("imgRegistrationUpload"))
        {
            var status = (bool)value;
            return status ? Visibility.Collapsed : Visibility.Visible;
        }
        else if (param.Equals("txtTransferredSize"))
        {
            var size = (long)value;
            return CommonUtils.ConvertByteToKMGT(size);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class DiagnosticTypeResultConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not RequestAnalysisResult requestAnalysisResult || parameter is not string param)
        {
            throw new NotImplementedException();
        }

        if (param.Equals("IsOpen"))
        {
            return false;
        }
        else if (param.Equals("Title"))
        {
            return requestAnalysisResult.Type.GetDescription();
            // return Diagnostic.GetTypeStringFromType(requestAnalysisResult.Type);
        }
        else if (param.Equals("Message"))
        {
            if (requestAnalysisResult.Type == WExpertAlgorithmsType.RUPTURE)
            {
                if (requestAnalysisResult.Points is null || !requestAnalysisResult.Points.HasValues)
                {
                    return string.Format("({0}) - No ruptures found", "StringNegative".GetLocalized());
                }
                else
                {
                    return "StringPositive".GetLocalized();
                }
            }
            else
            {
                return string.IsNullOrEmpty(requestAnalysisResult.Cls) ? "" : string.Format("({0})", requestAnalysisResult.Cls);
            }
        }      
        else
        {
            throw new NotImplementedException();
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public partial class WExpertFileConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (parameter is not string param)
        {
            throw new NotImplementedException();
        }

        if (param.Equals("Name"))
        {
            var fileName = Path.GetFileName((string)value);
            if (!string.IsNullOrEmpty(fileName) && File.Exists((string)value))
            {
                return fileName;
            }
            else
            {
                return "StringNoImage".GetLocalized();
            }
        }
        else if (param.Equals("FilesTooltip"))
        {
            var tooltip = new StringBuilder();
            if (value is UltrasoundFileInfo info)
            {
                // file name
                tooltip.Append(string.Format("\r\n{0} : {1}\r\n", "StringFileName".GetLocalized(), info.FileName));
                // item type
                tooltip.Append(string.Format("{0} : {1}\r\n", "StringItemType".GetLocalized(), FileUtils.GetItemTypeFromMimeType(info.ImageInfo.MimeType)));
                // Image size
                tooltip.Append(string.Format("{0} : {1} X {2}\r\n", "StringImageSize".GetLocalized(), info.ImageInfo.Width, info.ImageInfo.Height));
                // File size
                tooltip.Append(string.Format("{0} : {1}\r\n", "StringFileSize".GetLocalized(), CommonUtils.ConvertByteToKMGT(info.ImageInfo.size)));
            }

            return tooltip.ToString();
        }
        else if (param.Equals("ThumbnailCheck"))
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }
        else if (param.Equals("MultiSelectionCheckImage") && value is MultiSelectionType multiSelectionType4)
        {
            var imageResource = multiSelectionType4 switch
            {
                MultiSelectionType.RE_ANALYSIS => "ms-appx:///Assets/Images/SolidCheckGreen.png",
                MultiSelectionType.DELETE => "ms-appx:///Assets/Images/SolidCheckYellow.png",
                _ => null,
            };
            return imageResource != null ? new BitmapImage(new Uri(imageResource)) : null;
        }
        else if (param.Equals("AnalysisStatusIcon") && value is AnalysisStatusType analysisStatusType)
        {
            var imagePath = analysisStatusType switch
            {
                AnalysisStatusType.ANALYZING => "ms-appx:///Assets/Images/StatusAnalyzing.png",
                AnalysisStatusType.COMPLETED => "ms-appx:///Assets/Images/StatusCompletedBlue.png",
                AnalysisStatusType.INCOMPLETE => "ms-appx:///Assets/Images/StatusIncomplete.png",
                _ => null,
            };

            return imagePath != null ? new BitmapImage(new Uri(imagePath)) : null;
        }
        else if (param.Equals("AnalysisStatusTooltip") && value is AnalysisStatusType analysisStatusTooltip)
        {
            return analysisStatusTooltip switch
            {
                AnalysisStatusType.ANALYZING => "StringAnalysisAnalyzingTooltip".GetLocalized(),
                AnalysisStatusType.COMPLETED => "StringAnalysisCompletedTooltip".GetLocalized(),
                AnalysisStatusType.INCOMPLETE => "StringAnalysisIncompleteTooltip".GetLocalized(),
                _ => string.Empty,
            };
        }
        else if (param.Equals("ViewQuestionIcon") && value is int questionCount)
        {
            return questionCount > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        else if (param.Equals("QuestionIconSource") && value is bool existNewConsultationAnswer)
        {
            var uriString = existNewConsultationAnswer ? "ms-appx:///Assets/Images/QGreen.png" : "ms-appx:///Assets/Images/Q.png";
            return new BitmapImage(new Uri(uriString));
        }
        else if (param.Equals("TotalDiagnosingProgress") && value is bool isTotalDiagnosingComplete)
        {
            return !isTotalDiagnosingComplete;
        }
        else if (param.Equals("TotalDiagnosingComplete") && value is bool isTotalDiagnosingComplete2)
        {
            return isTotalDiagnosingComplete2 ? Visibility.Visible : Visibility.Collapsed;
        }
        else if (param.Equals("AnalysisStatusText") && value is AnalysisStatusType status)
        {
            return status switch
            {
                AnalysisStatusType.ANALYZING => "StringStatusAnalyzing".GetLocalized(),
                AnalysisStatusType.COMPLETED => "StringStatusCompleted".GetLocalized(),
                AnalysisStatusType.INCOMPLETE => "StringStatusIncomplete".GetLocalized(),
                _ => string.Empty,
            };
        }

        throw new NotImplementedException();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public partial class WExpertPatientListConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (parameter is not string param)
        {
            throw new NotImplementedException();
        }

        if (param.Equals("NameTooltip"))
        {
            return (string)value;
        }
        else if (param.Equals("RuptureTriageTooltip") && value is int valR)
        {
            return string.Format("StringRuptureCase".GetLocalized(), valR);
        }
        else if (param.Equals("TCTriageTooltip") && value is int valTC)
        {
            return string.Format("StringTCCase".GetLocalized(), valTC);
        }
        else if (param.Equals("RuptureTriageIcon") || param.Equals("TCTriageIcon"))
        {
            var count = (int)value;
            return count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        else if (param.Equals("AnalysisStatusImage") && value is AnalysisStatusType analysisStatusType1)
        {
            return analysisStatusType1 switch
            {
                AnalysisStatusType.ANALYZING => "ms-appx:///Assets/Images/StatusAnalyzing.png",
                AnalysisStatusType.COMPLETED => "ms-appx:///Assets/Images/StatusSuccess.png",
                AnalysisStatusType.INCOMPLETE => "ms-appx:///Assets/Images/StatusIncomplete.png",
                _ => string.Empty,
            };
        }
        else if (param.Equals("AnalysisStatusText") && value is AnalysisStatusType analysisStatusType2)
        {
            return analysisStatusType2 switch
            {
                AnalysisStatusType.ANALYZING => "StringStatusAnalyzing".GetLocalized(),
                AnalysisStatusType.COMPLETED => "StringStatusCompleted".GetLocalized(),
                AnalysisStatusType.INCOMPLETE => "StringStatusIncomplete".GetLocalized(),
                _ => string.Empty,
            };
        }
        else if (param.Equals("AnalysisStatusTextColor") && value is AnalysisStatusType analysisStatusTextColor)
        {
            var colorResourceKey = analysisStatusTextColor switch
            {
                AnalysisStatusType.ANALYZING => "TextTertiary",
                AnalysisStatusType.COMPLETED => "TextSecondary",
                AnalysisStatusType.INCOMPLETE => "TextTertiary",
                _ => "TextTertiary",
            };

            // 리소스를 가져오고 SolidColorBrush로 변환
            if (Application.Current.Resources.TryGetValue(colorResourceKey, out var colorResource) && colorResource is Color color)
            {
                return new SolidColorBrush(color);
            }

            // 기본 Brush를 지정하거나 null 반환
            return new SolidColorBrush(Colors.White);
        }
        else if (param.Equals("TotalCount") && value is int count)
        {
            // no result 체크를 위해 초기값 -1 로 셋팅(-1 인경우 0으로 변경 사용)
            return string.Format("{0}", count < 0 ? 0 : count);
        }
        else if (param.Equals("UserName") || param.Equals("HospitalName") || param.Equals("License"))
        {
            var ret = value as string;
            return string.IsNullOrEmpty(ret) ? "StringUnknown".GetLocalized() : ret;
        }
        else if (param.Equals("PatientType") && value is PatientType type)
        {
            return type.GetDescription();
        }
        else if (param.Equals("TotalNewAnswerCount") && value is int totalNewAnswerCount)
        {
            return totalNewAnswerCount > 0 ? 1.0 : 0.0;
        }
        else if (param.Equals("VisibilityConsultation") && value is int questionCount1)
        {
            return questionCount1 > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        else if (param.Equals("ConsultationQuestionCount") && value is int questionCount2)
        {
            return $"Q:{questionCount2}";
        }
        else if (param.Equals("ConsultationAnswerCount") && value is int answerCount)
        {
            return $"A:{answerCount}";
        }
        else if (param.Equals("ConsultationAnswerForeground") && value is bool hasNewAnswer)
        {
            var colorResourceKey = hasNewAnswer ? "ComponentsLtGreen" : "TextSecondary";
            // 리소스를 가져오고 SolidColorBrush로 변환
            if (Application.Current.Resources.TryGetValue(colorResourceKey, out var colorResource) && colorResource is Windows.UI.Color color)
            {
                return new SolidColorBrush(color);
            }
            return new SolidColorBrush(Colors.White);
        }
        else if (param.Equals("ConsultationAnswerFontWeight") && value is bool hasNewAnswer2)
        {
            return hasNewAnswer2 ? "Medium" : "Normal";
        }
        else if (param.Equals("ListDateCreated") && value is DateTime listDateCreated)
        {
            var locale = SettingUtils.GetLocale();
            return listDateCreated.ToString("MM/dd/yyyy hh:mm tt", new CultureInfo(locale?.Language ?? "en-US"));
        }
        else if (param.Equals("SortingMenuFieldTriage") && value is OrderType sortingMenuFieldTriage)
        {
            return (sortingMenuFieldTriage == OrderType.TRIAGE_ASC || sortingMenuFieldTriage == OrderType.TRIAGE_DESC);
        }
        else if (param.Equals("SortingMenuFieldName") && value is OrderType sortingMenuFieldName)
        {
            return (sortingMenuFieldName == OrderType.NAME_ASC || sortingMenuFieldName == OrderType.NAME_DESC);
        }
        else if (param.Equals("SortingMenuFieldDateCreated") && value is OrderType sortingMenuFieldDateCreated)
        {
            return (sortingMenuFieldDateCreated == OrderType.CREATED_AT_ASC || sortingMenuFieldDateCreated == OrderType.CREATED_AT_DESC);
        }
        else if (param.Equals("SortingMenuFieldStatus") && value is OrderType sortingMenuFieldStatus)
        {
            return (sortingMenuFieldStatus == OrderType.STATUS_ASC || sortingMenuFieldStatus == OrderType.STATUS_DESC);
        }
        else if (param.Equals("SortingMenuDirectionAsc") && value is OrderType sortingMenuDirectionAsc)
        {
            return (sortingMenuDirectionAsc == OrderType.CREATED_AT_ASC ||
                sortingMenuDirectionAsc == OrderType.NAME_ASC ||
                sortingMenuDirectionAsc == OrderType.STATUS_ASC ||
                sortingMenuDirectionAsc == OrderType.TRIAGE_ASC);
        }
        else if (param.Equals("SortingMenuDirectionDesc") && value is OrderType sortingMenuDirectionDesc)
        {
            return (sortingMenuDirectionDesc == OrderType.CREATED_AT_DESC ||
                sortingMenuDirectionDesc == OrderType.NAME_DESC ||
                sortingMenuDirectionDesc == OrderType.STATUS_DESC ||
                sortingMenuDirectionDesc == OrderType.TRIAGE_DESC);
        }
        else if (param.Equals("SortingTriage") && value is OrderType orderType)
        {
            return orderType switch
            {
                OrderType.TRIAGE_ASC => "↑",
                OrderType.TRIAGE_DESC => "↓",
                _ => string.Empty,
            };
        }
        else if (param.Equals("SortingName") && value is OrderType orderType2)
        {
            return orderType2 switch
            {
                OrderType.NAME_ASC => "↑",
                OrderType.NAME_DESC => "↓",
                _ => string.Empty,
            };
        }
        else if (param.Equals("SortingDateCreated") && value is OrderType orderType3)
        {
            return orderType3 switch
            {
                OrderType.CREATED_AT_ASC => "↑",
                OrderType.CREATED_AT_DESC => "↓",
                _ => string.Empty,
            };
        }
        else if (param.Equals("SortingStatus") && value is OrderType orderType4)
        {
            return orderType4 switch
            {
                OrderType.STATUS_ASC => "↑",
                OrderType.STATUS_DESC => "↓",
                _ => string.Empty,
            };
        }
        else if (param.Equals("AdminNoteText"))
        {
            var ret = string.Empty;
            if (value is AdminNoteOut adminNote && adminNote.Note is string note)
            {
                /*
                // 1. 문장 전체의 앞뒤 공백 제거
                var trimmedText = note.Trim();

                // 2. 라인 전체가 공백인 라인 제거
                var lines = trimmedText.Split(["\r\n", "\n", "\r"], StringSplitOptions.None);
                var nonEmptyLines = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();

                // 3. 공백 라인이 아닌 경우 줄바꿈 문자 제거 후 공백 문자 하나 추가
                var joinedText = string.Join(" ", nonEmptyLines);

                // 4. 문장 중간에 연속 공백이 있는 경우 공백 제거 후 공백 문자 하나만 출력
                ret = string.Join(" ", joinedText.Split([' '], StringSplitOptions.RemoveEmptyEntries));
                */

                // 문장 전체의 앞뒤 공백 제거
                ret = note.Trim();
            }

            return ret;
        }
        else if (param.Equals("AdminNoteDate"))
        {
            var ret = string.Empty;
            if (value is AdminNoteOut adminNote && adminNote.UpdatedAt is DateTime update)
            {
                var locale = SettingUtils.GetLocale();
                ret = update.ToString("MM/dd/yyyy hh:mm tt", new CultureInfo(locale?.Language ?? "en-US"));
            }
            return ret;
        }
        else if (param.Equals("EnableAdminNoteButton"))
        {
            return (value is AdminNoteOut note && note?.Note?.Trim().Length > 0) ? Visibility.Visible : Visibility.Collapsed;
        }

        throw new NotImplementedException();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class WExpertAnalysisViewerConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (parameter is not string param)
        {
            throw new NotImplementedException();
        }

        if (param.Equals("PatientName"))
        {
            var ret = string.Empty;

            if (value is PatientOneDataOut data && data.Name is not null)
            {
                ret = data.Name;
            }

            return ret;
        }
        else if (param.Equals("PatientRegisteredDate"))
        {
            var ret = string.Empty;

            if (value is PatientOneDataOut data)
            {
                ret = data.RegisteredAt.ToString("MMMM d, yyyy", new CultureInfo(SettingUtils.GetLocale()?.Language ?? "en-US")) ?? string.Empty;
            }

            return ret;
        }
        else if (param.Equals("PatientInfoTooltip"))
        {
            var ret = string.Empty;

            if (value is PatientOneDataOut data)
            {
                ret = $"Name : {CommonUtils.TruncateText(data?.Name ?? string.Empty, 30)}\r\n"; // 30 자 이상의 경우 줄임..
                ret += $"Type : {data?.Type.GetDescription()}\r\n"; // TODO. Type 항목은 임시 삭제(추후 버전 업시 추가 예정)
                ret += $"W Expert ID : {data?.WexpertId}\r\n";
                ret += $"Date created : {data?.RegisteredAt.ToString("MM/dd/yyyy hh:mm tt", new CultureInfo(SettingUtils.GetLocale()?.Language ?? "en-US"))}";
            }

            return ret;
        }
        else if (param.Equals("EnableAdminNoteButton"))
        {
            var ret = Visibility.Collapsed;
            if (value is PatientOneDataOut data && !string.IsNullOrEmpty(data?.AdminNote?.Note))
            {
                ret = Visibility.Visible;
            }

            return ret;
        }
        else if (param.Equals("ShowPatientInfobar"))
        {
            return (value is PatientOneDataOut) ? Visibility.Visible : Visibility.Collapsed;
        }
        else if (param.Equals("AdminNoteText"))
        {
            var ret = string.Empty;
            if (value is PatientOneDataOut data && !string.IsNullOrEmpty(data?.AdminNote?.Note))
            {
                ret = data.AdminNote.Note;
            }
            return ret;
        }
        else if (param.Equals("AdminNoteDate"))
        {
            var ret = string.Empty;
            if (value is PatientOneDataOut data && data?.AdminNote?.UpdatedAt is DateTime update)
            {
                var locale = SettingUtils.GetLocale();
                ret = update.ToString("MM/dd/yyyy hh:mm tt", new CultureInfo(locale?.Language ?? "en-US"));
            }
            return ret;
        }
        else if (param.Equals("FitToScreenBtnEnable") && value is bool fit)
        {
            return !fit;
        }
        else if (param.Equals("RealSizeBtnEnable") && value is bool real)
        {
            return !real;
        }
        else if (param.Equals("ZoomTextValue") && value is float zoomText)
        {
            // 미세하게 반영한 값이 틀린 오류로 인해 반올림 처리
            return Math.Round(zoomText * 100).ToString();
        }
        else if (param.Equals("ZoomPercent") && value is float zoomPercent)
        {
            return "StringCustomZoom".GetLocalized() + $": {(int)(zoomPercent * 100)}%";
        }
        else if (param.Equals("ZoomSlider") && value is float zoomSlider)
        {
            // Zoom(float) 값을 slider control 값(double) 로 계산 하여 변환 (ex. Zoom 1.0 -> Slider 100)
            return (double)(zoomSlider * 100);
        }
        else if (param.Equals("MultiSelectionTitleImage") && value is MultiSelectionType multiSelectionType)
        {
            var imagePath = multiSelectionType switch
            {
                MultiSelectionType.RE_ANALYSIS => "ms-appx:///Assets/Images/Refresh.png",
                MultiSelectionType.DELETE => "ms-appx:///Assets/Images/DeleteButtonYellow.png",
                _ => null,
            };

            return imagePath != null ? new BitmapImage(new Uri(imagePath)) : null;
        }
        else if (param.Equals("MultiSelectionTitleText") && value is MultiSelectionType multiSelectionType2)
        {
            return multiSelectionType2 switch
            {
                MultiSelectionType.RE_ANALYSIS => "StringReAnalysis".GetLocalized(),
                MultiSelectionType.DELETE => "StringDelete".GetLocalized(),
                _ => string.Empty,
            };
        }
        else if (param.Equals("MultiSelectionForegroundColor") && value is MultiSelectionType multiSelectionType3)
        {
            try
            {
                var colorResourceKey = multiSelectionType3 switch
                {
                    MultiSelectionType.RE_ANALYSIS => "ComponentsLtGreen",
                    MultiSelectionType.DELETE => "ComponentsYellow",
                    _ => null,
                };

                // 리소스를 가져오고 SolidColorBrush로 변환
                if (Application.Current.Resources.TryGetValue(colorResourceKey, out var colorResource) && colorResource is Color color)
                {
                    return new SolidColorBrush(color);
                }

                // 기본 Brush를 지정하거나 null 반환
                return new SolidColorBrush(Colors.Transparent);
            }
            catch
            {
                // 실패 시 기본 브러시 반환
                return new SolidColorBrush(Colors.Transparent);
            }
        }
        else if (param.Equals("NotExistTriage") && value is bool existTriage)
        {
            return existTriage ? Visibility.Collapsed : Visibility.Visible;
        }
        else if (param.Equals("DateConvert") && value is DateTime dateTime)
        {
            var returnTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, TimeZoneInfo.Local);
            return returnTime.ToString("yyyy.MM.dd HH:mm:ss");
        }
        else if (param.Equals("AnswerImageConvert"))
        {
            // null 또는 empty string 인경우 xaml 내에서 FallbackValue 처리 되지 않아(오류 발생) converter 로 별도 처리
            var imageUrl = value as string;
            return string.IsNullOrEmpty(imageUrl) ? null : new BitmapImage(new Uri(imageUrl));
        }
        else if (param.Equals("AnswerImageConvertMargin"))
        {
            // 이미지가 존재할 경우 상하 여백을 추가
            var imageUrl = value as string;
            return string.IsNullOrEmpty(imageUrl) ? new Thickness(0) : new Thickness(0, 20, 0, 0);
        }
        else if (param.Equals("ShowConsultAnswerMessage") && value is bool existAnswer)
        {
            return existAnswer ? Visibility.Visible : Visibility.Collapsed;
        }
        else if (param.Equals("ShowConsultNoAnswerMessage") && value is bool existAnswer2)
        {
            return existAnswer2 ? Visibility.Collapsed : Visibility.Visible;
        }
        else if (param.Equals("ViewROIIconImage") && value is bool viewROIIconImage)
        {
            return viewROIIconImage ? Visibility.Visible : Visibility.Collapsed;
        }
        else if (param.Equals("ViewChildIconImage"))
        {
            return value is WExpertAlgorithmsType ? Visibility.Visible : Visibility.Collapsed;
        }
        else if (param.Equals("ReportAll") && value is AnalysisReportOptionType reportAll)
        {
            return reportAll == AnalysisReportOptionType.ALL;
        }
        else if (param.Equals("ReportOnlyPositive") && value is AnalysisReportOptionType reportOnlyPositive)
        {
            return reportOnlyPositive == AnalysisReportOptionType.ONLY_POSITIVE_CASE;
        }
        else if (param.Equals("ReportAssmentInputLengthColor") && value is int reportAssmentInputLengthColor)
        {
            var colorResourceKey = reportAssmentInputLengthColor == WExpertDefine.MAX_REPORT_ASSESSMENT_INPUT ? "ComponentsRed" : "SolidLtSolid";

            // 리소스를 가져오고 SolidColorBrush로 변환
            if (Application.Current.Resources.TryGetValue(colorResourceKey, out var colorResource) && colorResource is Windows.UI.Color color)
            {
                return new SolidColorBrush(color);
            }

            // 기본 Brush를 지정하거나 null 반환
            return new SolidColorBrush(Colors.Transparent);
        }

        throw new NotImplementedException();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (parameter is not string param)
        {
            throw new NotImplementedException();
        }

        if (param.Equals("ZoomSlider") && value is double zoomSlider)
        {
            // slider control 값(double)를 Zoom 값(float) 로 변환(ex. Slider 100 -> Zoom 1.0)
            return (float)(zoomSlider / 100);
        }
        else if (param.Equals("ReportAll") && value is bool reportAll)
        {
            return reportAll ? AnalysisReportOptionType.ALL : AnalysisReportOptionType.ONLY_POSITIVE_CASE;
        }
        else if (param.Equals("ReportOnlyPositive") && value is bool reportOnlyPositive)
        {
            return reportOnlyPositive ? AnalysisReportOptionType.ONLY_POSITIVE_CASE : AnalysisReportOptionType.ONLY_POSITIVE_CASE;
        }

        throw new NotImplementedException();
    }
}

public class WEXRegisterPatientConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (parameter is not string param)
        {
            throw new NotImplementedException();
        }

        if (param.Equals("Type-Aesthetic") && value is PatientType type1)
        {
            return type1 == PatientType.AESTHETIC;
        }
        else if (param.Equals("Type-Reconstructive") && value is PatientType type2)
        {
            return type2 == PatientType.RECONSTRUCTIVE;
        }
        else if (param.Equals("Type-Both") && value is PatientType type3)
        {
            return type3 == PatientType.BOTH;
        }
        else if (param.Equals("ShowFileListNButton"))
        {
            var count = (int)value;
            return count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }
        else if (param.Equals("ShowAddFileButton") && value is int count2)
        {
            return count2 == WExpertDefine.MAX_REGISTRATION_COUNT ? Visibility.Collapsed : Visibility.Visible;
        }
        else if (param.Equals("ShowErrorMessage") && value is string message)
        {
            return string.IsNullOrEmpty(message) ? Visibility.Collapsed : Visibility.Visible;
        }
        else if (param.Equals("RegistrationFileCountColor") && value is int count)
        {
            var colorResourceKey = count == WExpertDefine.MAX_REGISTRATION_COUNT ? "ComponentsRed" : "TextPrimary";

            // 리소스를 가져오고 SolidColorBrush로 변환
            if (Application.Current.Resources.TryGetValue(colorResourceKey, out var colorResource) && colorResource is Color color)
            {
                return new SolidColorBrush(color);
            }

            // 기본 Brush를 지정하거나 null 반환
            return new SolidColorBrush(Colors.Transparent);
        }

        throw new NotImplementedException();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class WEXRegisterConsultationConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (parameter.Equals("InputQuestionLengthColor") && value is int inputLength)
        {
            var colorResourceKey = inputLength == WExpertDefine.MAX_QUESTION_INPUT ? "ComponentsRed" : "SolidLtSolid";

            // 리소스를 가져오고 SolidColorBrush로 변환
            if (Application.Current.Resources.TryGetValue(colorResourceKey, out var colorResource) && colorResource is Windows.UI.Color color)
            {
                return new SolidColorBrush(color);
            }

            // 기본 Brush를 지정하거나 null 반환
            return new SolidColorBrush(Colors.Transparent);
        }
        else if (parameter.Equals("ViewExceededCount") && value is int exceededCount)
        {
            return exceededCount > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        throw new NotImplementedException();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class WEXAccountDialogConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (parameter.Equals("DisplayExpiredText") && value is ProfileLicenseOut profileLicense)
        {
            if (profileLicense == null ||
                profileLicense.ServerNow == null ||
                profileLicense.ExpiresAt == null)
            {
                return string.Empty;
            }

            // Calculate the difference
            var timeDifference = profileLicense.ExpiresAt - profileLicense.ServerNow;

            // Check the time difference and print the appropriate message
            if (profileLicense.ServerNow > profileLicense.ExpiresAt)
            {
                return "StringLicenseExpired".GetLocalized();
            }
            else if (timeDifference.Value.TotalSeconds < 3600)  // Less than 1 hour
            {
                return "StringLicenseExpiredLessThan1h".GetLocalized();
            }
            else if (timeDifference.Value.TotalSeconds < 86400)  // Less than 24 hours
            {
                var hours = (int)timeDifference.Value.TotalHours;
                return string.Format("StringLicenseExpiredLessThan24h".GetLocalized(), hours); //$"Expires in {hours} hours";
            }
            else if (timeDifference.Value.TotalSeconds < 691200)  // Less than 7 days
            {
                var days = (int)timeDifference.Value.TotalDays;
                return string.Format("StringLicenseExpiredLessThan7d".GetLocalized(), days); //$"Expires in {days} days";
            }
            else  // More than 7 days
            {
                return string.Empty;
            }
        }
        else if (parameter.Equals("DisplayExpiredVisibility") && value is ProfileLicenseOut profileLicense2)
        {
            if (profileLicense2 == null ||
                profileLicense2.ServerNow == null ||
                profileLicense2.ExpiresAt == null)
            {
                return Visibility.Collapsed;
            }

            // Calculate the difference
            var timeDifference = profileLicense2.ExpiresAt - profileLicense2.ServerNow;

            if (profileLicense2.ServerNow > profileLicense2.ExpiresAt
                || timeDifference.Value.TotalSeconds < 691200)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }
        else if (parameter.Equals("DisplayExpiredColor") && value is ProfileLicenseOut profileLicense3)
        {
            if (profileLicense3 == null ||
                profileLicense3.ServerNow == null ||
                profileLicense3.ExpiresAt == null)
            {
                return new SolidColorBrush(Colors.Transparent);
            }

            object colorResourceKey;

            // Calculate the difference
            var timeDifference = profileLicense3.ExpiresAt - profileLicense3.ServerNow;

            // Check the time difference and print the appropriate message
            if (profileLicense3.ServerNow > profileLicense3.ExpiresAt)
            {
                colorResourceKey = "ComponentsRed";
            }
            else if (timeDifference.Value.TotalSeconds < 3600)  // Less than 1 hour
            {
                colorResourceKey = "ComponentsRed";
            }
            else if (timeDifference.Value.TotalSeconds < 86400)  // Less than 24 hours
            {
                colorResourceKey = "ComponentsRed";
            }
            else if (timeDifference.Value.TotalSeconds < 691200)  // Less than 7 days
            {
                colorResourceKey = "ComponentsBlue";
            }
            else  // More than 7 days
            {
                return new SolidColorBrush(Colors.Transparent);
            }

            // 리소스를 가져오고 SolidColorBrush로 변환
            if (Application.Current.Resources.TryGetValue(colorResourceKey, out var colorResource) && colorResource is Color color)
            {
                return new SolidColorBrush(color);
            }

            // 기본 Brush를 지정하거나 null 반환
            return new SolidColorBrush(Colors.Transparent);
        }          

        throw new NotImplementedException();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class CommonConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object param, string language)
    {
        if (param.Equals("InverseBool") && value is bool b)
        {
            return !b;
        }
        else if (param.Equals("BoolVisibility") && value is bool b1)
        {
            return b1 ? Visibility.Visible : Visibility.Collapsed;
        }
        else if (param.Equals("InverseBoolVisibility") && value is bool b2)
        {
            return b2 ? Visibility.Collapsed : Visibility.Visible;
        }
        else if (param.Equals("NullImageCheck"))
        {
            return value ?? new BitmapImage(new Uri("ms-appx:///Assets/Images/NoImageBig.png"));
        }
        else if (param.Equals("ResourceStringLoader"))
        {
            var resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            return resourceLoader.GetString((string)value);
        }  

        throw new NotImplementedException();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
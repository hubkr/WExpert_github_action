
using System.ComponentModel;
using Microsoft.UI.Xaml;
using WExpert.Code;
using WExpert.Helpers;

using static WExpert.WExpertDefine;

namespace WExpert.Models;

public partial class UltrasoundFileInfo : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    // ultrasound_file 일련번호(id)
    public string UltraSoundFileId
    {
        get; set;
    } = string.Empty;

    public string ImageUrl
    {
        get; set;
    } = string.Empty;

    public string? FileName
    {
        get; set;
    } = string.Empty;

    public int ConsultationQuestion
    {
        get; set;
    } = 0;

    public int ConsultationAnswer
    {
        get; set;
    } = 0;

    public bool IsExistNewConsultationAnswer
    {
        get; set;
    } = false;

    public ImageInfos ImageInfo
    {
        get; set;
    } = new("Unknown", 0, 0, 0);

    // 선택 체크 표시
    private bool _check = false;
    public bool Check
    {
        get => _check;
        set
        {
            if (_check != value)
            {
                _check = value;
                NotifyPropertyChanged(nameof(Check));
            }
        }
    }

    // Thumbnail 이미지 로딩중 Progress ring
    private bool _thumbnailLoading = true;
    public bool ThumbnailLoading
    {
        get => _thumbnailLoading; 
        set
        {
            if (_thumbnailLoading != value)
            {
                _thumbnailLoading = value;
                NotifyPropertyChanged(nameof(ThumbnailLoading));
            }
        }
    }

    // 현재 분석 상태
    private AnalysisStatusType _analysisStatus = AnalysisStatusType.NONE;
    public AnalysisStatusType AnalysisStatus
    {
        get => _analysisStatus;
        set
        {
            if (_analysisStatus != value)
            {
                _analysisStatus = value;
                NotifyPropertyChanged(nameof(AnalysisStatus));
            }
        }
    }

    // Rupture triage
    private Visibility _visibilityRuptureTriage = Visibility.Collapsed;
    public Visibility VisibilityRuptureTriage
    {
        get => _visibilityRuptureTriage;
        set
        {
            if (_visibilityRuptureTriage != value)
            {
                _visibilityRuptureTriage = value;
                NotifyPropertyChanged(nameof(VisibilityRuptureTriage));

                ExistTriage = (value == Visibility.Visible) || (VisibilityTCTriage == Visibility.Visible);
            }
        }
    }

    // TC triage
    private Visibility _visibilityTCTriage = Visibility.Collapsed;
    public Visibility VisibilityTCTriage
    {
        get => _visibilityTCTriage;
        set
        {
            if (_visibilityTCTriage != value)
            {
                _visibilityTCTriage = value;
                NotifyPropertyChanged(nameof(VisibilityTCTriage));

                ExistTriage = (value == Visibility.Visible) || (VisibilityRuptureTriage == Visibility.Visible);
            }
        }
    }

    // Exist triage
    private bool _existTriage = false;
    public bool ExistTriage
    {
        get => _existTriage;
        set
        {
            if (_existTriage != value)
            {
                _existTriage = value;
                NotifyPropertyChanged(nameof(ExistTriage));
            }
        }
    }

    private string _consultationStatus = "None";
    public string ConsultationStatus
    {
        get => _consultationStatus;
        set
        {
            if (_consultationStatus != value)
            {
                _consultationStatus = value;
                NotifyPropertyChanged(nameof(ConsultationStatus));
            }
        }
    }

    private MultiSelectionType _multiSelectionMode = MultiSelectionType.NONE;
    public MultiSelectionType MultiSelectionMode
    {
        get => _multiSelectionMode; 
        set
        {
            if (_multiSelectionMode != value)
            {
                _multiSelectionMode = value;
                NotifyPropertyChanged(nameof(MultiSelectionMode));
            }
        }
    }

    public UltrasoundFileInfo(string id, string imageUrl, string? fileName = null, bool isExistResult = false, int consultationQuestion = 0, int consultationAnswer = 0, bool isExistNewConsultationAnswer = false)
    {
        UltraSoundFileId = id;
        ImageUrl = imageUrl;
        FileName         = string.IsNullOrEmpty(fileName) ? "StringInfoNotExist".GetLocalized() : fileName;
        AnalysisStatus   = isExistResult ? AnalysisStatusType.COMPLETED : AnalysisStatusType.NONE;
        ConsultationQuestion = consultationQuestion;
        ConsultationAnswer = consultationAnswer;
        IsExistNewConsultationAnswer = isExistNewConsultationAnswer;
        ConsultationStatus = ConsultationQuestion  == 0 ? "None" : $"{ConsultationAnswer}/{ConsultationQuestion}";
    }

    private void NotifyPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
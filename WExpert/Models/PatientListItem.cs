using System.ComponentModel;
using System.Runtime.CompilerServices;
using WExpert.Code;
using WExpert.Models.Dto.Data;

namespace WExpert.Models;

public class PatientListItem : INotifyPropertyChanged
{    
    // 체크 상태
    private bool _check;
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

    // 일련 번호(Id)
    private int _id;
    public int Id
    {
        get => _id;
        set
        {
            if (_id != value)
            {
                _id = value;
                NotifyPropertyChanged(nameof(Id));
            }
        }
    }

    // 목록 식별 번호(W Expert ID)
    private string? _wexpertId = string.Empty;
    public string? WExpertId
    {
        get => _wexpertId;
        set
        {
            if (_wexpertId != value)
            {
                _wexpertId = value;
                NotifyPropertyChanged(nameof(WExpertId));
            }
        }
    }

    // Rupture 존재 여부
    private int _ruptureTriage = 0;
    public int RuptureTriage
    {
        get => _ruptureTriage;
        set
        {
            if (_ruptureTriage != value)
            {
                _ruptureTriage = value;
                NotifyPropertyChanged(nameof(RuptureTriage));
            }
        }
    }

    // Thickened Capsule 존재 여부
    private int _tcTriage = 0;
    public int TCTriage
    {
        get => _tcTriage;
        set
        {
            if (_tcTriage != value)
            {
                _tcTriage = value;
                NotifyPropertyChanged(nameof(TCTriage));
            }
        }
    }

    // 이름
    private string _name = string.Empty;
    public string Name 
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                NotifyPropertyChanged(nameof(Name));
            }
        }
    }

    // 환자 타입(성형, 제건, 둘다)
    private PatientType _patientType = PatientType.NONE;
    public PatientType PatientType
    {
        get => _patientType;
        set
        {
            if (_patientType != value)
            {
                _patientType = value;
                NotifyPropertyChanged(nameof(PatientType));
            }
        }
    }

    // 등록 파일 개수
    private int _files = 0;
    public int Files
    {
        get => _files;
        set
        {
            if (_files != value)
            {
                _files = value;
                NotifyPropertyChanged(nameof(Files));
            }
        }
    }

    // Consult 질문 개수
    private int _consultQuestCount = 0;
    public int ConsultQuestCount
    {
        get => _consultQuestCount;
        set
        {
            if (_consultQuestCount != value)
            {
                _consultQuestCount = value;
                NotifyPropertyChanged(nameof(ConsultQuestCount));
            }
        }
    }

    // Consult 답변 개수
    private int _consultAnswerCount = 0;
    public int ConsultAnswerCount
    {
        get => _consultAnswerCount;
        set
        {
            if (_consultAnswerCount != value)
            {
                _consultAnswerCount = value;
                NotifyPropertyChanged(nameof(ConsultAnswerCount));
            }
        }
    }

    // 신규 답변 유무
    private bool _hasNewConsultAnswer = false;
    public bool HasNewConsultAnswer
    {
        get => _hasNewConsultAnswer;
        set
        {
            if (_hasNewConsultAnswer != value)
            {
                _hasNewConsultAnswer = value;
                NotifyPropertyChanged(nameof(HasNewConsultAnswer));
            }
        }
    }

    // 생성일
    private DateTime? _dateCreated;
    public DateTime? DateCreated
    {
        get => _dateCreated;
        set
        {
            if (_dateCreated != value)
            {
                _dateCreated = value;
                NotifyPropertyChanged(nameof(DateCreated));
            }
        }
    }

    // Admin Note
    private AdminNoteOut? _adminNote = null;
    public AdminNoteOut? AdminNote
    {
        get => _adminNote;
        set
        {
            if (_adminNote != value)
            {
                _adminNote = value;
                NotifyPropertyChanged(nameof(AdminNote));
            }
        }
    }

    // 분석 진행 상태
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


    public event PropertyChangedEventHandler? PropertyChanged;

    public void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

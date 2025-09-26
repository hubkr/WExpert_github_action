using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WExpert.Models;

public class ConsultationListItem : INotifyPropertyChanged
{    
    // 질문 일련 번호
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

    // 등록일
    private DateTime _createdAt;
    public DateTime CreatedAt
    {
        get => _createdAt;
        set
        {
            if (_createdAt != value)
            {
                _createdAt = value;
                NotifyPropertyChanged(nameof(CreatedAt));
            }
        }
    }

    // 질문 내용
    private string _question = string.Empty;
    public string Question
    {
        get => _question;
        set
        {
            if (_question != value)
            {
                _question = value;
                NotifyPropertyChanged(nameof(Question));
            }
        }
    }

    // 답변 존재 유무
    private bool _answerExist = false;
    public bool AnswerExist
    {
        get => _answerExist;
        set
        {
            if (_answerExist != value)
            {
                _answerExist = value;
                NotifyPropertyChanged(nameof(AnswerExist));
            }
        }
    }

    // 답변 일련 번호
    private int _answerId;
    public int AnswerId 
    {
        get => _answerId;
        set
        {
            if (_answerId != value)
            {
                _answerId = value;
                NotifyPropertyChanged(nameof(AnswerId));
            }
        }
    }

    // 답변 생성일
    private DateTime _answerCreatedAt;
    public DateTime AnswerCreatedAt
    {
        get => _answerCreatedAt;
        set
        {
            if (_answerCreatedAt != value)
            {
                _answerCreatedAt = value;
                NotifyPropertyChanged(nameof(AnswerCreatedAt));
            }
        }
    }

    // 답변 내용
    private string _answer = string.Empty;
    public string Answer
    {
        get => _answer;
        set
        {
            if (_answer != value)
            {
                _answer = value;
                NotifyPropertyChanged(nameof(Answer));
            }
        }
    }

    // 답변 내용 attach 파일(이미지) URL
    private string _answerAttachmentUrl = string.Empty;
    public string AnswerAttachmentUrl
    {
        get => _answerAttachmentUrl;
        set
        {
            if (_answerAttachmentUrl != value)
            {
                _answerAttachmentUrl = value;
                NotifyPropertyChanged(nameof(AnswerAttachmentUrl));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

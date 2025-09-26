
namespace WExpert.Utils;

public class WExpertLogger
{
    private static readonly WExpertLogger _instance;

    public static WExpertLogger Instance => _instance;

    static WExpertLogger()
    {
        if (null == _instance)
        {
            _instance = new WExpertLogger();
            _instance.Initialize();
        }
    }

    private NLog.Logger? _log;

    public void Initialize()
    {
        if (null != _log)
        {
            return;
        }

        // 로그를 쓰기 가능한 경로 지정 (ex: C:\Users\<Username>\AppData\Local\WExpert)
        var logFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WExpert", "Logs");
        // 로그 보관 푤더
        var archiveFolder = Path.Combine(logFolder, "Archives");
        // 현재 사용될 로그 파일 경로
        var logFilePath = Path.Combine(logFolder, "WExpert.log");

        // 로그 보관(Archiving) 정책
        var target = new NLog.Targets.FileTarget("fileTarget")
        {
            Layout = @"[${date:format=yyyy\-MM\-dd HH\:mm\:ss.fff}][pid:${processid},tid:${threadid}][${level}] ${message}",
            FileName = logFilePath,
            // 보관 정책 (일별)
            ArchiveEvery = NLog.Targets.FileArchivePeriod.Day,

            // 보관 파일명: WExpert.2023-10-27.log 형태
            ArchiveFileName = Path.Combine(archiveFolder, "WExpert.{#}.log"),

            MaxArchiveDays = 30,  // 30일 데이터 까지만 저장
            MaxArchiveFiles = 100, // 단, 전체 보관 파일이 100개를 초과하면 기간에 상관없이 오래된 것부터 삭제

            CreateDirs = true,   // 폴더 자동 생성
            KeepFileOpen = false // 기본값; 다른 프로세스가 읽기 쉬움
        };

        // 비동기 래퍼
        var wrapper = new NLog.Targets.Wrappers.AsyncTargetWrapper(
            target, 1000, NLog.Targets.Wrappers.AsyncTargetWrapperOverflowAction.Grow);

        var conf = new NLog.Config.LoggingConfiguration();
        conf.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, wrapper);
        NLog.LogManager.Configuration = conf;

        _log = NLog.LogManager.GetLogger("System");

#if DEBUG
        NLog.LogManager.GlobalThreshold = NLog.LogLevel.Debug;
#else
        NLog.LogManager.GlobalThreshold = NLog.LogLevel.Warn;
#endif
    }

    public void Debug(string msg)
    {
        _log?.Debug(msg);
        System.Diagnostics.Debug.WriteLine(string.Format("[Debug]{0}", msg));
    }

    public void Warn(string msg)
    {
        _log?.Warn(msg);
        System.Diagnostics.Debug.WriteLine(string.Format("[Warn]{0}", msg));
    }

    public void Error(string msg)
    {
        _log?.Error(msg);
        System.Diagnostics.Debug.WriteLine(string.Format("[Error]{0}", msg));
    }

    public void Fatal<T>(T value)
    {
        _log?.Fatal<T>(value);
        System.Diagnostics.Debug.WriteLine(string.Format("[Fatal]{0}", value?.ToString()), "");
    }
}

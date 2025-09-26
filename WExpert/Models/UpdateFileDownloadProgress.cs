namespace WExpert.Models;

public class UpdateFileDownloadProgress
{
    public int ProgressPercentage
    {
        get; set;
    }

    public long DownloadedBytes
    {
        get; set;
    }

    public long TotalBytes
    {
        get; set;
    }

    public string FileName
    {
        get; set;
    }

    public string Version
    {
        get; set;
    }

    /*
    public string FormattedDownloadedSize => FormatBytes(DownloadedBytes);

    public string FormattedTotalSize => FormatBytes(TotalBytes);

    private string FormatBytes(long bytes)
    {
        const long KB = 1024;
        const long MB = KB * 1024;
        const long GB = MB * 1024;

        if (bytes >= GB)
        {
            return $"{bytes / (double)GB:F2} GB";
        }

        if (bytes >= MB)
        {
            return $"{bytes / (double)MB:F2} MB";
        }

        if (bytes >= KB)
        {
            return $"{bytes / (double)KB:F2} KB";
        }

        return $"{bytes} bytes";
    }
    */
}

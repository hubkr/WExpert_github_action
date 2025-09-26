using WExpert.Utils;

namespace WExpert.Models;

public class NewRegistrationFileInfo(string path, string? fileName = null, string? mimeType = null)
{
    public string FilePath
    {
        get; set;
    } = path;

    public string MimeType
    {
        get;
    } = string.IsNullOrEmpty(mimeType) ? FileUtils.GetMimeType(path) : mimeType;

    public string DestFileName
    {
        get;
    } = string.IsNullOrEmpty(fileName) ? Path.GetFileName(path) : fileName;
}
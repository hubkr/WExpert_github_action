using Newtonsoft.Json.Linq;

namespace WExpert.Models.Dto.Data;

public class Labels
{
    public string? Result_Type
    {
        get; set;
    }

    public string? Result_Class
    {
        get; set;
    }

    public JArray? Points
    {
        get; set;
    }
}

public class AnalysisDataOut
{
    public string? FileName
    {
        get; set;
    }

    public int? Width
    {
        get; set;
    }

    public int? Height
    {
        get; set;
    }

    public List<Labels>? Labels
    {
        get; set;
    }
}
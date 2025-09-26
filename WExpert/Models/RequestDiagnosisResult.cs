
using Newtonsoft.Json.Linq;
using WExpert.Code;

namespace WExpert.Models;
public class RequestAnalysisResult
{
    public WExpertAlgorithmsType Type
    {
        get; set;
    }

    public string Cls
    {
        get; set;
    }

    public JToken? Points
    {
        get; set; 
    }

    public void Init()
    {
        Type = WExpertAlgorithmsType.NONE;
        Cls = string.Empty;
        Points = null;
    }

    public RequestAnalysisResult(WExpertAlgorithmsType type, string? cls, JToken? points)
    { 
        Type = type;
        Cls = string.IsNullOrEmpty(cls) ? string.Empty : cls;
        Points = points;
    }

    public RequestAnalysisResult()
    {
        Type = WExpertAlgorithmsType.NONE;
        Cls = string.Empty;
        Points = null;
    }
}

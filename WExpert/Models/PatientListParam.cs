namespace WExpert.Models;

public class PatientListNaviInfo
{
    public int page { get; set; } = 1;

    public int limit { get; set; } = 20;

    public string? query { get; set; } = null;

    public string? filter_triage { get; set; } = null;

    public string? sort_by { get; set; } = null;

    public string? order { get; set; } = null;

    public void SetInfo(int page, int limit, string? query = null, string? filter = null, string? sortby = null, string? order = null)
    {
        this.page           = page;
        this.limit          = limit;
        this.query          = query;
        this.filter_triage  = filter;
        this.sort_by        = sortby;
        this.order          = order;
    }
}

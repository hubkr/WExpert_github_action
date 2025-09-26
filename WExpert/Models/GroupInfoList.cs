namespace WExpert.Core.Models;

public class GroupInfoList : List<object>
{
    public GroupInfoList(IEnumerable<object> items) : base(items)
    {
    }

    public object Key
    {
        get; set;
    }
}


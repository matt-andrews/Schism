using Schism.Lib.Core;

namespace Schism.Lib.Http;
public static class SchismRequestHttpExtensions
{
    public static SchismRequest WithQueryParam(this SchismRequest req, string key, object value)
    {
        req.AddProp(props =>
        {
            if (props.TryGetValue(HttpConsts.ConstQueryParams, out object? val))
            {
                if (val is List<string> strings)
                {
                    strings.Add($"{key}={value}");
                }
                else
                {
                    props[HttpConsts.ConstQueryParams] = new List<string>() { $"{key}={value}" };
                }
            }
            else
            {
                props.Add(HttpConsts.ConstQueryParams, new List<string>() { $"{key}={value}" });
            }
        });
        return req;
    }
}
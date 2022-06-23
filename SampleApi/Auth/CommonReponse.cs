namespace SampleApi.Auth;

public class BaseReponse
{
    public bool Status { get; set; }
    public object Data { get; set; }
    public string Message { get; set; }
}

using System.Runtime.InteropServices;

public class URLReader
{
    [DllImport("__Internal")]
    public static extern string GetURLFromPage();
   
    [DllImport("__Internal")]
    public static extern string GetQueryParam(string paramId);
}

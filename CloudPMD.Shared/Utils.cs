using System;
using System.Text.Json;

/// <summary>
/// Summary description for Class1
/// </summary>
public static class Utils
{
	public static string GetGuestUser(string jsonString)
    {
        using (JsonDocument document = JsonDocument.Parse(jsonString))
        {
            return document.RootElement.GetProperty("data").GetProperty("players").GetProperty("data")[0].GetProperty("name").GetString();
        }
    }

    public static string GetVersion(string jsonString, string srcVersionID)
    {
        using (JsonDocument document = JsonDocument.Parse(jsonString))
        {
            return document.RootElement.GetProperty("data").GetProperty("runs")[0].GetProperty("run").GetProperty("values").GetProperty(srcVersionID).GetString();
        }
    }
}

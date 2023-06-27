using UnityEngine;
using System.IO;

public static class PathInfo
{
    /**
     * 0 : GPT_GESTI_PYDLL
     * 1 : GPT_GESTI_PYTHON_HOME
     * 2 : STTS_PYDLL
     * 3 : STTS_PYTHON_HOME
     */
    private static readonly string[] PersonalPaths = new string[4];
    
    /**
     * pathinfo.txt을 읽어 PersonalPaths 값 세팅.
     */
    public static void SetPathInfo()
    {
        var i = 0;
        
        var reader = new StreamReader(Application.dataPath + "/pathinfo.txt");

        while (!reader.EndOfStream)
        {
            var tempStr = reader.ReadLine()?.Split('=');
            PersonalPaths[i] = tempStr?[1];
            i++;
        }
        
        reader.Close();
    }

    /**
     * PersonalPaths 값 조회.
     */
    public static string GetPathInfo(int idx)
    {
        return PersonalPaths[idx];
    }
}

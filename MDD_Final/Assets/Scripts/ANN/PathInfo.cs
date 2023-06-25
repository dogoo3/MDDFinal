using UnityEngine;
using System.IO;

public static class PathInfo
{
    /**
     * 0 : PYTHONNET_PYDLL
     * 1 : PYTHON_HOME
     */
    private static readonly string[] PersonalPaths = new string[2];
    
    /**
     * pathinfo.txt을 읽어 PersonalPaths 값 세팅.
     */
    public static void SetPathInfo()
    {
        var i = 0;
        
        var reader = new StreamReader(Application.dataPath + "/pathinfo.txt");

        while (!reader.EndOfStream)
        {
            var tempStr = reader.ReadLine()?.Split('>');
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

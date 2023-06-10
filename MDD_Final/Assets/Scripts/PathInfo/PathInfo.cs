using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class PathInfo
{
    private static string[] personalPaths = new string[2];

    public static void SetPathInfo()
    {
        int i = 0;
        
        StreamReader reader = new StreamReader(Application.dataPath + "/pathinfo.txt");

        while (!reader.EndOfStream)
        {
            string[] _tempStr = reader.ReadLine().Split('>');
            personalPaths[i] = _tempStr[1];
            i++;
        }
        
        reader.Close();
    }

    public static string GetPathInfo(int _index) // 0 : PYDLL 1:HOME
    {
        return personalPaths[_index];
    }
}

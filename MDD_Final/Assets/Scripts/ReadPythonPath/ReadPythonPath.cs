using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class ReadPythonPath
{
    private static string[] personalPaths = new string[5];

    public static void SetFilePath()
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

    public static string GetFilePath(int _index) // 0 : PYDLL 1:HOME 2:PACKAGE1 3:PACKAGE2 4:PACKAGE3
    {
        return personalPaths[_index];
    }
}

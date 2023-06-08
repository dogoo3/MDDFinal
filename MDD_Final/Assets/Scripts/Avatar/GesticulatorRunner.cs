using System;
using System.IO;
using UnityEngine;
using Python.Runtime;

public class GesticulatorRunner : MonoBehaviour
{
    private double[,] _gestureData; // 제스처 데이터
    
    private void Awake()
    {
        // pathinfo.txt 읽기
        ReadPythonPath.SetFilePath();

        // Pythonnet 세팅
        Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", ReadPythonPath.GetFilePath(0), EnvironmentVariableTarget.Process);
        Debug.Log("Python DLL : " + ReadPythonPath.GetFilePath(0));
        var pythonHome = Environment.ExpandEnvironmentVariables(ReadPythonPath.GetFilePath(1));
        Debug.Log("Python Home : " + pythonHome);
        var pythonPath = string.Join(
            Path.PathSeparator.ToString(),
            new string[]
            {
                Path.Combine(pythonHome, @"Lib\site-packages"),
                Path.Combine(pythonHome, @"Lib"),
                Path.Combine(pythonHome, @"DLLs"),
                Path.Combine(Application.dataPath, @"Plugins\Gesticulator\gesticulator\visualization"),
                Path.Combine(Application.dataPath, @"Plugins\Gesticulator\gesticulator"),
                Path.Combine(Application.dataPath, @"Plugins\Gesticulator")
            }
        );
        Debug.Log("Python Path : " + pythonPath);
        PythonEngine.PythonPath = pythonPath;
    }

    public double[,] RunGesticulator(string text, string wavFilePath)
    {
        Debug.Log("(6/8) Gesticulator 시작");
        Debug.Log("Text : " + text);
        Debug.Log("WAV File Path : " + wavFilePath);
        
        PythonEngine.Initialize();
        using (Py.GIL())
        {
            dynamic demo = Py.Import("demo.demo_custom");
            dynamic data = demo.main(text, wavFilePath);
            this._gestureData = this.NpArrayToArray(data);
        } 
        PythonEngine.Shutdown();
        
        Debug.Log("(7/8) Gesticulator 시작");
        
        return this._gestureData;
    }
    
    /**
     * NumpyArray를 C# 배열로 변환.
     */
    private double[,] NpArrayToArray(dynamic npArray)
    {
        dynamic np = Py.Import("numpy");
        dynamic npArrayConverted = npArray.astype(np.float64);
        int ndim = npArrayConverted.ndim;

        dynamic shapeTuple = npArrayConverted.shape;
        int[] shape = new int[ndim];

        for (int i = 0; i < ndim; i++)
        {
            shape[i] = shapeTuple[i];
        }

        dynamic flattenedArray = npArrayConverted.flatten();
        double[,] csharpArray = new double[shape[0], shape[1]];

        int index = 0;
        for (int i = 0; i < shape[0]; i++)
        {
            for (int j = 0; j < shape[1]; j++)
            {
                csharpArray[i, j] = flattenedArray[index++];
            }
        }

        return csharpArray;
    }
}

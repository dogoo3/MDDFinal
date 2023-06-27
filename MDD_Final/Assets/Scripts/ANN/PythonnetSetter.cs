using System;
using System.IO;
using UnityEngine;
using Python.Runtime;

public class PythonnetSetter : MonoBehaviour
{
    private void Awake()
    {
        // pathinfo.txt 읽기
        PathInfo.SetPathInfo();
    }
    
    public void SetPyEnvForStts()
    {
        // Python DLL
        Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", PathInfo.GetPathInfo(0), EnvironmentVariableTarget.Process);
        
        // Python Home
        var pythonHome = Environment.ExpandEnvironmentVariables(PathInfo.GetPathInfo(1));
        
        Debug.Log(pythonHome);
        
        // Python Path
        var pythonPath = string.Join(
            Path.PathSeparator.ToString(),
            new[]
            {
                Path.Combine(pythonHome, @"Lib\site-packages"),
                Path.Combine(pythonHome, @"Lib"),
                Path.Combine(pythonHome, @"DLLs"),
                Path.Combine(Application.dataPath, @"Plugins\STT"),
                Path.Combine(Application.dataPath, @"Plugins\TTS"),
                Path.Combine(Application.dataPath, @"Plugins\GPT"),
                Path.Combine(Application.dataPath, @"Plugins\Gesticulator\gesticulator\visualization"),
                Path.Combine(Application.dataPath, @"Plugins\Gesticulator\gesticulator"),
                Path.Combine(Application.dataPath, @"Plugins\Gesticulator")
            }
        );
        PythonEngine.PythonPath = pythonPath;
        
        Debug.Log(PythonEngine.PythonPath);
    }

    public void SetPyEnvForGptAndGesticulator()
    {
        // Python DLL
        Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", PathInfo.GetPathInfo(2), EnvironmentVariableTarget.Process);
        
        // Python Home
        var pythonHome = Environment.ExpandEnvironmentVariables(PathInfo.GetPathInfo(3));
        
        Debug.Log(pythonHome);
        
        // Python Path
        var pythonPath = string.Join(
            Path.PathSeparator.ToString(),
            new[]
            {
                Path.Combine(pythonHome, @"Lib\site-packages"),
                Path.Combine(pythonHome, @"Lib"),
                Path.Combine(pythonHome, @"DLLs"),
                Path.Combine(Application.dataPath, @"Plugins\STT"),
                Path.Combine(Application.dataPath, @"Plugins\TTS"),
                Path.Combine(Application.dataPath, @"Plugins\GPT"),
                Path.Combine(Application.dataPath, @"Plugins\Gesticulator\gesticulator\visualization"),
                Path.Combine(Application.dataPath, @"Plugins\Gesticulator\gesticulator"),
                Path.Combine(Application.dataPath, @"Plugins\Gesticulator")
            }
        );
        PythonEngine.PythonPath = pythonPath;
        
        Debug.Log(PythonEngine.PythonPath);
    }
}

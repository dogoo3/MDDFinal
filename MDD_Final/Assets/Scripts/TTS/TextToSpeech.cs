using UnityEngine;
using System;
using System.IO;
using Python.Runtime;


public class TextToSpeech : MonoBehaviour
{
    private AudioClip _audioClip;
    private void Awake()
    {
        /* Pythonnet ���� ���� */
        // pathinfo.txt �б�
        PathInfo.SetPathInfo();

        Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", PathInfo.GetPathInfo(0), EnvironmentVariableTarget.Process);
        var pythonHome = Environment.ExpandEnvironmentVariables(PathInfo.GetPathInfo(1));
        var pythonPath = string.Join(
            Path.PathSeparator.ToString(),
            new[]
            {
                Path.Combine(pythonHome, @"Lib\site-packages"),
                Path.Combine(pythonHome, @"Lib"),
                Path.Combine(pythonHome, @"DLLs"),
                Path.Combine(Application.dataPath, @"Plugins")
            }
        );
        PythonEngine.PythonPath = pythonPath;
        Debug.Log("Python Path : " + pythonPath);
        /* Pythonnet ���� �� */
    }

    /**
     * TTS ����.
     * MDDFinal\Assets\Resources\MDD_Final\speech.wav �� �����
     */
    public AudioClip RunTTS(string text)
    {
        Debug.Log("TTS ����");

        PythonEngine.Initialize();
        using (Py.GIL())
        {
            // 
            dynamic demo = Py.Import("TTS_custom");
            dynamic motion = demo.main(text);

        }
        PythonEngine.Shutdown();
        Debug.Log("TTS ����");

        this._audioClip = Resources.Load<AudioClip>("speech");
        Debug.Log(_audioClip);
        return this._audioClip;
    }

}

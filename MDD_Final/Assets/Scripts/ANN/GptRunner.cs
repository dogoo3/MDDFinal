using System;
using UnityEngine;
using Python.Runtime;

public class GptRunner : MonoBehaviour
{
    private PythonnetSetter _pythonnetSetter; // 파이썬넷 세팅 클래스
    private string _outputText; // GPT 실행 결과

    public void Awake()
    {
        this._pythonnetSetter = FindObjectOfType<PythonnetSetter>();
    }

    /**
     * GPT 실행.
     */
    public string RunGpt(string inputText)
    {
        Debug.Log("GPT 시작");

        // 파이썬넷 환경 세팅
        this._pythonnetSetter.SetPyEnvForGptAndGesticulator();

        try
        {
            PythonEngine.Initialize();
            using (Py.GIL())
            {
                dynamic gpt = Py.Import("gpt");
                dynamic outputText = gpt.main(inputText);
                this._outputText = (string) outputText;
            }
            PythonEngine.Shutdown();
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            return null;
        }

        Debug.Log("GPT 끝");
        Debug.Log("GPT Result : " + this._outputText);
        
        return this._outputText;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Python.Runtime;
using System;
using System.IO;

public class PythonNetSimulator : MonoBehaviour
{
    // Start is called before the first frame update
    private void Awake()
    {
        ReadPythonPath.SetFilePath();

        Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", ReadPythonPath.GetFilePath(0), EnvironmentVariableTarget.Process);

        var PYTHON_HOME = Environment.ExpandEnvironmentVariables(ReadPythonPath.GetFilePath(1));

        Debug.Log($"PythonHome={PYTHON_HOME}");

        // 모듈 패키지 패스 설정.
        PythonEngine.PythonPath = string.Join(

            Path.PathSeparator.ToString(),
            new string[] {
                  PythonEngine.PythonPath,
                    // pip하면 설치되는 패키지 폴더.
                     Path.Combine(PYTHON_HOME, @"Lib\site-packages"),
                    // 개인 패키지 폴더
                      ReadPythonPath.GetFilePath(2),  // the root folder itself  under which demo package resides; demo package has demo.py module
                      ReadPythonPath.GetFilePath(3),
                      ReadPythonPath.GetFilePath(4)
            }
        );

        // Python 엔진 초기화
        PythonEngine.Initialize();
        // Global Interpreter Lock을 취득
        using (Py.GIL())
        {
            dynamic os = Py.Import("os");
            dynamic sys = Py.Import("sys");

            Debug.Log(sys.path);
            dynamic test = Py.Import("demo.cls");
            Debug.Log(test);
            Debug.Log(test.Calculator(3, 2).add());
            Debug.Log(test.funcname(31));
            // test.main();


            //dynamic ges = Py.Import("demo.demo");
            //Debug.Log(ges);
            //dynamic arg_model_file = ges.main();
        }
        PythonEngine.Shutdown();
        PythonEngine.PythonPath = "";
    }
}

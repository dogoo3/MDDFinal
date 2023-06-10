using System;
using System.IO;
using UnityEngine;
using Python.Runtime;

public class GesticulatorRunner : MonoBehaviour
{
    private Quaternion[,] _gestureData; // 제스처 데이터
    private string[] _jointNames; // Gesticulator가 생성하는 Joint 이름
    private const int JointCount = 15; // Gesticulator가 생성하는 Joint 갯수
    private int _frameCount; // Gesticulator가 생성한 제스처 데이터의 프레임 갯수
    
    private void Awake()
    {
        // Gesticulator가 생성하는 Joint 이름 세팅
        this._jointNames = new[]
        {
            "Spine", "Spine1", "Spine2", "Spine3", "Neck", "Neck1", "Head",
            "RightShoulder", "RightArm", "RightForeArm", "RightHand",
            "LeftShoulder", "LeftArm", "LeftForeArm", "LeftHand"
        };

        /* Pythonnet 세팅 시작 */
        // pathinfo.txt 읽기
        PathInfo.SetPathInfo();
        
        Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", PathInfo.GetPathInfo(0), EnvironmentVariableTarget.Process);
        // Debug.Log("Python DLL : " + ReadPythonPath.GetFilePath(0));
        var pythonHome = Environment.ExpandEnvironmentVariables(PathInfo.GetPathInfo(1));
        // Debug.Log("Python Home : " + pythonHome);
        var pythonPath = string.Join(
            Path.PathSeparator.ToString(),
            new[]
            {
                Path.Combine(pythonHome, @"Lib\site-packages"),
                Path.Combine(pythonHome, @"Lib"),
                Path.Combine(pythonHome, @"DLLs"),
                Path.Combine(Application.dataPath, @"Plugins\Gesticulator\gesticulator\visualization"),
                Path.Combine(Application.dataPath, @"Plugins\Gesticulator\gesticulator"),
                Path.Combine(Application.dataPath, @"Plugins\Gesticulator")
            }
        );
        PythonEngine.PythonPath = pythonPath;
         Debug.Log("Python Path : " + pythonPath);
        /* Pythonnet 세팅 끝 */
    }

    /**
     * Gesticulator 실행.
     */
    public Quaternion[,] RunGesticulator(string text, string wavFilePath)
    {
        Debug.Log("(5/8) Gesticulator 시작");
        // Debug.Log("Text : " + text);
        // Debug.Log("WAV File Path : " + wavFilePath);
        
        PythonEngine.Initialize();
        using (Py.GIL())
        {
            // demo_custom.py의 main 메소드를 실행해 모션 데이터 생성 
            dynamic demo = Py.Import("demo.demo_custom");
            dynamic motion = demo.main(text, wavFilePath);

            // 모션 데이터를 pandas.core.frame.dataFrame 타입으로 변환(Euler 타입)
            dynamic joblib = Py.Import("joblib");
            dynamic dataPipeline = joblib.
                load(Path.Combine(Application.dataPath, @"Plugins\Gesticulator\gesticulator\utils\data_pipe.sav"));
            dynamic jointAngles = dataPipeline.inverse_transform(motion)[0].values;
            
            // 모션 데이터에서 프레임 갯수 획득
            this._frameCount = (int) jointAngles.shape[0];
            
            // 제스처 데이터 2차원 배열 크기 지정
            this._gestureData = new Quaternion[this._frameCount, JointCount];

            // 제스처 데이터의 각 Row는 프레임, 각 Col은 Joint 순서로 Rotation값(Quaternion 타입)
            for (var i = 0; i < JointCount; i++)
            {
                dynamic x = jointAngles[_jointNames[i] + "_Xrotation"];
                dynamic y = jointAngles[_jointNames[i] + "_Yrotation"];
                dynamic z = jointAngles[_jointNames[i] + "_Zrotation"];

                for (var j = 0; j < this._frameCount; j++)
                {
                    // Euler > Quaternion 변환
                    var rotation  = Quaternion.Euler(
                        (float) x[j] * -1,
                        (float) y[j],
                        (float) z[j]
                    );
                    rotation.w *= -1;
                    
                    this._gestureData[j, i] = rotation; 
                }
            }
        } 
        PythonEngine.Shutdown();
        
        // 제스처 데이터 확인
        // for (var i = 0; i < this._frameCount; i++)
        // {
        //     for (var j = 0; j < this._jointCount; j++)
        //     {
        //         Debug.Log(this._gestureData[i, j]);
        //     }
        // }
        
        Debug.Log("(6/8) Gesticulator 종료");
        
        return this._gestureData;
    }
}

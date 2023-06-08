using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.Networking;

public class STTClova : MonoBehaviour
{
    public static STTClova instance;
    private AvatarManager _avatarManager;

    private void Awake()
    {
        instance = this;
        this._avatarManager = FindObjectOfType<AvatarManager>();
    }

    private IEnumerator PostRecordData(string url, AudioClip _clip)
    {
        // 유니티에서 HTTP 양식으로 구성된 서버에 POST할 때 사용하는 함수
        WWWForm form = new WWWForm();

        UnityWebRequest www = UnityWebRequest.Post(url, form);

        www.SetRequestHeader("X-NCP-APIGW-API-KEY-ID", "qhat4s91jn");
        www.SetRequestHeader("X-NCP-APIGW-API-KEY", "oM48E0sHtOVQf0QOAIkSFrdjTjygqlLEjCSHPk9l");
        www.SetRequestHeader("Content-Type", "application/octet-stream");

        www.uploadHandler = new UploadHandlerRaw(SavWav.GetWav(_clip, out var length));

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ProtocolError ||
            www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
        }
        else
        {
            if (www.result == UnityWebRequest.Result.Success)
            {
                // 오디오 클립을 wav 파일로 저장
                var wavFileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".wav";
                SavWav.Save(wavFileName, _clip);
                var wavFilePath = Path.Combine(Application.persistentDataPath, wavFileName);
                
                Debug.Log("(4/8) STT 종료");
                
                // 아바타 실행
                _avatarManager.RunAvatar(
                    System.Text.Encoding.UTF8.GetString(www.downloadHandler.data),
                    _clip,
                    wavFilePath
                );
            }
        }

        www.Dispose();
    }

    public void SendAudioSample(AudioClip _clip)
    {
        Debug.Log("(3/8) STT 시작");
        
        StartCoroutine(PostRecordData(
            "https://naveropenapi.apigw.ntruss.com/recog/v1/stt?lang=Kor", 
            _clip
        ));
    }
}

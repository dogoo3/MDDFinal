using UnityEngine;

public class GameDirector : MonoBehaviour
{
    private bool _playing; // 아바타 재생 시작 여부
    private MicHandler _micHandler;

    private void Awake()
    {
        this._micHandler = FindObjectOfType<MicHandler>();
    }

    //이하 Getter/Setter
    public bool GetPlaying()
    {
        return this._playing;
    }
    
    public void SetPlaying(bool val)
    {
        this._playing = val;

        // Silence 체크 코루틴 시작.
        if (!val)
        {
            this._micHandler.StartSilenceCheckRoutine();
        }
    }
}

using UnityEngine;

public class PlayDirector : MonoBehaviour
{
    private bool _playing; // 아바타 재생 시작 여부

    //이하 Getter/Setter
    public bool GetPlaying()
    {
        return this._playing;
    }
    
    public void SetPlaying(bool val)
    {
        this._playing = val;
    }
}

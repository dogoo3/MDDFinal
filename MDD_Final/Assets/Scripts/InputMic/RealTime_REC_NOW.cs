using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RealTime_REC_NOW : MonoBehaviour
{
    private AudioClip mic;
    private Text _buttonText;

    public ToggleGroup _toggleGroup_micList;

    [Range(0.04f, 5.0f)]
    public float rec_second = 0.04f;

    private bool _isRecord;

    private int lastSample = 0;
    private int channels = 0;
    private int collect_index = 0;

    private float[] samples = null;
    private float[] rec_collect = null;

    private void Awake()
    {
        this._buttonText = this.gameObject.GetComponentInChildren<Text>();
    }

    void Start()
    {
        this._isRecord = false;
        rec_collect = new float[(int)(44100 * rec_second)]; // STT API로 보낼 배열을 할당
    }

    // Update is called once per frame
    void Update()
    {
        if (this._isRecord) // 녹음 중이 아니면 실행하지 않는다.
        {
            ReadMic();
            // PlayMic();
        }
    }

    public void ShowSelectToggleName()
    {
        if (!Microphone.IsRecording(selectToggle.name))
        {
            this._buttonText.text = "녹음 종료";
            this._isRecord = true;
            DeviceManager.instance.SetAllToggleInteracable(false); // 녹음을 시작하면 Toggle을 조작할 수 없도록 모든 토글을 비활성화하는 함수를 호출
            this.mic = Microphone.Start(selectToggle.name, true, 100, 44100); //You can just give null for the mic name, it's gonna automatically detect the default mic of your system (k)
            this.channels = this.mic.channels; //mono or stereo, for me it's 1 (k)
        }
        else
        {
            this._buttonText.text = "녹음 시작";
            this._isRecord = false;
            DeviceManager.instance.SetAllToggleInteracable(true); // 녹음을 정지하면 다시 Toggle을 조작할 수 있도록 모든 토글을 활성화하는 함수를 호출
            Microphone.End(selectToggle.name);
            this.lastSample = 0; // 샘플 값 초기화
        }

    }

    private Toggle selectToggle
    {
        get { return this._toggleGroup_micList.ActiveToggles().FirstOrDefault(); }
        // C#의 구문(LINQ)
        // 활성화된 토글을 검색하면서, 가장 처음으로 활성화된 토글을 반환한다.
        // First와 FirstOrDefault의 차이는, 반환 객체의 존재 유무이다. 
        // First는 반환객체가 없으면 오류가 발생하고, FirstOrDefault는 반환이 없어도 됨.
    }

    private void ReadMic()
    {
        int t_pos = Microphone.GetPosition(selectToggle.name);

        int t_diff = t_pos - this.lastSample;

        if (t_diff > 0)
        {
            this.samples = new float[t_diff * this.channels];
            this.mic.GetData(this.samples, this.lastSample);

            this.samples.CopyTo(rec_collect, collect_index);
            collect_index += t_diff;
            if (rec_collect.Length - collect_index <= 1764) // 더 이상 녹음 데이터를 저장할 수 없을 경우, 1764는 0.04초 분량을 저장할 수 있는 sample 수를 뜻한다.
            {
                // STT API로 전송하는 코드
                AudioClip clip = AudioClip.Create("Real_time", collect_index, this.channels, 44100, false);

                clip.SetData(rec_collect, 0);
                SavWav.Save(Time.time.ToString(), clip);
                STTAPI.instance.SendAudioSample(clip);
                collect_index = 0; // 녹음 데이터를 0번 샘플부터 다시 저장하기 위한 인덱스 초기화
            }
        }
        this.lastSample = t_pos;
    }

    //private void PlayMic()
    //{
    //    if (this.source == null)
    //        return;

    //    // 맨 첫 프레임에서는 diff가 0이어서 float가 할당되지 않으므로, null 체크를 해줘야 함.
    //    // 또한 sample이 할당된다는 것은 소리가 녹음되었다는 뜻이므로, 이 경우에만 재생하도록 if를 걸어준다.
    //    if (this.samples != null)
    //    {
    //        this.source.clip = AudioClip.Create("Real_time", this.samples.Length, this.channels, 44100, false);
    //        this.source.spatialBlend = 0; //2D sound

    //        this.source.clip.SetData(samples, 0);

    //        if (!this.source.isPlaying)
    //        {
    //            this.source.Play();
    //        }

    //        this.samples = null;
    //    }
    //}
}

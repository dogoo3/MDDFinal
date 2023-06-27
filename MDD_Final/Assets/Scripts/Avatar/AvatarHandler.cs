using System.Collections;
using UnityEngine;

public class AvatarHandler : MonoBehaviour
{
    private PlayDirector _playDirector; // PlayDirector 클래스
    [SerializeField] private GameObject skeleton; // 스켈레톤
    [SerializeField] private GameObject avatar; // 아바타
    [SerializeField] private AudioSource audioSource; // 오디오 소스
    private HumanPoseHandler _srcHumanPoseHandler; // 스켈레톤 휴먼 포즈 핸들러
    private HumanPoseHandler _destHumanPoseHandler; // 아바타 휴먼 포즈 핸들러

    public void Awake()
    {
        this._playDirector = FindObjectOfType<PlayDirector>();
        
        // 스켈레톤, 아바타의 휴먼 포즈 핸들러 생성
        var skeletonAnimator = this.skeleton.gameObject.GetComponent<Animator>();
        var avatarAnimator = this.avatar.gameObject.GetComponent<Animator>();
        this._srcHumanPoseHandler = new HumanPoseHandler(skeletonAnimator.avatar, skeletonAnimator.gameObject.transform);
        this._destHumanPoseHandler = new HumanPoseHandler(avatarAnimator.avatar, avatarAnimator.gameObject.transform);
        
        // 오디오 소스를 스테레오 출력으로 믹싱 설정
        this.audioSource.spatialBlend = 0;
    }

    public void Start()
    {
        // 스켈레톤 제스처 전이로 인한 위치 이동 보정
        this.avatar.transform.position = new Vector3(0, 1, -8);
    }

    void Update()
    {
        // 스켈레톤 제스처를 아바타로 전이
        HumanPose humanPose = new HumanPose();
        this._srcHumanPoseHandler.GetHumanPose(ref humanPose);
        this._destHumanPoseHandler.SetHumanPose(ref humanPose);
    }

    /**
     * 오디오 재생.
     */
    public IEnumerator PlayAudio(string outputWavFilePath)
    {
        var outputAudioClip = LoadWav.Load(outputWavFilePath);
        
        // 오디오 재생
        this.audioSource.clip = outputAudioClip;
        this.audioSource.Play();
        
        // 재생이 끝날 때까지 대기
        yield return new WaitForSeconds(this.audioSource.clip.length);
        
        // 아바타 재생 프로세스 종료
        this._playDirector.SetPlaying(false);
    }
}

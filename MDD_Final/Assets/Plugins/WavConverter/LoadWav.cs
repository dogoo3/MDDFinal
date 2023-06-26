using UnityEngine;

public static class LoadWav
{
    public static AudioClip Load(string filePath)
    {
        // WAV 파일을 바이트 배열로 읽어들임
        byte[] wavData = System.IO.File.ReadAllBytes(filePath);
        
        // WAV 파일 포맷에 따른 정보 추출
        int fileSize = wavData.Length - 44;
        int frequency = System.BitConverter.ToInt32(wavData, 24);
        int channels = wavData[22];

        // AudioClip 생성
        AudioClip audioClip = AudioClip.Create("WAV", fileSize / 2, channels, frequency, false);

        // WAV 데이터에서 오디오 데이터 추출
        float[] audioData = new float[fileSize / 2];
        int offset = 0;
        for (int i = 0; i < fileSize / 2; i++)
        {
            short intData = System.BitConverter.ToInt16(wavData, offset + 44);
            audioData[i] = intData / 32768f;
            offset += 2;
        }

        // AudioClip에 오디오 데이터 적용
        audioClip.SetData(audioData, 0);

        return audioClip;
    }
}
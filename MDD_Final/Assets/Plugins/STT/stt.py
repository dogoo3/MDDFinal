import whisper


def main(input_wav_file_path):
    model = whisper.load_model("base")
    result = model.transcribe(input_wav_file_path)

    return result["text"]

from STT.api import STT


def main(output_text, save_path):
	# Init TTS with the target model name
	tts = TTS(model_name="tts_models/en/ljspeech/tacotron2-DDC", progress_bar=False, gpu=False)
	
	# Run TTS
	tts.tts_to_file(text=output_text, file_path=save_path)

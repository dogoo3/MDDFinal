from TTS.api import TTS

def main(_text):

	# Running a multi-speaker and multi-lingual model
	# List available 🐸TTS models and choose the first one
	#model_name = TTS.list_models()[7]
	#model_name = "tts_models/de/thorsten/tacotron2-DDC"
	# Init TTS
	#tts = TTS(model_name)

	# Run TTS
	# Running a single speaker model

	# Init TTS with the target model name
	# tts = TTS(model_name="tts_models/de/thorsten/tacotron2-DDC", progress_bar=False, gpu=False)
	tts = TTS(model_name="tts_models/en/ljspeech/tacotron2-DDC", progress_bar=False, gpu=False)
	# Run TTS
	#tts.tts_to_file(text="This is a test!!!", file_path="./test.wav")
	tts.tts_to_file(text=_text, file_path="./Assets/Resources/speech.wav")




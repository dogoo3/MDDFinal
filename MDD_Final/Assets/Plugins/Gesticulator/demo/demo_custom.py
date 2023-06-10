import torch
from gesticulator.model.model import GesticulatorModel
from gesticulator.interface.gesture_predictor import GesturePredictor


def main(text, wav_file_path):

    model_file_path = 'Assets/Plugins/Gesticulator/demo/models/default.ckpt'

    # 0. Check feature type based on the model
    feature_type, audio_dim = check_feature_type(model_file_path)

    # 1. Load the model
    model = GesticulatorModel.load_from_checkpoint(model_file_path, inference_mode=True)
    # This interface is a wrapper around the model for predicting new gestures conveniently
    gp = GesturePredictor(model, feature_type)

    # 2. Predict the gestures with the loaded model
    motion = gp.predict_gestures(wav_file_path, text)

    return motion.detach().numpy()


def check_feature_type(model_file):
    """
    Return the audio feature type and the corresponding dimensionality
    after inferring it from the given model file.
    """
    params = torch.load(model_file, map_location=torch.device('cpu'))

    # audio feature dim. + text feature dim.
    audio_plus_text_dim = params['state_dict']['encode_speech.0.weight'].shape[1]

    # This is a bit hacky, but we can rely on the fact that 
    # BERT has 768-dimensional vectors
    # We add 5 extra features on top of that in both cases.
    text_dim = 768 + 5

    audio_dim = audio_plus_text_dim - text_dim

    if audio_dim == 4:
        feature_type = "Pros"
    elif audio_dim == 64:
        feature_type = "Spectro"
    elif audio_dim == 68:
        feature_type = "Spectro+Pros"
    elif audio_dim == 26:
        feature_type = "MFCC"
    elif audio_dim == 30:
        feature_type = "MFCC+Pros"
    else:
        print("Error: Unknown audio feature type of dimension", audio_dim)
        exit(-1)

    return feature_type, audio_dim

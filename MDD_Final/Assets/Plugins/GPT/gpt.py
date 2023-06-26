import torch
from transformers import AutoModelForCausalLM, AutoTokenizer


def main(input_text):
    # 모델 Load
    model_name = "gpt2"
    tokenizer = AutoTokenizer.from_pretrained(model_name)
    model = AutoModelForCausalLM.from_pretrained(model_name)

    # 모델 입력 전처리
    input_ids = tokenizer.encode(input_text, return_tensors='pt')
    attention_mask = torch.ones(input_ids.shape, dtype=torch.long, device=input_ids.device)

    # 모델 추론
    output_ids = model.generate(
        input_ids=input_ids,
        attention_mask=attention_mask,
        pad_token_id=tokenizer.eos_token_id,
        do_sample=True,
        max_length=20,
        top_p=0.95,
        top_k=50,
        temperature=0.7
    )

    # 모델 출력 후처리
    output_text = tokenizer.decode(output_ids[0], skip_special_tokens=True)

    return output_text

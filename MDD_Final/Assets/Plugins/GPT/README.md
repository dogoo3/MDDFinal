# python-gpt2-udp

---

## 실행 방법
### Anaconda 파이썬 가상환경 실행
```sh
$> conda create -n {가상환경 이름} python=3.7
$> conda activate {가상환경 이름}
```

### 의존성 설치
```sh
(가상환경 이름)$> pip install -r requirements.txt
```

### 실행
- 학습없이 바로 실행
    ```sh
    (가상환경 이름)$> python main.py
    # 또는
    (가상환경 이름)$> python main.py -addr 127.0.0.1 -port 5202 -unityaddr 127.0.0.1 -unityport 5200
    ```

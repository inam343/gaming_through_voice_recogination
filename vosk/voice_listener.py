import sys
import os
import ctypes
from vosk import Model, KaldiRecognizer
import pyaudio
import json

# --------------------------
# Base folder for EXE or Python
# --------------------------
BASE_DIR = getattr(sys, "_MEIPASS", os.path.dirname(os.path.abspath(__file__)))

# --------------------------
# Force load Vosk DLL first (PyInstaller compatible)
# --------------------------
dll_path = os.path.join(BASE_DIR, "vosk", "libvosk.dll")  # DLL must be added to "vosk" folder in PyInstaller
if os.path.exists(dll_path):
    ctypes.WinDLL(dll_path)
else:
    print(f"Warning: libvosk.dll not found at {dll_path}!")

# --------------------------
# Path to Vosk model folder
# --------------------------
model_path = os.path.join(BASE_DIR, "vosk-model-small-en-us-0.15")
if not os.path.exists(model_path):
    raise FileNotFoundError(f"Vosk model folder not found at {model_path}")

model = Model(model_path)
recognizer = KaldiRecognizer(model, 16000)

# --------------------------
# Microphone setup
# --------------------------
p = pyaudio.PyAudio()
stream = p.open(format=pyaudio.paInt16,
                channels=1,
                rate=16000,
                input=True,
                frames_per_buffer=8192)
stream.start_stream()

# --------------------------
# Output text file
# --------------------------
status_file = os.path.join(BASE_DIR, "voice_command.txt")

print("Listening...")
while True:
    try:
        data = stream.read(4096, exception_on_overflow=False)
        if recognizer.AcceptWaveform(data):
            result = json.loads(recognizer.Result())
            text = result.get("text", "").lower().strip()
            if text:
                print("Heard:", text)
                with open(status_file, "w", encoding="utf-8") as f:
                    f.write(text)
    except Exception as e:
        print("Error:", e)

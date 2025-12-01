import sys
import json
import queue
import sounddevice as sd
from vosk import Model, KaldiRecognizer

# -----------------------------
# CONFIGURATION
# -----------------------------
MODEL_PATH = "model"   # Put your Vosk model folder here
SAMPLE_RATE = 16000     # Do not change


# -----------------------------
# Load Offline Model
# -----------------------------
try:
    print("Loading Vosk model...")
    model = Model(MODEL_PATH)
    recognizer = KaldiRecognizer(model, SAMPLE_RATE)
    print("Model loaded successfully!")
except Exception as e:
    print("ERROR loading model:", str(e))
    sys.exit(1)


# -----------------------------
# Prepare Microphone
# -----------------------------
q = queue.Queue()

def audio_callback(indata, frames, time, status):
    if status:
        print("Mic status:", status, file=sys.stderr)
    q.put(bytes(indata))


try:
    stream = sd.RawInputStream(
        samplerate=SAMPLE_RATE,
        blocksize=8000,
        dtype="int16",
        channels=1,
        callback=audio_callback
    )
except Exception as e:
    print("ERROR initializing microphone:", str(e))
    sys.exit(1)

stream.start()

print("Vosk Voice Listener Started (Offline)")
print("Say something...")

# -----------------------------
# Main Loop
# -----------------------------
while True:
    data = q.get()

    if recognizer.AcceptWaveform(data):
        result_json = json.loads(recognizer.Result())
        text = result_json.get("text", "").strip()

        if text != "":
            print(text)       # <-- C# reads this output
            sys.stdout.flush()
    else:
        # Partial results can be skipped to reduce noise
        pass

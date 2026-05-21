# SimpleOfflineTTS
High quality, offline text-to-speech in Unity, with multi-platform support.

# Platform Support
This plugin should work in all platforms supported by Unity Inference, there is no platform-specific code or library included.

# Requirements
The Project has been tested with Unity 6.0 and above. Some code changes to the inference would be required
to get it working in older Unity versions - it may be possible, but it is not officially supported.

Please add the Unity.Inference package (v2.5.0) in Package Manager.

Please add NewtonSoft JSON either in Package Manager, or using this git url:
com.unity.nuget.newtonsoft-json

# Demo Scene
Load up the DemoScene in Unity Editor, hit play, and then click on "Generate" for either voice.

Use the input text fields to change the text to whatever you want to generate.

You can change the backend from CPU to GPU by inspecting the TTSManager object, and selecting it in the drop-down.

Use the included Supertonic voices by dragging one of the voice assets from /ThirdParty/Supertonic-TTS-3/Voices/
into the TTSVoice component. Adjust steps to balance responsiveness and quality.

Feel free to download other Piper models and give them a try. Many are available here:

https://huggingface.co/rhasspy/piper-voices/tree/main

# Use in other projects
To use SimpleOfflineTTS in your own project, I suggest you take the TTSManager and customise it to your needs.
You could use the TTSVoice MonoBehaviour for each speaker, and its Speak() method for convenience.

# Reduce Deployment Size
You can use the quantization tools in Tools -> SimpleOfflineTTS to reduce some of the model sizes. The Supertonic
models still work well at fp16 (approximately half size), and the text encoder can even be reduced to uint8 (quarter)
without a significant loss in quality.

For a very small deployment, use the Piper models. They can be quantized to about 16MB (uint8) and still sound good.
If you are only using Piper as the TTS method, remove the references to the Supertonic ONNX models in TTSManager,
so they don't get included in your build.

# Known Issues
The "GPU Compute" or "GPU Pixel" backends may not work on mobile platforms, or with quantized models.
It's best to experiment with what works the best on your target platform.

Licensed under the Unity Asset Store Standard EULA.
© 2026 David Addis. All Rights Reserved.

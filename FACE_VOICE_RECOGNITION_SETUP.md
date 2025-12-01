# Face & Voice Recognition Setup Guide

## Overview
This implementation adds three authentication methods:
1. **Manual Login** - Traditional email/password
2. **Face Recognition** - Camera-based face capture
3. **Voice Recognition** - Microphone-based voice recording

## Required NuGet Packages

Install these packages via NuGet Package Manager or Package Manager Console:

```powershell
Install-Package AForge -Version 2.2.5
Install-Package AForge.Video -Version 2.2.5
Install-Package AForge.Video.DirectShow -Version 2.2.5
Install-Package NAudio -Version 2.1.0
```

## Database Schema Updates

Add these columns to your `user_info` table:

```sql
ALTER TABLE user_info
ADD FaceData VARBINARY(MAX) NULL,
    VoiceData VARBINARY(MAX) NULL;
```

## New Files Created

### Services:
- `Services/FaceRecognitionService.cs` - Camera integration and face capture
- `Services/VoiceRecognitionService.cs` - Microphone integration and voice recording

### Windows:
- `Views/FaceCaptureWindow.xaml` - Face capture UI
- `Views/FaceCaptureWindow.xaml.cs` - Face capture logic
- `Views/VoiceRecordingWindow.xaml` - Voice recording UI
- `Views/VoiceRecordingWindow.xaml.cs` - Voice recording logic

## Integration Points

### LoginWindow.xaml.cs - Add these methods:

```csharp
private async void CaptureFaceButton_Click(object sender, RoutedEventArgs e)
{
    var faceWindow = new FaceCaptureWindow();
    if (faceWindow.ShowDialog() == true && faceWindow.IsCaptured)
    {
        // TODO: Implement face matching logic
        GlassMessageBox.Show("Face recognition login will be available soon!");
    }
}

private async void VoiceLoginButton_Click(object sender, RoutedEventArgs e)
{
    var voiceWindow = new VoiceRecordingWindow();
    if (voiceWindow.ShowDialog() == true && voiceWindow.IsRecorded)
    {
        // TODO: Implement voice matching logic
        GlassMessageBox.Show("Voice recognition login will be available soon!");
    }
}
```

### SignUpWindow.xaml.cs - Add these methods:

```csharp
private byte[] capturedFaceData = null;
private byte[] capturedVoiceData = null;

private async void CaptureFaceButton_Click(object sender, RoutedEventArgs e)
{
    var faceWindow = new FaceCaptureWindow();
    if (faceWindow.ShowDialog() == true && faceWindow.IsCaptured)
    {
        capturedFaceData = faceWindow.CapturedFaceData;
        GlassMessageBox.Show("Face captured! You can now complete signup.");
    }
}

private async void VoiceSignupButton_Click(object sender, RoutedEventArgs e)
{
    var voiceWindow = new VoiceRecordingWindow();
    if (voiceWindow.ShowDialog() == true && voiceWindow.IsRecorded)
    {
        capturedVoiceData = voiceWindow.RecordedVoiceData;
        GlassMessageBox.Show("Voice recorded! You can now complete signup.");
    }
}

// Update SignupButton_Click to save face/voice data:
private void SignupButton_Click(object sender, RoutedEventArgs e)
{
    // ... existing validation code ...
    
    bool success;
    if (capturedFaceData != null)
    {
        success = db.AddUserWithFace(new UserModel { ... }, capturedFaceData);
    }
    else if (capturedVoiceData != null)
    {
        success = db.AddUserWithVoice(new UserModel { ... }, capturedVoiceData);
    }
    else
    {
        success = db.SignUp(name, age, email, password);
    }
    
    // ... rest of code ...
}
```

## Features

### Face Recognition:
- ✅ Real-time camera feed
- ✅ Face detection frame overlay
- ✅ Capture and store face image
- ✅ Success animation
- ⏳ Face matching (ML backend - to be implemented)

### Voice Recognition:
- ✅ Real-time audio recording
- ✅ Audio level visualization
- ✅ Pulse animation during recording
- ✅ Store voice data
- ⏳ Voice matching (ML backend - to be implemented)

## Next Steps

1. Install NuGet packages
2. Update database schema
3. Add new files to project
4. Update LoginWindow and SignUpWindow with integration code
5. Test camera and microphone access
6. Implement ML-based face/voice matching (future enhancement)

## Notes

- Camera and microphone permissions are handled by Windows
- Face/voice data is stored as VARBINARY in database
- Backend ML matching logic is placeholder for future implementation
- Works completely offline
- Supports multiple authentication methods per user

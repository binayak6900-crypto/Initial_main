using ADLCompiler.SemanticAnalysis;
using Xunit;

namespace ADLCompiler.Tests;

public class NDKAPITests
{
    [Fact]
    public void APIRegistry_LoadsOpenGLES20APIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var gles2API = bundle.LookupClass("android.opengl.GLES20");
        Assert.NotNull(gles2API);
        
        // Core functions
        var glClearMethod = bundle.LookupMethod("android.opengl.GLES20", "glClear");
        Assert.NotNull(glClearMethod);
        Assert.Equal("void", glClearMethod.ReturnType);
        Assert.Single(glClearMethod.Parameters);
        
        var glClearColorMethod = bundle.LookupMethod("android.opengl.GLES20", "glClearColor");
        Assert.NotNull(glClearColorMethod);
        Assert.Equal(4, glClearColorMethod.Parameters.Count);
        
        var glViewportMethod = bundle.LookupMethod("android.opengl.GLES20", "glViewport");
        Assert.NotNull(glViewportMethod);
        Assert.Equal(4, glViewportMethod.Parameters.Count);
        
        // Shader functions
        var glCreateShaderMethod = bundle.LookupMethod("android.opengl.GLES20", "glCreateShader");
        Assert.NotNull(glCreateShaderMethod);
        Assert.Equal("int", glCreateShaderMethod.ReturnType);
        
        var glCompileShaderMethod = bundle.LookupMethod("android.opengl.GLES20", "glCompileShader");
        Assert.NotNull(glCompileShaderMethod);
        
        var glCreateProgramMethod = bundle.LookupMethod("android.opengl.GLES20", "glCreateProgram");
        Assert.NotNull(glCreateProgramMethod);
        Assert.Equal("int", glCreateProgramMethod.ReturnType);
        
        // Drawing functions
        var glDrawArraysMethod = bundle.LookupMethod("android.opengl.GLES20", "glDrawArrays");
        Assert.NotNull(glDrawArraysMethod);
        Assert.Equal(3, glDrawArraysMethod.Parameters.Count);
        
        // Constants
        var colorBufferBitField = bundle.LookupField("android.opengl.GLES20", "GL_COLOR_BUFFER_BIT");
        Assert.NotNull(colorBufferBitField);
        Assert.True(colorBufferBitField.IsStatic);
        
        var trianglesField = bundle.LookupField("android.opengl.GLES20", "GL_TRIANGLES");
        Assert.NotNull(trianglesField);
    }

    [Fact]
    public void APIRegistry_LoadsOpenGLES30APIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var gles3API = bundle.LookupClass("android.opengl.GLES30");
        Assert.NotNull(gles3API);
        
        var glGenVertexArraysMethod = bundle.LookupMethod("android.opengl.GLES30", "glGenVertexArrays");
        Assert.NotNull(glGenVertexArraysMethod);
        Assert.Equal(3, glGenVertexArraysMethod.Parameters.Count);
        
        var glBindVertexArrayMethod = bundle.LookupMethod("android.opengl.GLES30", "glBindVertexArray");
        Assert.NotNull(glBindVertexArrayMethod);
        
        var glDrawArraysInstancedMethod = bundle.LookupMethod("android.opengl.GLES30", "glDrawArraysInstanced");
        Assert.NotNull(glDrawArraysInstancedMethod);
        Assert.Equal(4, glDrawArraysInstancedMethod.Parameters.Count);
    }

    [Fact]
    public void APIRegistry_LoadsOpenGLES31APIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var gles31API = bundle.LookupClass("android.opengl.GLES31");
        Assert.NotNull(gles31API);
        
        var glDispatchComputeMethod = bundle.LookupMethod("android.opengl.GLES31", "glDispatchCompute");
        Assert.NotNull(glDispatchComputeMethod);
        Assert.Equal(3, glDispatchComputeMethod.Parameters.Count);
        
        var glMemoryBarrierMethod = bundle.LookupMethod("android.opengl.GLES31", "glMemoryBarrier");
        Assert.NotNull(glMemoryBarrierMethod);
    }

    [Fact]
    public void APIRegistry_LoadsOpenGLES32APIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(24); // GLES 3.2 requires API 24+
        
        Assert.NotNull(bundle);
        var gles32API = bundle.LookupClass("android.opengl.GLES32");
        Assert.NotNull(gles32API);
        
        var glBlendBarrierMethod = bundle.LookupMethod("android.opengl.GLES32", "glBlendBarrier");
        Assert.NotNull(glBlendBarrierMethod);
        
        var glPrimitiveBoundingBoxMethod = bundle.LookupMethod("android.opengl.GLES32", "glPrimitiveBoundingBox");
        Assert.NotNull(glPrimitiveBoundingBoxMethod);
        Assert.Equal(8, glPrimitiveBoundingBoxMethod.Parameters.Count);
    }

    [Fact]
    public void APIRegistry_LoadsEGLAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var eglAPI = bundle.LookupClass("android.opengl.EGL14");
        Assert.NotNull(eglAPI);
        
        var eglGetDisplayMethod = bundle.LookupMethod("android.opengl.EGL14", "eglGetDisplay");
        Assert.NotNull(eglGetDisplayMethod);
        Assert.Equal("android.opengl.EGLDisplay", eglGetDisplayMethod.ReturnType);
        
        var eglInitializeMethod = bundle.LookupMethod("android.opengl.EGL14", "eglInitialize");
        Assert.NotNull(eglInitializeMethod);
        Assert.Equal("boolean", eglInitializeMethod.ReturnType);
        
        var eglCreateContextMethod = bundle.LookupMethod("android.opengl.EGL14", "eglCreateContext");
        Assert.NotNull(eglCreateContextMethod);
        Assert.Equal("android.opengl.EGLContext", eglCreateContextMethod.ReturnType);
        
        var eglSwapBuffersMethod = bundle.LookupMethod("android.opengl.EGL14", "eglSwapBuffers");
        Assert.NotNull(eglSwapBuffersMethod);
        
        var eglDefaultDisplayField = bundle.LookupField("android.opengl.EGL14", "EGL_DEFAULT_DISPLAY");
        Assert.NotNull(eglDefaultDisplayField);
    }

    [Fact]
    public void APIRegistry_LoadsVulkanAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(24); // Vulkan requires API 24+
        
        Assert.NotNull(bundle);
        
        // Vulkan support through SurfaceHolder
        var surfaceHolderAPI = bundle.LookupClass("android.view.SurfaceHolder");
        Assert.NotNull(surfaceHolderAPI);
        
        var getSurfaceMethod = bundle.LookupMethod("android.view.SurfaceHolder", "getSurface");
        Assert.NotNull(getSurfaceMethod);
        Assert.Equal("android.view.Surface", getSurfaceMethod.ReturnType);
    }

    [Fact]
    public void APIRegistry_LoadsVulkanHardwareBufferAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(26); // HardwareBuffer requires API 26+
        
        Assert.NotNull(bundle);
        var hardwareBufferAPI = bundle.LookupClass("android.hardware.HardwareBuffer");
        Assert.NotNull(hardwareBufferAPI);
        
        var usageGpuSampledField = bundle.LookupField("android.hardware.HardwareBuffer", "USAGE_GPU_SAMPLED_IMAGE");
        Assert.NotNull(usageGpuSampledField);
        
        var usageGpuColorField = bundle.LookupField("android.hardware.HardwareBuffer", "USAGE_GPU_COLOR_OUTPUT");
        Assert.NotNull(usageGpuColorField);
    }

    [Fact]
    public void APIRegistry_LoadsAudioTrackAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var audioTrackAPI = bundle.LookupClass("android.media.AudioTrack");
        Assert.NotNull(audioTrackAPI);
        
        var playMethod = bundle.LookupMethod("android.media.AudioTrack", "play");
        Assert.NotNull(playMethod);
        
        var pauseMethod = bundle.LookupMethod("android.media.AudioTrack", "pause");
        Assert.NotNull(pauseMethod);
        
        var writeMethod = bundle.LookupMethod("android.media.AudioTrack", "write");
        Assert.NotNull(writeMethod);
        Assert.Equal("int", writeMethod.ReturnType);
        Assert.Equal(3, writeMethod.Parameters.Count);
        
        var setStereoVolumeMethod = bundle.LookupMethod("android.media.AudioTrack", "setStereoVolume");
        Assert.NotNull(setStereoVolumeMethod);
        Assert.Equal(2, setStereoVolumeMethod.Parameters.Count);
        
        var modeStreamField = bundle.LookupField("android.media.AudioTrack", "MODE_STREAM");
        Assert.NotNull(modeStreamField);
    }

    [Fact]
    public void APIRegistry_LoadsAudioRecordAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var audioRecordAPI = bundle.LookupClass("android.media.AudioRecord");
        Assert.NotNull(audioRecordAPI);
        
        var startRecordingMethod = bundle.LookupMethod("android.media.AudioRecord", "startRecording");
        Assert.NotNull(startRecordingMethod);
        
        var stopMethod = bundle.LookupMethod("android.media.AudioRecord", "stop");
        Assert.NotNull(stopMethod);
        
        var readMethod = bundle.LookupMethod("android.media.AudioRecord", "read");
        Assert.NotNull(readMethod);
        Assert.Equal("int", readMethod.ReturnType);
        
        var recordStateField = bundle.LookupField("android.media.AudioRecord", "RECORDSTATE_RECORDING");
        Assert.NotNull(recordStateField);
    }

    [Fact]
    public void APIRegistry_LoadsAAudioAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(26); // AAudio requires API 26+
        
        Assert.NotNull(bundle);
        var aaudioAPI = bundle.LookupClass("android.media.AudioTrack");
        Assert.NotNull(aaudioAPI);
        
        var setPerformanceModeMethod = bundle.LookupMethod("android.media.AudioTrack", "setPerformanceMode");
        Assert.NotNull(setPerformanceModeMethod);
        
        var lowLatencyField = bundle.LookupField("android.media.AudioTrack", "PERFORMANCE_MODE_LOW_LATENCY");
        Assert.NotNull(lowLatencyField);
    }

    [Fact]
    public void APIRegistry_LoadsSensorManagerAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var sensorManagerAPI = bundle.LookupClass("android.hardware.SensorManager");
        Assert.NotNull(sensorManagerAPI);
        
        var getDefaultSensorMethod = bundle.LookupMethod("android.hardware.SensorManager", "getDefaultSensor");
        Assert.NotNull(getDefaultSensorMethod);
        Assert.Equal("android.hardware.Sensor", getDefaultSensorMethod.ReturnType);
        
        var registerListenerMethod = bundle.LookupMethod("android.hardware.SensorManager", "registerListener");
        Assert.NotNull(registerListenerMethod);
        Assert.Equal("boolean", registerListenerMethod.ReturnType);
        Assert.Equal(3, registerListenerMethod.Parameters.Count);
        
        var unregisterListenerMethod = bundle.LookupMethod("android.hardware.SensorManager", "unregisterListener");
        Assert.NotNull(unregisterListenerMethod);
        
        var delayFastestField = bundle.LookupField("android.hardware.SensorManager", "SENSOR_DELAY_FASTEST");
        Assert.NotNull(delayFastestField);
    }

    [Fact]
    public void APIRegistry_LoadsSensorAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var sensorAPI = bundle.LookupClass("android.hardware.Sensor");
        Assert.NotNull(sensorAPI);
        
        var getNameMethod = bundle.LookupMethod("android.hardware.Sensor", "getName");
        Assert.NotNull(getNameMethod);
        Assert.Equal("String", getNameMethod.ReturnType);
        
        var getTypeMethod = bundle.LookupMethod("android.hardware.Sensor", "getType");
        Assert.NotNull(getTypeMethod);
        Assert.Equal("int", getTypeMethod.ReturnType);
        
        var getMaximumRangeMethod = bundle.LookupMethod("android.hardware.Sensor", "getMaximumRange");
        Assert.NotNull(getMaximumRangeMethod);
        Assert.Equal("float", getMaximumRangeMethod.ReturnType);
        
        // Sensor types
        var accelerometerField = bundle.LookupField("android.hardware.Sensor", "TYPE_ACCELEROMETER");
        Assert.NotNull(accelerometerField);
        
        var gyroscopeField = bundle.LookupField("android.hardware.Sensor", "TYPE_GYROSCOPE");
        Assert.NotNull(gyroscopeField);
        
        var magneticField = bundle.LookupField("android.hardware.Sensor", "TYPE_MAGNETIC_FIELD");
        Assert.NotNull(magneticField);
        
        var gravityField = bundle.LookupField("android.hardware.Sensor", "TYPE_GRAVITY");
        Assert.NotNull(gravityField);
        
        var proximityField = bundle.LookupField("android.hardware.Sensor", "TYPE_PROXIMITY");
        Assert.NotNull(proximityField);
    }

    [Fact]
    public void APIRegistry_LoadsSensorEventAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var sensorEventAPI = bundle.LookupClass("android.hardware.SensorEvent");
        Assert.NotNull(sensorEventAPI);
        
        var valuesField = bundle.LookupField("android.hardware.SensorEvent", "values");
        Assert.NotNull(valuesField);
        Assert.Equal("float[]", valuesField.Type);
        
        var timestampField = bundle.LookupField("android.hardware.SensorEvent", "timestamp");
        Assert.NotNull(timestampField);
        Assert.Equal("long", timestampField.Type);
        
        var accuracyField = bundle.LookupField("android.hardware.SensorEvent", "accuracy");
        Assert.NotNull(accuracyField);
        
        var sensorField = bundle.LookupField("android.hardware.SensorEvent", "sensor");
        Assert.NotNull(sensorField);
        Assert.Equal("android.hardware.Sensor", sensorField.Type);
    }

    [Fact]
    public void APIRegistry_LoadsMotionEventAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var motionEventAPI = bundle.LookupClass("android.view.MotionEvent");
        Assert.NotNull(motionEventAPI);
        
        var getActionMethod = bundle.LookupMethod("android.view.MotionEvent", "getAction");
        Assert.NotNull(getActionMethod);
        Assert.Equal("int", getActionMethod.ReturnType);
        
        var getXMethod = bundle.LookupMethod("android.view.MotionEvent", "getX");
        Assert.NotNull(getXMethod);
        Assert.Equal("float", getXMethod.ReturnType);
        
        var getPointerCountMethod = bundle.LookupMethod("android.view.MotionEvent", "getPointerCount");
        Assert.NotNull(getPointerCountMethod);
        
        var actionDownField = bundle.LookupField("android.view.MotionEvent", "ACTION_DOWN");
        Assert.NotNull(actionDownField);
        
        var actionMoveField = bundle.LookupField("android.view.MotionEvent", "ACTION_MOVE");
        Assert.NotNull(actionMoveField);
    }

    [Fact]
    public void APIRegistry_LoadsKeyEventAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var keyEventAPI = bundle.LookupClass("android.view.KeyEvent");
        Assert.NotNull(keyEventAPI);
        
        var getActionMethod = bundle.LookupMethod("android.view.KeyEvent", "getAction");
        Assert.NotNull(getActionMethod);
        
        var getKeyCodeMethod = bundle.LookupMethod("android.view.KeyEvent", "getKeyCode");
        Assert.NotNull(getKeyCodeMethod);
        Assert.Equal("int", getKeyCodeMethod.ReturnType);
        
        var isShiftPressedMethod = bundle.LookupMethod("android.view.KeyEvent", "isShiftPressed");
        Assert.NotNull(isShiftPressedMethod);
        Assert.Equal("boolean", isShiftPressedMethod.ReturnType);
        
        var keyCodeAField = bundle.LookupField("android.view.KeyEvent", "KEYCODE_A");
        Assert.NotNull(keyCodeAField);
        
        var keyCodeSpaceField = bundle.LookupField("android.view.KeyEvent", "KEYCODE_SPACE");
        Assert.NotNull(keyCodeSpaceField);
        
        var keyCodeDpadUpField = bundle.LookupField("android.view.KeyEvent", "KEYCODE_DPAD_UP");
        Assert.NotNull(keyCodeDpadUpField);
    }

    [Fact]
    public void APIRegistry_LoadsInputDeviceAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var inputDeviceAPI = bundle.LookupClass("android.view.InputDevice");
        Assert.NotNull(inputDeviceAPI);
        
        var getDeviceMethod = bundle.LookupMethod("android.view.InputDevice", "getDevice");
        Assert.NotNull(getDeviceMethod);
        Assert.Equal("android.view.InputDevice", getDeviceMethod.ReturnType);
        
        var getDeviceIdsMethod = bundle.LookupMethod("android.view.InputDevice", "getDeviceIds");
        Assert.NotNull(getDeviceIdsMethod);
        Assert.Equal("int[]", getDeviceIdsMethod.ReturnType);
        
        var getNameMethod = bundle.LookupMethod("android.view.InputDevice", "getName");
        Assert.NotNull(getNameMethod);
        
        var sourceGamepadField = bundle.LookupField("android.view.InputDevice", "SOURCE_GAMEPAD");
        Assert.NotNull(sourceGamepadField);
        
        var sourceJoystickField = bundle.LookupField("android.view.InputDevice", "SOURCE_JOYSTICK");
        Assert.NotNull(sourceJoystickField);
    }

    [Fact]
    public void APIRegistry_LoadsCameraManagerAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var cameraManagerAPI = bundle.LookupClass("android.hardware.camera2.CameraManager");
        Assert.NotNull(cameraManagerAPI);
        
        var getCameraIdListMethod = bundle.LookupMethod("android.hardware.camera2.CameraManager", "getCameraIdList");
        Assert.NotNull(getCameraIdListMethod);
        Assert.Equal("String[]", getCameraIdListMethod.ReturnType);
        
        var getCameraCharacteristicsMethod = bundle.LookupMethod("android.hardware.camera2.CameraManager", "getCameraCharacteristics");
        Assert.NotNull(getCameraCharacteristicsMethod);
        Assert.Equal("android.hardware.camera2.CameraCharacteristics", getCameraCharacteristicsMethod.ReturnType);
        
        var openCameraMethod = bundle.LookupMethod("android.hardware.camera2.CameraManager", "openCamera");
        Assert.NotNull(openCameraMethod);
        Assert.Equal(3, openCameraMethod.Parameters.Count);
    }

    [Fact]
    public void APIRegistry_LoadsCameraDeviceAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var cameraDeviceAPI = bundle.LookupClass("android.hardware.camera2.CameraDevice");
        Assert.NotNull(cameraDeviceAPI);
        
        var createCaptureSessionMethod = bundle.LookupMethod("android.hardware.camera2.CameraDevice", "createCaptureSession");
        Assert.NotNull(createCaptureSessionMethod);
        Assert.Equal(3, createCaptureSessionMethod.Parameters.Count);
        
        var createCaptureRequestMethod = bundle.LookupMethod("android.hardware.camera2.CameraDevice", "createCaptureRequest");
        Assert.NotNull(createCaptureRequestMethod);
        Assert.Equal("android.hardware.camera2.CaptureRequest.Builder", createCaptureRequestMethod.ReturnType);
        
        var templatePreviewField = bundle.LookupField("android.hardware.camera2.CameraDevice", "TEMPLATE_PREVIEW");
        Assert.NotNull(templatePreviewField);
        
        var templateCaptureField = bundle.LookupField("android.hardware.camera2.CameraDevice", "TEMPLATE_STILL_CAPTURE");
        Assert.NotNull(templateCaptureField);
    }

    [Fact]
    public void APIRegistry_LoadsCaptureRequestBuilderAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var captureRequestAPI = bundle.LookupClass("android.hardware.camera2.CaptureRequest.Builder");
        Assert.NotNull(captureRequestAPI);
        
        var addTargetMethod = bundle.LookupMethod("android.hardware.camera2.CaptureRequest.Builder", "addTarget");
        Assert.NotNull(addTargetMethod);
        
        var setMethod = bundle.LookupMethod("android.hardware.camera2.CaptureRequest.Builder", "set");
        Assert.NotNull(setMethod);
        Assert.Equal(2, setMethod.Parameters.Count);
        
        var buildMethod = bundle.LookupMethod("android.hardware.camera2.CaptureRequest.Builder", "build");
        Assert.NotNull(buildMethod);
        Assert.Equal("android.hardware.camera2.CaptureRequest", buildMethod.ReturnType);
    }

    [Fact]
    public void APIRegistry_LoadsCameraCaptureSessionAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var captureSessionAPI = bundle.LookupClass("android.hardware.camera2.CameraCaptureSession");
        Assert.NotNull(captureSessionAPI);
        
        var setRepeatingRequestMethod = bundle.LookupMethod("android.hardware.camera2.CameraCaptureSession", "setRepeatingRequest");
        Assert.NotNull(setRepeatingRequestMethod);
        Assert.Equal("int", setRepeatingRequestMethod.ReturnType);
        Assert.Equal(3, setRepeatingRequestMethod.Parameters.Count);
        
        var captureMethod = bundle.LookupMethod("android.hardware.camera2.CameraCaptureSession", "capture");
        Assert.NotNull(captureMethod);
        Assert.Equal("int", captureMethod.ReturnType);
        
        var closeMethod = bundle.LookupMethod("android.hardware.camera2.CameraCaptureSession", "close");
        Assert.NotNull(closeMethod);
    }

    [Fact]
    public void APIRegistry_NDKAPIsNotAvailableInLowerSDKVersions()
    {
        var registry = new APIRegistry();
        
        // GLES 3.2 should not be available in API 23
        var bundle23 = registry.GetSDKBundle(23);
        Assert.NotNull(bundle23);
        var gles32API = bundle23.LookupClass("android.opengl.GLES32");
        Assert.Null(gles32API); // Should not exist in API 23
        
        // GLES 3.2 should be available in API 24+
        var bundle24 = registry.GetSDKBundle(24);
        Assert.NotNull(bundle24);
        var gles32API24 = bundle24.LookupClass("android.opengl.GLES32");
        Assert.NotNull(gles32API24); // Should exist in API 24
    }

    [Fact]
    public void APIRegistry_AAudioNotAvailableBeforeAPI26()
    {
        var registry = new APIRegistry();
        
        // AAudio should not have performance mode in API 23
        var bundle23 = registry.GetSDKBundle(23);
        Assert.NotNull(bundle23);
        var audioTrack23 = bundle23.LookupClass("android.media.AudioTrack");
        Assert.NotNull(audioTrack23);
        var perfModeMethod23 = bundle23.LookupMethod("android.media.AudioTrack", "setPerformanceMode");
        Assert.Null(perfModeMethod23); // Should not exist in API 23
        
        // AAudio should have performance mode in API 26+
        var bundle26 = registry.GetSDKBundle(26);
        Assert.NotNull(bundle26);
        var perfModeMethod26 = bundle26.LookupMethod("android.media.AudioTrack", "setPerformanceMode");
        Assert.NotNull(perfModeMethod26); // Should exist in API 26
    }
}

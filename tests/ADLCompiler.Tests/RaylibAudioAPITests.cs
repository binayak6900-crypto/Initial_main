using ADLCompiler.SemanticAnalysis;
using Xunit;

namespace ADLCompiler.Tests;

/// <summary>
/// Tests for raylib audio API bindings
/// Validates Requirement 14.9: Built-in access to all raylib audio functions
/// Task 16.3: Implement raylib audio functions
/// </summary>
public class RaylibAudioAPITests
{
    [Fact]
    public void APIRegistry_LoadsRaylibAudioDeviceAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var raylibAudioAPI = bundle.LookupClass("raylib.Audio");
        Assert.NotNull(raylibAudioAPI);
        
        // Audio device - initAudioDevice
        var initAudioDeviceMethod = bundle.LookupMethod("raylib.Audio", "initAudioDevice");
        Assert.NotNull(initAudioDeviceMethod);
        Assert.Equal("void", initAudioDeviceMethod.ReturnType);
        Assert.Empty(initAudioDeviceMethod.Parameters);
        Assert.True(initAudioDeviceMethod.IsStatic);
        
        // Audio device - closeAudioDevice
        var closeAudioDeviceMethod = bundle.LookupMethod("raylib.Audio", "closeAudioDevice");
        Assert.NotNull(closeAudioDeviceMethod);
        Assert.Equal("void", closeAudioDeviceMethod.ReturnType);
        Assert.Empty(closeAudioDeviceMethod.Parameters);
        Assert.True(closeAudioDeviceMethod.IsStatic);
        
        // Audio device - isAudioDeviceReady
        var isAudioDeviceReadyMethod = bundle.LookupMethod("raylib.Audio", "isAudioDeviceReady");
        Assert.NotNull(isAudioDeviceReadyMethod);
        Assert.Equal("boolean", isAudioDeviceReadyMethod.ReturnType);
        Assert.Empty(isAudioDeviceReadyMethod.Parameters);
        Assert.True(isAudioDeviceReadyMethod.IsStatic);
    }

    [Fact]
    public void APIRegistry_LoadsRaylibSoundLoadingAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var raylibAudioAPI = bundle.LookupClass("raylib.Audio");
        Assert.NotNull(raylibAudioAPI);
        
        // Sound loading - loadSound
        var loadSoundMethod = bundle.LookupMethod("raylib.Audio", "loadSound");
        Assert.NotNull(loadSoundMethod);
        Assert.Equal("raylib.Sound", loadSoundMethod.ReturnType);
        Assert.Single(loadSoundMethod.Parameters);
        Assert.Equal("fileName", loadSoundMethod.Parameters[0].Name);
        Assert.Equal("String", loadSoundMethod.Parameters[0].Type);
        Assert.True(loadSoundMethod.IsStatic);
        
        // Sound unloading - unloadSound
        var unloadSoundMethod = bundle.LookupMethod("raylib.Audio", "unloadSound");
        Assert.NotNull(unloadSoundMethod);
        Assert.Equal("void", unloadSoundMethod.ReturnType);
        Assert.Single(unloadSoundMethod.Parameters);
        Assert.Equal("sound", unloadSoundMethod.Parameters[0].Name);
        Assert.Equal("raylib.Sound", unloadSoundMethod.Parameters[0].Type);
        Assert.True(unloadSoundMethod.IsStatic);
    }

    [Fact]
    public void APIRegistry_LoadsRaylibSoundPlaybackAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var raylibAudioAPI = bundle.LookupClass("raylib.Audio");
        Assert.NotNull(raylibAudioAPI);
        
        // Sound playback - playSound
        var playSoundMethod = bundle.LookupMethod("raylib.Audio", "playSound");
        Assert.NotNull(playSoundMethod);
        Assert.Equal("void", playSoundMethod.ReturnType);
        Assert.Single(playSoundMethod.Parameters);
        Assert.Equal("sound", playSoundMethod.Parameters[0].Name);
        Assert.Equal("raylib.Sound", playSoundMethod.Parameters[0].Type);
        Assert.True(playSoundMethod.IsStatic);
        
        // Sound playback - stopSound
        var stopSoundMethod = bundle.LookupMethod("raylib.Audio", "stopSound");
        Assert.NotNull(stopSoundMethod);
        Assert.Equal("void", stopSoundMethod.ReturnType);
        Assert.Single(stopSoundMethod.Parameters);
        Assert.Equal("sound", stopSoundMethod.Parameters[0].Name);
        Assert.Equal("raylib.Sound", stopSoundMethod.Parameters[0].Type);
        Assert.True(stopSoundMethod.IsStatic);
        
        // Sound playback - pauseSound
        var pauseSoundMethod = bundle.LookupMethod("raylib.Audio", "pauseSound");
        Assert.NotNull(pauseSoundMethod);
        Assert.Equal("void", pauseSoundMethod.ReturnType);
        Assert.Single(pauseSoundMethod.Parameters);
        Assert.Equal("sound", pauseSoundMethod.Parameters[0].Name);
        Assert.Equal("raylib.Sound", pauseSoundMethod.Parameters[0].Type);
        Assert.True(pauseSoundMethod.IsStatic);
        
        // Sound playback - resumeSound
        var resumeSoundMethod = bundle.LookupMethod("raylib.Audio", "resumeSound");
        Assert.NotNull(resumeSoundMethod);
        Assert.Equal("void", resumeSoundMethod.ReturnType);
        Assert.Single(resumeSoundMethod.Parameters);
        Assert.Equal("sound", resumeSoundMethod.Parameters[0].Name);
        Assert.Equal("raylib.Sound", resumeSoundMethod.Parameters[0].Type);
        Assert.True(resumeSoundMethod.IsStatic);
        
        // Sound playback - isSoundPlaying
        var isSoundPlayingMethod = bundle.LookupMethod("raylib.Audio", "isSoundPlaying");
        Assert.NotNull(isSoundPlayingMethod);
        Assert.Equal("boolean", isSoundPlayingMethod.ReturnType);
        Assert.Single(isSoundPlayingMethod.Parameters);
        Assert.Equal("sound", isSoundPlayingMethod.Parameters[0].Name);
        Assert.Equal("raylib.Sound", isSoundPlayingMethod.Parameters[0].Type);
        Assert.True(isSoundPlayingMethod.IsStatic);
    }

    [Fact]
    public void APIRegistry_LoadsRaylibSoundPropertiesAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var raylibAudioAPI = bundle.LookupClass("raylib.Audio");
        Assert.NotNull(raylibAudioAPI);
        
        // Sound properties - setSoundVolume
        var setSoundVolumeMethod = bundle.LookupMethod("raylib.Audio", "setSoundVolume");
        Assert.NotNull(setSoundVolumeMethod);
        Assert.Equal("void", setSoundVolumeMethod.ReturnType);
        Assert.Equal(2, setSoundVolumeMethod.Parameters.Count);
        Assert.Equal("sound", setSoundVolumeMethod.Parameters[0].Name);
        Assert.Equal("raylib.Sound", setSoundVolumeMethod.Parameters[0].Type);
        Assert.Equal("volume", setSoundVolumeMethod.Parameters[1].Name);
        Assert.Equal("float", setSoundVolumeMethod.Parameters[1].Type);
        Assert.True(setSoundVolumeMethod.IsStatic);
        
        // Sound properties - setSoundPitch
        var setSoundPitchMethod = bundle.LookupMethod("raylib.Audio", "setSoundPitch");
        Assert.NotNull(setSoundPitchMethod);
        Assert.Equal("void", setSoundPitchMethod.ReturnType);
        Assert.Equal(2, setSoundPitchMethod.Parameters.Count);
        Assert.Equal("sound", setSoundPitchMethod.Parameters[0].Name);
        Assert.Equal("raylib.Sound", setSoundPitchMethod.Parameters[0].Type);
        Assert.Equal("pitch", setSoundPitchMethod.Parameters[1].Name);
        Assert.Equal("float", setSoundPitchMethod.Parameters[1].Type);
        Assert.True(setSoundPitchMethod.IsStatic);
        
        // Sound properties - setSoundPan
        var setSoundPanMethod = bundle.LookupMethod("raylib.Audio", "setSoundPan");
        Assert.NotNull(setSoundPanMethod);
        Assert.Equal("void", setSoundPanMethod.ReturnType);
        Assert.Equal(2, setSoundPanMethod.Parameters.Count);
        Assert.Equal("sound", setSoundPanMethod.Parameters[0].Name);
        Assert.Equal("raylib.Sound", setSoundPanMethod.Parameters[0].Type);
        Assert.Equal("pan", setSoundPanMethod.Parameters[1].Name);
        Assert.Equal("float", setSoundPanMethod.Parameters[1].Type);
        Assert.True(setSoundPanMethod.IsStatic);
    }

    [Fact]
    public void APIRegistry_LoadsRaylibMusicLoadingAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var raylibAudioAPI = bundle.LookupClass("raylib.Audio");
        Assert.NotNull(raylibAudioAPI);
        
        // Music loading - loadMusicStream
        var loadMusicStreamMethod = bundle.LookupMethod("raylib.Audio", "loadMusicStream");
        Assert.NotNull(loadMusicStreamMethod);
        Assert.Equal("raylib.Music", loadMusicStreamMethod.ReturnType);
        Assert.Single(loadMusicStreamMethod.Parameters);
        Assert.Equal("fileName", loadMusicStreamMethod.Parameters[0].Name);
        Assert.Equal("String", loadMusicStreamMethod.Parameters[0].Type);
        Assert.True(loadMusicStreamMethod.IsStatic);
        
        // Music unloading - unloadMusicStream
        var unloadMusicStreamMethod = bundle.LookupMethod("raylib.Audio", "unloadMusicStream");
        Assert.NotNull(unloadMusicStreamMethod);
        Assert.Equal("void", unloadMusicStreamMethod.ReturnType);
        Assert.Single(unloadMusicStreamMethod.Parameters);
        Assert.Equal("music", unloadMusicStreamMethod.Parameters[0].Name);
        Assert.Equal("raylib.Music", unloadMusicStreamMethod.Parameters[0].Type);
        Assert.True(unloadMusicStreamMethod.IsStatic);
    }

    [Fact]
    public void APIRegistry_LoadsRaylibMusicPlaybackAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var raylibAudioAPI = bundle.LookupClass("raylib.Audio");
        Assert.NotNull(raylibAudioAPI);
        
        // Music playback - playMusicStream
        var playMusicStreamMethod = bundle.LookupMethod("raylib.Audio", "playMusicStream");
        Assert.NotNull(playMusicStreamMethod);
        Assert.Equal("void", playMusicStreamMethod.ReturnType);
        Assert.Single(playMusicStreamMethod.Parameters);
        Assert.Equal("music", playMusicStreamMethod.Parameters[0].Name);
        Assert.Equal("raylib.Music", playMusicStreamMethod.Parameters[0].Type);
        Assert.True(playMusicStreamMethod.IsStatic);
        
        // Music playback - stopMusicStream
        var stopMusicStreamMethod = bundle.LookupMethod("raylib.Audio", "stopMusicStream");
        Assert.NotNull(stopMusicStreamMethod);
        Assert.Equal("void", stopMusicStreamMethod.ReturnType);
        Assert.Single(stopMusicStreamMethod.Parameters);
        Assert.Equal("music", stopMusicStreamMethod.Parameters[0].Name);
        Assert.Equal("raylib.Music", stopMusicStreamMethod.Parameters[0].Type);
        Assert.True(stopMusicStreamMethod.IsStatic);
        
        // Music playback - pauseMusicStream
        var pauseMusicStreamMethod = bundle.LookupMethod("raylib.Audio", "pauseMusicStream");
        Assert.NotNull(pauseMusicStreamMethod);
        Assert.Equal("void", pauseMusicStreamMethod.ReturnType);
        Assert.Single(pauseMusicStreamMethod.Parameters);
        Assert.Equal("music", pauseMusicStreamMethod.Parameters[0].Name);
        Assert.Equal("raylib.Music", pauseMusicStreamMethod.Parameters[0].Type);
        Assert.True(pauseMusicStreamMethod.IsStatic);
        
        // Music playback - resumeMusicStream
        var resumeMusicStreamMethod = bundle.LookupMethod("raylib.Audio", "resumeMusicStream");
        Assert.NotNull(resumeMusicStreamMethod);
        Assert.Equal("void", resumeMusicStreamMethod.ReturnType);
        Assert.Single(resumeMusicStreamMethod.Parameters);
        Assert.Equal("music", resumeMusicStreamMethod.Parameters[0].Name);
        Assert.Equal("raylib.Music", resumeMusicStreamMethod.Parameters[0].Type);
        Assert.True(resumeMusicStreamMethod.IsStatic);
        
        // Music playback - updateMusicStream
        var updateMusicStreamMethod = bundle.LookupMethod("raylib.Audio", "updateMusicStream");
        Assert.NotNull(updateMusicStreamMethod);
        Assert.Equal("void", updateMusicStreamMethod.ReturnType);
        Assert.Single(updateMusicStreamMethod.Parameters);
        Assert.Equal("music", updateMusicStreamMethod.Parameters[0].Name);
        Assert.Equal("raylib.Music", updateMusicStreamMethod.Parameters[0].Type);
        Assert.True(updateMusicStreamMethod.IsStatic);
        
        // Music playback - isMusicStreamPlaying
        var isMusicStreamPlayingMethod = bundle.LookupMethod("raylib.Audio", "isMusicStreamPlaying");
        Assert.NotNull(isMusicStreamPlayingMethod);
        Assert.Equal("boolean", isMusicStreamPlayingMethod.ReturnType);
        Assert.Single(isMusicStreamPlayingMethod.Parameters);
        Assert.Equal("music", isMusicStreamPlayingMethod.Parameters[0].Name);
        Assert.Equal("raylib.Music", isMusicStreamPlayingMethod.Parameters[0].Type);
        Assert.True(isMusicStreamPlayingMethod.IsStatic);
    }

    [Fact]
    public void APIRegistry_LoadsRaylibMusicPropertiesAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var raylibAudioAPI = bundle.LookupClass("raylib.Audio");
        Assert.NotNull(raylibAudioAPI);
        
        // Music properties - setMusicVolume
        var setMusicVolumeMethod = bundle.LookupMethod("raylib.Audio", "setMusicVolume");
        Assert.NotNull(setMusicVolumeMethod);
        Assert.Equal("void", setMusicVolumeMethod.ReturnType);
        Assert.Equal(2, setMusicVolumeMethod.Parameters.Count);
        Assert.Equal("music", setMusicVolumeMethod.Parameters[0].Name);
        Assert.Equal("raylib.Music", setMusicVolumeMethod.Parameters[0].Type);
        Assert.Equal("volume", setMusicVolumeMethod.Parameters[1].Name);
        Assert.Equal("float", setMusicVolumeMethod.Parameters[1].Type);
        Assert.True(setMusicVolumeMethod.IsStatic);
        
        // Music properties - setMusicPitch
        var setMusicPitchMethod = bundle.LookupMethod("raylib.Audio", "setMusicPitch");
        Assert.NotNull(setMusicPitchMethod);
        Assert.Equal("void", setMusicPitchMethod.ReturnType);
        Assert.Equal(2, setMusicPitchMethod.Parameters.Count);
        Assert.Equal("music", setMusicPitchMethod.Parameters[0].Name);
        Assert.Equal("raylib.Music", setMusicPitchMethod.Parameters[0].Type);
        Assert.Equal("pitch", setMusicPitchMethod.Parameters[1].Name);
        Assert.Equal("float", setMusicPitchMethod.Parameters[1].Type);
        Assert.True(setMusicPitchMethod.IsStatic);
        
        // Music properties - setMusicPan
        var setMusicPanMethod = bundle.LookupMethod("raylib.Audio", "setMusicPan");
        Assert.NotNull(setMusicPanMethod);
        Assert.Equal("void", setMusicPanMethod.ReturnType);
        Assert.Equal(2, setMusicPanMethod.Parameters.Count);
        Assert.Equal("music", setMusicPanMethod.Parameters[0].Name);
        Assert.Equal("raylib.Music", setMusicPanMethod.Parameters[0].Type);
        Assert.Equal("pan", setMusicPanMethod.Parameters[1].Name);
        Assert.Equal("float", setMusicPanMethod.Parameters[1].Type);
        Assert.True(setMusicPanMethod.IsStatic);
    }

    [Fact]
    public void APIRegistry_LoadsRaylibMusicTimeAPIs()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var raylibAudioAPI = bundle.LookupClass("raylib.Audio");
        Assert.NotNull(raylibAudioAPI);
        
        // Music time - getMusicTimeLength
        var getMusicTimeLengthMethod = bundle.LookupMethod("raylib.Audio", "getMusicTimeLength");
        Assert.NotNull(getMusicTimeLengthMethod);
        Assert.Equal("float", getMusicTimeLengthMethod.ReturnType);
        Assert.Single(getMusicTimeLengthMethod.Parameters);
        Assert.Equal("music", getMusicTimeLengthMethod.Parameters[0].Name);
        Assert.Equal("raylib.Music", getMusicTimeLengthMethod.Parameters[0].Type);
        Assert.True(getMusicTimeLengthMethod.IsStatic);
        
        // Music time - getMusicTimePlayed
        var getMusicTimePlayedMethod = bundle.LookupMethod("raylib.Audio", "getMusicTimePlayed");
        Assert.NotNull(getMusicTimePlayedMethod);
        Assert.Equal("float", getMusicTimePlayedMethod.ReturnType);
        Assert.Single(getMusicTimePlayedMethod.Parameters);
        Assert.Equal("music", getMusicTimePlayedMethod.Parameters[0].Name);
        Assert.Equal("raylib.Music", getMusicTimePlayedMethod.Parameters[0].Type);
        Assert.True(getMusicTimePlayedMethod.IsStatic);
        
        // Music time - seekMusicStream
        var seekMusicStreamMethod = bundle.LookupMethod("raylib.Audio", "seekMusicStream");
        Assert.NotNull(seekMusicStreamMethod);
        Assert.Equal("void", seekMusicStreamMethod.ReturnType);
        Assert.Equal(2, seekMusicStreamMethod.Parameters.Count);
        Assert.Equal("music", seekMusicStreamMethod.Parameters[0].Name);
        Assert.Equal("raylib.Music", seekMusicStreamMethod.Parameters[0].Type);
        Assert.Equal("position", seekMusicStreamMethod.Parameters[1].Name);
        Assert.Equal("float", seekMusicStreamMethod.Parameters[1].Type);
        Assert.True(seekMusicStreamMethod.IsStatic);
    }

    [Fact]
    public void APIRegistry_RaylibAudioAPIsAvailableInAllSDKVersions()
    {
        var registry = new APIRegistry();
        
        // Raylib audio should be available in all SDK versions from 23 onwards
        foreach (var apiLevel in new[] { 23, 24, 26, 28, 30, 33, 35 })
        {
            var bundle = registry.GetSDKBundle(apiLevel);
            Assert.NotNull(bundle);
            
            var raylibAudioAPI = bundle.LookupClass("raylib.Audio");
            Assert.NotNull(raylibAudioAPI);
            
            // Verify key audio functions exist
            var initAudioDeviceMethod = bundle.LookupMethod("raylib.Audio", "initAudioDevice");
            Assert.NotNull(initAudioDeviceMethod);
            
            var loadSoundMethod = bundle.LookupMethod("raylib.Audio", "loadSound");
            Assert.NotNull(loadSoundMethod);
            
            var playSoundMethod = bundle.LookupMethod("raylib.Audio", "playSound");
            Assert.NotNull(playSoundMethod);
            
            var loadMusicStreamMethod = bundle.LookupMethod("raylib.Audio", "loadMusicStream");
            Assert.NotNull(loadMusicStreamMethod);
            
            var playMusicStreamMethod = bundle.LookupMethod("raylib.Audio", "playMusicStream");
            Assert.NotNull(playMusicStreamMethod);
            
            var updateMusicStreamMethod = bundle.LookupMethod("raylib.Audio", "updateMusicStream");
            Assert.NotNull(updateMusicStreamMethod);
        }
    }

    [Fact]
    public void APIRegistry_RaylibAudioMethodsAreStatic()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        
        // All raylib audio functions should be static (global functions)
        var initAudioDeviceMethod = bundle.LookupMethod("raylib.Audio", "initAudioDevice");
        Assert.NotNull(initAudioDeviceMethod);
        Assert.True(initAudioDeviceMethod.IsStatic);
        
        var loadSoundMethod = bundle.LookupMethod("raylib.Audio", "loadSound");
        Assert.NotNull(loadSoundMethod);
        Assert.True(loadSoundMethod.IsStatic);
        
        var playSoundMethod = bundle.LookupMethod("raylib.Audio", "playSound");
        Assert.NotNull(playSoundMethod);
        Assert.True(playSoundMethod.IsStatic);
        
        var setSoundVolumeMethod = bundle.LookupMethod("raylib.Audio", "setSoundVolume");
        Assert.NotNull(setSoundVolumeMethod);
        Assert.True(setSoundVolumeMethod.IsStatic);
        
        var loadMusicStreamMethod = bundle.LookupMethod("raylib.Audio", "loadMusicStream");
        Assert.NotNull(loadMusicStreamMethod);
        Assert.True(loadMusicStreamMethod.IsStatic);
        
        var playMusicStreamMethod = bundle.LookupMethod("raylib.Audio", "playMusicStream");
        Assert.NotNull(playMusicStreamMethod);
        Assert.True(playMusicStreamMethod.IsStatic);
        
        var updateMusicStreamMethod = bundle.LookupMethod("raylib.Audio", "updateMusicStream");
        Assert.NotNull(updateMusicStreamMethod);
        Assert.True(updateMusicStreamMethod.IsStatic);
        
        var setMusicVolumeMethod = bundle.LookupMethod("raylib.Audio", "setMusicVolume");
        Assert.NotNull(setMusicVolumeMethod);
        Assert.True(setMusicVolumeMethod.IsStatic);
    }

    [Fact]
    public void APIRegistry_RaylibAudioAPIHasCorrectMinSDK()
    {
        var registry = new APIRegistry();
        var bundle = registry.GetSDKBundle(23);
        
        Assert.NotNull(bundle);
        var raylibAudioAPI = bundle.LookupClass("raylib.Audio");
        Assert.NotNull(raylibAudioAPI);
        
        // All raylib audio functions should have minSDK of 23
        Assert.Equal(23, raylibAudioAPI.MinSDK);
        
        var initAudioDeviceMethod = bundle.LookupMethod("raylib.Audio", "initAudioDevice");
        Assert.NotNull(initAudioDeviceMethod);
        Assert.Equal(23, initAudioDeviceMethod.MinSDK);
        
        var loadSoundMethod = bundle.LookupMethod("raylib.Audio", "loadSound");
        Assert.NotNull(loadSoundMethod);
        Assert.Equal(23, loadSoundMethod.MinSDK);
        
        var loadMusicStreamMethod = bundle.LookupMethod("raylib.Audio", "loadMusicStream");
        Assert.NotNull(loadMusicStreamMethod);
        Assert.Equal(23, loadMusicStreamMethod.MinSDK);
    }
}

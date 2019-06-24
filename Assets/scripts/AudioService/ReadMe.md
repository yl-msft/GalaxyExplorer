## Audio Service
# Code 
The AudioService is surfaced via a generic interface IAudioService\<EnumType> where EnumType is an Enum. This generic interface allows users to define their own enums for audio ids.  The generic interface is then extended to a concrete interface IAudioService.  This allows for users to define their own ids but not have to redefine the concrete type at every usage. This also allows for addtional interfaces to be added to the API surface such as the IAudioMixer. 

# Using the AudioService
The AudioService can be used by following the service locator pattern currently used in MRTK.  First get the Service by calling MixedRealityToolkit.Instance.GetService\<IAudioService>(), then use the result to play audio clips. You may want to store the resulting service in a member variable if you have multiple audio calls in one class.
 
#   Adding new sounds
To add a new sound simply extend the AudioId Enum found in the audio profile, and then map that enum to an audio file in the audio profile. The profile is found under scriptable_objects->AudioServiceProfile. Background music can be added by adding a new audio source in core_systems_scene under Managers->MusicAudioSources.  A transition can be applied using the audio mixer found in audio->audio_mixers->music_audio_mixer. 
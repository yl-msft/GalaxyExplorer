using System;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Core.Interfaces;

public interface IAudioService<EnumType> : IMixedRealityExtensionService where EnumType : Enum
{
    void PlayClip(EnumType audioId);
}
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class RotationPlayableClip : PlayableAsset, ITimelineClipAsset
{
    public RotationPlayableBehaviour template = new RotationPlayableBehaviour ();

    public ClipCaps clipCaps
    {
        get { return ClipCaps.Blending; }
    }

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<RotationPlayableBehaviour>.Create (graph, template);
        return playable;
    }
}

using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class RotationPlayableMixerBehaviour : PlayableBehaviour
{
    Vector3 m_DefaultLocalEulerAngles;

    Vector3 m_AssignedLocalEulerAngles;

    Transform m_TrackBinding;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        m_TrackBinding = playerData as Transform;

        if (m_TrackBinding == null)
            return;

        if (m_TrackBinding.localEulerAngles != m_AssignedLocalEulerAngles)
            m_DefaultLocalEulerAngles = m_TrackBinding.localEulerAngles;

        int inputCount = playable.GetInputCount ();

        Vector3 blendedLocalEulerAngles = Vector3.zero;
        float totalWeight = 0f;
        float greatestWeight = 0f;

        for (int i = 0; i < inputCount; i++)
        {
            float inputWeight = playable.GetInputWeight(i);
            ScriptPlayable<RotationPlayableBehaviour> inputPlayable = (ScriptPlayable<RotationPlayableBehaviour>)playable.GetInput(i);
            RotationPlayableBehaviour input = inputPlayable.GetBehaviour ();
            
            blendedLocalEulerAngles += input.localEulerAngles * inputWeight;
            totalWeight += inputWeight;

            if (inputWeight > greatestWeight)
            {
                greatestWeight = inputWeight;
            }
        }

        m_AssignedLocalEulerAngles = blendedLocalEulerAngles + m_DefaultLocalEulerAngles * (1f - totalWeight);
        m_TrackBinding.localEulerAngles = m_AssignedLocalEulerAngles;
    }
}

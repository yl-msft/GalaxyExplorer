using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class SpritePlayableMixerBehaviour : PlayableBehaviour
{
    Color m_DefaultColor;
    bool m_DefaultEnabled;

    Color m_AssignedColor;
    bool m_AssignedEnabled;

    SpriteRenderer m_TrackBinding;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        m_TrackBinding = playerData as SpriteRenderer;

        if (m_TrackBinding == null)
            return;

        if (m_TrackBinding.color != m_AssignedColor)
            m_DefaultColor = m_TrackBinding.color;
        if (m_TrackBinding.enabled != m_AssignedEnabled)
            m_DefaultEnabled = m_TrackBinding.enabled;

        int inputCount = playable.GetInputCount ();

        Color blendedColor = Color.clear;
        float totalWeight = 0f;
        float greatestWeight = 0f;
        int currentInputs = 0;

        for (int i = 0; i < inputCount; i++)
        {
            float inputWeight = playable.GetInputWeight(i);
            ScriptPlayable<SpritePlayableBehaviour> inputPlayable = (ScriptPlayable<SpritePlayableBehaviour>)playable.GetInput(i);
            SpritePlayableBehaviour input = inputPlayable.GetBehaviour ();
            
            blendedColor += input.color * inputWeight;
            totalWeight += inputWeight;

            if (inputWeight > greatestWeight)
            {
                m_AssignedEnabled = input.enabled;
                m_TrackBinding.enabled = m_AssignedEnabled;
                greatestWeight = inputWeight;
            }

            if (!Mathf.Approximately (inputWeight, 0f))
                currentInputs++;
        }

        m_AssignedColor = blendedColor + m_DefaultColor * (1f - totalWeight);
        m_TrackBinding.color = m_AssignedColor;

        if (currentInputs != 1 && 1f - totalWeight > greatestWeight)
        {
            m_TrackBinding.enabled = m_DefaultEnabled;
        }
    }
}

// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Component responsible for playing background music 
/// </summary>
namespace GalaxyExplorer
{
    public class MusicManager : MonoBehaviour
    {
        [SerializeField]
        private AudioMixer Mixer = null;

        [SerializeField]
        private string Welcome = "01_Welcome";

        [SerializeField]
        private string Galaxy = "02_Galaxy";

        [SerializeField]
        private string SolarSystem = "03_SolarSystem";

        [SerializeField]
        private string Planet = "04_PlanetaryView";

        private const float TransitionTime = 3.8f;

        public string WelcomeTrack
        {
            get { return Welcome; }
        }

        public string GalaxyTrack
        {
            get { return Galaxy; }
        }

        public string SolarSystemTrack
        {
            get { return SolarSystem; }
        }

        public string PlanetTrack
        {
            get { return Planet; }
        }

        public bool FindSnapshotAndTransition(string name, float time = TransitionTime)
        {
            bool transitioned = false;

            if (Mixer)
            {
                AudioMixerSnapshot snapshot = Mixer.FindSnapshot(name);

                if (snapshot)
                {
                    snapshot.TransitionTo(time);
                    transitioned = true;
                }
                else
                {
                    Debug.LogWarning("Couldn't find AudioMixer Snapshot with name " + name);
                }
            }

            return transitioned;
        }
    }
}
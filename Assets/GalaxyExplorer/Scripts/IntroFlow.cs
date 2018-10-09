using MRS.FlowManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GalaxyExplorer
{
    public class IntroFlow : MonoBehaviour
    {
        [System.Serializable]
        public class IntroStage
        {
            [SerializeField]
            private AudioClip Audio;

            [SerializeField]
            private float AudioDelay = 0.0f;

            [SerializeField]
            private IntroFlowState Stage;

            public AudioClip GetAudio { get { return Audio; } }

            public float GetAudioDelay { get { return AudioDelay; } }

            public IntroFlowState GetStage { get { return Stage; } }
        }

  
        [SerializeField]
        private List<IntroStage> IntroStages = new List<IntroStage>();

        [SerializeField]
        private VOManager VOManagerScript = null;

        private IntroFlowState currentState = IntroFlowState.kLogo;

        private MusicManager musicManagerScript = null;
        private FlowManager flowManagerScript = null;
        private ViewLoader viewLoaderScript = null;

        public delegate void IntroFinishedCallback();
        public IntroFinishedCallback OnIntroFinished;


        public enum IntroFlowState
        {
            kLogo,
            kEarthPin,
            kSolarView,
            kGalaxyView
        }

        public void OnStageTransition(int timedstage)
        {
            if (timedstage > 0 && timedstage - 1 < IntroStages.Count)
            {
                if (VOManagerScript)
                {
                    VOManagerScript.PlayClip(IntroStages[timedstage - 1].GetAudio, IntroStages[timedstage - 1].GetAudioDelay);
                }
                
                currentState = IntroStages[timedstage - 1].GetStage;
            }

            // Intro has finished
            if (timedstage + 1 == IntroStages.Count && OnIntroFinished != null)
            {
                OnIntroFinished.Invoke();
            }
        }

        public void OnSceneIsLoaded()
        {
            PlacementControl placement = FindObjectOfType<PlacementControl>();
            if (placement)
            {
                placement.OnContentPlaced += OnPlacementFinished;
            }
        }

        public void OnPlacementFinished(Vector3 position)
        {
            flowManagerScript.AdvanceStage();

            viewLoaderScript.transform.position = position;

            // rotate to face camera
            var lookPos = Camera.main.transform.position - position;
            lookPos.y = 0;
            var rotation = Quaternion.LookRotation(-lookPos);
            viewLoaderScript.transform.rotation = rotation;

            FindObjectOfType<WorldAnchorHandler>().CreateWorldAnchor();
        }

        void Start()
        {
            musicManagerScript = FindObjectOfType<MusicManager>();

            flowManagerScript = FindObjectOfType<FlowManager>();
            flowManagerScript.OnStageTransition += OnStageTransition;

            viewLoaderScript = FindObjectOfType<ViewLoader>();
            viewLoaderScript.OnSceneIsLoaded += OnSceneIsLoaded;

            StartCoroutine(PlayWelcomeMusic());
        }

        private IEnumerator PlayWelcomeMusic()
        {
            yield return new WaitForEndOfFrame();

            if (musicManagerScript)
            {
                musicManagerScript.FindSnapshotAndTransition(musicManagerScript.WelcomeTrack);
            }

            yield return null;
        }
    }
}

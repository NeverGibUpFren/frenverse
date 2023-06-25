using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Tessera
{
    public static class AnimatedGeneratorEditorUpdates
    {
        private static List<AnimatedGenerator> generators = new List<AnimatedGenerator>();

        public static void Add(AnimatedGenerator ag)
        {
            if(generators.Count == 0)
            {
                EditorApplication.update += Update;
            }
            generators.Add(ag);
        }

        public static void Remove(AnimatedGenerator ag)
        {
            generators.Remove(ag);
            if (generators.Count == 0)
            {
                EditorApplication.update -= Update;
            }
        }

        private static void Update()
        {
            generators.RemoveAll(x => x == null || x.State == AnimatedGenerator.AnimatedGeneratorState.Stopped || x.State == AnimatedGenerator.AnimatedGeneratorState.Paused);
            if (generators.Count == 0)
            {
                EditorApplication.update -= Update;
            }
            foreach(var ag in generators)
            {
                ag.Step();
            }
        }
    }


    [CustomEditor(typeof(AnimatedGenerator))]
    class AnimatedGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var ag = (AnimatedGenerator)target;
            bool showStart = false;
            bool showStop = false;
            bool showPause = false;
            bool showResume = false;

            switch (ag.State)
            {
                case AnimatedGenerator.AnimatedGeneratorState.Stopped:
                    showStart = true;
                    break;
                case AnimatedGenerator.AnimatedGeneratorState.Initializing:
                    showStop = true;
                    break;
                case AnimatedGenerator.AnimatedGeneratorState.Running:
                    showStop = showPause = true;
                    break;
                case AnimatedGenerator.AnimatedGeneratorState.Paused:
                    showStop = showResume = true;
                    break;
            }

            if(showPause)
            {

                if (GUILayout.Button("Pause"))
                {
                    ag.PauseGeneration();

                    if (!Application.isPlaying)
                    {
                        AnimatedGeneratorEditorUpdates.Remove(ag);
                    }
                }
            }

            if(showResume)
            {
                if (GUILayout.Button("Resume"))
                {
                    ag.ResumeGeneration();

                    if (!Application.isPlaying)
                    {
                        AnimatedGeneratorEditorUpdates.Add(ag);
                    }
                }
            }

            if (showStart)
            {
                if (GUILayout.Button("Start"))
                {
                    ag.StartGeneration();

                    if (!Application.isPlaying)
                    {
                        AnimatedGeneratorEditorUpdates.Add(ag);
                    }
                }
            }

            if (showStop)
            {
                if (GUILayout.Button("Stop"))
                {
                    ag.StopGeneration();

                    if (!Application.isPlaying)
                    {
                        AnimatedGeneratorEditorUpdates.Remove(ag);
                    }
                }
            }
        }
    }
}

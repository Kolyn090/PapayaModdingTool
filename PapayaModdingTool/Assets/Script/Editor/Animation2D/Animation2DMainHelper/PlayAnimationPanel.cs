using System;
using System.Collections.Generic;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using PapayaModdingTool.Assets.Script.Editor.Universal;
using PapayaModdingTool.Assets.Script.Editor.Universal.GraphicUI;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Animation2D.Animation2DMainHelper
{
    public class PlayAnimationPanel : BaseEditorWindow
    {
        private const int Target_Size = 256;

        private List<SpriteButtonData> _frames = new();
        private int _currentFrame = 0;
        private float _fps = 9f;
        private bool _isPlaying = false;
        private double _lastFrameTime;

        public static void Open(List<SpriteButtonData> animationData)
        {
            var window = CreateInstance<PlayAnimationPanel>(); // create new instance
            window.titleContent = new GUIContent($"{ELT("play_animation")}: {Guid.NewGuid()}");
            window._frames = animationData;
            window.Show();
        }

        private void OnEnable()
        {
            _currentFrame = 0;
            _lastFrameTime = EditorApplication.timeSinceStartup;

            // hook into editor update loop
            EditorApplication.update += UpdateAnimation;
        }

        private void OnDisable()
        {
            EditorApplication.update -= UpdateAnimation;
        }

        private void UpdateAnimation()
        {
            if (!_isPlaying || _frames.Count == 0) return;

            double timeNow = EditorApplication.timeSinceStartup;
            double frameDuration = 1.0 / _fps;

            if (timeNow - _lastFrameTime >= frameDuration)
            {
                _currentFrame = (_currentFrame + 1) % _frames.Count;
                _lastFrameTime = timeNow;
                Repaint();
            }
        }

        private void OnGUI()
        {
            GUILayout.Space(10);

            // Controls
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(_isPlaying ? "Pause" : "Play", GUILayout.Width(80)))
            {
                _isPlaying = !_isPlaying;
                _lastFrameTime = EditorApplication.timeSinceStartup;
            }
            if (GUILayout.Button("Stop", GUILayout.Width(80)))
            {
                _isPlaying = false;
                _currentFrame = 0;
                Repaint();
            }
            GUILayout.EndHorizontal();

            // FPS slider
            _fps = EditorGUILayout.Slider("Speed (FPS)", _fps, 1f, 60f);

            GUILayout.Space(10);

            // Draw current frame
            if (_frames.Count > 0 && _frames[_currentFrame] != null)
            {
                Texture2D tex = _frames[_currentFrame].sprite;
                tex.filterMode = FilterMode.Point;

                Rect rect = GUILayoutUtility.GetRect(Target_Size, Target_Size, GUILayout.ExpandWidth(false));

                // Add padding
                float padding = 10f;
                rect.x += padding;
                rect.y += padding;
                rect.width -= padding * 2;
                rect.height -= padding * 2;

                GUI.DrawTexture(rect, tex, ScaleMode.ScaleToFit, true);

                PivotPoint.MakePivot(_frames[_currentFrame].pivot, rect);
            }
            else
            {
                EditorGUILayout.LabelField("No frames loaded.");
            }
        }
    }
}

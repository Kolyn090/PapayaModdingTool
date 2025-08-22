using System;
using System.Collections.Generic;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using PapayaModdingTool.Assets.Script.Editor.Universal;
using PapayaModdingTool.Assets.Script.Editor.Universal.GraphicUI;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Atlas2D.Atlas2DMainHelper
{
    public class PlayAnimationPanel : BaseEditorWindow
    {
        private List<SpriteButtonData> _frames = new();
        private int _targetSize = 256;
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
            GUILayout.BeginHorizontal();
            GUILayout.Label("Target Size:", GUILayout.Width(80));
            _targetSize = EditorGUILayout.IntField(_targetSize, GUILayout.Width(60));
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Draw current frame
            if (_frames.Count > 0 && _frames[_currentFrame] != null)
            {
                Texture2D tex = _frames[_currentFrame].sprite;
                tex.filterMode = FilterMode.Point;

                Rect rect = GUILayoutUtility.GetRect(_targetSize, _targetSize, GUILayout.ExpandWidth(false));

                EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f, 1f));

                Vector2 fixedPivotPos = new(0.5f, 0f);
                PivotPoint.MakePivot(fixedPivotPos, rect);

                SpriteButtonData sprite = _frames[_currentFrame];
                float scale = 4f;

                Vector2 drawSize = new(sprite.width * scale, sprite.height * scale);

                Vector2 drawPos = new(_targetSize/2 + rect.x - scale * sprite.pivot.x * sprite.width,
                                        rect.y + _targetSize - scale * ((1 - sprite.pivot.y) * sprite.height));
                Rect drawRect = new(drawPos.x, drawPos.y, drawSize.x, drawSize.y);

                GUI.DrawTexture(drawRect, sprite.sprite, ScaleMode.ScaleToFit, true);
            }
            else
            {
                EditorGUILayout.LabelField("No frames loaded.");
            }
        }
    }
}

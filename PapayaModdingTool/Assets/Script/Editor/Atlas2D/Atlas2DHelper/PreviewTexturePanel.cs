using System;
using System.Collections.Generic;
using PapayaModdingTool.Assets.Script.DataStruct.PreviewWorkplace;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using PapayaModdingTool.Assets.Script.Editor.Atlas2D.Atlas2DHelper;
using PapayaModdingTool.Assets.Script.Writer.Atlas2D;
using UnityEditor;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.Editor.Atlas2DMainHelper
{
    public class PreviewTexturePanel
    {
        public Func<string, string> ELT;
        public Func<WorkplaceExportor> GetWorkplaceExportor;
        public Func<List<SpriteButtonData>> GetDatas;

        private Rect _guiRect;
        private Rect _imageRect;

        private bool _hasInit;

        private Texture2D _workplaceTexture;
        private RenderTexture _renderTexture;
        private Texture2D _checkerBoardTexture;
        private List<SpriteButtonData> _workplace;
        private bool _needUpdateWorkplaceTexture = false;

        private PreviewMarkPanel _previewMarkPanel;
        private ZoomPanController _zoomPanController;

        public void Initialize(Rect bound)
        {
            float totalHeight = bound.height;
            float guiHeight = totalHeight * 0.155f;
            float imageHeight = totalHeight - guiHeight;
            _guiRect = new Rect(bound.x, bound.y, bound.width, guiHeight);
            _imageRect = new Rect(bound.x, bound.y + guiHeight, bound.width, imageHeight);

            // GetTexture().filterMode = FilterMode.Point;

            _checkerBoardTexture = new(2, 2)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Repeat
            };
            _checkerBoardTexture.SetPixels32(new Color32[]
            {
                new(153,153,153,255), new(102,102,102,255),
                new(102,102,102,255), new(153,153,153,255)
            });
            _checkerBoardTexture.Apply();

            _zoomPanController = new(_imageRect, () => _workplaceTexture);
            _previewMarkPanel = new(_imageRect, _zoomPanController);

            // _panOffset = new(0, -GetTexture().height / 2f);

            _hasInit = true;
        }

        public void UpdateWorkplace(List<SpriteButtonData> workplace)
        {
            _workplace = workplace;
            _needUpdateWorkplaceTexture = true;
        }

        public void CreatePanel()
        {
            if (!_hasInit)
                return;

            if (_needUpdateWorkplaceTexture && _workplace != null)
            {
                _workplaceTexture = Workplace.CreatePreview(_workplace);
                _previewMarkPanel.MakeWorkplaceTexture(new() { new() }, _workplaceTexture.width, _workplaceTexture.height);
                _needUpdateWorkplaceTexture = false;
            }

            GUI.BeginGroup(_guiRect);
            EditorGUILayout.LabelField(ELT("workplace"), EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(_workplaceTexture == null);
            if (GUILayout.Button(ELT("export_workplace"), GUILayout.Width(_guiRect.width - 15f)))
            {
                GetWorkplaceExportor().Export(_workplaceTexture, GetDatas());
                Debug.Log("Export success!");
            }

            GUILayout.BeginHorizontal();

            // Set buttons to a fixed width
            float buttonWidth = _guiRect.width * 0.5f - 10f;
            if (GUILayout.Button(ELT("zoom_in"), GUILayout.Width(buttonWidth)))
            {
                _zoomPanController.ZoomAtCenter(1.2f, _imageRect);
            }

            if (GUILayout.Button(ELT("zoom_out"), GUILayout.Width(buttonWidth)))
            {
                _zoomPanController.ZoomAtCenter(1 / 1.2f, _imageRect);
            }
            GUILayout.EndHorizontal();

            // Compute pan bounds based on zoomed image size
            float panWidth = Mathf.Max((_workplaceTexture != null ?
                                        _workplaceTexture.width : 0f) * _zoomPanController.Zoom - _imageRect.width, 0f) / 2f;
            float panHeight = Mathf.Max((_workplaceTexture != null ?
                                        _workplaceTexture.height : 0f) * _zoomPanController.Zoom - _imageRect.height, 0f) / 2f;
            float sliderWidth = _guiRect.width - 20f;
            _zoomPanController.SetPanOffsetX(EditorGUILayout.Slider(ELT("pan_x"),
                                            _zoomPanController.PanOffset.x,
                                            -panWidth,
                                            panWidth,
                                            GUILayout.Width(sliderWidth)));
            _zoomPanController.SetPanOffsetY(EditorGUILayout.Slider(ELT("pan_y"),
                                            _zoomPanController.PanOffset.y,
                                            -panHeight,
                                            panHeight,
                                            GUILayout.Width(sliderWidth)));
            EditorGUI.EndDisabledGroup();
            GUI.EndGroup();

            // --- Preview Panel ---
            GUI.Box(_imageRect, ELT("preview"));

            // Update and draw the preview texture
            GUI.BeginGroup(_imageRect);

            // Checkerboard background
            GUI.DrawTextureWithTexCoords(
                new Rect(0, 0, _imageRect.width, _imageRect.height),
                _checkerBoardTexture,
                new Rect(0, 0, _imageRect.width / 16f, _imageRect.height / 16f)
            );

            // Workplace preview
            if (_workplaceTexture != null)
            {
                if (NeedsRenderUpdate())
                {
                    UpdatePreviewTexture((int)_imageRect.width, (int)_imageRect.height);
                    _previewMarkPanel.UpdatePreviewTexture();
                    _zoomPanController.UpdateLast();
                }

                GUI.DrawTexture(
                    new Rect(0, 0, _imageRect.width, _imageRect.height), // local group coords
                    _renderTexture,
                    ScaleMode.StretchToFill,
                    true
                );
                GUI.DrawTexture(
                    new Rect(0, 0, _imageRect.width, _imageRect.height), // local group coords
                    _previewMarkPanel.RenderTexture,
                    ScaleMode.StretchToFill,
                    true
                );
            }

            GUI.EndGroup();
            _zoomPanController.HandleMouseInput(_imageRect);
        }

        private bool NeedsRenderUpdate()
        {
            return _renderTexture == null || _zoomPanController.HasChanged;
        }

        private void UpdatePreviewTexture(int previewWidth, int previewHeight)
        {
            if (_renderTexture == null ||
                _renderTexture.width != previewWidth ||
                _renderTexture.height != previewHeight)
            {
                if (_renderTexture != null)
                    _renderTexture.Release();

                _renderTexture = new RenderTexture(previewWidth, previewHeight, 0)
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
                };
            }

            // Make active before clearing/blitting
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = _renderTexture;

            // Clear it
            GL.Clear(true, true, Color.clear);

            // Blit with shader
            Material blitMat = new Material(Shader.Find("Hidden/BlitZoomPan"));
            blitMat.SetTexture("_MainTex", _workplaceTexture);

            // Set zoom, pan, scale as before
            blitMat.SetFloat("_Zoom", _zoomPanController.Zoom);
            blitMat.SetVector("_PanOffset", new Vector2(_zoomPanController.PanOffset.x / _workplaceTexture.width,
                                                        _zoomPanController.PanOffset.y / _workplaceTexture.height));

            float panelAspect = (float)previewWidth / previewHeight;
            float textureAspect = (float)_workplaceTexture.width / _workplaceTexture.height;
            Vector2 scale = Vector2.one;
            if (textureAspect > panelAspect) scale.y = panelAspect / textureAspect;
            else scale.x = textureAspect / panelAspect;
            blitMat.SetVector("_Scale", scale);

            Graphics.Blit(_workplaceTexture, _renderTexture, blitMat);

            // Restore previous RenderTexture
            RenderTexture.active = prev;
        }
    }
}
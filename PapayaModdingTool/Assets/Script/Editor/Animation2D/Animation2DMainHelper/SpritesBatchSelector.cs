using System;
using System.Collections.Generic;
using System.Linq;
using PapayaModdingTool.Assets.Script.DataStruct.TextureData;
using UnityEngine;

using UEvent = UnityEngine.Event;

namespace PapayaModdingTool.Assets.Script.Editor.Animation2D.Animation2DMainHelper
{
    public class SpritesBatchSelector
    {
        public Func<List<SpriteButtonData>> GetDatas;

        public void ClickSpriteButton(SpriteButtonData data, bool isShiftHeld, bool isCtrlHeld)
        {
            if (!GetDatas().Contains(data))
            {
                Debug.LogWarning("Datas doesn't contain data. This shouldn't be possible. Abort.");
                return;
            }

            if (isShiftHeld)
            {
                // Debug.Log("Held shift");
                ShiftOperation(data);
            }
            else if (isCtrlHeld)
            {
                // Debug.Log("Held ctrl");
                CtrlOperation(data);
            }
            else // Not holding neither
            {
                // Debug.Log("Held nothing");
                bool wasSelected = data.isSelected;
                UnselectAllDatas();
                if (!wasSelected) data.isSelected = true;
            }
        }

        private void UnselectAllDatas()
        {
            foreach (SpriteButtonData data in GetDatas())
            {
                data.isSelected = false;
            }
        }

        private void ShiftOperation(SpriteButtonData data)
        {
            if (GetDatas() == null || GetDatas().Count == 0)
                return;

            int currIndex = GetDatas().IndexOf(data);
            List<bool> highlights = GetDatas().Select(x => x.isSelected).ToList();
            int closestTrueOffset = FindClosestTrueOffset(highlights, currIndex);

            if (closestTrueOffset == int.MaxValue)
            {
                GetDatas()[currIndex].isSelected = !GetDatas()[currIndex].isSelected;
                return;
            }

            int start = Mathf.Min(currIndex, currIndex + closestTrueOffset);
            int end = Mathf.Max(currIndex, currIndex + closestTrueOffset);

            for (int i = start; i <= end; i++)
            {
                if (i < 0 || i >= GetDatas().Count)
                    return;
                GetDatas()[i].isSelected = true;
            }
        }

        private static int FindClosestTrueOffset(List<bool> boolList, int targetIndex)
        {
            int n = boolList.Count;
            if (n == 0 || targetIndex < 0 || targetIndex >= n)
                return int.MaxValue;  // or throw an exception

            int offset = 0;
            while (offset < n)
            {
                int left = targetIndex - offset;
                int right = targetIndex + offset;

                if (left >= 0 && boolList[left]) return left - targetIndex;   // negative or zero
                if (right < n && boolList[right]) return right - targetIndex; // positive or zero

                offset++;
            }

            return int.MaxValue;  // No true found
        }

        private void CtrlOperation(SpriteButtonData data)
        {
            data.isSelected = !data.isSelected;
        }

        public static bool IsShiftHeld()
        {
            UEvent e = UEvent.current;
            return e != null && (e.shift);
        }

        public static bool IsCtrlHeld()
        {
            UEvent e = UEvent.current;
            return e != null && (e.control || e.command); // "command" = Ctrl on Windows, Cmd on Mac
        }

        public int GetNumOfSelected()
        {
            if (GetDatas == null || GetDatas() == null)
                return 0;
            return GetDatas().Count(x => x.isSelected);
        }
    }
}
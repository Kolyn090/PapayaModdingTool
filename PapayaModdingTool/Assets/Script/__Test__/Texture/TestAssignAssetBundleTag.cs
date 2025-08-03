using System;
using System.IO;
using PapayaModdingTool.Assets.Script.__Test__.TestUtil;
using PapayaModdingTool.Assets.Script.Writer.ProjectUtil;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.__Test__.Texture
{
    public class TestAssignAssetBundleTag : MonoBehaviour, ITestable
    {
        public void Test(Action onComplete)
        {
            TestHelper.TestOnCleanLabDesk(() =>
            {
                string texturePath = Path.Combine(PredefinedTestPaths.UnityLabDeskPath, "Texture");
                string targetAssetPath = Path.Combine(texturePath, "alchemist_0.png");

                string BUNDLE_NAME = "alchemist";
                AssetBundleTagEditor.SetAssetBundleTag(targetAssetPath, BUNDLE_NAME);
                // ! Remember to delete the asset from lab desk after you are done.
                // ! Leaving it there can impact performance on AssetBundles building process.
            },
            PredefinedTestPaths.UnityDoNotOverridePath,
            PredefinedTestPaths.UnityLabDeskPath,
            true);
        }
    }
}
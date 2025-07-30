using System.Runtime.InteropServices;
using UnityEngine;

namespace PapayaModdingTool.Assets.Script.__Test__.NotImportant
{
    public class TestMinimal : MonoBehaviour
    {
        [DllImport("minimal", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint TestFunc(int a);

        void Start()
        {
            Debug.Log("2 + 1 = " + TestFunc(2));
        }
    }
}
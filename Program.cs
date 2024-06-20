using System;
using System.Runtime.InteropServices;
//using UnityEngine;

public class UnmanagedFunctionPointers /*: MonoBehaviour*/ {
    [DllImport("kernel32.dll")]
    static extern IntPtr LoadLibrary(string lpLibFileName);

    [DllImport("kernel32.dll")]
    static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

    // You must enable "Allow 'unsafe' code" in the Player Settings
    unsafe void Start() {
// #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    // This example is only valid on Windows
    
    // Get a pointer to an unmanaged function
    IntPtr kernel32 = LoadLibrary("kernel32.dll");
    IntPtr getCurrentThreadId = GetProcAddress(kernel32, "GetCurrentThreadId");

    // The unmanaged calling convention
    delegate* unmanaged[Stdcall]<UInt32> getCurrentThreadIdUnmanagedStdcall = (delegate* unmanaged[Stdcall]<UInt32>)getCurrentThreadId;
    // Debug.Log(getCurrentThreadIdUnmanagedStdcall());
// #endif
    }
}

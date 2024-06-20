using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

/*
 * reference:
https://github.com/forrestthewoods/fts_unity_native_plugin_reloader
*/

//using UnityEngine;
// #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
// #endif

public unsafe static class Dll /*: MonoBehaviour*/ {

    /*************************************************************************/
    // core funcs

    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern IntPtr LoadLibrary(string lpLibFileName);

    [DllImport("kernel32", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool FreeLibrary(IntPtr hModule);

    [DllImport("kernel32")]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

    /*************************************************************************/
    // mapped funcs

    public static IntPtr hDll;
    public static delegate* unmanaged[Stdcall]<int> Init;

    /*************************************************************************/
    // utils

    public static void LoadDll() {
        Debug.Assert(hDll == IntPtr.Zero);

        hDll = LoadLibrary(
#if DEBUG
        "../../../../x64/Debug/dll.dll"
#else
        "../../../../x64/Release/dll.dll"
#endif
        );

        Init = (delegate* unmanaged[Stdcall]<int>)GetProcAddress(hDll, "Init");
        // ...
    }

    public static void UnloadDll() {
        Debug.Assert(hDll != IntPtr.Zero);
        FreeLibrary(hDll);
        hDll = IntPtr.Zero;
        Init = null;
        // ...
    }
}

/************************************************************************************************************************/
/************************************************************************************************************************/

public unsafe static class MainClass {
    public static void Main() {
        Dll.LoadDll();
        Console.WriteLine(Dll.Init());
        Dll.UnloadDll();
    }
}

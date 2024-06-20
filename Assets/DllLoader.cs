#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using UnityEngine;
#else
using System.Diagnostics;
#endif
using System.Runtime.InteropServices;

/*
 * reference:
https://github.com/forrestthewoods/fts_unity_native_plugin_reloader


public static class Dll
{
    public delegate void __void_();
    public delegate int __int_();
    public delegate int __int_ptr_int(System.IntPtr buf, int len);
    public delegate int __int_float(float v);
    // ...

    public static __int_ New;

    public static __int_ptr_int InitBuf;
    public static __int_float InitFrameDelay;

    public static __int_ Begin;
    public static __int_float Update;
    public static __int_ Draw;
    public static __int_ End;

    public static __int_ Delete;
    // ...
}

......

// put following code into ENTRY class

void Awake()
{
    DllLoader.Load(typeof(Dll));
    Dll.New();
    Debug.Log("dll loaded");
}

void OnDestroy() {
    Dll.Delete();
    DllLoader.Unload();
    Debug.Log("dll unloaded");
}

*/

/************************************************************************************************************************/
/************************************************************************************************************************/

public static class DllLoader
{

    public static System.IntPtr dllHandle;

    /*************************************************************************/
    // core funcs

    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern System.IntPtr LoadLibrary(string lpLibFileName);

    [DllImport("kernel32", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool FreeLibrary(System.IntPtr hModule);

    [DllImport("kernel32")]
    public static extern System.IntPtr GetProcAddress(System.IntPtr hModule, string lpProcName);

    [DllImport("kernel32")]
    static private extern uint GetLastError();

    /*************************************************************************/

    public static void Load(System.Type t)
    {
        Debug.Assert(dllHandle == System.IntPtr.Zero);

        dllHandle = LoadLibrary(
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        Application.dataPath + "/Plugins/"
#else
#if DEBUG
        "../../../../x64/Debug/"
#else
        "../../../../x64/Release/"
#endif
#endif
         + "dll.dll");

        var fields = t.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

        foreach (var field in fields)
        {
            var fnPtr = GetProcAddress(dllHandle, field.Name);
            if (fnPtr != System.IntPtr.Zero)
            {
                field.SetValue(null, Marshal.GetDelegateForFunctionPointer(fnPtr, field.FieldType));
            }
        }
    }

    public static void Unload()
    {
        Debug.Assert(dllHandle != System.IntPtr.Zero);
        FreeLibrary(dllHandle);
        dllHandle = System.IntPtr.Zero;
        // ...
    }
}

using System.Diagnostics;
using System.Runtime.InteropServices;

/*
 * reference:
https://github.com/forrestthewoods/fts_unity_native_plugin_reloader
*/

//using UnityEngine;
// #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
// #endif
//public static delegate* unmanaged[Stdcall]<int> Init = (delegate* unmanaged[Stdcall]<int>)GetProcAddress(hDll, "Init");

public static class Dll
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

    [DllImport("kernel32.dll")]
    static private extern uint GetLastError();

    /*************************************************************************/

    public static void LoadDll()
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

        var t = typeof(Dll);
        var fields = t.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

        foreach (var field in fields)
        {
            if (field.Name == "dllHandle") continue;

            var fnPtr = GetProcAddress(dllHandle, field.Name);
            if (fnPtr != System.IntPtr.Zero)
            {
                field.SetValue(null, Marshal.GetDelegateForFunctionPointer(fnPtr, field.FieldType));
            }
        }
    }

    public static void UnloadDll()
    {
        Debug.Assert(dllHandle != System.IntPtr.Zero);
        FreeLibrary(dllHandle);
        dllHandle = System.IntPtr.Zero;
        // ...
    }

    /*************************************************************************/
    // more dll funcs here ( auto map )

    public static Init_ Init;
    public delegate int Init_();

    // ...
}

/************************************************************************************************************************/
/************************************************************************************************************************/

/*
 * put these code into ENTRY class

void Awake()
{
    Dll.LoadDll();
}

void OnDestroy() {
    Dll.UnloadDll();
}

*/


/************************************************************************************************************************/
/************************************************************************************************************************/

// invoke tests
public static class MainClass {
    public static void Main() {
        Dll.LoadDll();

        Console.WriteLine(Dll.Init());

        Dll.UnloadDll();
    }
}

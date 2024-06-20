using System.Diagnostics;
using System.Runtime.InteropServices;

public static class Dll
{
    public delegate void __void_();
    public delegate int __int_();
    public delegate int __int_ptr_int(IntPtr buf, int len);
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


[StructLayout(LayoutKind.Sequential)]
public struct XY
{
    public float x, y;
    public override string ToString()
    {
        return "x = " + x + ", y = " + y;
    }
}

public class Scene : IDisposable
{
    public const float fps = 60;
    public const float frameDelay = 1.0f / fps;
    public int cap;
    public XY[] buf;
    public GCHandle bufHandle;
    public IntPtr bufIntPtr;

    public Scene()
    {
        cap = 100000;
        buf = new XY[cap];
        bufHandle = GCHandle.Alloc(buf, GCHandleType.Pinned);
        bufIntPtr = bufHandle.AddrOfPinnedObject();
    }

    public void Run()
    {
        DllLoader.Load(typeof(Dll));

        int r = Dll.New();
        Debug.Assert(r == 0);

        r = Dll.InitBuf(bufIntPtr, cap);
        Debug.Assert(r == 0);

        r = Dll.InitFrameDelay(frameDelay);
        Debug.Assert(r == 0);

        r = Dll.Begin();
        Debug.Assert(r == 0);

        for (int i = 0; i < 10; i++)
        {
            r = Dll.Update(frameDelay);
            Debug.Assert(r == 0);

            r = Dll.Draw();
            Debug.Assert(r > 0);

            Console.WriteLine("draw return " + r);
            for (int j = 0; j < r; j++)
            {
                Console.WriteLine(buf[j]);
            }
        }

        r = Dll.End();
        Debug.Assert(r == 0);

        r = Dll.Delete();
        Debug.Assert(r == 0);

        DllLoader.Unload();
    }

    public void Dispose()
    {
        bufIntPtr = default;
        bufHandle.Free();
        buf = null;
        cap = 0;
    }
}

public static class MainClass
{
    public static void Main()
    {
        new Scene().Run();
    }
}

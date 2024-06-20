using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public GameObject prefab;

    private const float fps = 60;
    private const float frameDelay = 1.0f / fps;
    private int cap;
    private XY[] buf;
    private GCHandle bufHandle;
    private IntPtr bufIntPtr;

    private List<GameObject> gos = new List<GameObject>();

    void Start()
    {
        Debug.Assert(prefab != null);
        cap = 100000;
        buf = new XY[cap];
        bufHandle = GCHandle.Alloc(buf, GCHandleType.Pinned);
        bufIntPtr = bufHandle.AddrOfPinnedObject();

        DllLoader.Load(typeof(Dll));

        int r = Dll.New();
        Debug.Assert(r == 0);

        r = Dll.InitBuf(bufIntPtr, cap);
        Debug.Assert(r == 0);

        r = Dll.InitFrameDelay(frameDelay);
        Debug.Assert(r == 0);

        r = Dll.Begin();
        Debug.Assert(r == 0);
    }


    void Update()
    {
        int r = Dll.Update(frameDelay);
        Debug.Assert(r == 0);

        r = Dll.Draw();
        Debug.Assert(r > 0);

        if (gos.Count < r)
        {
            for (int i = 0; i < r - gos.Count; ++i)
            {
                gos.Add(Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity));
            }
        }
        else if (gos.Count > r)
        {
            for (int i = gos.Count - 1; i >= r; --i)
            {
                Destroy(gos[i]);
            }
            gos.RemoveRange(r, gos.Count - r);
        }

        for (int i = 0; i < r; i++)
        {
            gos[i].GetComponent<Transform>().position = new Vector3(buf[i].x, buf[i].y, 0);
        }
    }

    void OnDestroy()
    {
        int r = Dll.End();
        Debug.Assert(r == 0);

        bufIntPtr = default;
        bufHandle.Free();
        buf = null;
        cap = 0;

        Dll.Delete();
        DllLoader.Unload();
        Debug.Log("dll unloaded");
    }

    void Awake()
    {
        DllLoader.Load(typeof(Dll));
        Dll.New();
        Debug.Log("dll loaded");
    }
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

//private int count;
//StartCoroutine(CreatePrefabs());

//private IEnumerator CreatePrefabs()
//{
//    while (true)
//    {
//        var x = Random.Range(-10f, 10f);
//        var y = Random.Range(-10f, 10f);
//        Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity);
//        yield return new WaitForSeconds(0.01f);
//    }
//}

//for (int i = 0; i < 20; i++)
//{
//    var x = Random.Range(-9f, 9f);
//    var y = Random.Range(-6f, 6f);
//    Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity);
//    ++count;
//}
//Debug.Log(count);
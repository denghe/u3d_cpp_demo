using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public GameObject prefab;
    //public Texture2D tex;

    private const float fps = 60;
    private const float frameDelay = 1.0f / fps;
    private int cap;
    private XY[] buf;
    private GCHandle bufHandle;
    private System.IntPtr bufIntPtr;

    private List<GameObject> gos = new List<GameObject>();

    void Start()
    {
        Debug.Assert(prefab != null);
        cap = 50000;
        buf = new XY[cap];
        bufHandle = GCHandle.Alloc(buf, GCHandleType.Pinned);
        bufIntPtr = bufHandle.AddrOfPinnedObject();

        var r = Dll.InitBuf(bufIntPtr, cap);
        Debug.Assert(r == 0, r);

        r = Dll.InitFrameDelay(frameDelay);
        Debug.Assert(r == 0, r);

        r = Dll.Begin();
        Debug.Assert(r == 0, r);

        //for (int i = 0; i < 100000; i++)
        //{
        //    //var go = new GameObject();
        //    //go.AddComponent<SpriteRenderer>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        //    //gos.Add(go);

        //    gos.Add(Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity));
        //}
    }


    void Update()
    {
        int r = Dll.Update(frameDelay);
        Debug.Assert(r == 0, r);

        r = Dll.Draw();
        Debug.Assert(r > 0, r);

        var d = r - gos.Count;
        if (d > 0)
        {
            //gos.Capacity = d;
            for (int i = 0; i < d; ++i)
            {
                gos.Add(Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity));
            }
            Debug.Assert(r == gos.Count, $"r = {r} gos.Count = {gos.Count}");
        }
        else if (d < 0)
        {
            for (int i = gos.Count - 1; i >= r; --i)
            {
                Destroy(gos[i]);
            }
            gos.RemoveRange(r, -d);
            Debug.Assert(r == gos.Count, $"r = {r} gos.Count = {gos.Count}");
        }

        for (int i = 0; i < r; i++)
        {
            gos[i].GetComponent<Transform>().position = new Vector3(buf[i].x, buf[i].y, 0);
        }

        //for (int i = 0; i < gos.Count; i++)
        //{
        //    var x = Random.Range(-9f, 9f);
        //    var y = Random.Range(-6f, 6f);
        //    gos[i].GetComponent<Transform>().position = new Vector3(x, y, 0);
        //}
    }

    void OnDestroy()
    {
        int r = Dll.End();
        Debug.Assert(r == 0, r);

        bufIntPtr = default;
        bufHandle.Free();
        buf = null;
        cap = 0;

        r = Dll.Delete();
        Debug.Assert(r == 0, r);
        DllLoader.Unload();
        Debug.Log("dll unloaded");
    }

    void Awake()
    {
        DllLoader.Load(typeof(Dll), "dll.dll");
        var r = Dll.New();
        Debug.Assert(r == 0);
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
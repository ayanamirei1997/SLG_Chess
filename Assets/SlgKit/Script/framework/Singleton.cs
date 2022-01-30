using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    static T instance;


    public static T GteInstance(string Soucese)
    {


        if (instance == null)
        {
            UnityEngine.Object[] objs = FindObjectsOfType(typeof(T));
            if (objs.Length >= 2)
            {
              
                Debug.LogError("An instance of " + typeof(T) + "单例模式组件在场景中找到了2个");

                return null;
            }

            instance = (T)FindObjectOfType(typeof(T));

            if (instance == null)
            {
                Debug.LogError(Soucese + ":调用单例模式实类 " + typeof(T) + " 需要存在于场景，但是没有找到其组件");
            }
        }
        return instance;
    }


    public static T Instance
    {
        get
        {
            return instance;
        }
    }


    static bool mIni = false;
    protected virtual void Awake()
    {
        if (mIni && Application.isPlaying)
        {

            Destroy(gameObject);


            return;
        }


        mIni = true;
        instance = this as T;




        UnityEngine.Object[] objs = FindObjectsOfType(typeof(T));
        if (objs.Length >= 2)
        {
#if UNITY_EDITOR




            //var lis = objs.Select(x=>((Component)x).GetFullName().ToString()).ToArray();
            //string hasgob = "";
            //for (int i = 0; i < lis.Length; i++)
            //{
            //    hasgob += lis[i]+"/n";
            //}
            UnityEditor.Selection.objects = objs;

            UnityEditor.EditorUtility.DisplayDialog("错误", "单例模式组件在场景中找到了" + objs.Length + "个" + typeof(T), "OK");
            UnityEditor.EditorApplication.isPlaying = false;

#endif
        }


    }
    
}

public class Singleton<T> where T : class, new()
{
    private static T _instance;
   
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                    if (_instance == null)
                    {
                        _instance = new T();
                    }
                
            }
            return _instance;
        }
    }

  

}
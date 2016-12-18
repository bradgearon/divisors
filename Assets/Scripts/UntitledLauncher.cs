using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine.Advertisements;

public class UntitledLauncher {

    private AndroidJavaClass activityClass;
    private AndroidJavaObject activity;
    private AndroidJavaClass untitledNative;
    private static UntitledLauncher instance;

    public static void Init()
    {
        if (instance != null)
        {
            return;
        }

        instance = new UntitledLauncher();
        Debug.Log("untitled launcher is not inited");
        instance.StartPackage();
    }

    public static void Show()
    {
        UntitledLauncher.instance.BringToFront();
    }


    private void BringToFront()
    {
        untitledNative.CallStatic("bringToFront", activity);
    }

    private void StartPackage()
    {
        Debug.Log("calling start package");
        activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        activity = activityClass.GetStatic<AndroidJavaObject>("currentActivity");

        untitledNative = new AndroidJavaClass("com.wds.untitled.UntitledNative");
        untitledNative.CallStatic("init", activity);
    }

}

using System;
using UnityEngine;
using System.Collections;

public class AppNextTracking : MonoBehaviour
{
    public void Track()
    {
#if UNITY_ANDROID
        using (AndroidJavaClass appNextTrack = new AndroidJavaClass("com.appnext.appnextsdk.AppnextTrack"),
                    player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                appNextTrack.CallStatic("track", activity);
            }
        }
#endif
    }
}

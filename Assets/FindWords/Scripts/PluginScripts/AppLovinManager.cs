using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppLovinManager : MonoBehaviour
{
    private string applovinToken = "SkwvnkQqr_6nRgTxzKfWoRVNNeiS5rVRZuxLLeCfOA21seXZ_7ZZ2Um8ynwrGqfJpfl-Fid2fExGmb9lT1QDHp";
    
    private void Awake()
    {
        MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
        {
            
        };
        MaxSdk.SetSdkKey(applovinToken);
        MaxSdk.SetUserId(SystemInfo.deviceUniqueIdentifier);
        MaxSdk.InitializeSdk();
    }
}

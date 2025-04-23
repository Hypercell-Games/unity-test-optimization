using System;
using Services.Ads;
using Services.Data;
using Services.IAP;
using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable UnassignedField.Global
// ReSharper disable CheckNamespace

public static class HyperKit
{
    static HyperKit()
    {
        IAP = new IAPService();
        Data = new DataService();
        Ads = new AdsService();
    }

    public static IAPService IAP { get; }

    public static DataService Data { get; }

    public static AdsService Ads { get; }

    public static bool Initialized => Time.realtimeSinceStartup > 3f;

    public interface IExtension
    {
    }
}

[Serializable]
public class HyperKitConfig
{
    public AdConfig adConfig;
    public IAPProduct[] iapProducts;
}

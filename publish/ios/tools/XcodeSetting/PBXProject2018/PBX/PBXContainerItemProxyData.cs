﻿// Decompiled with JetBrains decompiler
// Type: UnityEditor.iOS.Xcode.PBX.PBXContainerItemProxyData
// Assembly: UnityEditor.iOS.Extensions.Xcode, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 36E9AB58-F6A7-4B4D-BAB5-BE8059694DCD
// Assembly location: D:\workspace\pk\trunk\usdk\publish\ios\tools\UnityEditor.iOS.Extensions.Xcode.dll

using System.Collections.Generic;

namespace UnityEditor.iOS.Xcode.PBX
{
  internal class PBXContainerItemProxyData : PBXObjectData
  {
    private static PropertyCommentChecker checkerData;

    internal override PropertyCommentChecker checker
    {
      get
      {
        return PBXContainerItemProxyData.checkerData;
      }
    }

    static PBXContainerItemProxyData()
    {
      string[] strArray = new string[1];
      int index = 0;
      string str = "containerPortal/*";
      strArray[index] = str;
      PBXContainerItemProxyData.checkerData = new PropertyCommentChecker((IEnumerable<string>) strArray);
    }

    public static PBXContainerItemProxyData Create(string containerRef, string proxyType, string remoteGlobalGUID, string remoteInfo)
    {
      PBXContainerItemProxyData containerItemProxyData = new PBXContainerItemProxyData();
      containerItemProxyData.guid = PBXGUID.Generate();
      containerItemProxyData.SetPropertyString("isa", "PBXContainerItemProxy");
      containerItemProxyData.SetPropertyString("containerPortal", containerRef);
      containerItemProxyData.SetPropertyString("proxyType", proxyType);
      containerItemProxyData.SetPropertyString("remoteGlobalIDString", remoteGlobalGUID);
      containerItemProxyData.SetPropertyString("remoteInfo", remoteInfo);
      return containerItemProxyData;
    }
  }
}

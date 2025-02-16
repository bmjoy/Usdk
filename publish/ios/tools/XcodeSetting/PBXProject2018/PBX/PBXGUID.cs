﻿// Decompiled with JetBrains decompiler
// Type: UnityEditor.iOS.Xcode.PBX.PBXGUID
// Assembly: UnityEditor.iOS.Extensions.Xcode, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 36E9AB58-F6A7-4B4D-BAB5-BE8059694DCD
// Assembly location: D:\workspace\pk\trunk\usdk\publish\ios\tools\UnityEditor.iOS.Extensions.Xcode.dll

using System;

namespace UnityEditor.iOS.Xcode.PBX
{
  internal class PBXGUID
  {
        private static PBXGUID.GuidGenerator guidGenerator = new PBXGUID.GuidGenerator(PBXGUID.DefaultGuidGenerator);


    internal static string DefaultGuidGenerator()
    {
      return Guid.NewGuid().ToString("N").Substring(8).ToUpper();
    }

    internal static void SetGuidGenerator(PBXGUID.GuidGenerator generator)
    {
      PBXGUID.guidGenerator = generator;
    }

    public static string Generate()
    {
      return PBXGUID.guidGenerator();
    }

    internal delegate string GuidGenerator();
  }
}

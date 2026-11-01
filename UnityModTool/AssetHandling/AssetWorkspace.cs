using System;
using AssetsTools.NET.Extra;

namespace UnityModTool.AssetHandling;

public sealed class AssetWorkspace : IDisposable
{
    public AssetsTools.NET.Extra.AssetsManager Manager { get; } = new();

    public AssetWorkspace()
    {
        Manager.LoadClassPackage("classdata.tpk");
    }

    public void Dispose()
    {
        Manager.UnloadAllAssetsFiles(true);
    }
}

using System.IO;
using AssetsTools.NET;
using AssetsTools.NET.Extra;

namespace UnityModTool.ViewModels;

public sealed class ComponentViewModel(AssetFileInfo assetFileInfo) : ViewModelBase
{
    public long PathID => assetFileInfo.PathId;
    public string Name => ((AssetClassID)assetFileInfo.TypeId).ToString();

    public AssetFileInfo AssetFileInfo { get; } = assetFileInfo;
}

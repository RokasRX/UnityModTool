using System.IO;
using AssetsTools.NET;
using AssetsTools.NET.Extra;

namespace UnityModTool.ViewModels;

public sealed class AssetsFileViewModel(string filePath, AssetsFileInstance assetsFileInstance) : ViewModelBase
{
    public string FilePath { get; } = filePath;

    public string Name => Path.GetFileName(FilePath);

    public AssetsFileInstance AssetsFileInstance { get; } = assetsFileInstance;

    public AssetsFile assetsFile => AssetsFileInstance.file;
}

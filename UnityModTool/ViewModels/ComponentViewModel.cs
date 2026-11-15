using System;
using AssetsTools.NET;
using AssetsTools.NET.Extra;

namespace UnityModTool.ViewModels;

public sealed class ComponentViewModel(AssetFileInfo assetFileInfo) : ViewModelBase
{
    private AssetTypeValueField? data;

    public long PathID => AssetFileInfo.PathId;
    public string ComponentType => ((AssetClassID)AssetFileInfo.TypeId).ToString();
    public string Name => $"{ComponentType}{(IsModified ? " *" : string.Empty)}";

    public AssetFileInfo AssetFileInfo { get; } = assetFileInfo;
    public bool IsModified { get; private set; }
    public event EventHandler? DataReplaced;

    public void SetData(AssetTypeValueField componentData)
    {
        data = componentData;
    }

    public byte[] GetRawBytes(AssetsManager manager, AssetsFileInstance assetsFileInstance)
    {
        data ??= manager.GetBaseField(assetsFileInstance, AssetFileInfo);
        return data.WriteToByteArray(assetsFileInstance.file.Header.Endianness);
    }

    public void ReplaceRawData(byte[] bytes)
    {
        AssetFileInfo.SetNewData(bytes);
        data = null;
        SetModified();
        DataReplaced?.Invoke(this, EventArgs.Empty);
    }

    public void MarkModified()
    {
        if (data is not null)
            AssetFileInfo.SetNewData(data);

        SetModified();
    }

    private void SetModified()
    {
        if (IsModified)
            return;

        IsModified = true;
        OnPropertyChanged(nameof(IsModified));
        OnPropertyChanged(nameof(Name));
    }

    public void MarkSaved()
    {
        if (!IsModified)
            return;

        IsModified = false;
        OnPropertyChanged(nameof(IsModified));
        OnPropertyChanged(nameof(Name));
    }
}

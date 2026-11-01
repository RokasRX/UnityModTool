using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using AssetsTools.NET;
using CommunityToolkit.Mvvm.Input;

namespace UnityModTool.ViewModels;

public sealed class PropertyViewModel : ViewModelBase
{
    private const int BytePreviewLength = 128;
    private const int BytePageSize = 128;

    private readonly AssetTypeValueField? _field;
    private readonly Action _save;
    private readonly PropertyViewModel? _byteArrayOwner;
    private readonly Action? _remove;
    private decimal? _numberValue;
    private string? _textValue;
    private string? _byteHexValue;
    private bool _booleanValue;
    private string? _validationMessage;
    private int _arrayIndex;
    private int _loadedByteCount;
    private bool _batchingChildren;
    private PropertyViewModel? _vectorParent;

    private string BaseName { get; }
    private bool IsArrayItem { get; }
    public bool IsArrayAction { get; }
    public bool IsLoadMoreAction { get; }
    public bool IsAction => IsArrayAction || IsLoadMoreAction;
    private int ByteIndex { get; set; }

    public string Name => IsArrayItem
        ? _byteArrayOwner is null ? $"[{_arrayIndex}]" : $"[{ByteIndex}]"
        : IsVectorContainer || IsArray ? $"{BaseName} ({ArrayCount})" : BaseName;

    public string TypeName { get; }
    public ObservableCollection<PropertyViewModel> Children { get; } = [];
    public ObservableCollection<PropertyViewModel> TreeChildren { get; } = [];

    public bool IsText => ValueType is AssetValueType.String or AssetValueType.Float or AssetValueType.Double;
    public bool IsBoolean => ValueType == AssetValueType.Bool;
    public bool IsNumber => _byteArrayOwner is null && IsNumeric(ValueType);
    public bool IsEditable => IsText || IsBoolean || IsNumber;
    public bool IsReadOnly => (!IsEditable && !IsByteArrayPreview) || IsByteArray;
    public bool IsPropertyRow => !IsAction;
    public bool IsArrayElement => IsArrayItem;
    public bool IsByteArrayElement => _byteArrayOwner is not null;
    public bool IsByteArrayPreview { get; }
    public bool IsArray { get; }
    public bool IsByteArray => !IsByteArrayPreview
        && _field?.TemplateField.ValueType == AssetValueType.ByteArray;
    private bool IsVectorContainer { get; }
    public int ArrayCount => _byteArrayOwner is not null
        ? 0
        : IsVectorContainer ? Children[0].ArrayCount
        : IsByteArray ? _field!.AsByteArray.Length : Children.Count;
    public decimal Minimum { get; }
    public decimal Maximum { get; }
    public string? DisplayValue => IsByteArrayPreview
        ? GetByteArrayPreview()
        : IsByteArray || IsEditable ? null : TypeName;
    public string ActionText => IsArrayAction ? "Add element" : IsLoadMoreAction ? "Load next bytes" : "Remove";
    public ICommand? ActionCommand { get; }
    public string? ValidationMessage
    {
        get => _validationMessage;
        private set => SetProperty(ref _validationMessage, value);
    }

    public bool CanImportByteArray => IsByteArrayPreview;

    public void ImportByteArray(byte[] bytes)
    {
        if (!IsByteArrayPreview)
            throw new InvalidOperationException("Only a TypelessData property preview can import file bytes.");

        _byteArrayOwner!._field!.AsByteArray = bytes;
        _byteArrayOwner._save();
        _byteArrayOwner.OnPropertyChanged(nameof(Name));
        _byteArrayOwner.OnPropertyChanged(nameof(ArrayCount));
        OnPropertyChanged(nameof(DisplayValue));
        ValidationMessage = null;
    }

    public void SetImportError(string message)
    {
        if (IsByteArrayPreview)
            ValidationMessage = message;
    }

    private AssetValueType ValueType => _byteArrayOwner is not null
        ? AssetValueType.UInt8
        : _field!.TemplateField.ValueType;

    public string? TextValue
    {
        get => _textValue;
        set
        {
            if (!SetProperty(ref _textValue, value))
                return;

            var input = value ?? string.Empty;
            switch (ValueType)
            {
                case AssetValueType.String:
                    _field!.Value.AsString = input;
                    ValidationMessage = null;
                    _save();
                    break;
                case AssetValueType.Float when float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out var floatValue):
                    _field!.Value.AsFloat = floatValue;
                    ValidationMessage = null;
                    _save();
                    break;
                case AssetValueType.Double when double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleValue):
                    _field!.Value.AsDouble = doubleValue;
                    ValidationMessage = null;
                    _save();
                    break;
                case AssetValueType.Float:
                case AssetValueType.Double:
                    ValidationMessage = "Enter a valid number.";
                    break;
            }
        }
    }

    public string? ByteHexValue
    {
        get => _byteHexValue;
        set
        {
            if (!SetProperty(ref _byteHexValue, value))
                return;

            var input = value ?? string.Empty;
            if (input.Length is > 0 and <= 2
                && input.All(Uri.IsHexDigit)
                && byte.TryParse(input, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var byteValue))
            {
                _byteArrayOwner!.SetByte(ByteIndex, byteValue);
                ValidationMessage = null;
            }
            else
            {
                ValidationMessage = "Enter one or two hexadecimal digits.";
            }
        }
    }

    public bool BooleanValue
    {
        get => _booleanValue;
        set
        {
            if (SetProperty(ref _booleanValue, value) && IsBoolean)
            {
                _field!.Value.AsBool = value;
                _save();
            }
        }
    }

    public decimal? NumberValue
    {
        get => _numberValue;
        set
        {
            if (value is null || !SetProperty(ref _numberValue, value) || !IsNumber)
                return;

            if (value < Minimum || value > Maximum || value != decimal.Truncate(value.Value))
            {
                ValidationMessage = "Enter a whole number in the supported range.";
                _numberValue = GetNumericValue();
                OnPropertyChanged(nameof(NumberValue));
                return;
            }

            if (_byteArrayOwner is not null)
            {
                _byteArrayOwner.SetByte(ByteIndex, decimal.ToByte(value.Value));
            }
            else
            {
                SetNumericValue(value.Value);
                _save();
            }

            ValidationMessage = null;
        }
    }

    public PropertyViewModel(string name, AssetTypeValueField field, Action save)
        : this(name, field, save, false, null)
    {
    }

    private PropertyViewModel(string name, AssetTypeValueField field, Action save, bool isArrayItem, Action? remove)
    {
        BaseName = name;
        TypeName = field.TemplateField.Type;
        _field = field;
        _save = save;
        IsArrayItem = isArrayItem;
        _remove = remove;
        IsArray = field.TemplateField.IsArray;
        IsVectorContainer = field.TemplateField.Type == "vector"
            && field.Children.Count == 1
            && field.Children[0].TemplateField.IsArray;
        if (isArrayItem)
            ActionCommand = new RelayCommand(() => _remove?.Invoke());

        if (IsText)
        {
            _textValue = field.TemplateField.ValueType switch
            {
                AssetValueType.String => field.Value.AsString,
                AssetValueType.Float => field.Value.AsFloat.ToString("R", CultureInfo.InvariantCulture),
                AssetValueType.Double => field.Value.AsDouble.ToString("R", CultureInfo.InvariantCulture),
                _ => null
            };
        }
        else if (IsBoolean)
        {
            _booleanValue = field.Value.AsBool;
        }
        else if (IsNumeric(field.TemplateField.ValueType))
        {
            _numberValue = GetNumericValue();
            (Minimum, Maximum) = GetNumericBounds(field.TemplateField.ValueType);
        }

        if (!IsByteArray)
        {
            for (var index = 0; index < field.Children.Count; index++)
                Children.Add(CreateChild(field.Children[index], index));
        }

        Children.CollectionChanged += OnChildrenChanged;
        RebuildTreeChildren();

        if (IsVectorContainer)
        {
            Children[0]._vectorParent = this;
        }
    }

    private PropertyViewModel(PropertyViewModel owner, int index)
    {
        BaseName = string.Empty;
        TypeName = "UInt8";
        _save = owner._save;
        _byteArrayOwner = owner;
        ByteIndex = index;
        IsArrayItem = true;
        ActionCommand = new RelayCommand(() => _byteArrayOwner.RemoveArrayElement(ByteIndex));
        Minimum = byte.MinValue;
        Maximum = byte.MaxValue;
        _numberValue = owner._field!.AsByteArray[index];
        _byteHexValue = ((byte)_numberValue.Value).ToString("X2", CultureInfo.InvariantCulture);
    }

    private PropertyViewModel(PropertyViewModel owner, bool isLoadMore = false)
    {
        BaseName = string.Empty;
        TypeName = string.Empty;
        _save = owner._save;
        IsArrayAction = !isLoadMore;
        IsLoadMoreAction = isLoadMore;
        ActionCommand = new RelayCommand(isLoadMore ? owner.LoadMoreBytes : owner.AddArrayElement);
    }

    private PropertyViewModel(PropertyViewModel owner, ByteArrayPreview _)
    {
        BaseName = "Hex";
        TypeName = "Hexadecimal";
        _field = owner._field;
        _save = owner._save;
        _byteArrayOwner = owner;
        IsByteArrayPreview = true;
    }

    private PropertyViewModel CreateChild(AssetTypeValueField childField, int index)
    {
        if (!IsArray)
            return new PropertyViewModel(childField.TemplateField.Name, childField, _save);

        return new PropertyViewModel($"[{index}]", childField, _save, true, () => RemoveArrayElement(childField));
    }

    private PropertyViewModel CreateByteElement(int index)
    {
        return new PropertyViewModel(this, index);
    }

    private void OnChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_batchingChildren)
            return;

        if (!IsByteArray)
        {
            for (var index = 0; index < Children.Count; index++)
            {
                Children[index]._arrayIndex = index;
                Children[index].OnPropertyChanged(nameof(Name));
            }
        }

        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(ArrayCount));
        RebuildTreeChildren();
        _vectorParent?.OnVectorChildrenChanged();
    }

    private void OnVectorChildrenChanged()
    {
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(ArrayCount));
        RebuildTreeChildren();
    }

    private void RebuildTreeChildren()
    {
        TreeChildren.Clear();

        if (IsByteArray)
        {
            TreeChildren.Add(new PropertyViewModel(this, ByteArrayPreview.Instance));
            return;
        }

        if (IsVectorContainer)
        {
            var array = Children[0];
            foreach (var item in array.Children)
                TreeChildren.Add(item);
            array.AddArrayActions(TreeChildren);
            return;
        }

        foreach (var child in Children)
            TreeChildren.Add(child);

        AddArrayActions(TreeChildren);
    }

    private string GetByteArrayPreview()
    {
        var bytes = _field!.AsByteArray;
        var length = Math.Min(bytes.Length, BytePreviewLength);
        var preview = string.Join(" ", bytes.Take(length).Select(value => value.ToString("X2", CultureInfo.InvariantCulture)));
        return bytes.Length > length
            ? $"{preview} ... ({bytes.Length - length} more bytes)"
            : preview;
    }

    private void AddArrayActions(ObservableCollection<PropertyViewModel> target)
    {
        if (IsArray && !IsByteArray)
        {
            target.Add(new PropertyViewModel(this));
        }
    }

    private void AddArrayElement()
    {
        if (IsByteArray)
        {
            var bytes = _field!.AsByteArray;
            Array.Resize(ref bytes, bytes.Length + 1);
            _field.AsByteArray = bytes;
            Children.Add(CreateByteElement(bytes.Length - 1));
            if (_loadedByteCount == bytes.Length - 1)
                _loadedByteCount++;
            OnPropertyChanged(nameof(ArrayCount));
            OnPropertyChanged(nameof(Name));
            RebuildTreeChildren();
        }
        else
        {
            var elementTemplate = _field!.TemplateField.Children[1];
            var element = CreateEmptyField(elementTemplate);
            _field.Children.Add(element);
            Children.Add(CreateChild(element, Children.Count));
            UpdateArraySize();
        }

        _save();
    }

    private void RemoveArrayElement(AssetTypeValueField childField)
    {
        var index = _field!.Children.IndexOf(childField);
        if (index >= 0)
            RemoveArrayElement(index);
    }

    private void RemoveArrayElement(int index)
    {
        if (IsByteArray)
        {
            var oldBytes = _field!.AsByteArray;
            if (index < 0 || index >= oldBytes.Length)
                return;

            var bytes = new byte[oldBytes.Length - 1];
            Array.Copy(oldBytes, 0, bytes, 0, index);
            Array.Copy(oldBytes, index + 1, bytes, index, oldBytes.Length - index - 1);
            _field.AsByteArray = bytes;
            if (index < _loadedByteCount)
                _loadedByteCount--;

            var removedItem = Children.FirstOrDefault(child => child.ByteIndex == index);
            if (removedItem is not null)
                Children.Remove(removedItem);

            foreach (var child in Children.Where(child => child.ByteIndex > index))
            {
                child.ByteIndex--;
                child._numberValue = bytes[child.ByteIndex];
                child.OnPropertyChanged(nameof(Name));
                child.OnPropertyChanged(nameof(NumberValue));
            }

            OnPropertyChanged(nameof(ArrayCount));
            OnPropertyChanged(nameof(Name));
            RebuildTreeChildren();
            _save();
            return;
        }
        else
        {
            if (index < 0 || index >= _field!.Children.Count)
                return;

            _field.Children.RemoveAt(index);
            UpdateArraySize();
        }

        Children.RemoveAt(index);
        _save();
    }

    private void LoadMoreBytes()
    {
        var bytes = _field!.AsByteArray;
        var nextLimit = Math.Min(bytes.Length, _loadedByteCount + BytePageSize);
        _batchingChildren = true;
        try
        {
            for (var index = _loadedByteCount; index < nextLimit; index++)
                Children.Add(CreateByteElement(index));

            _loadedByteCount = nextLimit;
        }
        finally
        {
            _batchingChildren = false;
        }

        OnChildrenChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    private void SetByte(int index, byte value)
    {
        var bytes = _field!.AsByteArray;
        if (index < 0 || index >= bytes.Length)
            return;

        bytes[index] = value;
        _field.AsByteArray = bytes;
        _save();
    }

    private void UpdateArraySize()
    {
        var arrayInfo = _field!.Value.AsArray;
        arrayInfo.size = _field.Children.Count;
        _field.Value.AsArray = arrayInfo;
    }

    private decimal GetNumericValue()
    {
        if (_byteArrayOwner is not null)
            return _byteArrayOwner._field!.AsByteArray[ByteIndex];

        return ValueType switch
        {
            AssetValueType.Int8 => _field!.Value.AsSByte,
            AssetValueType.UInt8 => _field!.Value.AsByte,
            AssetValueType.Int16 => _field!.Value.AsShort,
            AssetValueType.UInt16 => _field!.Value.AsUShort,
            AssetValueType.Int32 => _field!.Value.AsInt,
            AssetValueType.UInt32 => _field!.Value.AsUInt,
            AssetValueType.Int64 => _field!.Value.AsLong,
            AssetValueType.UInt64 => _field!.Value.AsULong,
            _ => throw new InvalidOperationException($"Unsupported numeric property type: {TypeName}.")
        };
    }

    private void SetNumericValue(decimal value)
    {
        switch (ValueType)
        {
            case AssetValueType.Int8:
                _field!.Value.AsSByte = decimal.ToSByte(value);
                break;
            case AssetValueType.UInt8:
                _field!.Value.AsByte = decimal.ToByte(value);
                break;
            case AssetValueType.Int16:
                _field!.Value.AsShort = decimal.ToInt16(value);
                break;
            case AssetValueType.UInt16:
                _field!.Value.AsUShort = decimal.ToUInt16(value);
                break;
            case AssetValueType.Int32:
                _field!.Value.AsInt = decimal.ToInt32(value);
                break;
            case AssetValueType.UInt32:
                _field!.Value.AsUInt = decimal.ToUInt32(value);
                break;
            case AssetValueType.Int64:
                _field!.Value.AsLong = decimal.ToInt64(value);
                break;
            case AssetValueType.UInt64:
                _field!.Value.AsULong = decimal.ToUInt64(value);
                break;
        }
    }

    private static AssetTypeValueField CreateEmptyField(AssetTypeTemplateField template)
    {
        var field = new AssetTypeValueField
        {
            TemplateField = template,
            Children = []
        };

        if (template.IsArray)
        {
            field.Value = template.ValueType == AssetValueType.ByteArray
                ? new AssetTypeValue(AssetValueType.ByteArray, Array.Empty<byte>())
                : new AssetTypeValue(AssetValueType.Array, new AssetTypeArrayInfo(0));
            return field;
        }

        if (template.ValueType == AssetValueType.None)
        {
            foreach (var childTemplate in template.Children)
                field.Children.Add(CreateEmptyField(childTemplate));
            return field;
        }

        field.Value = template.ValueType switch
        {
            AssetValueType.Bool => new AssetTypeValue(false),
            AssetValueType.Int8 => new AssetTypeValue((sbyte)0),
            AssetValueType.UInt8 => new AssetTypeValue((byte)0),
            AssetValueType.Int16 => new AssetTypeValue((short)0),
            AssetValueType.UInt16 => new AssetTypeValue((ushort)0),
            AssetValueType.Int32 => new AssetTypeValue(0),
            AssetValueType.UInt32 => new AssetTypeValue(0u),
            AssetValueType.Int64 => new AssetTypeValue(0L),
            AssetValueType.UInt64 => new AssetTypeValue(0UL),
            AssetValueType.Float => new AssetTypeValue(0f),
            AssetValueType.Double => new AssetTypeValue(0d),
            AssetValueType.String => new AssetTypeValue(string.Empty),
            AssetValueType.ByteArray => new AssetTypeValue(AssetValueType.ByteArray, Array.Empty<byte>()),
            AssetValueType.ManagedReferencesRegistry => new AssetTypeValue(
                new ManagedReferencesRegistry { version = 1, references = [] }),
            _ => throw new InvalidOperationException($"Cannot initialize array element type '{template.Type}'.")
        };

        return field;
    }

    private static bool IsNumeric(AssetValueType valueType)
    {
        return valueType is AssetValueType.Int8
            or AssetValueType.UInt8
            or AssetValueType.Int16
            or AssetValueType.UInt16
            or AssetValueType.Int32
            or AssetValueType.UInt32
            or AssetValueType.Int64
            or AssetValueType.UInt64;
    }

    private static (decimal Minimum, decimal Maximum) GetNumericBounds(AssetValueType valueType)
    {
        return valueType switch
        {
            AssetValueType.Int8 => (sbyte.MinValue, sbyte.MaxValue),
            AssetValueType.UInt8 => (byte.MinValue, byte.MaxValue),
            AssetValueType.Int16 => (short.MinValue, short.MaxValue),
            AssetValueType.UInt16 => (ushort.MinValue, ushort.MaxValue),
            AssetValueType.Int32 => (int.MinValue, int.MaxValue),
            AssetValueType.UInt32 => (uint.MinValue, uint.MaxValue),
            AssetValueType.Int64 => (long.MinValue, long.MaxValue),
            AssetValueType.UInt64 => (ulong.MinValue, ulong.MaxValue),
            _ => throw new InvalidOperationException($"Unsupported integer property type: {valueType}.")
        };
    }

    private enum ByteArrayPreview
    {
        Instance
    }
}

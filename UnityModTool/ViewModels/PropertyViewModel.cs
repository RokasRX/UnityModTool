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

    private readonly AssetTypeValueField? _field;
    private readonly AssetTypeValueField? _arrayField;
    private readonly Action _save;
    private readonly PropertyViewModel? _byteArrayOwner;
    private readonly Action? _remove;
    private decimal? _numberValue;
    private string? _textValue;
    private bool _booleanValue;
    private string? _validationMessage;
    private int _arrayIndex;

    private string BaseName { get; }
    private bool IsArrayItem { get; }
    public bool IsArrayAction { get; }
    public bool IsAction => IsArrayAction;

    public string Name => IsArrayItem
        ? $"[{_arrayIndex}]"
        : IsVectorContainer || IsArray ? $"{BaseName} ({ArrayCount})" : BaseName;

    public string TypeName { get; }
    public ObservableCollection<PropertyViewModel> Children { get; } = [];

    public bool IsText => ValueType is AssetValueType.String or AssetValueType.Float or AssetValueType.Double;
    public bool IsBoolean => ValueType == AssetValueType.Bool;
    public bool IsNumber => _byteArrayOwner is null && IsNumeric(ValueType);
    public bool IsEditable => IsText || IsBoolean || IsNumber;
    public bool IsReadOnly => (!IsEditable && !IsByteArrayPreview) || IsByteArray;
    public bool IsPropertyRow => !IsAction;
    public bool IsArrayElement => IsArrayItem;
    public bool IsByteArrayPreview { get; }
    public bool IsArray { get; }
    private bool IsVectorContainer { get; }
    public bool IsByteArray => !IsByteArrayPreview
        && _arrayField?.TemplateField.ValueType == AssetValueType.ByteArray;
    public int ArrayCount => _byteArrayOwner is not null
        ? 0
        : IsByteArray ? _arrayField!.AsByteArray.Length
        : IsArray ? _arrayField!.Children.Count : Children.Count;
    public decimal Minimum { get; }
    public decimal Maximum { get; }
    public string? DisplayValue => IsByteArrayPreview
        ? GetByteArrayPreview()
        : IsByteArray || IsEditable ? null : TypeName;
    public string ActionText => "Add element";
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

        _byteArrayOwner!._arrayField!.AsByteArray = bytes;
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

    private AssetValueType ValueType => _field!.TemplateField.ValueType;

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

            SetNumericValue(value.Value);
            _save();

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
        IsVectorContainer = field.TemplateField.Type == "vector"
            && field.Children.Count == 1
            && field.Children[0].TemplateField.IsArray;
        _arrayField = IsVectorContainer
            ? field.Children[0]
            : field.TemplateField.IsArray ? field : null;
        IsArray = _arrayField is not null;
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

        if (IsArray)
        {
            if (IsByteArray)
                Children.Add(new PropertyViewModel(this, ByteArrayPreview.Instance));
            else
            {
                for (var index = 0; index < _arrayField!.Children.Count; index++)
                    Children.Add(CreateArrayElement(_arrayField.Children[index], index));

                Children.Add(new PropertyViewModel(this));
            }
        }
        else
        {
            for (var index = 0; index < field.Children.Count; index++)
                Children.Add(CreateChild(field.Children[index], index));
        }

        Children.CollectionChanged += OnChildrenChanged;
    }

    private PropertyViewModel(PropertyViewModel owner, ByteArrayPreview _)
    {
        BaseName = "Hex";
        TypeName = "Hexadecimal";
        _field = owner._arrayField;
        _save = owner._save;
        _byteArrayOwner = owner;
        IsByteArrayPreview = true;
    }

    private PropertyViewModel(PropertyViewModel owner)
    {
        BaseName = string.Empty;
        TypeName = string.Empty;
        _save = owner._save;
        IsArrayAction = true;
        ActionCommand = new RelayCommand(owner.AddArrayElement);
    }

    private PropertyViewModel CreateChild(AssetTypeValueField childField, int index)
    {
        if (!IsArray)
            return new PropertyViewModel(childField.TemplateField.Name, childField, _save);

        return CreateArrayElement(childField, index);
    }

    private PropertyViewModel CreateArrayElement(AssetTypeValueField childField, int index)
    {
        return new PropertyViewModel($"[{index}]", childField, _save, true, () => RemoveArrayElement(childField));
    }

    private void OnChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (IsArray && !IsByteArray)
        {
            var itemCount = Children.Count - (Children.Count > 0 && Children[^1].IsArrayAction ? 1 : 0);
            for (var index = 0; index < itemCount; index++)
            {
                Children[index]._arrayIndex = index;
                Children[index].OnPropertyChanged(nameof(Name));
            }
        }

        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(ArrayCount));
    }

    private string GetByteArrayPreview()
    {
        var bytes = _byteArrayOwner!._arrayField!.AsByteArray;
        var length = Math.Min(bytes.Length, BytePreviewLength);
        var preview = string.Join(" ", bytes.Take(length).Select(value => value.ToString("X2", CultureInfo.InvariantCulture)));
        return bytes.Length > length
            ? $"{preview} ... ({bytes.Length - length} more bytes)"
            : preview;
    }

    private void AddArrayElement()
    {
        var elementTemplate = _arrayField!.TemplateField.Children[1];
        var element = CreateEmptyField(elementTemplate);
        var index = _arrayField.Children.Count;
        _arrayField.Children.Add(element);
        Children.Insert(index, CreateArrayElement(element, index));
        UpdateArraySize();
        _save();
    }

    private void RemoveArrayElement(AssetTypeValueField childField)
    {
        var index = _arrayField!.Children.IndexOf(childField);
        if (index >= 0)
            RemoveArrayElement(index);
    }

    private void RemoveArrayElement(int index)
    {
        if (index < 0 || index >= _arrayField!.Children.Count)
            return;

        _arrayField.Children.RemoveAt(index);
        UpdateArraySize();
        Children.RemoveAt(index);
        _save();
    }

    private void UpdateArraySize()
    {
        var arrayInfo = _arrayField!.Value.AsArray;
        arrayInfo.size = _arrayField.Children.Count;
        _arrayField.Value.AsArray = arrayInfo;
    }

    private decimal GetNumericValue()
    {
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

namespace Replay.Models.Descriptors;

public sealed class RpcDescriptor
{
    public required string Name { get; init; }
    public required string FunctionExportPath { get; init; }
    public ExportCategory Categories { get; init; }
    public ExportGroupDescriptor? ParameterDescriptor { get; init; }
    public IReadOnlyList<FieldDescriptor> Fields { get; init; } = [];
    public IRpcDecoderDescriptor? Decoder { get; init; }
}

public sealed class RpcDescriptorBuilder
{
    private readonly List<FieldDescriptorBuilder> _fieldBuilders = [];
    private readonly string _name;
    private readonly string _functionExportPath;
    private readonly ExportGroupDescriptor? _parameterDescriptor;

    internal RpcDescriptorBuilder(
        string name,
        string functionExportPath,
        ExportCategory categories,
        ExportGroupDescriptor? parameterDescriptor = null)
    {
        _name = name;
        _functionExportPath = functionExportPath;
        _parameterDescriptor = parameterDescriptor;
        Categories = categories;
    }

    private ExportCategory Categories { get; set; }

    private IRpcDecoderDescriptor? Decoder { get; set; }

    public void Decode(IRpcDecoderDescriptor decoder)
    {
        Decoder = decoder;
    }

    public FieldDescriptorBuilder AddField(string exportName,
        string propertyName,
        ExportCategory categories = ExportCategory.None)
    {
        var builder = new FieldDescriptorBuilder(exportName, propertyName, targetProperty: null, handle: null, categories);
        _fieldBuilders.Add(builder);
        return builder;
    }

    public FieldDescriptorBuilder AddFieldHandle(
        uint handle,
        string propertyName,
        ExportCategory categories = ExportCategory.None)
    {
        var builder = new FieldDescriptorBuilder(exportName: null, propertyName, targetProperty: null, handle, categories);
        _fieldBuilders.Add(builder);
        return builder;
    }

    internal RpcDescriptor Build()
    {
        FieldDescriptor[] fields;
        if (_fieldBuilders.Count == 0 && _parameterDescriptor is not null)
        {
            fields = _parameterDescriptor.Fields.ToArray();
        }
        else
        {
            fields = new FieldDescriptor[_fieldBuilders.Count];
            for (var i = 0; i < fields.Length; i++)
            {
                fields[i] = _fieldBuilders[i].Build();
            }
        }

        return new RpcDescriptor
        {
            Name = _name,
            FunctionExportPath = _functionExportPath,
            Categories = Categories,
            ParameterDescriptor = _parameterDescriptor,
            Decoder = Decoder,
            Fields = fields,
        };
    }
}

public interface IRpcDecoderDescriptor;

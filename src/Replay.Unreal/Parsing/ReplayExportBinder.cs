using Replay.Models.Descriptors;
using Replay.Models.Net;

namespace Replay.Unreal.Parsing;

internal sealed class ReplayExportBinder
{
    private readonly DescriptorCatalogIndex _catalogIndex;
    private readonly BoundExportStore _store;
    private readonly ParseProfile _parseProfile;

    public ReplayExportBinder(
        DescriptorCatalogIndex catalogIndex,
        BoundExportStore store,
        ParseProfile parseProfile)
    {
        _catalogIndex = catalogIndex;
        _store = store;
        _parseProfile = parseProfile;
    }

    public BoundExportGroup BindExportGroup(
        NetFieldExportGroup replayGroup,
        ExportGroupDescriptor descriptor)
    {
        var maxHandle = checked((int)replayGroup.NetFieldExportsLength);
        var fields = new FieldBinding[maxHandle];
        var groupEnabled = IsPathSelected(descriptor.Path) && IsCategorySelected(descriptor.Categories);

        var allFields = new List<FieldDescriptor>();
        _catalogIndex.CollectFields(descriptor, allFields);

        foreach (var fieldDesc in allFields)
        {
            var handle = ResolveHandle(fieldDesc, replayGroup);
            if (handle is null || handle.Value >= maxHandle)
            {
                continue;
            }

            var h = handle.Value;
            var exportName = fieldDesc.ExportName ?? replayGroup.NetFieldExports[h]?.Name;
            var propertyName = fieldDesc.PropertyName ?? exportName;
            var categories = fieldDesc.Categories == ExportCategory.None
                ? descriptor.Categories
                : fieldDesc.Categories;
            var decoder = ResolveFieldDecoder(fieldDesc);
            var enabled = decoder is not null
                          && groupEnabled
                          && IsFieldSelected(descriptor.Path, propertyName, exportName, categories);

            fields[h] = new FieldBinding
            {
                Enabled = enabled,
                Categories = categories,
                Decoder = enabled ? decoder : null,
                Name = propertyName,
                ExportName = exportName,
                TargetProperty = fieldDesc.TargetProperty,
            };
        }

        for (var i = 0; i < maxHandle; i++)
        {
            if (fields[i].Name is null && fields[i].Decoder is null)
            {
                fields[i] = new FieldBinding
                {
                    Enabled = false,
                    Name = replayGroup.NetFieldExports[i]?.Name,
                    ExportName = replayGroup.NetFieldExports[i]?.Name,
                };
            }
        }

        return new BoundExportGroup
        {
            SourceDescriptor = descriptor,
            Categories = descriptor.Categories,
            Grammar = descriptor.Grammar,
            Enabled = groupEnabled,
            CaptureDiagnosticFields = _parseProfile.CaptureDiagnosticFields,
            FieldsByHandle = fields,
        };
    }

    public BoundClassNetCache BindClassNetCache(
        NetFieldExportGroup replayGroup,
        ClassNetCacheDescriptor descriptor)
    {
        var maxHandle = checked((int)replayGroup.NetFieldExportsLength);
        var functions = new BoundRpcFunction[maxHandle];
        var cacheEnabled = IsPathSelected(descriptor.Path);

        foreach (var rpcDesc in descriptor.FunctionFields)
        {
            var handle = ResolveFunctionHandle(rpcDesc, replayGroup);
            if (handle is null || handle.Value >= maxHandle)
            {
                continue;
            }

            var categories = rpcDesc.Categories;
            var decoder = ResolveRpcDecoder(rpcDesc);
            var functionGroup = _store.GetBoundGroup(rpcDesc.FunctionExportPath);
            var hasFunctionDescriptor = _catalogIndex.TryGetExportDescriptor(rpcDesc.FunctionExportPath, out _);
            var enabled = cacheEnabled
                          && IsFieldSelected(descriptor.Path, rpcDesc.Name, rpcDesc.Name, categories)
                          && (decoder is not null || hasFunctionDescriptor);

            var function = new BoundRpcFunction
            {
                Name = rpcDesc.Name,
                FunctionExportPath = rpcDesc.FunctionExportPath,
                Categories = categories,
                Enabled = enabled,
                CaptureDiagnosticFields = _parseProfile.CaptureDiagnosticFields,
                FunctionGroup = functionGroup,
                Decoder = enabled ? decoder : null,
            };

            functions[handle.Value] = function;
            if (functionGroup is null && hasFunctionDescriptor)
            {
                _store.AddPendingRpcFunction(rpcDesc.FunctionExportPath, function);
            }
        }

        return new BoundClassNetCache
        {
            Path = descriptor.Path,
            SourceDescriptor = descriptor,
            Grammar = descriptor.Grammar,
            Enabled = cacheEnabled,
            FunctionsByHandle = functions,
        };
    }

    private static uint? ResolveHandle(FieldDescriptor fieldDesc, NetFieldExportGroup replayGroup)
    {
        if (fieldDesc.Handle.HasValue)
        {
            return fieldDesc.Handle.Value;
        }

        if (fieldDesc.ExportName is not null)
        {
            for (uint i = 0; i < replayGroup.NetFieldExportsLength; i++)
            {
                if (replayGroup.NetFieldExports[i]?.Name == fieldDesc.ExportName)
                {
                    return i;
                }
            }
        }

        return null;
    }

    private static uint? ResolveFunctionHandle(RpcDescriptor rpcDesc, NetFieldExportGroup replayGroup)
    {
        for (uint i = 0; i < replayGroup.NetFieldExportsLength; i++)
        {
            if (replayGroup.NetFieldExports[i]?.Name == rpcDesc.Name)
            {
                return i;
            }
        }

        return null;
    }

    private static IFieldDecoder? ResolveFieldDecoder(FieldDescriptor fieldDesc)
    {
        if (fieldDesc.Decoder is null)
        {
            return null;
        }

        return fieldDesc.Decoder as IFieldDecoder
               ?? throw new InvalidOperationException(
                   $"Field descriptor '{fieldDesc.PropertyName ?? fieldDesc.ExportName ?? fieldDesc.Handle?.ToString() ?? "<unnamed>"}' uses an incompatible decoder type.");
    }

    private static IRpcDecoder? ResolveRpcDecoder(RpcDescriptor rpcDesc)
    {
        if (rpcDesc.Decoder is null)
        {
            return null;
        }

        return rpcDesc.Decoder as IRpcDecoder
               ?? throw new InvalidOperationException(
                   $"RPC descriptor '{rpcDesc.Name}' uses an incompatible decoder type.");
    }

    private bool IsPathSelected(string path)
    {
        if (_parseProfile.IncludedPaths is not null && !_parseProfile.IncludedPaths.Contains(path))
        {
            return false;
        }

        return _parseProfile.ExcludedPaths is null || !_parseProfile.ExcludedPaths.Contains(path);
    }

    private bool IsFieldSelected(string path, string? propertyName, string? exportName, ExportCategory categories)
    {
        if (!IsCategorySelected(categories))
        {
            return false;
        }

        if (_parseProfile.IncludedFields is null)
        {
            return true;
        }

        return IsIncludedFieldName(propertyName, path) || IsIncludedFieldName(exportName, path);
    }

    private bool IsIncludedFieldName(string? fieldName, string path)
    {
        return fieldName is not null
               && _parseProfile.IncludedFields is not null
               && (_parseProfile.IncludedFields.Contains(fieldName)
                   || _parseProfile.IncludedFields.Contains($"{path}:{fieldName}"));
    }

    private bool IsCategorySelected(ExportCategory categories)
    {
        if (_parseProfile.EnabledCategories == ExportCategory.None)
        {
            return false;
        }

        return categories == ExportCategory.None
               || (categories & _parseProfile.EnabledCategories) != 0;
    }
}

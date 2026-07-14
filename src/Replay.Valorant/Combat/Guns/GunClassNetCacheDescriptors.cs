using Replay.Models.Descriptors;
using Replay.Valorant.Descriptors;

namespace Replay.Valorant.Combat.Guns;

internal static class GunClassNetCacheDescriptors
{
    private const string RpcName = "MulticastPlayContinuousEffectFromClient";
    private const string RpcExportPath = "/Script/ShooterGame.AresEquippable:MulticastPlayContinuousEffectFromClient";

    public static IEnumerable<ClassNetCacheDescriptor> Create()
    {
        foreach (var gunClassPath in ValorantEquippableResolver.GunClassPaths)
        {
            yield return new ClassNetCacheDescriptor(
                gunClassPath + "_ClassNetCache",
                [
                    new RpcDescriptor
                    {
                        Name = RpcName,
                        FunctionExportPath = RpcExportPath,
                        Categories = ExportCategory.Gunplay,
                        Decoder = ValorantPayloadDecoders.SkipPayloadRpc,
                    },
                ]);
        }
    }
}

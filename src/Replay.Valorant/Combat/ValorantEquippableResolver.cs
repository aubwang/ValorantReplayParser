using Replay.Encoding.Net;
using Replay.Models.Net;

namespace Replay.Valorant.Combat;

public static class ValorantEquippableResolver
{
    private const int MaxOuterDepth = 16;

    private static readonly IReadOnlyDictionary<string, Definition> Definitions = CreateDefinitions();

    public static ValorantEquippable Resolve(uint netGuid, NetGuidCache? netGuidCache)
    {
        string? firstPath = null;
        foreach (var path in GetPaths(netGuid, netGuidCache))
        {
            firstPath ??= path;
            if (Definitions.TryGetValue(path, out var definition))
            {
                return new ValorantEquippable(netGuid, definition.Name, definition.Category, definition.ClassPath);
            }
        }

        return new ValorantEquippable(netGuid, null, ValorantEquippableCategory.Unknown, firstPath);
    }

    private static IEnumerable<string> GetPaths(uint netGuid, NetGuidCache? netGuidCache)
    {
        if (netGuidCache is null || netGuid == 0)
        {
            yield break;
        }

        var current = new NetworkGuid(netGuid);
        for (var depth = 0; depth < MaxOuterDepth && current.IsValid; depth++)
        {
            if (netGuidCache.TryGetPath(current.Value, out var path))
            {
                yield return path;
            }

            if (!netGuidCache.TryGetOuterNetGuid(current.Value, out current))
            {
                yield break;
            }
        }
    }

    private static IReadOnlyDictionary<string, Definition> CreateDefinitions()
    {
        Definition[] definitions =
        [
            Define("/Game/Characters/_Core/Equippable_Unarmed.Equippable_Unarmed_C", "Unarmed", ValorantEquippableCategory.Unarmed),
            Define("/Game/Equippables/Melee/Ability_Melee_Base.Ability_Melee_Base_C", "Melee", ValorantEquippableCategory.Melee),
            Define("/Game/Equippables/Bomb/BombEquippable.BombEquippable_C", "Spike", ValorantEquippableCategory.Bomb),
            Define("/Game/Equippables/Guns/Sidearms/BasePistol/BasePistol.BasePistol_C", "Classic", ValorantEquippableCategory.Sidearm),
            Define("/Game/Equippables/Guns/Sidearms/Slim/SawedOffShotgun.SawedOffShotgun_C", "Shorty", ValorantEquippableCategory.Sidearm),
            Define("/Game/Equippables/Guns/Sidearms/AutoPistol/AutomaticPistol.AutomaticPistol_C", "Frenzy", ValorantEquippableCategory.Sidearm),
            Define("/Game/Equippables/Guns/Sidearms/Luger/LugerPistol.LugerPistol_C", "Ghost", ValorantEquippableCategory.Sidearm),
            Define("/Game/Equippables/Guns/Sidearms/Compact/CompactPistol.CompactPistol_C", "Compact Pistol", ValorantEquippableCategory.Sidearm),
            Define("/Game/Equippables/Guns/Sidearms/Revolver/RevolverPistol.RevolverPistol_C", "Sheriff", ValorantEquippableCategory.Sidearm),
            Define("/Game/Equippables/Guns/SubMachineGuns/Vector/Vector.Vector_C", "Stinger", ValorantEquippableCategory.Smg),
            Define("/Game/Equippables/Guns/SubMachineGuns/MP5/SubMachineGun_MP5.SubMachineGun_MP5_C", "Spectre", ValorantEquippableCategory.Smg),
            Define("/Game/Equippables/Guns/Shotguns/PumpShotgun/PumpShotgun.PumpShotgun_C", "Bucky", ValorantEquippableCategory.Shotgun),
            Define("/Game/Equippables/Guns/Shotguns/AutoShotgun/AutomaticShotgun.AutomaticShotgun_C", "Judge", ValorantEquippableCategory.Shotgun),
            Define("/Game/Equippables/Guns/Rifles/Burst/AssaultRifle_Burst.AssaultRifle_Burst_C", "Bulldog", ValorantEquippableCategory.Rifle),
            Define("/Game/Equippables/Guns/SniperRifles/Dmr/DMR.DMR_C", "Guardian", ValorantEquippableCategory.Rifle),
            Define("/Game/Equippables/Guns/Rifles/Carbine/AssaultRifle_ACR.AssaultRifle_ACR_C", "Phantom", ValorantEquippableCategory.Rifle),
            Define("/Game/Equippables/Guns/Rifles/AK/AssaultRifle_AK.AssaultRifle_AK_C", "Vandal", ValorantEquippableCategory.Rifle),
            Define("/Game/Equippables/Guns/SniperRifles/Leversniper/LeverSniperRifle.LeverSniperRifle_C", "Marshal", ValorantEquippableCategory.SniperRifle),
            Define("/Game/Equippables/Guns/SniperRifles/Boltsniper/BoltSniper.BoltSniper_C", "Outlaw", ValorantEquippableCategory.SniperRifle),
            Define("/Game/Equippables/Guns/SniperRifles/Doublesniper/DS_Gun.DS_Gun_C", "Operator", ValorantEquippableCategory.SniperRifle),
            Define("/Game/Equippables/Guns/HvyMachineGuns/LMG/LightMachineGun.LightMachineGun_C", "Ares", ValorantEquippableCategory.MachineGun),
            Define("/Game/Equippables/Guns/HvyMachineGuns/HMG/HeavyMachineGun.HeavyMachineGun_C", "Odin", ValorantEquippableCategory.MachineGun),
            Define("/Game/Characters/Deadeye/S0/Ability_Q/Gun/Gun_Deadeye_Q_Pistol.Gun_Deadeye_Q_Pistol_C", "Headhunter", ValorantEquippableCategory.Sidearm),
        ];

        var values = new Dictionary<string, Definition>(StringComparer.Ordinal);
        foreach (var definition in definitions)
        {
            values.Add(definition.ClassPath, definition);

            var separatorIndex = definition.ClassPath.LastIndexOf('.');
            var packagePath = definition.ClassPath[..separatorIndex];
            var className = definition.ClassPath[(separatorIndex + 1)..];
            values.Add(packagePath, definition);
            values.Add("Default__" + className, definition);
        }

        return values;
    }

    private static Definition Define(string classPath, string name, ValorantEquippableCategory category) =>
        new(classPath, name, category);

    private sealed record Definition(string ClassPath, string Name, ValorantEquippableCategory Category);
}
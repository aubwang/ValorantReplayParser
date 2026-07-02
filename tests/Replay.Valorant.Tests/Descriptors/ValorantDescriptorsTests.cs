using Replay.Models.Descriptors;
using Replay.Valorant.Descriptors;

namespace Replay.Valorant.Tests.Descriptors;

public class ValorantDescriptorsTests
{
    [Test]
    public void CreateCatalog_IncludesPlayableAgentDescriptors()
    {
        string[] expectedAgentPaths =
        [
            "/Game/Characters/Aggrobot/Aggrobot_PC.Aggrobot_PC_C",
            "/Game/Characters/BountyHunter/BountyHunter_PC.BountyHunter_PC_C",
            "/Game/Characters/Breach/Breach_PC.Breach_PC_C",
            "/Game/Characters/Cable/Cable_PC.Cable_PC_C",
            "/Game/Characters/Cashew/Cashew_PC.Cashew_PC_C",
            "/Game/Characters/Clay/Clay_PC.Clay_PC_C",
            "/Game/Characters/Deadeye/Deadeye_PC.Deadeye_PC_C",
            "/Game/Characters/Grenadier/Grenadier_PC.Grenadier_PC_C",
            "/Game/Characters/Guide/Guide_PC.Guide_PC_C",
            "/Game/Characters/Gumshoe/Gumshoe_PC.Gumshoe_PC_C",
            "/Game/Characters/Hunter/Hunter_PC.Hunter_PC_C",
            "/Game/Characters/Iris/Iris_PC.Iris_PC_C",
            "/Game/Characters/Killjoy/Killjoy_PC.Killjoy_PC_C",
            "/Game/Characters/Mage/Mage_PC.Mage_PC_C",
            "/Game/Characters/Nox/Nox_PC.Nox_PC_C",
            "/Game/Characters/Pandemic/Pandemic_PC.Pandemic_PC_C",
            "/Game/Characters/Phoenix/Phoenix_PC.Phoenix_PC_C",
            "/Game/Characters/Pine/Pine_PC.Pine_PC_C",
            "/Game/Characters/Rift/Rift_PC.Rift_PC_C",
            "/Game/Characters/Sarge/Sarge_PC.Sarge_PC_C",
            "/Game/Characters/Sequoia/Sequoia_PC.Sequoia_PC_C",
            "/Game/Characters/Smonk/Smonk_PC.Smonk_PC_C",
            "/Game/Characters/Sprinter/Sprinter_PC.Sprinter_PC_C",
            "/Game/Characters/Stealth/Stealth_PC.Stealth_PC_C",
            "/Game/Characters/Terra/Terra_PC.Terra_PC_C",
            "/Game/Characters/Thorne/Thorne_PC.Thorne_PC_C",
            "/Game/Characters/Vampire/Vampire_PC.Vampire_PC_C",
            "/Game/Characters/Wraith/Wraith_PC.Wraith_PC_C",
            "/Game/Characters/Wushu/Wushu_PC.Wushu_PC_C",
        ];

        var actualAgentPaths = ValorantDescriptors.CreateCatalog()
            .ExportGroupDescriptors
            .Where(descriptor => descriptor.Categories.HasFlag(ExportCategory.Agent))
            .Select(descriptor => descriptor.Path)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();

        Assert.That(actualAgentPaths, Is.EqualTo(expectedAgentPaths));
    }

    [Test]
    public void CreateCatalog_IncludesRequestedGameplayExportDescriptors()
    {
        string[] expectedPaths =
        [
            "/Game/GameModes/Bomb/BombPlayerState.BombPlayerState_C",
            "/Game/GameModes/Common/BaseReplayPlayerState.BaseReplayPlayerState_C",
            "/Game/GameModes/Bomb/BombGameState.BombGameState_C",
            "/Game/GameModes/Bomb/Bomb_CombatReportComponent.Bomb_CombatReportComponent_C",
            "/Script/ShooterGame.AresInventory",
            "/Script/ShooterGame.EquippableStateMachineComponent",
            "/Script/ShooterGame.AmmoComponent",
            "/Script/ShooterGame.AresAttributeSet",
            "/Script/ShooterGame.ChildDamageSectionComponent",
            "/Script/ShooterGame.ChildRegionDamageSectionComponent",
            "/Script/ShooterGame.AttachedDamageSectionComponent",
            "/Script/ShooterGame.FiringStateComponent",
        ];

        var descriptors = ValorantDescriptors.CreateCatalog().ExportGroupDescriptors;
        var descriptorPaths = descriptors
            .Select(descriptor => descriptor.Path)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Multiple(() =>
        {
            foreach (var path in expectedPaths)
            {
                var descriptor = descriptors.Single(d => d.Path == path);
                Assert.That(descriptorPaths.Contains(path), Is.True, path);
                Assert.That(descriptor.Fields, Is.Not.Empty, path);
                Assert.That(descriptor.Fields.All(field => field.Decoder is not null), Is.True, path);
            }
        });
    }

    [Test]
    public void CreateCatalog_IncludesRequestedClassNetCacheFunctions()
    {
        var cacheDescriptors = ValorantDescriptors.CreateCatalog()
            .ClassNetCacheDescriptors
            .ToDictionary(descriptor => descriptor.Path, StringComparer.Ordinal);

        Assert.Multiple(() =>
        {
            AssertFunctions(
                cacheDescriptors["/Game/Characters/_Core/BaseReplayController.BaseReplayController_C_ClassNetCache"],
                "ClientReplayReceiveInputEventProcessingCapture",
                "ReplaysClientReceiveRemoteCharacterUpdatesSingleArrayNoAutonomous",
                "ClientGamePhaseBegin",
                "ClientGamePhaseEnded");

            AssertFunctions(
                cacheDescriptors["/Game/GameModes/Bomb/BombGameState.BombGameState_C_ClassNetCache"],
                "ClientBuyPhaseEnd",
                "ClientRoundStart",
                "Multicast Side Switch Event",
                "ClientResetRound",
                "MulticastEndRound",
                "MulticastEnterPlayspace",
                "MulticastReceivePlayerResurrectEvent",
                "MulticastReceivePlayerTemporaryDeathEvent_Base",
                "MulticastReceivePlayerTemporaryDeathEvent_Point",
                "MulticastSetPhase",
                "MulticastResetForRespawn");
        });
    }

    [Test]
    public void CreateCatalog_DecodesRoundLifecycleRpcFields()
    {
        var functions = ValorantDescriptors.CreateCatalog()
            .ClassNetCacheDescriptors
            .Single(descriptor => descriptor.Path == "/Game/GameModes/Bomb/BombGameState.BombGameState_C_ClassNetCache")
            .FunctionFields
            .ToDictionary(function => function.Name, StringComparer.Ordinal);

        Assert.Multiple(() =>
        {
            AssertDecodedRpcFields(functions["MulticastReceivePlayerResurrectEvent"],
                "ResurrectorPlayer",
                "ResurrectedPlayer");
            AssertDecodedRpcFields(functions["MulticastReceivePlayerTemporaryDeathEvent_Base"],
                "DamagerPlayer",
                "DownedPlayer");
            AssertDecodedRpcFields(functions["MulticastReceivePlayerTemporaryDeathEvent_Point"],
                "DamagerPlayer",
                "DownedPlayer");
            AssertDecodedRpcFields(functions["MulticastResetForRespawn"], "ShooterCharacter");
        });
    }

    [Test]
    public void CreateCatalog_IncludesFiringStateAttackVector16FromDump()
    {
        var descriptor = ValorantDescriptors.CreateCatalog()
            .ExportGroupDescriptors
            .Single(descriptor => descriptor.Path == "/Script/ShooterGame.FiringStateComponent");

        var exportNames = descriptor.Fields
            .Select(field => field.ExportName)
            .ToHashSet(StringComparer.Ordinal);

        Assert.That(exportNames.Contains("FiringState.AttackVector.16"), Is.True);
    }

    private static void AssertDecodedRpcFields(RpcDescriptor descriptor, params string[] fieldNames)
    {
        Assert.That(descriptor.Decoder, Is.Null, $"{descriptor.Name} should use named function fields");

        var fieldsByName = descriptor.Fields
            .Where(field => field.PropertyName is not null)
            .ToDictionary(field => field.PropertyName!, StringComparer.Ordinal);

        foreach (var fieldName in fieldNames)
        {
            Assert.That(fieldsByName.ContainsKey(fieldName), Is.True, $"{descriptor.Name}:{fieldName}");
            Assert.That(fieldsByName[fieldName].Decoder, Is.Not.Null, $"{descriptor.Name}:{fieldName}");
        }
    }

    private static void AssertFunctions(ClassNetCacheDescriptor descriptor, params string[] functionNames)
    {
        var actualNames = descriptor.FunctionFields
            .Select(function => function.Name)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var functionName in functionNames)
        {
            Assert.That(actualNames.Contains(functionName), Is.True, $"{descriptor.Path}:{functionName}");
        }
    }
}

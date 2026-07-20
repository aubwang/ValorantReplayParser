using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Replay.Encoding.Archives;
using Replay.Models.Descriptors;
using Replay.Models.Events;
using Replay.Valorant;
using Replay.Valorant.Descriptors.Agents.Mage;

namespace Test.Integration;

[Category("Integration")]
public class HarborAbilityTests
{
    private class Sink : IReplayEventSink
    {
        private Action<CoveAbilityDescriptor> CoveConsumer { get; set; }
        
        public Sink(Action<CoveAbilityDescriptor> coveConsumer)
        {
            CoveConsumer = coveConsumer;
        }

        public void Emit(ReplayEvent replayEvent)
        {
            switch (replayEvent)
            {
                case ExportGroupReceived export:
                    if (export.Payload is CoveAbilityDescriptor cove)
                    {
                        CoveConsumer(cove);
                    }
                    break;
            }
        }
    }

    [Test]
    public void TestCove()
    {
        var archive = new FBinaryArchive(TestHelper.ReadReplayBytes("harbor.vrf"));

        var coveConsumer = Substitute.For<Action<CoveAbilityDescriptor>>();
        var parseNothingProfile = new ParseProfile
        {
            EnabledCategories = ExportCategory.Ability,
        };
        var eventSink = new Sink(coveConsumer);

        _ = ValorantReplayReader.CreateDefault(new NullLoggerFactory(), eventSink, parseNothingProfile).Read(archive);
        Assert.Multiple(() =>
        {
            Assert.That(coveConsumer.ReceivedCalls().All(_ => _.GetArguments().Cast<CoveAbilityDescriptor>().First().Instigator is 134 or null), Is.True);
            Assert.That(coveConsumer.ReceivedCalls().Select(_ => _.GetArguments().Cast<CoveAbilityDescriptor>().First().ReplicatedMovement).Count(_ => _ is not null), Is.EqualTo(1276));
            Assert.That(coveConsumer.ReceivedCalls().Select(_ => _.GetArguments().Cast<CoveAbilityDescriptor>().First().ReplicatedMovement).Count(_ => _ is null), Is.EqualTo(1));
        });
    }
}
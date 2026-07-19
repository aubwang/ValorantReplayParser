using Replay.Encoding.Archives;
using Replay.Unreal.Parsing;
using Replay.Valorant.GameState;

namespace Replay.Valorant.Tests.GameState;

public class CombatRoundReportsDecoderTests
{
    [Test]
    public void Decode_DecodesRelease1301RoundAndCharacterEnvelope()
    {
        const string payload =
            "AgIIQAAAAAAKMwwCAgxAAAAAAA4gNyIQQAAAgL8SQAAAgL8UEP7GEADIID8cykAE8O9BzEAAAAAAzhAC0AKkAwUAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACoQW5ErIEAAAB/sYEAAAB/tSH8AQAAAAA=";
        var archive = new BitArchiveReader(Convert.FromBase64String(payload), 897);
        var context = new FieldDecodeContext();

        var value = Decoder().Decode(ref context, archive);
        var rounds = (CombatRoundReportUpdate[])value.ObjectValue!;
        var report = rounds.Single().Reports.Single();

        Assert.Multiple(() =>
        {
            Assert.That(rounds.Single().RoundNumber, Is.Zero);
            Assert.That(report.RoundNumber, Is.Zero);
            Assert.That(report.Died, Is.False);
            Assert.That(archive.AtEnd, Is.True);
        });
    }

    [Test]
    public void Decode_DecodesParticipantDamageUpdates()
    {
        const string payload =
            "AgIKVRwCAg4gcy4QQG6awUISQAAAQEIUEAQWtRgCAhiRBCUAAAA3YmVhMzNjMC02N2JhLTViODgtYjY1NS01ODI2ZTcwODkzZDMAGsIIAAAApMrIAAAAAAA4QJZgPEDmXECA3DSDhUSAAACAhEggCEyAADggglCAAgAAAFSAAAAAAFiAAAAAAFwEwCgAGQgAGgE2tQgCAjggcy46QG6awUI8QAAAQEI+EARAIHMuQkBumsFCREAAAEBCRhAESCArMFqlAgICXBAAXkABAAAAYEAAHBBBYgLIBJhBAAAAAAAIAwUAAAAAAAAAAAAA";
        var archive = new BitArchiveReader(Convert.FromBase64String(payload), 1890);
        var context = new FieldDecodeContext();

        var value = Decoder().Decode(ref context, archive);
        var participants = ((CombatRoundReportUpdate[])value.ObjectValue!)
            .SelectMany(round => round.Reports)
            .SelectMany(report => report.Interactions)
            .ToArray();

        Assert.That(participants, Is.Not.Empty);
        Assert.That(participants.Any(item => item.Subject is not null), Is.True);
        Assert.That(archive.AtEnd, Is.True);
    }

    private static IFieldDecoder Decoder() => new CombatRoundReportsDecoder();
}

using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Networking.Sidecar;

namespace TiebaDIY.Scripts.Cards;

internal static class ElectronicSheepCandidateSync
{
    private const int MaxCandidateCount = ushort.MaxValue;
    private static readonly TimeSpan SyncTimeout = TimeSpan.FromSeconds(30);
    // Use an ordinary monitor object here: System.Threading.Lock's special lock
    // statement support starts in C# 13, while TiebaDIY intentionally targets C# 12.
    private static readonly object Gate = new();
    private static readonly Dictionary<CandidateKey, TaskCompletionSource<IReadOnlyList<SerializableCard>>> Waiters = [];
    private static readonly Dictionary<CandidateKey, IReadOnlyList<SerializableCard>> EarlyMessages = [];

    private static readonly RitsuLibSidecarSyncMessageDescriptor<CandidateMessage> Descriptor = new(
        ModuleId: Entry.ModId,
        MessageKey: "electronic_sheep_candidates_v1",
        Serialize: Serialize,
        Deserialize: Deserialize,
        Handle: Handle,
        LocationTargeted: true,
        ShouldBuffer: true,
        FailurePolicy: RitsuLibSidecarSyncFailurePolicy.Required,
        BroadcastScope: RitsuLibSidecarSyncBroadcastScope.ReadyPeers,
        DispatchLocalOnBroadcast: true,
        ShouldBroadcast: false);

    public static void Register()
    {
        RitsuLibSidecarSyncMessages.Register(Descriptor);
    }

    public static async Task<IReadOnlyList<SerializableCard>> GetAuthoritativeCandidates(
        PlayerChoiceContext choiceContext,
        Player owner,
        Func<IReadOnlyList<SerializableCard>> createLocalCandidates)
    {
        var runManager = RunManager.Instance;
        if (runManager.NetService.Type is NetGameType.Singleplayer or NetGameType.Replay)
            return createLocalCandidates();

        if (choiceContext is not GameActionPlayerChoiceContext actionContext ||
            actionContext.Action.Id is not { } actionId)
        {
            throw new InvalidOperationException(
                "Electronic Sheep requires a synchronized game action id in multiplayer.");
        }

        var key = new CandidateKey(owner.NetId, actionId, owner.RunState.RunLocation);
        var waiter = GetOrCreateWaiter(key);

        if (LocalContext.IsMe(owner))
        {
            var candidates = createLocalCandidates();
            var message = new CandidateMessage(owner.NetId, actionId, candidates);
            var sent = runManager.NetService.Type == NetGameType.Host
                ? RitsuLibSidecarSyncMessages.Broadcast(runManager, Descriptor, message)
                : RitsuLibSidecarSyncMessages.SendToHost(runManager, Descriptor, message);

            if (!sent)
            {
                var exception = new InvalidOperationException(
                    "Failed to synchronize Electronic Sheep candidates with every peer.");
                Entry.Log.Error(exception.Message);
                lock (Gate)
                {
                    Waiters.Remove(key);
                    EarlyMessages.Remove(key);
                }
                throw exception;
            }
        }

        try
        {
            return await waiter.Task.WaitAsync(SyncTimeout);
        }
        catch (TimeoutException exception)
        {
            Entry.Log.Error(
                $"Timed out synchronizing Electronic Sheep candidates for action {actionId}.");
            throw new TimeoutException(
                $"Timed out synchronizing Electronic Sheep candidates for action {actionId}.",
                exception);
        }
        finally
        {
            lock (Gate)
            {
                Waiters.Remove(key);
                EarlyMessages.Remove(key);
            }
        }
    }

    private static Task Handle(RitsuLibSidecarSyncMessageContext<CandidateMessage> context)
    {
        var message = context.Message;
        if (context.Location is not { } location || context.NetService is null)
        {
            Entry.Log.Error(
                $"Rejected invalid Electronic Sheep candidate message from {context.SenderNetId} " +
                $"for owner {message.OwnerNetId}.");
            return Task.CompletedTask;
        }

        if (context.NetService.Type == NetGameType.Host &&
            context.IsHostIngest &&
            context.SenderNetId != context.NetService.NetId)
        {
            if (context.SenderNetId != message.OwnerNetId)
            {
                Entry.Log.Error(
                    $"Rejected Electronic Sheep candidates sent by {context.SenderNetId} " +
                    $"for owner {message.OwnerNetId}.");
                return Task.CompletedTask;
            }

            // Re-broadcast from the host (including the originating client) as an acknowledgement.
            // Nobody consumes shared RNG until this required broadcast has reached every ready peer.
            if (!RitsuLibSidecarSyncMessages.Broadcast(context.NetService, Descriptor, message))
            {
                Entry.Log.Error(
                    $"Host failed to broadcast Electronic Sheep candidates for action {message.ActionId}.");
            }
            return Task.CompletedTask;
        }

        var sentByHost = context.NetService switch
        {
            NetHostGameService host => context.SenderNetId == host.NetId,
            NetClientGameService client => context.SenderNetId == client.HostNetId,
            _ => false,
        };
        if (!sentByHost)
        {
            Entry.Log.Error(
                $"Rejected non-host Electronic Sheep broadcast from {context.SenderNetId}.");
            return Task.CompletedTask;
        }

        Accept(new CandidateKey(message.OwnerNetId, message.ActionId, location), message.Candidates);
        return Task.CompletedTask;
    }

    private static TaskCompletionSource<IReadOnlyList<SerializableCard>> GetOrCreateWaiter(CandidateKey key)
    {
        lock (Gate)
        {
            if (Waiters.TryGetValue(key, out var existing))
                return existing;

            var waiter = new TaskCompletionSource<IReadOnlyList<SerializableCard>>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            Waiters[key] = waiter;
            if (EarlyMessages.Remove(key, out var candidates))
                waiter.TrySetResult(candidates);
            return waiter;
        }
    }

    private static void Accept(CandidateKey key, IReadOnlyList<SerializableCard> candidates)
    {
        lock (Gate)
        {
            if (Waiters.TryGetValue(key, out var waiter))
                waiter.TrySetResult(candidates);
            else
                EarlyMessages[key] = candidates;
        }
    }

    private static byte[] Serialize(CandidateMessage message)
    {
        if (message.Candidates.Count > MaxCandidateCount)
            throw new InvalidOperationException("Electronic Sheep candidate list is too large to synchronize.");

        var writer = new PacketWriter { WarnOnGrow = false };
        writer.WriteULong(message.OwnerNetId);
        writer.WriteUInt(message.ActionId);
        writer.WriteUInt((uint)message.Candidates.Count, 16);
        foreach (var candidate in message.Candidates)
            writer.Write(candidate);
        writer.ZeroByteRemainder();
        return [.. writer.Buffer.AsSpan(0, writer.BytePosition)];
    }

    private static CandidateMessage Deserialize(ReadOnlySpan<byte> bytes)
    {
        var reader = new PacketReader();
        reader.Reset([.. bytes]);
        var ownerNetId = reader.ReadULong();
        var actionId = reader.ReadUInt();
        var count = (int)reader.ReadUInt(16);
        var candidates = new List<SerializableCard>(count);
        for (var index = 0; index < count; index++)
            candidates.Add(reader.Read<SerializableCard>());
        return new CandidateMessage(ownerNetId, actionId, candidates);
    }

    private readonly record struct CandidateKey(ulong OwnerNetId, uint ActionId, RunLocation Location);

    private sealed record CandidateMessage(
        ulong OwnerNetId,
        uint ActionId,
        IReadOnlyList<SerializableCard> Candidates);
}

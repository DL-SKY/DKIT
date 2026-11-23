using System.Collections.Generic;
using Modules.Space.Scripts.Implementation.Definitions;
using Modules.Space.Scripts.Implementation.Queues;
using Modules.State.Scripts.Actions.Core;
using Modules.State.Scripts.Actions.Models;

namespace Modules.Space.Scripts.Implementation.State.Actions
{
    /// <summary>
    /// Appends one timed row to a queue. Starts it when the queue has no running row.
    /// </summary>
    public sealed class EnqueueSpaceStateAction : StateActionBase<SpaceProfile>
    {
        private readonly SpaceDefinitionsManager _definitions;
        private readonly string _queueId;
        private readonly string _targetId;
        private readonly IReadOnlyList<SpaceTimeModifier> _modifiers;
        private readonly long _nowUnixMs;

        public EnqueueSpaceStateAction(
            SpaceDefinitionsManager definitions,
            string queueId,
            string targetId,
            IReadOnlyList<SpaceTimeModifier> modifiers,
            long nowUnixMs)
        {
            _definitions = definitions;
            _queueId = queueId;
            _targetId = targetId;
            _modifiers = modifiers;
            _nowUnixMs = nowUnixMs;
        }

        public override StateChangeSource Source => StateChangeSource.Space;

        public override StateActionValidationResult Validate(SpaceProfile state)
        {
            if (state == null)
                return StateActionValidationResult.Fail("Space profile is not loaded.", 931);

            if (_definitions == null || !SpaceProfileQueues.TryGet(state, _queueId, out var queue))
                return StateActionValidationResult.Fail("Queue was not found.", 932);

            if (queue.Rows != null && queue.Rows.Count >= queue.MaxCount)
                return StateActionValidationResult.Fail("Queue is full.", 933);

            if (!SpaceQueueIds.TryGetKind(_queueId, out var queueKind))
                return StateActionValidationResult.Fail("Queue kind is unknown.", 934);

            if (!_definitions.TryGetTimedTarget(_targetId, out var target) || target.Kind != queueKind)
                return StateActionValidationResult.Fail("Timed target does not belong to this queue.", 935);

            return StateActionValidationResult.Ok;
        }

        public override void Execute(SpaceProfile state)
        {
            SpaceProfileQueues.TryGet(state, _queueId, out var queue);
            SpaceQueueIds.TryGetKind(_queueId, out _);
            _definitions.TryGetTimedTarget(_targetId, out var target);
            if (queue.Rows == null)
                queue.Rows = new List<SpaceQueueRow>();

            var row = new SpaceQueueRow
            {
                Kind = target.Kind,
                TargetId = target.Id,
                BaseTimeMs = target.BaseTimeMs,
                Modifiers = CopyModifiers(_modifiers)
            };

            var hasActiveRow = false;
            for (var i = 0; i < queue.Rows.Count; i++)
            {
                if (queue.Rows[i] != null && queue.Rows[i].IsStarted)
                {
                    hasActiveRow = true;
                    break;
                }
            }

            if (!hasActiveRow)
                row.Start(_nowUnixMs);

            queue.Rows.Add(row);
        }

        private static List<SpaceTimeModifier> CopyModifiers(IReadOnlyList<SpaceTimeModifier> modifiers)
        {
            var copy = new List<SpaceTimeModifier>();
            if (modifiers == null)
                return copy;

            for (var i = 0; i < modifiers.Count; i++)
            {
                var modifier = modifiers[i];
                if (modifier == null)
                    continue;

                copy.Add(new SpaceTimeModifier
                {
                    Kind = modifier.Kind,
                    Value = modifier.Value
                });
            }

            return copy;
        }
    }
}

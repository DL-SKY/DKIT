using Modules.State.Scripts.Actions.Core;
using Modules.State.Scripts.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using System.Collections.Generic;

namespace Modules.State.Scripts.Implementation.Adventure.Actions
{
    public class AddInventoryItemsStateAction : StateActionBase<StateData>
    {
        public override StateChangeSource Source => StateChangeSource.AddInventoryItems;

        private readonly AddInventoryItemsRequestData _request;

        public AddInventoryItemsStateAction(AddInventoryItemsRequestData request)
        {
            _request = request;
        }

        public override StateActionValidationResult Validate(StateData state)
        {
            if (state?.Inventory == null)
                return StateActionValidationResult.Fail("Inventory state is null.", 120);

            if (_request == null)
                return StateActionValidationResult.Fail("Add inventory items request is null.", 121);

            if (string.IsNullOrWhiteSpace(_request.ItemId))
                return StateActionValidationResult.Fail("Item id is null or empty.", 122);

            if (_request.Count <= 0)
                return StateActionValidationResult.Fail("Item count must be greater than zero.", 123);

            return StateActionValidationResult.Ok;
        }

        public override void Execute(StateData state)
        {
            state.Inventory.Items ??= new Dictionary<string, int>();
            InventoryItemsOperator.TryAdd(state.Inventory.Items, _request.ItemId, _request.Count);
        }
    }
}

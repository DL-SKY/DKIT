using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using System;

namespace Modules.State.Scripts.Implementation.Adventure.Actions.Models
{
    /// <summary>
    /// Input payload for character creation state-action.
    /// This model is shared by manual creator flow and pre-generated character flow.
    /// </summary>
    public class CreateCharacterRequestData
    {
        public event Action OnUpdate;

        public string Avatar;
        public string Name;
        public CharacterGender Gender;

        public string Ancestry;
        public string Class;
        public string Background;

        public CharacterRequestData CharacterData;

        /// <summary>
        /// When true, the created character id is appended to ActivePartyCharacterIds.
        /// </summary>
        public bool AddToActiveParty;

        /// <summary>
        /// Raises <see cref="OnUpdate"/> after the draft is consistent.
        /// Intended caller: <c>CreateCharacterRequestApplicator</c> after a finished write, not mid-rebuild.
        /// </summary>
        public void NotifyUpdated()
        {
            OnUpdate?.Invoke();
        }
    }
}

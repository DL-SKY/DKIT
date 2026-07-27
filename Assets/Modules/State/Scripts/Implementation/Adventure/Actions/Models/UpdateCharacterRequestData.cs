using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs;

namespace Modules.State.Scripts.Implementation.Adventure.Actions.Models
{
    /// <summary>
    /// Входные данные для <c>UpdateCharacterStateAction</c>.
    /// </summary>
    /// <remarks>
    /// Контракт: только прокачка персонажа (достижение нового уровня), не произвольное редактирование.
    /// UI прокачки делает слепок через <see cref="CharacterParametersOperator.CreateCharacterRequestSnapshot"/>,
    /// затем последовательно применяет feats / патчи нового уровня (методы Apply* / Unapply* ниже).
    /// Итоговый mutable-блок уходит в <see cref="CharacterData"/>; экшен персистит слепок wholesale.
    /// </remarks>
    public class UpdateCharacterRequestData
    {
        public int CharacterId;
        public CharacterRequestData CharacterData;

        public void ApplyFeat(string featId, DefinitionsManager definitionsManager)
        {
            EnsureCharacterData().ApplyFeat(featId, definitionsManager);
        }

        public void UnapplyFeat(string featId, DefinitionsManager definitionsManager)
        {
            EnsureCharacterData().UnapplyFeat(featId, definitionsManager);
        }

        public void ApplyPatch(CharacterParamsPatchData patch, DefinitionsManager definitionsManager)
        {
            EnsureCharacterData().ApplyPatch(patch, definitionsManager);
        }

        public void UnapplyPatch(CharacterParamsPatchData patch, DefinitionsManager definitionsManager)
        {
            EnsureCharacterData().UnapplyPatch(patch, definitionsManager);
        }

        private CharacterRequestData EnsureCharacterData()
        {
            CharacterData ??= new CharacterRequestData();
            return CharacterData;
        }
    }
}

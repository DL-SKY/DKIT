using Modules.Definitions.Scripts.Implementation.Adventures;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Ancestries;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Backgrounds;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Classes;
using Modules.Definitions.Scripts.Implementation.Adventures.Defs.Feats;
using Modules.State.Scripts.Implementation.Adventure.Actions.Models;
using Modules.State.Scripts.Implementation.Adventure.StateDatas;
using Modules.Windows.Scripts.Implementation.Adventure.Characters.CursorCreateCharacter;
using System.Collections.Generic;

namespace Modules.Definitions.Scripts.Editor.Adventures
{
    public sealed class AdventureCharacterDerivedDataService
    {
        private const string ANCESTRIES_DIRECTORY = "Assets/Modules/Definitions/Resources/Definitions/_ADVENTURES_/Ancestries";
        private const string CLASSES_DIRECTORY = "Assets/Modules/Definitions/Resources/Definitions/_ADVENTURES_/Classes";
        private const string BACKGROUNDS_DIRECTORY = "Assets/Modules/Definitions/Resources/Definitions/_ADVENTURES_/Backgrounds";
        private const string FEATS_DIRECTORY = "Assets/Modules/Definitions/Resources/Definitions/_ADVENTURES_/Feats";

        private readonly AdventureDefinitionEditorRepository _repository;

        public AdventureCharacterDerivedDataService(AdventureDefinitionEditorRepository repository)
        {
            _repository = repository ?? new AdventureDefinitionEditorRepository();
        }

        public CharacterRequestData BuildFromLinkedDefinitions(
            string ancestryId,
            string classId,
            string backgroundId,
            CharacterGender gender)
        {
            var definitionsManager = new DefinitionsManager
            {
                Ancestries = _repository.LoadAllDefinitions<AncestryDef>(ANCESTRIES_DIRECTORY),
                Classes = _repository.LoadAllDefinitions<ClassDef>(CLASSES_DIRECTORY),
                Backgrounds = _repository.LoadAllDefinitions<BackgroundDef>(BACKGROUNDS_DIRECTORY),
                Feats = _repository.LoadAllDefinitions<FeatDef>(FEATS_DIRECTORY),
            };

            var request = new CreateCharacterRequestData
            {
                Gender = gender,
                Ancestry = ancestryId ?? string.Empty,
                Class = classId ?? string.Empty,
                Background = backgroundId ?? string.Empty,
                CharacterData = new CharacterRequestData
                {
                    Parameters = new Dictionary<string, int>(),
                    EquippedItems = new List<EquippedItemStateData>(),
                    Spells = new Dictionary<string, int>(),
                    StatusEffects = new Dictionary<string, int>(),
                },
            };

            CursorCreateCharacterDraftApplier.RebuildFromSelections(request, definitionsManager);
            request.CharacterData ??= new CharacterRequestData();
            request.CharacterData.Parameters ??= new Dictionary<string, int>();
            request.CharacterData.EquippedItems ??= new List<EquippedItemStateData>();
            request.CharacterData.Spells ??= new Dictionary<string, int>();
            request.CharacterData.StatusEffects ??= new Dictionary<string, int>();
            return request.CharacterData;
        }
    }
}

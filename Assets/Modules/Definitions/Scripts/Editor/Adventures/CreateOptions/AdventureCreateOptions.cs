using Modules.RPG.Scripts.Adventure.Choice;
using Modules.RPG.Scripts.Adventure.Choice.Actions;
using Modules.RPG.Scripts.Adventure.Data;
using System.Collections.Generic;
using static Modules.Definitions.Scripts.Implementation.Adventures.Constants.Glossary;

namespace Modules.Definitions.Scripts.Editor.Adventures.CreateOptions
{
    public interface ICreateOptionsRegistry<T>
    {
        IReadOnlyList<CreateOptionDescriptor<T>> GetOptions();
    }

    public sealed class AdventureCreateOptionsRegistry : ICreateOptionsRegistry<AdventureData>
    {
        private readonly List<CreateOptionDescriptor<AdventureData>> _options;

        public AdventureCreateOptionsRegistry()
        {
            _options = new List<CreateOptionDescriptor<AdventureData>>
            {
                new CreateOptionDescriptor<AdventureData>
                {
                    Id = "adventure.default",
                    ButtonText = "Adventure",
                    Tooltip = "Create a standard adventure template with one start scene.",
                    IconName = "SceneAsset Icon",
                    IconAssetName = "path-distance",
                    Create = () => BuildAdventureTemplate(AdventureType.Adventure),
                },
                new CreateOptionDescriptor<AdventureData>
                {
                    Id = "adventure.location",
                    ButtonText = "Location",
                    Tooltip = "Create a location template with one start scene.",
                    IconName = "Prefab Icon",
                    IconAssetName = "wireframe-globe",
                    Create = () => BuildAdventureTemplate(AdventureType.Location),
                },
            };
        }

        public IReadOnlyList<CreateOptionDescriptor<AdventureData>> GetOptions()
        {
            return _options;
        }

        private static AdventureData BuildAdventureTemplate(AdventureType type)
        {
            const string START_SCENE_ID = "start";

            SceneData startScene = new SceneData
            {
                Id = START_SCENE_ID,
                Tags = new List<string>(),
                Content = new List<SceneContentData>
                {
                    new SceneContentData
                    {
                        Type = SceneContentType.Text,
                        Restrictions = new List<Modules.Restrictions.Scripts.Core.Restriction>(),
                        Value = string.Empty,
                    },
                },
                Choices = new List<ChoiceData>(),
            };

            return new AdventureData
            {
                Disabled = false,
                Tags = new List<string>(),
                IgnoredTags = new List<string>(),
                IsRepeatable = type == AdventureType.Location,
                Type = type,
                AdventureLinks = new List<string>(),
                Title = string.Empty,
                Description = string.Empty,
                Restrictions = new List<Modules.Restrictions.Scripts.Core.Restriction>(),
                StartScenes = new List<string> { START_SCENE_ID },
                Scenes = new Dictionary<string, SceneData>
                {
                    { START_SCENE_ID, startScene },
                },
            };
        }
    }

    public sealed class SceneCreateOptionsRegistry : ICreateOptionsRegistry<SceneData>
    {
        private readonly List<CreateOptionDescriptor<SceneData>> _options;

        public SceneCreateOptionsRegistry()
        {
            _options = new List<CreateOptionDescriptor<SceneData>>
            {
                new CreateOptionDescriptor<SceneData>
                {
                    Id = "scene.empty",
                    ButtonText = "Empty Scene",
                    Tooltip = "Create an empty scene with no content and choices.",
                    IconName = "SceneAsset Icon",
                    Create = () => new SceneData
                    {
                        Id = "new_scene",
                        Tags = new List<string>(),
                        Content = new List<SceneContentData>(),
                        Choices = new List<ChoiceData>(),
                    },
                },
                new CreateOptionDescriptor<SceneData>
                {
                    Id = "scene.text",
                    ButtonText = "Text Scene",
                    Tooltip = "Create a scene with one text content block.",
                    IconName = "TextAsset Icon",
                    Create = () => new SceneData
                    {
                        Id = "new_scene",
                        Tags = new List<string>(),
                        Content = new List<SceneContentData>
                        {
                            new SceneContentData
                            {
                                Type = SceneContentType.Text,
                                Restrictions = new List<Modules.Restrictions.Scripts.Core.Restriction>(),
                                Value = string.Empty,
                            },
                        },
                        Choices = new List<ChoiceData>(),
                    },
                },
            };
        }

        public IReadOnlyList<CreateOptionDescriptor<SceneData>> GetOptions()
        {
            return _options;
        }
    }

    public sealed class SceneContentCreateOptionsRegistry : ICreateOptionsRegistry<SceneContentData>
    {
        private readonly List<CreateOptionDescriptor<SceneContentData>> _options;

        public SceneContentCreateOptionsRegistry()
        {
            _options = new List<CreateOptionDescriptor<SceneContentData>>
            {
                BuildOption(SceneContentType.Text, "Text", "TextAsset Icon"),
                BuildOption(SceneContentType.Image, "Image", "d_Image Icon"),
                BuildOption(SceneContentType.RandomImage, "Random Image", "d_Refresh", "perspective-dice-six-faces-random"),
                BuildOption(SceneContentType.Slideshow, "Slideshow", "Animation Icon"),
                BuildOption(SceneContentType.Splitter, "Splitter", "Toolbar Minus"),
                BuildOption(SceneContentType.Item, "Item", "PrefabVariant Icon"),
            };
        }

        public IReadOnlyList<CreateOptionDescriptor<SceneContentData>> GetOptions()
        {
            return _options;
        }

        private static CreateOptionDescriptor<SceneContentData> BuildOption(
            SceneContentType type,
            string label,
            string iconName,
            string iconAssetName = null)
        {
            return new CreateOptionDescriptor<SceneContentData>
            {
                Id = $"content.{type}",
                ButtonText = label,
                Tooltip = $"Add '{type}' content block.",
                IconName = iconName,
                IconAssetName = iconAssetName,
                Create = () => CreateSceneContentTemplate(type),
            };
        }

        private static SceneContentData CreateSceneContentTemplate(SceneContentType type)
        {
            SceneContentData contentData = new SceneContentData
            {
                Type = type,
                Restrictions = new List<Modules.Restrictions.Scripts.Core.Restriction>(),
            };

            if (type == SceneContentType.RandomImage || type == SceneContentType.Slideshow)
                contentData.Values = new List<string>();
            else
                contentData.Value = string.Empty;

            return contentData;
        }
    }

    public sealed class ChoiceCreateOptionsRegistry : ICreateOptionsRegistry<ChoiceData>
    {
        private readonly List<CreateOptionDescriptor<ChoiceData>> _options;

        public ChoiceCreateOptionsRegistry()
        {
            _options = new List<CreateOptionDescriptor<ChoiceData>>
            {
                new CreateOptionDescriptor<ChoiceData>
                {
                    Id = "choice.default",
                    ButtonText = "Choice",
                    Tooltip = "Add a default choice with no actions.",
                    IconName = "d_FilterByLabel",
                    IconAssetName = "click",
                    Create = () => new ChoiceData
                    {
                        Id = "new_choice",
                        Tags = new List<string>(),
                        Type = ChoiceType.Default,
                        Text = string.Empty,
                        Description = string.Empty,
                        AlwaysShow = true,
                        Restrictions = new List<Modules.Restrictions.Scripts.Core.Restriction>(),
                        Actions = new List<ChoiceActionData>(),
                    },
                },
            };
        }

        public IReadOnlyList<CreateOptionDescriptor<ChoiceData>> GetOptions()
        {
            return _options;
        }
    }

    public sealed class ChoiceActionCreateOptionsRegistry : ICreateOptionsRegistry<ChoiceActionData>
    {
        private readonly List<CreateOptionDescriptor<ChoiceActionData>> _options;

        public ChoiceActionCreateOptionsRegistry()
        {
            _options = new List<CreateOptionDescriptor<ChoiceActionData>>
            {
                new CreateOptionDescriptor<ChoiceActionData>
                {
                    Id = "action.goto_scene",
                    ButtonText = "Go To Scene",
                    Tooltip = "Add action that moves player to another scene.",
                    IconName = "Animation.NextKey",
                    IconAssetName = "play-button",
                    Create = () => new ChoiceActionData
                    {
                        Type = ChoiceActionType.GoToScene,
                        Params = new ChoiceActionParamsData
                        {
                            Strings = new Dictionary<string, string>
                            {
                                { ChoiceActions.SCENE_ID, "start" },
                            },
                            Ints = new Dictionary<string, int>(),
                            Bools = new Dictionary<string, bool>(),
                        },
                    },
                },
                new CreateOptionDescriptor<ChoiceActionData>
                {
                    Id = "action.set_world_params",
                    ButtonText = "Set World Params",
                    Tooltip = "Set world-level adventure params from action Params.",
                    IconName = "d_winbtn_mac_max_h",
                    IconAssetName = "load(1)",
                    Create = () => new ChoiceActionData
                    {
                        Type = ChoiceActionType.SetWorldParams,
                        Params = new ChoiceActionParamsData
                        {
                            Strings = new Dictionary<string, string>
                            {
                                { "world.sample", "value" },
                            },
                            Ints = new Dictionary<string, int>(),
                            Bools = new Dictionary<string, bool>(),
                        },
                    },
                },
                new CreateOptionDescriptor<ChoiceActionData>
                {
                    Id = "action.set_adventure_params",
                    ButtonText = "Set Adventure Params",
                    Tooltip = "Set current adventure params from action Params.",
                    IconName = "d_winbtn_mac_max",
                    IconAssetName = "load",
                    Create = () => new ChoiceActionData
                    {
                        Type = ChoiceActionType.SetAdventureParams,
                        Params = new ChoiceActionParamsData
                        {
                            Strings = new Dictionary<string, string>(),
                            Ints = new Dictionary<string, int>
                            {
                                { "adventure.sample", 1 },
                            },
                            Bools = new Dictionary<string, bool>(),
                        },
                    },
                },
                new CreateOptionDescriptor<ChoiceActionData>
                {
                    Id = "action.set_global_params",
                    ButtonText = "Set Global Params",
                    Tooltip = "Set global params from action Params (persisted independently from adventure restarts).",
                    IconName = "d_winbtn_mac_min_h",
                    IconAssetName = "save.png",
                    Create = () => new ChoiceActionData
                    {
                        Type = ChoiceActionType.SetGlobalParams,
                        Params = new ChoiceActionParamsData
                        {
                            Strings = new Dictionary<string, string>(),
                            Ints = new Dictionary<string, int>
                            {
                                { "global.sample", 1 },
                            },
                            Bools = new Dictionary<string, bool>(),
                        },
                    },
                },
            };
        }

        public IReadOnlyList<CreateOptionDescriptor<ChoiceActionData>> GetOptions()
        {
            return _options;
        }
    }
}

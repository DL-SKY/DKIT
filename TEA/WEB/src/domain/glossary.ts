export const ChoiceActions = {
  SCENE_ID: "SceneId",
  ADVENTURE_ID: "AdventureId",
  WINDOW_ID: "WindowId",
  PARAMETER_KEY: "ParameterKey",
  PARAMETER_VALUE: "ParameterValue",
  PARAMETER_DELTA: "ParameterDelta",
  SCENE_IDS_SEPARATOR: ";",
} as const;

export const Windows = {
  CREATE_CHARACTER: "CreateCharacter",
  SELECT_CHARACTER: "SelectCharacter",
  TRADE: "Trade",
  PARTY: "Party",
  ADVENTURE_LIST: "AdventureList",
} as const;

export const Characters = {
  LEVEL: "Level",
  EXPERIENCE: "Experience",
  BOOST_POINTS: "BoostPoints",
  STR: "STR",
  DEX: "DEX",
  CON: "CON",
  INT: "INT",
  WIS: "WIS",
  CHA: "CHA",
  PERCEPTION: "Perception",
  THIEVERY: "Thievery",
} as const;

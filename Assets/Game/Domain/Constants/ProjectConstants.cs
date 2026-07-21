namespace Lumbre.Game.Domain.Constants
{
    public static class ProjectConstants
    {
        public const string ProductName = "Lumbre de Nácar";
        public const string BootstrapScenePath = "Assets/Game/Scenes/Bootstrap.unity";
        public const string VerticalSliceScenePath = "Assets/Game/Scenes/VerticalSlice.unity";

        public const int GreyboxGridWidth = 32;
        public const int GreyboxGridHeight = 20;
        public const float IsometricTileWidth = 1.2f;
        public const float IsometricTileHeight = 0.6f;

        public const int PlazaMinX = 1;
        public const int PlazaMaxX = 11;
        public const int PlazaMinY = 5;
        public const int PlazaMaxY = 15;

        public const int TrailMinX = 10;
        public const int TrailMaxX = 22;
        public const int TrailMinY = 8;
        public const int TrailMaxY = 10;

        public const int CaveMinX = 22;
        public const int CaveMaxX = 30;
        public const int CaveMinY = 5;
        public const int CaveMaxY = 15;

        public const int NavigationStartX = 3;
        public const int NavigationStartY = 10;
        public const int NavigationGoalX = 28;
        public const int NavigationGoalY = 10;
        public const int NavigationNodeBudget = GreyboxGridWidth * GreyboxGridHeight;

        public const int PlazaObstacleMinX = 7;
        public const int PlazaObstacleMaxX = 8;
        public const int PlazaObstacleMinY = 7;
        public const int PlazaObstacleMaxY = 8;

        public const int CaveObstacleMinX = 25;
        public const int CaveObstacleMaxX = 26;
        public const int CaveObstacleMinY = 10;
        public const int CaveObstacleMaxY = 10;

        public const int MaxGreyboxVisualMeshes = 3;
        public const int MaxGreyboxLineRenderers = 1;

        public const int PlayerCombatMaxHealth = 100;
        public const int PlayerBasicAttackDamage = 20;
        public const float PlayerBasicAttackCooldown = 0.6f;
        public const float PlayerBasicAttackRange = 1.45f;

        public const int MordeluzMaxHealth = 60;
        public const int MordeluzBasicAttackDamage = 10;
        public const float MordeluzBasicAttackCooldown = 1f;
        public const float MordeluzDetectionRange = 4.5f;
        public const float MordeluzAttackRange = 1.05f;
        public const float MordeluzLeashRange = 6.5f;
        public const float MordeluzMovementSpeed = 1.2f;

        public const int PlayerHeatMax = 100;
        public const int PlayerHeatInitial = 0;
        public const int PlayerHeatPerBasicAttack = 25;
        public const float PlayerDefenseDuration = 1.5f;
        public const float PlayerDefenseCooldown = 4f;
        public const float PlayerDefenseDamageReduction = 0.65f;
        public const int PlayerAreaAttackHeatCost = 50;
        public const int PlayerAreaAttackDamage = 45;
        public const float PlayerAreaAttackRadius = 2.2f;
        public const float PlayerAreaAttackCooldown = 2.5f;

        public const int MordeluzResonanteMaxHealth = 120;
        public const int MordeluzResonanteWaveDamage = 18;
        public const float MordeluzResonanteWaveRadius = 1.8f;
        public const float MordeluzResonanteWaveWindup = 0.8f;
        public const float MordeluzResonanteWaveCooldown = 2.4f;
        public const int H4BCommonEnemyCount = 3;
        public const int H4BCommonEnemyGridX = 8;
        public const int H4BCommonEnemyGridY = 13;
        public const int H4BResonantGridX = 27;
        public const int H4BResonantGridY = 13;

        public const int H5InventoryCapacity = 6;
        public const int H5NaraGridX = 4;
        public const int H5NaraGridY = 10;
        public const float H5InteractionRange = 1.65f;

        public const int H6StartingLevel = 1;
        public const int H6MaximumLevel = 2;
        public const int H6ExperienceToLevelTwo = 100;
        public const int MordeluzExperience = 10;
        public const int MordeluzResonanteExperience = 30;
        public const int PlayerMissionExperience = 40;
    }
}

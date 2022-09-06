using System;
using UnityEngine;
using Settings;

namespace GameProgress
{
    class GameProgressContainer: SaveableSettingsContainer
    {
        protected override string FolderPath { get { return Application.dataPath + "/UserData/GameProgress"; } }
        protected override string FileName { get { return "GameProgress"; } }
        protected override bool Encrypted => true;

        public AchievementContainer Achievement = new AchievementContainer();
        public QuestContainer Quest = new QuestContainer();
        public GameStatContainer GameStat = new GameStatContainer();

        // backwards compatibility for misnamed variable
        public AchievementContainer Achievment = new AchievementContainer();

        public override void Save()
        {
            Quest.CollectRewards();
            base.Save();
        }

        // backwards compatibility
        public override void Load()
        {
            base.Load();
            if (Achievement.AchievementItems.GetCount() == 0)
                Achievement.AchievementItems.Copy(Achievment.AchievmentItems);
        }
    }
}

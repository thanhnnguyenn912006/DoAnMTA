using System.Reflection;
using NUnit.Framework;
using SurvivorGame.Player;
using SurvivorGame.Systems;
using UnityEngine;

namespace SurvivorGame.Tests.EditMode
{
    public class SurvivorCoreEditModeTests
    {
        [Test]
        public void PlayerExperience_AddExpAcrossMultipleLevels_IncrementsLevelAndKeepsRemainder()
        {
            GameObject go = new GameObject("PlayerExpTest");
            try
            {
                PlayerExperience exp = go.AddComponent<PlayerExperience>();
                MethodInfo awakeMethod = typeof(PlayerExperience).GetMethod(
                    "Awake",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                awakeMethod?.Invoke(exp, null);

                int lv1Required = exp.ExpToNextLevel;
                Assert.Greater(lv1Required, 0);

                // Cộng dư đủ để lên ít nhất 2 cấp
                exp.AddExp(lv1Required + 45 + 10);

                Assert.GreaterOrEqual(exp.CurrentLevel, 2);
                Assert.GreaterOrEqual(exp.CurrentExp, 0);
                Assert.Less(exp.CurrentExp, exp.ExpToNextLevel);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GameManager_FormatTime_FormatsMinutesAndSecondsWithLeadingZeros()
        {
            Assert.AreEqual("00:00", GameManager.FormatTime(0f));
            Assert.AreEqual("00:09", GameManager.FormatTime(9.8f));
            Assert.AreEqual("02:30", GameManager.FormatTime(150f));
        }

        [Test]
        public void PlayerExperience_IncreaseMagnetRadius_AddsPositiveBonus()
        {
            GameObject go = new GameObject("PlayerMagnetTest");
            try
            {
                PlayerExperience exp = go.AddComponent<PlayerExperience>();
                MethodInfo awakeMethod = typeof(PlayerExperience).GetMethod(
                    "Awake",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                awakeMethod?.Invoke(exp, null);

                float initialRadius = exp.MagnetRadius;
                exp.IncreaseMagnetRadius(1.5f);

                Assert.AreEqual(initialRadius + 1.5f, exp.MagnetRadius, 0.0001f);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}

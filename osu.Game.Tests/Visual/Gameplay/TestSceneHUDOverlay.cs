// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Skinning;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneHUDOverlay : OsuManualInputManagerTestScene
    {
        private HUDOverlay hudOverlay;

        [Cached]
        private ScoreProcessor scoreProcessor = new ScoreProcessor();

        [Cached(typeof(HealthProcessor))]
        private HealthProcessor healthProcessor = new DrainingHealthProcessor(0);

        // best way to check without exposing.
        private Drawable hideTarget => hudOverlay.KeyCounter;
        private FillFlowContainer<KeyCounter> keyCounterFlow => hudOverlay.KeyCounter.ChildrenOfType<FillFlowContainer<KeyCounter>>().First();

        [Resolved]
        private OsuConfigManager config { get; set; }

        [Test]
        public void TestComboCounterIncrementing()
        {
            createNew();

            AddRepeatStep("increase combo", () => { scoreProcessor.Combo.Value++; }, 10);

            AddStep("reset combo", () => { scoreProcessor.Combo.Value = 0; });
        }

        [Test]
        public void TestShownByDefault()
        {
            createNew();

            AddAssert("showhud is set", () => hudOverlay.ShowHud.Value);

            AddAssert("hidetarget is visible", () => hideTarget.IsPresent);
            AddAssert("key counter flow is visible", () => keyCounterFlow.IsPresent);
            AddAssert("pause button is visible", () => hudOverlay.HoldToQuit.IsPresent);
        }

        [Test]
        public void TestFadesInOnLoadComplete()
        {
            float? initialAlpha = null;

            createNew(h => h.OnLoadComplete += _ => initialAlpha = hideTarget.Alpha);
            AddUntilStep("wait for load", () => hudOverlay.IsAlive);
            AddAssert("initial alpha was less than 1", () => initialAlpha < 1);
        }

        [Test]
        public void TestHideExternally()
        {
            createNew();

            AddStep("set showhud false", () => hudOverlay.ShowHud.Value = false);

            AddUntilStep("hidetarget is hidden", () => !hideTarget.IsPresent);
            AddAssert("pause button is still visible", () => hudOverlay.HoldToQuit.IsPresent);

            // Key counter flow container should not be affected by this, only the key counter display will be hidden as checked above.
            AddAssert("key counter flow not affected", () => keyCounterFlow.IsPresent);
        }

        [Test]
        public void TestMomentaryShowHUD()
        {
            createNew();

            HUDVisibilityMode originalConfigValue = HUDVisibilityMode.HideDuringGameplay;

            AddStep("get original config value", () => originalConfigValue = config.Get<HUDVisibilityMode>(OsuSetting.HUDVisibilityMode));

            AddStep("set hud to never show", () => config.SetValue(OsuSetting.HUDVisibilityMode, HUDVisibilityMode.Never));

            AddUntilStep("wait for fade", () => !hideTarget.IsPresent);

            AddStep("trigger momentary show", () => InputManager.PressKey(Key.ControlLeft));
            AddUntilStep("wait for visible", () => hideTarget.IsPresent);

            AddStep("stop trigering", () => InputManager.ReleaseKey(Key.ControlLeft));
            AddUntilStep("wait for fade", () => !hideTarget.IsPresent);

            AddStep("set original config value", () => config.SetValue(OsuSetting.HUDVisibilityMode, originalConfigValue));
        }

        [Test]
        public void TestExternalHideDoesntAffectConfig()
        {
            HUDVisibilityMode originalConfigValue = HUDVisibilityMode.HideDuringGameplay;

            createNew();

            AddStep("get original config value", () => originalConfigValue = config.Get<HUDVisibilityMode>(OsuSetting.HUDVisibilityMode));

            AddStep("set showhud false", () => hudOverlay.ShowHud.Value = false);
            AddAssert("config unchanged", () => originalConfigValue == config.Get<HUDVisibilityMode>(OsuSetting.HUDVisibilityMode));

            AddStep("set showhud true", () => hudOverlay.ShowHud.Value = true);
            AddAssert("config unchanged", () => originalConfigValue == config.Get<HUDVisibilityMode>(OsuSetting.HUDVisibilityMode));
        }

        [Test]
        public void TestChangeHUDVisibilityOnHiddenKeyCounter()
        {
            bool keyCounterVisibleValue = false;

            createNew();
            AddStep("save keycounter visible value", () => keyCounterVisibleValue = config.Get<bool>(OsuSetting.KeyOverlay));

            AddStep("set keycounter visible false", () =>
            {
                config.SetValue(OsuSetting.KeyOverlay, false);
                hudOverlay.KeyCounter.AlwaysVisible.Value = false;
            });

            AddStep("set showhud false", () => hudOverlay.ShowHud.Value = false);
            AddUntilStep("hidetarget is hidden", () => !hideTarget.IsPresent);
            AddAssert("key counters hidden", () => !keyCounterFlow.IsPresent);

            AddStep("set showhud true", () => hudOverlay.ShowHud.Value = true);
            AddUntilStep("hidetarget is visible", () => hideTarget.IsPresent);
            AddAssert("key counters still hidden", () => !keyCounterFlow.IsPresent);

            AddStep("return value", () => config.SetValue(OsuSetting.KeyOverlay, keyCounterVisibleValue));
        }

        [Test]
        public void TestHiddenHUDDoesntBlockSkinnableComponentsLoad()
        {
            HUDVisibilityMode originalConfigValue = default;

            AddStep("get original config value", () => originalConfigValue = config.Get<HUDVisibilityMode>(OsuSetting.HUDVisibilityMode));

            AddStep("set hud to never show", () => config.SetValue(OsuSetting.HUDVisibilityMode, HUDVisibilityMode.Never));

            createNew();
            AddUntilStep("wait for hud load", () => hudOverlay.IsLoaded);
            AddUntilStep("skinnable components loaded", () => hudOverlay.ChildrenOfType<SkinnableTargetContainer>().Single().ComponentsLoaded);

            AddStep("set original config value", () => config.SetValue(OsuSetting.HUDVisibilityMode, originalConfigValue));
        }

        private void createNew(Action<HUDOverlay> action = null)
        {
            AddStep("create overlay", () =>
            {
                hudOverlay = new HUDOverlay(null, Array.Empty<Mod>());

                // Add any key just to display the key counter visually.
                hudOverlay.KeyCounter.Add(new KeyCounterKeyboard(Key.Space));

                scoreProcessor.Combo.Value = 1;

                action?.Invoke(hudOverlay);

                Child = hudOverlay;
            });
        }
    }
}

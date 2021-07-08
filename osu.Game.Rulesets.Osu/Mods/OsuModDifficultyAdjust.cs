// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModDifficultyAdjust : ModDifficultyAdjust
    {
        [SettingSource("Circle Size", "Override a beatmap's set CS.", FIRST_SETTING_ORDER - 1)]
        public Bindable<float?> CircleSize { get; } = new Bindable<float?>
        {
            /*
            Precision = 0.1f,
            MinValue = 0,
            MaxValue = 10,
            */
        };

        [SettingSource("Approach Rate", "Override a beatmap's set AR.", LAST_SETTING_ORDER + 1)]
        public Bindable<float?> ApproachRate { get; } = new Bindable<float?>
        {
            /*
            Precision = 0.1f,
            MinValue = 0,
            MaxValue = 10,
            */
        };

        protected override void ApplyLimits(bool extended)
        {
            base.ApplyLimits(extended);

            // TODO: reimplement
            // CircleSize.MaxValue = extended ? 11 : 10;
            // ApproachRate.MaxValue = extended ? 11 : 10;
        }

        public override string SettingDescription
        {
            get
            {
                string circleSize = CircleSize.IsDefault ? string.Empty : $"CS {CircleSize.Value:N1}";
                string approachRate = ApproachRate.IsDefault ? string.Empty : $"AR {ApproachRate.Value:N1}";

                return string.Join(", ", new[]
                {
                    circleSize,
                    base.SettingDescription,
                    approachRate
                }.Where(s => !string.IsNullOrEmpty(s)));
            }
        }

        protected override void ApplySettings(BeatmapDifficulty difficulty)
        {
            base.ApplySettings(difficulty);

            if (CircleSize.Value != null) difficulty.CircleSize = CircleSize.Value.Value;
            if (ApproachRate.Value != null) difficulty.ApproachRate = ApproachRate.Value.Value;
        }
    }
}

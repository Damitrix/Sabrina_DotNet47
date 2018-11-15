using System;

namespace Sabrina.Models
{
    internal class SlaveReportsExtension
    {
        public enum Outcome
        {
            denial = 1,
            ruin = 2,
            orgasm = 4,
            task = 8
        }

        public enum WheelDifficultyPreference
        {
            Baby,
            Easy,
            Default,
            Hard,
            Masterbater
        }

        [Flags]
        public enum WheelTaskPreferenceSetting
        {
            Default,
            Task,
            Time,
            Amount
        }
    }
}
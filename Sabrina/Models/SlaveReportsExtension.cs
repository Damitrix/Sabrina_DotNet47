using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sabrina.Models
{
    class SlaveReportsExtension
    {
        public enum Outcome
        {
            denial = 1,
            ruin = 2,
            orgasm = 4,
            task = 8
        }

        [Flags]
        public enum WheelTaskPreferenceSetting
        {
            Default,
            Task,
            Time,
            Amount
        }

        public enum WheelDifficultyPreference
        {
            Baby,
            Easy,
            Default,
            Hard,
            Masterbater
        }
    }
}

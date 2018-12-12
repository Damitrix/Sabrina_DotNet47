using System;

public static class SlaveReportsExtension
{
    [Flags]
    public enum Outcome
    {
        Denial = 1,
        Ruin = 2,
        Orgasm = 4,
        Task = 8
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
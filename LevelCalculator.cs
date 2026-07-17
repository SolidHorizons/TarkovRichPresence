namespace TarkovRichPresence
{
    /// <summary>
    /// Converts total XP to a level using a lookup table.
    /// The underlying XP curve doesn't follow a single mathematical formula
    /// (it's tiered/hand-tuned), so this uses the actual cumulative thresholds
    /// with a binary search for O(log n) lookups.
    /// </summary>
    public static class LevelCalculator
    {
        // Index 0 = Level 1. Value = cumulative XP required to REACH that level.
        private static readonly long[] CumulativeXpThresholds =
        {
            0,          // Level 1
            1000,       // Level 2
            4017,       // Level 3
            8432,       // Level 4
            14256,      // Level 5
            21477,      // Level 6
            30023,      // Level 7
            39936,      // Level 8
            51204,      // Level 9
            63723,      // Level 10
            77563,      // Level 11
            93279,      // Level 12
            115302,     // Level 13
            143253,     // Level 14
            177337,     // Level 15
            217885,     // Level 16
            264432,     // Level 17
            316851,     // Level 18
            374400,     // Level 19
            437465,     // Level 20
            505161,     // Level 21
            577978,     // Level 22
            656347,     // Level 23
            741150,     // Level 24
            836066,     // Level 25
            944133,     // Level 26
            1066259,    // Level 27
            1199423,    // Level 28
            1343743,    // Level 29
            1499338,    // Level 30
            1666320,    // Level 31
            1846664,    // Level 32
            2043349,    // Level 33
            2258436,    // Level 34
            2492126,    // Level 35
            2750217,    // Level 36
            3032022,    // Level 37
            3337766,    // Level 38
            3663831,    // Level 39
            4010401,    // Level 40
            4377662,    // Level 41
            4765799,    // Level 42
            5182399,    // Level 43
            5627732,    // Level 44
            6102063,    // Level 45
            6630287,    // Level 46
            7189442,    // Level 47
            7779792,    // Level 48
            8401607,    // Level 49
            9055144,    // Level 50
            9740666,    // Level 51
            10458431,   // Level 52
            11219666,   // Level 53
            12024744,   // Level 54
            12874041,   // Level 55
            13767918,   // Level 56
            14706741,   // Level 57
            15690872,   // Level 58
            16720667,   // Level 59
            17816442,   // Level 60
            19041492,   // Level 61
            20360945,   // Level 62
            21792266,   // Level 63
            23350443,   // Level 64
            25098462,   // Level 65
            27100775,   // Level 66
            29581231,   // Level 67
            33028574,   // Level 68
            37953544,   // Level 69
            44260543,   // Level 70
            51901513,   // Level 71
            60887711,   // Level 72
            71228846,   // Level 73
            82933459,   // Level 74
            96009180,   // Level 75
            110462910,  // Level 76
            126300949,  // Level 77
            144924572,  // Level 78
            172016256,  // Level 79
        };

        /// <summary>Highest defined level in the table.</summary>
        public static int MaxLevel => CumulativeXpThresholds.Length;

        /// <summary>
        /// Returns the level for a given total XP amount.
        /// XP at or below 0 returns level 1. XP above the top threshold is capped at MaxLevel
        /// (the table doesn't extrapolate beyond level 79).
        /// </summary>
        public static int GetLevelForXp(long xp)
        {
            if (xp <= 0) return 1;

            int idx = Array.BinarySearch(CumulativeXpThresholds, xp);
            if (idx < 0)
            {
                // ~idx = index of first threshold greater than xp.
                // Subtract 1 to get the largest threshold that is <= xp.
                idx = ~idx - 1;
            }

            idx = Math.Max(idx, 0);
            return idx + 1; // convert 0-based array index to 1-based level
        }

        /// <summary>Cumulative XP required to reach a given level.</summary>
        public static long GetXpForLevel(int level)
        {
            if (level <= 1) return 0;
            if (level > MaxLevel) return CumulativeXpThresholds[MaxLevel - 1];
            return CumulativeXpThresholds[level - 1];
        }

        /// <summary>XP still needed to reach the next level. Returns 0 if already at MaxLevel.</summary>
        public static long GetXpToNextLevel(long xp)
        {
            int currentLevel = GetLevelForXp(xp);
            if (currentLevel >= MaxLevel) return 0;
            return GetXpForLevel(currentLevel + 1) - xp;
        }
    }
}
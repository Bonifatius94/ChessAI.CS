using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.GameLib
{
    /// <summary>
    /// An enumeration of chess game difficulties from random to godlike.
    /// </summary>
    public enum ChessDifficultyLevel
    {
        /// <summary>
        /// Representing a completely stupid playstyle (random draws).
        /// </summary>
        Random = 0,

        /// <summary>
        /// Representing a completely stupid playstyle (random draws).
        /// </summary>
        VeryStupid = 1,

        /// <summary>
        /// Representing a completely stupid playstyle (random draws).
        /// </summary>
        Stupid = 2,

        /// <summary>
        /// Representing a somewhat easy playstyle (taking one future draw in consideration).
        /// </summary>
        VeryEasy = 3,

        /// <summary>
        /// Representing a somewhat easy playstyle (taking one future draw in consideration).
        /// </summary>
        Easy = 4,

        /// <summary>
        /// Representing a beginner playstyle (taking two future draws in consideration).
        /// </summary>
        Medium = 5,

        /// <summary>
        /// Representing an experienced playstyle (taking three future draws in consideration).
        /// </summary>
        Hard = 6,

        /// <summary>
        /// Representing an experienced playstyle (taking three future draws in consideration).
        /// </summary>
        VeryHard = 7,

        /// <summary>
        /// Representing a very experienced playstyle (taking four future draws in consideration).
        /// </summary>
        Extreme = 8,

        /// <summary>
        /// Representing a master's playstyle (taking five future draws in consideration).
        /// </summary>
        Godlike = 9
    }
}

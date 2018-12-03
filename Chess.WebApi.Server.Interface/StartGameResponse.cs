using System;

namespace Chess.WebApi.Server.Interface
{
    /// <summary>
    /// Response data containing information for starting a new chess game.
    /// </summary>
    public class StartGameResponse
    {
        #region Members

        /// <summary>
        /// The id of the new game.
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        /// Indicates whether the player owns the white or the black chess pieces.
        /// </summary>
        public bool IsFirst { get; set; }

        #endregion Members
    }
}

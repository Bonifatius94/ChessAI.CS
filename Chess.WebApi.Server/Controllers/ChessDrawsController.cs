using Chess.Lib;
using Chess.WebApi.Server.Helpers;
using Chess.WebApi.Server.Interface;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Chess.WebApi.Server.Controllers
{
    /// <summary>
    /// Implements all HTTP operations of chess gameplay.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ChessDrawsController : ControllerBase
    {
        #region Methods

        // GET api/chessdraws
        /// <summary>
        /// Triggered by a client for starting a new game. Therefore the controller waits for a second player requesting a new game and matches them.
        /// </summary>
        /// <returns>a resonse message containing the game id and whether the player owns white or black chess pieces</returns>
        [HttpGet]
        public async Task<ActionResult<StartGameResponse>> RequestNewGame()
        {
            // TODO: implement match-making according to difficulty level
            ActionResult result;

            try
            {
                // find a game and return the response to start the game
                var response = await Task.Run(() => new MatchmakingDispatcher().FindGame());
                result = Ok(response);
            }
            catch (Exception /*ex*/)
            {
                result = BadRequest();
            }

            return result;
        }

        // PUT api/chessdraws/{id}
        /// <summary>
        /// Triggered by a client submitting his chess draw. 
        /// Therefore the draw is validated and sent to the opponent (if valid). Otherwise the player is asked to submit another draw that is valid.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="draw"></param>
        /// <returns>a HTTP response</returns>
        [HttpPut("{id}")]
        public ActionResult SubmitDraw(int id, [FromBody] ChessDraw draw)
        {
            // make sure the game with the given id exists
            if (!MatchmakingDispatcher.Matches.ContainsKey(id)) { throw new ArgumentException($"game with id { id } does not exist!"); }

            // submit the ches draw if valid
            var matchmaker = MatchmakingDispatcher.Matches[id];
            bool isValid = matchmaker.TrySubmitDraw(draw);

            // send a response whether the submitted draw could be applied
            var result = isValid ? Ok() as ActionResult : BadRequest();
            return result;
        }

        // GET api/chessdraws/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ChessDraw?>> RequestOpponentDraw(int id)
        {
            // make sure the game with the given id exists
            if (!MatchmakingDispatcher.Matches.ContainsKey(id)) { throw new ArgumentException($"game with id { id } does not exist!"); }

            // wait for an answer from the opponent
            var matchmaker = MatchmakingDispatcher.Matches[id];
            var answer = await matchmaker.WaitForAnswer();

            // return the opponent's answer
            var result = Ok(answer);
            return result;
        }

        #endregion Methods
    }
}

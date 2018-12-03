using Chess.Lib;
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

    /// <summary>
    /// Dispatch players looking to start a new game and store match data.
    /// </summary>
    public class MatchmakingDispatcher
    {
        #region Members

        // the game id counter
        private static int _maxId = 0;

        /// <summary>
        /// A dictionary of all matches that have been played.
        /// </summary>
        public static Dictionary<int, ChessMatchSession> Matches { get; } = new Dictionary<int, ChessMatchSession>();

        // some multi-threading synchronization tools
        private static Semaphore _semaphoreMatchmaking = new Semaphore(0, 2);
        private static Mutex _mutexGameCreation = new Mutex();
        private static Mutex _mutexIdIncrement = new Mutex();
        private static Barrier _barrierGameCreationSync = new Barrier(2);

        #endregion Members

        #region Methods

        /// <summary>
        /// Try to find a game.
        /// </summary>
        /// <returns>required data for starting the game</returns>
        public StartGameResponse FindGame()
        {
            StartGameResponse ret;

            // make sure only 2 players enter the game creation
            _semaphoreMatchmaking.WaitOne();

            // determine the game id (same id for both players)
            int gameId = _maxId + 1;

            // wait until both players have set the game id (this makes sure they have the same id)
            _barrierGameCreationSync.SignalAndWait();

            // make sure only the first of the two players creates the game
            _mutexGameCreation.WaitOne();
            bool isFirst = !Matches.ContainsKey(gameId);

            if (isFirst)
            {
                // first player's part: create a new game 
                Matches.Add(gameId, new ChessMatchSession(gameId));
                _maxId++;

                // now that the game is created, let the second player enter the section
                _mutexGameCreation.ReleaseMutex();
            }
            else
            {
                // let the next 2 players enter the game creation section
                _mutexGameCreation.ReleaseMutex();
                _semaphoreMatchmaking.Release(2);
            }

            // return the required response data
            ret = new StartGameResponse() { GameId = gameId, IsFirst = isFirst };
            return ret;
        }

        #endregion Methods
    }
    
    public class ChessMatchSession
    {
        #region Constructor

        public ChessMatchSession(int gameId)
        {
            _gameId = gameId;
        }

        #endregion Constructor

        #region Members

        public const int TIMEOUT = 300000;

        private int _gameId;
        private ChessGame _game = new ChessGame();
        private readonly Semaphore _semaporeWait = new Semaphore(0, 2);
        private bool _isWaiting = false;

        #endregion Members

        #region Methods

        public async Task<ChessDraw?> WaitForAnswer()
        {
            ChessDraw? answer = null;

            try
            {
                // wait for opponent's next draw
                await Task.Run(() => _semaporeWait.WaitOne(TIMEOUT));
                answer = _game.LastDraw;
            }
            catch (Exception /*ex*/)
            {
                _semaporeWait.Release();
            }

            return answer;
        }

        /// <summary>
        /// Try to submit the a chess draw if it is valid.
        /// </summary>
        /// <param name="draw">The draw to be submitted</param>
        /// <returns>a boolean indicating whether the submitted chess draw is valid</returns>
        public bool TrySubmitDraw(ChessDraw draw)
        {
            return _game.ApplyDraw(draw, true);
        }

        /// <summary>
        /// Retrieve the last draw that was made.
        /// </summary>
        /// <returns>the last draw that was made</returns>
        public ChessDraw GetLastDraw()
        {
            return _game.LastDraw;
        }

        /// <summary>
        /// Indicates if there is a waiting thread by the opponent.
        /// </summary>
        /// <returns>boolean whether another thread is waiting</returns>
        public bool IsWaiting()
        {
            return _isWaiting;
        }

        #endregion Methods
    }
}

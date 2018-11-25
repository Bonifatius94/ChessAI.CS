using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Chess.Lib;
using Microsoft.AspNetCore.Mvc;

namespace Chess.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChessDrawsController : ControllerBase
    {
        #region Members

        private static int _maxId = 0;
        private static Dictionary<int, ChessMatchmaker> _matchmaker = new Dictionary<int, ChessMatchmaker>();
        
        private static Semaphore _semaphoreMatchmaking = new Semaphore(0, 2);
        private static Mutex _mutexGameCreation = new Mutex();
        private static Mutex _mutexIdIncrement = new Mutex();
        private static Barrier _barrierGameCreationSync = new Barrier(2);

        #endregion Members

        #region Methods

        // GET api/chessdraws
        [HttpGet]
        public ActionResult<StartGameResponse> RequestNewGame()
        {
            try
            {
                ActionResult<StartGameResponse> result;

                // make sure only 2 players enter the game creation
                _semaphoreMatchmaking.WaitOne();

                // determine the game id (same id for both players)
                int gameId = _maxId + 1;

                // wait until both players have set the game id (this makes sure they have the same id)
                _barrierGameCreationSync.SignalAndWait();

                // make sure only the first of the two players creates the game
                _mutexGameCreation.WaitOne();
                bool isFirst = !_matchmaker.ContainsKey(gameId);
                
                if (isFirst)
                {
                    // first player's part: create a new game 
                    _matchmaker.Add(gameId, new ChessMatchmaker(gameId));
                    _maxId++;

                    // now that the game is created, let the second player enter the section
                    _mutexGameCreation.ReleaseMutex();

                    // send empty draw as response (this signals that the first player has to begin)
                    result = Ok(new StartGameResponse() { GameId = gameId, IsFirst = true });
                }
                else
                {
                    // let the next 2 players enter the game creation section
                    _mutexGameCreation.ReleaseMutex();
                    _semaphoreMatchmaking.Release(2);
                    
                    // send a start game response as answer
                    result = Ok(new StartGameResponse() { GameId = gameId, IsFirst = false });
                }
                
                return result;
            }
            catch (Exception /*ex*/)
            {
                return BadRequest();
            }
        }

        // PUT api/chessdraws/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<ChessDraw?>> SubmitDraw(int id, [FromBody] ChessDraw draw)
        {
            // make sure the game with the given id exists
            if (!_matchmaker.ContainsKey(id)) { throw new ArgumentException($"game with id { id } does not exist!"); }

            // submit the draw and wait for an answer from the opponent
            var matchmaker = _matchmaker[id];
            var hasAnswer = await matchmaker.SubmitDrawAndTryWaitForAnswer(draw);
            
            ActionResult<ChessDraw?> result;

            if (hasAnswer == true)
            {
                // send enemy draw as answer
                var answer = matchmaker.GetLastDraw();
                result = Ok(answer);
            }
            else if (hasAnswer == false)
            {
                // send reconnect request as answer
                result =  Ok(null);
            }
            else
            {
                // send 'bad request' answer
                result = BadRequest();
            }

            return result;
        }

        // GET api/chessdraws/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ChessDraw?>> ReconnectGame(int id)
        {
            // make sure the game with the given id exists
            if (!_matchmaker.ContainsKey(id)) { throw new ArgumentException($"game with id { id } does not exist!"); }

            // wait for an answer from the opponent
            var matchmaker = _matchmaker[id];
            var answer = await matchmaker.WaitForAnswer();

            // return the opponent's answer
            var result = Ok(answer);
            return result;
        }

        #endregion Methods
    }

    public class StartGameResponse
    {
        #region Members

        public int GameId { get; set; }
        public bool IsFirst { get; set; }

        #endregion Members
    }

    public class ChessMatchmaker
    {
        #region Constructor

        public ChessMatchmaker(int gameId)
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
        /// Try to submit the a chess draw and wait for the opponent's answer.
        /// </summary>
        /// <param name="draw">The draw to be submitted</param>
        /// <returns>
        /// true:  The draw was successfully submitted; enemy answer can be retrieved by GetLastDraw().
        /// false: The draw was successfully submitted; enemy answer request timed out.
        /// null:  The Submitted draw is invalid. Send another draw that is valid.
        /// </returns>
        public async Task<bool?> SubmitDrawAndTryWaitForAnswer(ChessDraw draw)
        {
            bool? ret = null;

            try
            {
                // TODO: implement waiting with barrier

                if (_game.ApplyDraw(draw, true))
                {
                    // make the opponent's waiting request thread continue
                    await Task.Run(() => _semaporeWait.WaitOne(TIMEOUT));
                    ret = true;
                }
            }
            catch (Exception /*ex*/)
            {
                _semaporeWait.Release();
            }

            return ret;
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

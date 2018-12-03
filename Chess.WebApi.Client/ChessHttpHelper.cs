using Chess.Lib;
using Chess.WebApi.Server.Interface;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Chess.WebApi.Client
{
    public class ChessHttpHelper
    {
        #region Constructor

        public ChessHttpHelper(string baseAddress)
        {
            _baseAddress = baseAddress;
        }

        #endregion Constructor

        #region Members

        private string _baseAddress;
        private const string API_CONTROLLER = "api/chessdraws";

        #endregion Members

        #region Methods

        public async Task<StartGameResponse> TryStartNewGame()
        {
            StartGameResponse ret = null;

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"{ _baseAddress }/{ API_CONTROLLER }");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    ret = JsonConvert.DeserializeObject<StartGameResponse>(json);
                }
            }

            return ret;
        }

        public async Task<bool> TrySubmitDraw(int gameId, ChessDraw draw)
        {
            bool ret = false;

            using (var client = new HttpClient())
            {
                string json = JsonConvert.SerializeObject(draw);

                using (var content = new StringContent(json))
                {
                    var response = await client.PutAsync($"{ _baseAddress }/{ API_CONTROLLER }/{ gameId }", content);
                    ret = response.IsSuccessStatusCode;
                }
            }

            return ret;
        }

        public async Task<ChessDraw?> TryGetOpponentDraw(int gameId)
        {
            ChessDraw? ret = null;

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"{ _baseAddress }/{ API_CONTROLLER }/{ gameId }");

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    ret = JsonConvert.DeserializeObject<ChessDraw?>(json);
                }
            }

            return ret;
        }

        #endregion Methods
    }
}

using System.Text.Json;

/// <summary>
/// arbiter that creates sessions, stores checkpoint hashes, stores final results and returns a verdict
/// </summary>
public class Program
{
    #region variables
    private Dictionary<string, MatchSession> sessions;
    private object sessionLock;
    #endregion

    #region main
    /// <summary>
    /// creates the arbiter program data
    /// </summary>
    private Program()
    {
        sessions = new Dictionary<string, MatchSession>();
        sessionLock = new object();
    }

    /// <summary>
    /// starts the arbiter service
    /// </summary>
    /// <param name="arguments">command line arguments</param>
    public static void Main(string[] arguments)
    {
        Program program = new Program();
        WebApplication webApplication = WebApplication.CreateBuilder(arguments).Build();
        webApplication.MapGet("/api/health", program.Health); // check to see if the arbiter is online

        // endpoints used by the game during a match
        webApplication.MapPost("/api/session/create", program.CreateSession);
        webApplication.MapPost("/api/session/checkpoint", program.Checkpoint);
        webApplication.MapPost("/api/session/finish", program.Finish);
        webApplication.Run();
    }
    #endregion

    #region handlers
    /// <summary>
    /// returns arbiter health
    /// </summary>
    /// <returns>health result</returns>
    private IResult Health()
    {
        string response = "{\"status\":\"online\",\"build\":\"" + BuildInfo.Version + "\"}"; // the build helps me check which version is live on the vps
        return Results.Text(response, "application/json");
    }

    /// <summary>
    /// creates new session
    /// </summary>
    /// <param name="request">HTTP request</param>
    /// <returns>session data</returns>
    private async Task<IResult> CreateSession(HttpRequest request)
    {
        try
        {
            CleanOldSessions();
            JsonDocument jsonDocument = await JsonDocument.ParseAsync(request.Body);
            JsonElement jsonRoot = jsonDocument.RootElement;

            // read the two steam ids from the game
            ulong player1Id = jsonRoot.GetProperty("player1").GetUInt64();
            ulong player2Id = jsonRoot.GetProperty("player2").GetUInt64();

            if (player1Id == 0 || player2Id == 0 || player1Id == player2Id)
            {
                return Results.BadRequest("Invalid players.");
            }

            string player1 = player1Id.ToString();
            string player2 = player2Id.ToString();

            // create the session values that both players will use
            string sessionId = Guid.NewGuid().ToString();
            DateTime expirationTime = DateTime.Now.AddHours(2);
            long expires = new DateTimeOffset(expirationTime).ToUnixTimeSeconds();
            string token = Guid.NewGuid().ToString();

            lock (sessionLock)
            {
                sessions[sessionId] = new MatchSession(token, player1, player2, expirationTime);
            }

            string response = "{\"session\":\"" + sessionId + "\",\"key\":\"" + token + "\",\"expires\":" + expires + "}";
            return Results.Text(response, "application/json");
        }
        catch
        {
            return Results.BadRequest("Invalid request.");
        }
    }

    /// <summary>
    /// saves checkpoint hash for a frame
    /// </summary>
    /// <param name="request">HTTP request</param>
    /// <returns>checkpoint result</returns>
    private async Task<IResult> Checkpoint(HttpRequest request)
    {
        try
        {
            CleanOldSessions();
            JsonDocument jsonDocument = await JsonDocument.ParseAsync(request.Body);
            JsonElement jsonRoot = jsonDocument.RootElement;

            // read checkpoint data sent by the game
            string sessionId = jsonRoot.GetProperty("sessionId").GetString();
            string token = jsonRoot.GetProperty("token").GetString();
            ulong playerId = jsonRoot.GetProperty("player").GetUInt64();
            string player = playerId.ToString();
            string stateHash = jsonRoot.GetProperty("stateHash").GetString();
            int frame = jsonRoot.GetProperty("frame").GetInt32();

            if (string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(token) || playerId == 0 || string.IsNullOrEmpty(stateHash) || frame < 0)
            {
                return Results.BadRequest("Invalid checkpoint.");
            }

            // make sure this player is allowed to use this session
            MatchSession session = GetSession(sessionId);
            IResult error = CheckSession(session, token, player);

            if (error != null)
            {
                return error;
            }

            // save the checkpoint data and compare it with the other player if it exists
            session.AddCheckpoint(player, frame, stateHash);
            string response = "{\"saved\":true,\"mismatch\":" + session.HasMismatch().ToString().ToLowerInvariant() + ",\"reason\":\"" + session.GetMismatchReason() + "\",\"frame\":" + session.GetMismatchFrame() +
                              ",\"player\":\"" + session.GetMismatchPlayer() + "\",\"build\":\"" + BuildInfo.Version + "\"}";

            return Results.Text(response, "application/json");
        }
        catch
        {
            return Results.BadRequest("Invalid request.");
        }
    }

    /// <summary>
    /// saves one player's final match result
    /// </summary>
    /// <param name="request">HTTP request</param>
    /// <returns>final verdict</returns>
    private async Task<IResult> Finish(HttpRequest request)
    {
        try
        {
            CleanOldSessions();
            JsonDocument jsonDocument = await JsonDocument.ParseAsync(request.Body);
            JsonElement jsonRoot = jsonDocument.RootElement;

            // read the final match data sent by the game
            string sessionId = jsonRoot.GetProperty("sessionId").GetString();
            string token = jsonRoot.GetProperty("token").GetString();
            ulong playerId = jsonRoot.GetProperty("player").GetUInt64();
            ulong winnerId = jsonRoot.GetProperty("winner").GetUInt64();
            string player = playerId.ToString();
            string winner = winnerId.ToString();
            string finalStateHash = jsonRoot.GetProperty("finalStateHash").GetString();
            int player1Rounds = jsonRoot.GetProperty("p1Rounds").GetInt32();
            int player2Rounds = jsonRoot.GetProperty("p2Rounds").GetInt32();
            int finalFrame = jsonRoot.GetProperty("finalFrame").GetInt32();

            if (string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(token) || playerId == 0 || winnerId == 0 || string.IsNullOrEmpty(finalStateHash) ||
                player1Rounds < 0 || player2Rounds < 0 || finalFrame < 0)
            {
                return Results.BadRequest("Invalid finish.");
            }

            // make sure this player is allowed to finish this session
            MatchSession matchSession = GetSession(sessionId);
            IResult error = CheckSession(matchSession, token, player);

            if (error != null)
            {
                return error;
            }

            if (winner != matchSession.player1 && winner != matchSession.player2)
            {
                return Results.BadRequest("Invalid winner.");
            }

            lock (matchSession.dataLock)
            {
                // save this player's final result
                matchSession.finishes[player] = new FinishData(player, winner, player1Rounds, player2Rounds, finalFrame, finalStateHash);

                if (!matchSession.finishes.ContainsKey(matchSession.player1) || !matchSession.finishes.ContainsKey(matchSession.player2))
                {
                    return Results.Text("{\"result\":\"waiting\",\"reason\":\"other player has not finished yet\"}", "application/json");
                }
            }

            FinishData firstFinish = matchSession.GetFinish(matchSession.player1);
            FinishData secondFinish = matchSession.GetFinish(matchSession.player2);

            // both players must report the same final result
            bool sameResult = firstFinish.winner == secondFinish.winner && firstFinish.player1Rounds == secondFinish.player1Rounds && firstFinish.player2Rounds == secondFinish.player2Rounds &&
                              firstFinish.finalStateHash == secondFinish.finalStateHash;

            if (sameResult && !matchSession.HasMismatch())
            {
                return Results.Text("{\"result\":\"verified\",\"reason\":\"both players sent the same result\"}", "application/json");
            }

            string reason = matchSession.GetMismatchReason();

            if (reason == "none")
            {
                reason = GetFinishMismatchReason(firstFinish, secondFinish);
            }

            return Results.Text("{\"result\":\"flagged\",\"reason\":\"" + reason + "\"}", "application/json");
        }
        catch
        {
            return Results.BadRequest("Invalid request.");
        }
    }
    #endregion

    #region methods
    /// <summary>
    /// returns session by id
    /// </summary>
    /// <param name="sessionId">session id</param>
    /// <returns>session or null</returns>
    private MatchSession GetSession(string sessionId)
    {
        MatchSession session;

        lock (sessionLock)
        {
            if (sessions.TryGetValue(sessionId, out session))
            {
                return session;
            }
        }

        return null;
    }

    /// <summary>
    /// removes expired sessions
    /// </summary>
    private void CleanOldSessions()
    {
        lock (sessionLock)
        {
            List<string> sessionIds = new List<string>(sessions.Keys); // copy the keys first because expired sessions are removed inside the loop

            foreach (string sessionId in sessionIds)
            {
                if (sessions[sessionId].IsExpired())
                {
                    sessions.Remove(sessionId);
                }
            }
        }
    }

    /// <summary>
    /// checks if session can be used
    /// </summary>
    /// <param name="session">match session</param>
    /// <param name="token">session token</param>
    /// <param name="player">player id</param>
    /// <returns>error result or null</returns>
    private IResult CheckSession(MatchSession session, string token, string player)
    {
        if (session == null)
        {
            return Results.NotFound("Unknown session.");
        }

        if (session.IsExpired())
        {
            return Results.BadRequest("Session expired.");
        }

        if (session.token != token)
        {
            return Results.Unauthorized();
        }

        if (player != session.player1 && player != session.player2)
        {
            return Results.Unauthorized();
        }

        return null;
    }

    /// <summary>
    /// finds finish mismatch reason
    /// </summary>
    /// <param name="firstFinish">first finish data</param>
    /// <param name="secondFinish">second finish data</param>
    /// <returns>finish mismatch reason</returns>
    private string GetFinishMismatchReason(FinishData firstFinish, FinishData secondFinish)
    {
        // checks if both players reported the same winner
        if (firstFinish.winner != secondFinish.winner)
        {
            return "winner_mismatch";
        }

        // checks if both players reported the same round score
        if (firstFinish.player1Rounds != secondFinish.player1Rounds || firstFinish.player2Rounds != secondFinish.player2Rounds)
        {
            return "rounds_mismatch";
        }

        // checks if both players ended with the same final state hash
        if (firstFinish.finalStateHash != secondFinish.finalStateHash)
        {
            return "final_hash_mismatch";
        }

        // checks if both players finished on the same frame
        if (firstFinish.finalFrame != secondFinish.finalFrame)
        {
            return "final_frame_mismatch";
        }

        return "unknown_finish_mismatch";
    }
    #endregion

    #region nested classes
    private class FinishData
    {
        #region finish data
        public string player;
        public string winner;
        public int player1Rounds;
        public int player2Rounds;
        public int finalFrame;
        public string finalStateHash;
        #endregion

        #region finish data constructor
        /// <summary>
        /// creates final match data
        /// </summary>
        /// <param name="player">player who sent the result</param>
        /// <param name="winner">winner id</param>
        /// <param name="p1Rounds">player 1 rounds</param>
        /// <param name="p2Rounds">player 2 rounds</param>
        /// <param name="finalFrame">final frame</param>
        /// <param name="finalStateHash">final state hash</param>
        public FinishData(string player, string winner, int p1Rounds, int p2Rounds, int finalFrame, string finalStateHash)
        {
            this.player = player;
            this.winner = winner;
            this.player1Rounds = p1Rounds;
            this.player2Rounds = p2Rounds;
            this.finalFrame = finalFrame;
            this.finalStateHash = finalStateHash;
        }
        #endregion
    }

    private class MatchSession
    {
        #region match session data
        public string token;
        public string player1;
        public string player2;
        private DateTime expirationTime;
        private Dictionary<string, Dictionary<int, string>> checkpoints;
        private bool mismatch;
        private string mismatchReason;
        private int mismatchFrame;
        private string mismatchPlayer;
        public Dictionary<string, FinishData> finishes;
        public object dataLock;
        #endregion

        #region match session constructor
        /// <summary>
        /// creates match session
        /// </summary>
        /// <param name="token">session token</param>
        /// <param name="player1">player 1 id</param>
        /// <param name="player2">player 2 id</param>
        /// <param name="expirationTime">expiration time</param>
        public MatchSession(string token, string player1, string player2, DateTime expirationTime)
        {
            this.token = token;
            this.player1 = player1;
            this.player2 = player2;
            this.expirationTime = expirationTime;
            checkpoints = new Dictionary<string, Dictionary<int, string>>();
            finishes = new Dictionary<string, FinishData>();
            mismatch = false;
            mismatchReason = "none";
            mismatchFrame = -1;
            mismatchPlayer = "";
            dataLock = new object();
        }
        #endregion

        #region match session methods
        /// <summary>
        /// checks if the session is expired
        /// </summary>
        /// <returns>true if expired</returns>
        public bool IsExpired()
        {
            return DateTime.Now > expirationTime;
        }

        /// <summary>
        /// saves one checkpoint and checks for mismatch
        /// </summary>
        /// <param name="player">player id</param>
        /// <param name="frame">frame number</param>
        /// <param name="stateHash">state hash</param>
        public void AddCheckpoint(string player, int frame, string stateHash)
        {
            lock (dataLock)
            {
                Dictionary<int, string> playerCheckpoints;

                if (!checkpoints.TryGetValue(player, out playerCheckpoints))
                {
                    playerCheckpoints = new Dictionary<int, string>();
                    checkpoints[player] = playerCheckpoints;
                }

                // same player should not send a different hash for the same frame
                string savedHash;

                if (playerCheckpoints.TryGetValue(frame, out savedHash) && savedHash != stateHash)
                {
                    SetMismatch("same_player_changed_hash", frame, player);
                }

                playerCheckpoints[frame] = stateHash;
                string otherPlayer = player1;

                if (player == player1)
                {
                    otherPlayer = player2;
                }

                // compare with the other player if that frame already exists
                Dictionary<int, string> otherPlayerCheckpoints;
                string otherPlayerHash;

                if (checkpoints.TryGetValue(otherPlayer, out otherPlayerCheckpoints) &&
                    otherPlayerCheckpoints.TryGetValue(frame, out otherPlayerHash) &&
                    otherPlayerHash != stateHash)
                {
                    SetMismatch("state_hash_mismatch", frame, player);
                }
            }
        }

        /// <summary>
        /// returns final data for one player
        /// </summary>
        /// <param name="player">player id</param>
        /// <returns>final match data</returns>
        public FinishData GetFinish(string player)
        {
            lock (dataLock)
            {
                return finishes[player];
            }
        }

        /// <summary>
        /// returns mismatch state
        /// </summary>
        /// <returns>true if mismatch exists</returns>
        public bool HasMismatch()
        {
            lock (dataLock)
            {
                return mismatch;
            }
        }

        /// <summary>
        /// saves first mismatch reason
        /// </summary>
        /// <param name="reason">mismatch reason</param>
        /// <param name="frame">mismatch frame</param>
        /// <param name="player">player that triggered the check</param>
        private void SetMismatch(string reason, int frame, string player)
        {
            if (mismatch)
            {
                return;
            }

            mismatch = true;
            mismatchReason = reason;
            mismatchFrame = frame;
            mismatchPlayer = player;
        }

        /// <summary>
        /// returns mismatch reason
        /// </summary>
        /// <returns>mismatch reason</returns>
        public string GetMismatchReason()
        {
            lock (dataLock)
            {
                return mismatchReason;
            }
        }

        /// <summary>
        /// returns mismatch frame
        /// </summary>
        /// <returns>mismatch frame</returns>
        public int GetMismatchFrame()
        {
            lock (dataLock)
            {
                return mismatchFrame;
            }
        }

        /// <summary>
        /// returns mismatch player
        /// </summary>
        /// <returns>mismatch player</returns>
        public string GetMismatchPlayer()
        {
            lock (dataLock)
            {
                return mismatchPlayer;
            }
        }
        #endregion
    }
    #endregion
}
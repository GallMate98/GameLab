using GameLab.Services.DataService;
using GameLab.Services.GameLobbyAssignment;
using GameLab.Services.TicTacToe;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace GameLab.Hubs
{
    [Authorize]
    public class NineMensMorrisHub : Hub
    {
        private readonly IGameAssignmentService _gameAssignmentService;
        private readonly SharedDb _sharedDb;

        public NineMensMorrisHub(SharedDb sharedDb, IGameAssignmentService gameAssignmentService)
        {

            _sharedDb = sharedDb;
            _gameAssignmentService = gameAssignmentService;

        }
    }
}

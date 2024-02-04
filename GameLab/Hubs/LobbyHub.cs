using GameLab.Models;
using GameLab.Services.LobbyAssignment;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;

public class LobbyHub : Hub
{

    private readonly ILobbyAssignmentService _lobbyAssignmentService;


    public LobbyHub(ILobbyAssignmentService lobbyAssignment)
    {
        _lobbyAssignmentService = lobbyAssignment;
    }

    public async Task JoinLobby(string lobbyId)
    {
        // Itt kezeld a játékossal való csatlakozást a lobby-hoz

        await Groups.AddToGroupAsync(Context.ConnectionId, lobbyId);

        var lobbyData = GetLobbyData(lobbyId);
        // Frissítsd a lobby-t a klienseknek
        await Clients.Group(lobbyId).SendAsync("UpdateLobby", lobbyData);
    }

    public async Task SendMessage(string lobbyId, Player player, string message) //sendInvite
    {
        // Itt kezeld a chat üzeneteket a lobby-ban
        await Clients.Group(lobbyId).SendAsync("ReceiveMessage", player, message);
    }

    public async Task SendToAllInLobby(string lobbyId, string message)
    {
        // Itt küldhetsz üzenetet minden lobby-ban lévő kliensnek
        await Clients.Group(lobbyId).SendAsync("ReceiveMessageToAll", message);
    }

    private List<Player> GetLobbyData(string lobbyId)
    {
        return _lobbyAssignmentService.GetLobbyPalyers(Guid.Parse(lobbyId));
    }

    public async Task LeaveLobby(string lobbyId, string userName)
    {
        if (!string.IsNullOrEmpty(userName))
        {
            // Itt kezeld a játékossal való kilépést a lobby-ból
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, lobbyId);


            var lobbyData = _lobbyAssignmentService.RemovePlayer(Guid.Parse(lobbyId), userName);


            // Frissítsd a lobby-t a klienseknek
            await Clients.Group(lobbyId).SendAsync("UpdateLobby", lobbyData);
        }
    }


    public override async Task OnDisconnectedAsync(Exception exception)
    {
        // Itt végezheted el a szükséges lezáró műveleteket vagy eseményeket
        await base.OnDisconnectedAsync(exception);
    }
}

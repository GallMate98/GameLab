using GameLab.Controllers;
using GameLab.Data;
using GameLab.Models;
using GameLab.Services.DataService;
using GameLab.Services.GameLobbyAssignment;
using GameLab.Services.LobbyAssignment;
using MailKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

[Authorize]
public class LobbyHub : Hub
{

    private readonly ILobbyAssignmentService _lobbyAssignmentService;
    private readonly IGameAssignmentService _gameAssignmentService;
    private readonly SharedDb _sharedDb;
   private readonly DataContext _dataContext;
    private object senderData;

    public LobbyHub(ILobbyAssignmentService lobbyAssignment ,SharedDb sharedDb, DataContext dataContext, IGameAssignmentService gameAssignmentService)
    {
        _lobbyAssignmentService = lobbyAssignment;
        _sharedDb = sharedDb;
       _gameAssignmentService = gameAssignmentService;
        _dataContext = dataContext;
 
    }

    
    public async Task JoinLobby(string lobbyId)
    {
        // Itt kezeld a játékossal való csatlakozást a lobby-hoz

        string userName = Context.User.FindFirstValue(ClaimTypes.Name);

        var connectionId = Context.ConnectionId;

        _sharedDb.Userconn[userName] = connectionId;


        await Groups.AddToGroupAsync(Context.ConnectionId, lobbyId);

        var lobbyData = GetLobbyData(lobbyId);
        // Frissítsd a lobby-t a klienseknek
        if (_sharedDb.Messages.ContainsKey(lobbyId))
        {
            var messages = _sharedDb.Messages[lobbyId];
            await Clients.Group(lobbyId).SendAsync("UpdateLobby", lobbyData, messages);
        }
        else
        {
            // Ha a lobby üzenetei nem elérhetők, akkor csak a lobby adatokat küldjük
            await Clients.Group(lobbyId).SendAsync("UpdateLobby", lobbyData, null);
        }
    }

    public async Task SendInvate(string receiveUserName) //sendInvite for game 
    {
        var senderUserName = Context.User.FindFirstValue(ClaimTypes.Name);

        if(_sharedDb.Userconn.TryGetValue(receiveUserName, out var recevieConnectionId)) 
        {

            await Clients.Client(recevieConnectionId).SendAsync("ReceiveInvate", senderUserName);
        }
            
        //await Clients.Group(lobbyId).SendAsync("ReceiveInvate", senderUserName);

       
    }

    public async Task SendToAllInLobby(string lobbyId, string userName, string message)
    {

        var newMessage = new Message
        {
            userName = userName,
            message = message
        };

     
        if (_sharedDb.Messages.TryGetValue(lobbyId, out List<Message> existingMessages))
        {
            
            existingMessages.Add(newMessage);
        }
        else
        {
            existingMessages = new List<Message> { newMessage };
        }

       
        _sharedDb.Messages[lobbyId] = existingMessages;

       
            await Clients.Group(lobbyId).SendAsync("ReceiveMessageToAll", newMessage.userName, newMessage.message);

    }


    public async Task AcceptGameRequest(Player receiverPlayer, Player senderPlayer, string gameUrl, string lobbyId)
    {
        var game = await _dataContext.Games.FirstOrDefaultAsync(g => g.Url == gameUrl);
        if (game == null)
        {
            
            throw new Exception("A játék nem található.");
        }

        // Létrehozunk egy új játékot
        GameLobby gameLobby = _gameAssignmentService.AssignPlayerToGame(receiverPlayer, senderPlayer, game);
        var gameLobbyId = gameLobby.Id.ToString();

        var senderUserName = senderPlayer.UserName;
        var senderConnectionId = _sharedDb.Userconn.GetValueOrDefault(senderUserName);

        var receiverUserName = receiverPlayer.UserName;
        var receiverConnectionId = _sharedDb.Userconn.GetValueOrDefault(receiverUserName);

        if (!string.IsNullOrEmpty(senderConnectionId) && !string.IsNullOrEmpty(receiverConnectionId))
        {
           
           // await Clients.Group(gameLobbyId).SendAsync("StartGame", game)
            LeaveLobby(lobbyId, senderUserName);
            LeaveLobby(lobbyId, receiverUserName);

            // Játék lobby ID elküldése
            await Clients.Client(senderConnectionId).SendAsync("GameId", gameLobbyId);
            await Clients.Client(receiverConnectionId).SendAsync("GameId", gameLobbyId);
        }
    }


    private List<Player> GetLobbyData(string lobbyId)
    {
        return _lobbyAssignmentService.GetLobbyPalyers(Guid.Parse(lobbyId));
    }

    public async Task LeaveLobby(string lobbyId, string userName)
    {

       
        if (!string.IsNullOrEmpty(userName))
        {

                  string? userId = _sharedDb.Userconn[userName];
                _sharedDb.Userconn.Remove(userName,  out userId);
            

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

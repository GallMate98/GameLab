﻿using GameLab.Models;
using System.Collections.Concurrent;

namespace GameLab.Services.DataService
{
    public class SharedDb
    {
        private readonly ConcurrentDictionary<string, List<Message>> _messages = new ConcurrentDictionary<string, List<Message>>();

        public ConcurrentDictionary<string, List<Message>> Messages => _messages;


        private readonly ConcurrentDictionary<string, string> _userconn = new ConcurrentDictionary<string, string>();

        public ConcurrentDictionary<string, string> Userconn => _userconn;

        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _playerconn = new ConcurrentDictionary<string, ConcurrentDictionary<string,string>>();

        public ConcurrentDictionary<string, ConcurrentDictionary<string, string>> Playerconn => _playerconn;

        private readonly ConcurrentDictionary<string, BoardX0> _x0game = new ConcurrentDictionary<string, BoardX0>();

        public ConcurrentDictionary<string,BoardX0> X0game => _x0game;

        private readonly ConcurrentDictionary<string, BoardNineMens> _ninemensgame = new ConcurrentDictionary<string, BoardNineMens>();

        public ConcurrentDictionary<string, BoardNineMens> ninemensgame => _ninemensgame;

        private readonly ConcurrentDictionary<string, string> _piecePostionsList = new ConcurrentDictionary<string, string>();
        
        public ConcurrentDictionary<string, string> PiecePositionList => _piecePostionsList;

        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, DateTime>> _moveTime = new ConcurrentDictionary<string, ConcurrentDictionary<string, DateTime>>();

        public ConcurrentDictionary<string, ConcurrentDictionary<string, DateTime>> MoveTime => _moveTime;

        private readonly ConcurrentDictionary<string,  DateTime> _leftGameTime = new ConcurrentDictionary<string, DateTime>();

        public ConcurrentDictionary<string,  DateTime> LeftGameTime => _leftGameTime;

        private readonly ConcurrentDictionary<string, int> _remainsecondes = new ConcurrentDictionary<string, int>();

        public ConcurrentDictionary<string, int> Remainsecondes => _remainsecondes;


    }
}

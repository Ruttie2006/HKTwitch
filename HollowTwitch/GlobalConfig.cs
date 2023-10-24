using System;
using System.Collections.Generic;
using HollowTwitch.Clients;
using Modding;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine.Serialization;

namespace HollowTwitch
{
    [Serializable]
    public sealed class GlobalConfig
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ClientType Client = ClientType.Twitch;

        public TwitchConfig Twitch { get; set; }
        public BilibiliConfig Bilibili { get; set; }

        public string Prefix = "!";

        public bool CaseSensitive = false;

        public List<string> BlacklistedCommands = [];

        public List<string> AdminUsers = [];

        public List<string> BannedUsers = [];

        public sealed class TwitchConfig
        {
            public string Token;

            public string Username;

            public string Channel;
        }
        public sealed class BilibiliConfig
        {
            public int RoomID;
        }
    }
}
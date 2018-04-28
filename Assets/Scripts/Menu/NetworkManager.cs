﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using UnityEngine;

namespace Menu
{   
    public class NetworkManager : MonoBehaviour
    {
        public class Lobby
        {
            [JsonProperty("id")]
            public int Id;
            [JsonProperty("players_count")]
            public int PlayersCount;
            [JsonProperty("players")]
            public Player[] Players;
        }
        public class Player
        {
            [JsonProperty("address")] 
            public string Address;
            [JsonProperty("ready")]
            public bool IsReady;
        }

        /*
        Specify IP address of matchmaking server here
        */
        private string serverAddress = "";
        
        private const string createLobby = "/create";
        private const string getLobby = "/lobby?lobby_id=";
        private const string getLobbies = "/lobbies";
        private const string deleteLobby = "/delete?lobby_id=";
        private const string ready = "/ready";

        private static string GetLocalIP()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            }
            return "";
        }

        /// <summary>
        /// Returns id of created lobby or -1 in case of failure
        /// </summary>
        public int CreateLobby()
        {
            var createRequest = (HttpWebRequest) WebRequest.Create(serverAddress + createLobby);
            var createResponse = (HttpWebResponse) createRequest.GetResponse();
            if (createResponse.StatusCode != HttpStatusCode.OK) return -1;

            return JsonConvert.DeserializeObject<Lobby>(
                new StreamReader(createResponse.GetResponseStream()).ReadToEnd()).Id;
        }

        /// <summary>
        /// Returns, if join is successfull
        /// </summary>
        public bool JoinLobby(int lobbyId)
        {
            var request = (HttpWebRequest) WebRequest.Create(
                serverAddress + string.Format("/join?lobby_id={0}&address={1}", lobbyId, GetLocalIP()));
            var response = (HttpWebResponse) request.GetResponse();
            
            return response.StatusCode == HttpStatusCode.OK && 
                   new StreamReader(response.GetResponseStream()).ReadToEnd() == "Added to lobby";
        }

        /// <summary>
        /// Returns null in case of failure
        /// </summary>
        public Lobby GetLobby(int lobbyId)
        {
            var request = (HttpWebRequest) WebRequest.Create(serverAddress + getLobby + lobbyId);
            var response = (HttpWebResponse) request.GetResponse();
            return response.StatusCode == HttpStatusCode.OK 
                ? JsonConvert.DeserializeObject<Lobby>(new StreamReader(response.GetResponseStream()).ReadToEnd())
                : null;
        }

        /// <summary>
        /// Returns list of lobbies
        /// </summary>
        public List<Lobby> GetLobbies()
        {
            var request = (HttpWebRequest) WebRequest.Create(serverAddress + getLobbies);
            var response = (HttpWebResponse) request.GetResponse();
            if (response.StatusCode != HttpStatusCode.OK) return new List<Lobby>();

            return JsonConvert.DeserializeObject<Dictionary<string, Lobby>>(
                new StreamReader(response.GetResponseStream()).ReadToEnd()).Values.ToList();
        }

        public bool LeaveLobby(int id)
        {
            var request = (HttpWebRequest) WebRequest.Create(
                serverAddress + string.Format("/leave?lobby_id={0}&address={1}", id, GetLocalIP()));
            var response = (HttpWebResponse) request.GetResponse();
            if (response.StatusCode != HttpStatusCode.OK) return false;
            
            return response.StatusCode == HttpStatusCode.OK &&
                   new StreamReader(response.GetResponseStream()).ReadToEnd() == "Removed from lobby";
        }

        public bool DeleteLobby(int id)
        {
            var request = (HttpWebRequest) WebRequest.Create(
                serverAddress + deleteLobby + id);
            var response = (HttpWebResponse) request.GetResponse();
            
            return response.StatusCode == HttpStatusCode.OK;
        }

        public bool SetReady(int id)
        {
            var request = (HttpWebRequest) WebRequest.Create(
                serverAddress + string.Format("/ready?lobby_id={0}&address={1}", id, GetLocalIP()));
            var response = (HttpWebResponse) request.GetResponse();

            return response.StatusCode == HttpStatusCode.OK;
        }
        
        public bool SetNotReady(int id)
        {
            var request = (HttpWebRequest) WebRequest.Create(
                serverAddress + string.Format("/notready?lobby_id={0}&address={1}", id, GetLocalIP()));
            var response = (HttpWebResponse) request.GetResponse();

            return response.StatusCode == HttpStatusCode.OK;
        }
    }
}

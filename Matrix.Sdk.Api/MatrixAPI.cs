// This net5 Matrix.Sdk is a fork from https://github.com/VRocker/MatrixAPI

// Following changes were made in order to make it a pure .net 5 library without the need for UWP.
// - Upgraded to .net5, made namespaces uniform
// - Merged all MatrixAPI partial classes into one
// - Removed custom MatrixSpec attribute class
// - Removed dependency on UWP (Windows Timers)
// - Added option to send matrix formatted text (html) 
// - Fixed null pointer expceptions in ParseClientSync
// - Standardized property naming

// The Matrix.Sdk is under Apache License, Version 2.0, January 2004, http://www.apache.org/licenses/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Matrix.Sdk.Api.Backends;

namespace Matrix.Sdk.Api
{
    public class MatrixAPI
    {

        public string ApplicationID { get; set; } 
        public string ApplicationName { get; set; }
    
        public string UserID { get; set; }
        public string DeviceID { get; set; }
        public string DeviceName { get; set; }

      

        public int SyncTimeout = 10000;

        public bool RunningInitialSync { get; private set; }
        public bool IsConnected { get; private set; }


        public Events.Events Events { get; }


        private string SyncToken { get; set; }

        private IMatrixAPIBackend HttpBackend { get; }



        public MatrixAPI(string Url, string accessToken = "", string syncToken = "")
        {
            Events = new Events.Events();

            if (!Uri.IsWellFormedUriString(Url, UriKind.Absolute))
            {
                throw new MatrixException("URL is not valid.");
            }

            HttpBackend = new HttpBackend(Url);
          
         
            if (!string.IsNullOrEmpty(accessToken))
            {
                HttpBackend.SetAccessToken(accessToken);
            }

            SyncToken = syncToken;
            if (string.IsNullOrEmpty(SyncToken))
            {
                RunningInitialSync = true;
            }
        }

        private void FlushMessageQueue()
        {
        }

     

        private async Task ClientSync(bool connectionFailureTimeout = false, bool fullState = false)
        {
            string url = "/_matrix/client/r0/sync?timeout=" + SyncTimeout;

            if (!string.IsNullOrEmpty(SyncToken))
            {
                url += "&since=" + SyncToken;
            }

            if (fullState)
            {
                url += "&full_state=true";
            }

            var tuple = await HttpBackend.Get(url, true);
            MatrixRequestError err = tuple.Item1;
            string response = tuple.Item2;
            if (err.IsOk)
            {
                await ParseClientSync(response);
            }
            else if (connectionFailureTimeout)
            {

            }

            if (RunningInitialSync)
            {
                // Fire an event to say sync has been done
                RunningInitialSync = false;
            }
        }

        public async Task<string[]> ClientVersions()
        {
            var tuple = await HttpBackend.Get("/_matrix/client/versions", false);
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                // Parse the version request
                return ParseClientVersions(result);
            }

            throw new MatrixException("Failed to validate version.");
        }

        public async void ClientRegister(Requests.Session.MatrixRegister registration)
        {
            var tuple = await HttpBackend.Post("/_matrix/client/r0/register", false, Helpers.JsonHelper.Serialize(registration));
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                // Parse registration response
            }
            else
            {
                throw new MatrixException(err.ToString());
            }
        }

        public async void ClientLogin(Requests.Session.MatrixLogin login)
        {
            var tuple = await HttpBackend.Post("/_matrix/client/r0/login", false, Helpers.JsonHelper.Serialize(login));
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                // We logged in!
                ParseLoginResponse(result);
            }
            else
            {
                //throw new MatrixException(err.ToString());
                Events.FireLoginFailEvent(err.ToString());
            }
        }

        public async void ClientProfile(string userId)
        {
            var tuple = await HttpBackend.Get("/_matrix/client/r0/profile/" + userId, true);
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                Responses.UserData.UserProfileResponse profileResponse = ParseUserProfile(result);

                Events.FireUserProfileReceivedEvent(userId, profileResponse.AvatarUrl, profileResponse.DisplayName);
            }
            else
            {
                // Fire an error
            }
        }

        public async Task<bool> ClientSetDisplayName(string displayName)
        {
            Requests.UserData.UserProfileSetDisplayName req = new Requests.UserData.UserProfileSetDisplayName() { DisplayName = displayName };
            var tuple = await HttpBackend.Put(string.Format("/_matrix/client/r0/profile/{0}/displayname", Uri.EscapeDataString(UserID)), true, Helpers.JsonHelper.Serialize(req));
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> ClientSetAvatar(string avatarUrl)
        {
            Requests.UserData.UserProfileSetAvatar req = new Requests.UserData.UserProfileSetAvatar() { AvatarUrl = avatarUrl };
            var tuple = await HttpBackend.Put(string.Format("/_matrix/client/r0/profile/{0}/displayname", Uri.EscapeDataString(UserID)), true, Helpers.JsonHelper.Serialize(req));
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> ClientSetPresence(string presence, string statusMessage = null)
        {
            Requests.Presence.MatrixSetPresence req = new Requests.Presence.MatrixSetPresence()
            {
                Presence = presence
            };

            if (statusMessage != null)
            {
                req.StatusMessage = statusMessage;
            }

            var tuple = await HttpBackend.Put(string.Format("/_matrix/client/r0/presence/{0}/status", Uri.EscapeDataString(UserID)), true, Helpers.JsonHelper.Serialize(req));
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                return true;
            }
            return false;
        }

        public async Task<string> MediaUpload(string contentType, byte[] data)
        {
            var tuple = await HttpBackend.Post("/_matrix/media/r0/upload", true, data, new Dictionary<string, string>() { { "Content-Type", contentType } });
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                // Parse response
                return ParseMediaUpload(result);
            }

            return "";
        }

        public string GetMediaDownloadUri(string contentUrl)
        {
            if (!contentUrl.StartsWith("mxc://"))
                return string.Empty;

            var newUrl = contentUrl.Remove(0, 6);
            var contentUrlSplit = newUrl.Split('/');
            if (contentUrlSplit.Count() < 2)
                return string.Empty;

            string uriPath = HttpBackend.GetPath(string.Format("/_matrix/media/r0/download/{0}/{1}", contentUrlSplit[0], contentUrlSplit[1]), false);
            return uriPath;
        }

        public async void JoinedRooms()
        {
            var tuple = await HttpBackend.Get("/_matrix/client/r0/joined_rooms", true);
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                // Parse joined rooms
                ParseJoinedRooms(result);
            }
        }

        public async Task<bool> InviteToRoom(string roomId, string userId)
        {
            Requests.Rooms.MatrixRoomInvite invite = new Requests.Rooms.MatrixRoomInvite() { UserID = userId };
            var tuple = await HttpBackend.Post(string.Format("/_matrix/client/r0/rooms/{0}/invite", System.Uri.EscapeDataString(roomId)), true, Helpers.JsonHelper.Serialize(invite));
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                return true;
            }

            throw new MatrixException(err.ToString());
        }

        public async Task<bool> JoinRoom(string roomId)
        {
            Requests.Rooms.MatrixRoomJoin roomJoin = new Requests.Rooms.MatrixRoomJoin();
            var tuple = await HttpBackend.Post(string.Format("/_matrix/client/r0/rooms/{0}/join", Uri.EscapeDataString(roomId)), true, Helpers.JsonHelper.Serialize(roomJoin));
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> CreateRoom(string roomName, string roomTopic, bool isDirect = false)
        {
            if (string.IsNullOrEmpty(roomName))
                return false;

            Requests.Rooms.MatrixRoomCreate roomCreate = new Requests.Rooms.MatrixRoomCreate
            {
                Name = roomName,
                IsDirect = isDirect
            };
            if (string.IsNullOrEmpty(roomTopic))
                roomCreate.Topic = roomTopic;

            var tuple = await HttpBackend.Post("/_matrix/client/r0/createRoom", true, Helpers.JsonHelper.Serialize(roomCreate));
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                try
                {
                    ParseCreatedRoom(result);
                    return true;
                }
                catch (MatrixException)
                {
                    // Failed to create the room
                    return false;
                }
            }

            return false;
        }

        public async Task<bool> AddRoomAlias(string roomId, string alias)
        {
            Requests.Rooms.MatrixRoomAddAlias roomAddAlias = new Requests.Rooms.MatrixRoomAddAlias
            {
                RoomID = roomId
            };

            var tuple = await HttpBackend.Put(string.Format("/_matrix/client/r0/directory/room/{0}", Uri.EscapeDataString(alias)), true, Helpers.JsonHelper.Serialize(roomAddAlias));
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteRoomAlias(string roomAlias)
        {
            var tuple = await HttpBackend.Delete(string.Format("/_matrix/client/r0/directory/room/{0}", Uri.EscapeDataString(roomAlias)), true);
            MatrixRequestError err = tuple.Item1;
            if (err.IsOk)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> LeaveRoom(string roomId)
        {
            var tuple = await HttpBackend.Post(string.Format("/_matrix/client/r0/rooms/{0}/leave", Uri.EscapeDataString(roomId)), true, "");
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                return true;
            }

            return false;
        }

        public async void RoomTypingSend(string roomId, bool typing, int timeout = 0)
        {
            Requests.Rooms.MatrixRoomSendTyping req = new Requests.Rooms.MatrixRoomSendTyping() { Typing = typing };
            if (timeout > 0)
                req.Timeout = timeout;

            var tuple = await HttpBackend.Put(string.Format("/_matrix/client/r0/rooms/{0}/typing/{1}", Uri.EscapeDataString(roomId), Uri.EscapeDataString(UserID)), true, Helpers.JsonHelper.Serialize(req));
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (!err.IsOk)
                throw new MatrixException(err.ToString());
        }

        public async Task<bool> GetRoomState(string roomId, string eventType = null, string stateKey = null)
        {
            string url = string.Format("/_matrix/client/r0/rooms/{0}/state", roomId);
            if (!string.IsNullOrEmpty(eventType))
                url += "/" + eventType;
            if (!string.IsNullOrEmpty(stateKey))
                url += "/" + stateKey;

            var tuple = await HttpBackend.Get(url, true);
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                // Parse stuff
                // Parsing will differ if there is no eventType specified

                if (!string.IsNullOrEmpty(eventType))
                {

                }
                else
                {

                }

                return true;
            }

            return false;
        }

        private async Task<bool> SendEventToRoom(string roomId, string eventType, string content)
        {
            var tuple = await HttpBackend.Put(string.Format("/_matrix/client/r0/rooms/{0}/send/{1}/{2}", Uri.EscapeDataString(roomId), eventType, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()), true, content);
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
                return true;

            return false;
        }
        public async Task<bool> SendTextMessageToRoom(string roomId, string message)
        {
            Requests.Rooms.Message.MatrixRoomMessageText req = new Requests.Rooms.Message.MatrixRoomMessageText()
            {
                Body = message
            };

            return await SendEventToRoom(roomId, "m.room.message", Helpers.JsonHelper.Serialize(req));
        }

        public async Task<bool> SendTextMessageToRoom(string roomId, string message, string formattedMessage)
        {
            Requests.Rooms.Message.MatrixRoomMessageText req = new Requests.Rooms.Message.MatrixRoomMessageText()
            {
                Body = message,
                Format = "org.matrix.custom.html",
                FormattedBody = formattedMessage
            };

            return await SendEventToRoom(roomId, "m.room.message", Helpers.JsonHelper.Serialize(req));
        }

        public async Task<bool> SendLocationToRoom(string roomId, string description, double lat, double lon)
        {
            StringBuilder sb = new StringBuilder("geo:");
            sb.Append(lat);
            sb.Append(",");
            sb.Append(lon);
            Requests.Rooms.Message.MatrixRoomMessageLocation req = new Requests.Rooms.Message.MatrixRoomMessageLocation()
            {
                Description = description,
                GeoUri = sb.ToString()
            };

            return await SendEventToRoom(roomId, "m.room.message", Helpers.JsonHelper.Serialize(req));
        }

        public async Task<bool> SetPusher(string pushUrl, string pushKey)
        {
            Requests.Pushers.MatrixSetPusher req = new Requests.Pushers.MatrixSetPusher()
            {
                PushKey = pushKey,
                Kind = "http",
                AppID = ApplicationID,
                AppDisplayName = ApplicationName,
                DeviceDisplayName = DeviceName,
                Language = "en",
                Append = false
            };
            req.Data = new Requests.Pushers.MatrixPusherData()
            {
                Url = pushUrl,
                Format = "event_id_only"
            };

            var jsonData = Helpers.JsonHelper.Serialize(req);

            var tuple = await HttpBackend.Post("/_matrix/client/r0/pushers/set", true, jsonData);
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> GetNotifications(string from = "", int limit = -1, string only = "")
        {
            StringBuilder url = new StringBuilder("/_matrix/client/r0/notifications");

            Dictionary<string, string> urlParams = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(from))
                urlParams.Add("from", from);
            if (limit != -1)
                urlParams.Add("limit", limit.ToString());
            if (!string.IsNullOrEmpty(only))
                urlParams.Add("only", only);

            if (urlParams.Count > 0)
            {
                var enc = new System.Net.Http.FormUrlEncodedContent(urlParams);
                url.Append("?" + enc.ReadAsStringAsync().Result);
            }

            var tuple = await HttpBackend.Get(url.ToString(), true);
            MatrixRequestError err = tuple.Item1;
            string result = tuple.Item2;
            if (err.IsOk)
            {
                // Parse the response
                ParseNotifications(result);
                return true;
            }
            return false;
        }

        private bool isRunningSync = false;

        public bool shouldFullSync = false;

        private bool Running { get; set; }

        public async Task StartSync(TimeSpan interval)
        {
            Running = true;
            while (Running)
            {
                await SyncQueue();
                await Task.Delay(interval);
            }
        }


        public async Task SyncQueue()
        {

            if (!isRunningSync)
            {
                isRunningSync = true;
                try
                {
                    if (shouldFullSync)
                    {
                        await ClientSync(true, true);
                        shouldFullSync = false;
                    }
                    else
                        await ClientSync(true);
                }
                catch (Exception e)
                {

                    Debug.WriteLine("Sync Exception: " + e.Message);
                }

                FlushMessageQueue();

                isRunningSync = false;
            }
        }

        public void StopSync()
        {
            Running = false;

            FlushMessageQueue();
        }

        private string[] ParseClientVersions(string resp)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(resp)))
                {
                    var ser = new DataContractJsonSerializer(typeof(Responses.VersionResponse));
                    Responses.VersionResponse response = (ser.ReadObject(stream) as Responses.VersionResponse);

                    return response.Versions;
                }
            }
            catch
            {
                throw new MatrixException("Failed to parse ClientVersions");
            }
        }

        private void ParseLoginResponse(string resp)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(resp)))
                {
                    var ser = new DataContractJsonSerializer(typeof(Responses.Session.LoginResponse));
                    Responses.Session.LoginResponse response = (ser.ReadObject(stream) as Responses.Session.LoginResponse);

                    this.UserID = response.UserID;
                    this.DeviceID = response.DeviceID;
                    //this.HomeServer = response.HomeServer;

                    this.HttpBackend.SetAccessToken(response.AccessToken);

                    Events.FireLoginEvent(response);
                }
            }
            catch
            {
                throw new MatrixException("Failed to parse LoginResponse");
            }
        }

        private Responses.UserData.UserProfileResponse ParseUserProfile(string resp)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(resp)))
                {
                    var ser = new DataContractJsonSerializer(typeof(Responses.UserData.UserProfileResponse));
                    Responses.UserData.UserProfileResponse response = (ser.ReadObject(stream) as Responses.UserData.UserProfileResponse);

                    return response;
                }
            }
            catch
            {
                return null;
                //throw new MatrixException("Failed to parse UserProfile");
            }
        }

        private async Task ParseClientSync(string resp)
        {
            try
            {
                {
            
                    Responses.MatrixSync response = Newtonsoft.Json.JsonConvert.DeserializeObject<Responses.MatrixSync>(resp);

                    if (response == null) return;

                     SyncToken = response.NextBatch;

                    if (response.Rooms?.Join != null)
                    {
                        foreach (var room in response.Rooms.Join)
                        {
                            // Fire event for room joined
                            Events.FireRoomJoinEvent(room.Key, room.Value);
                        }
                    }

                    if (response.Rooms?.Invite != null)
                    {
                        foreach (var room in response.Rooms.Invite)
                        {
                            // Fire event for invite
                            Events.FireRoomInviteEvent(room.Key, room.Value);
                        }
                    }

                    if (response.Rooms?.Leave != null)
                    {
                        foreach (var room in response.Rooms.Leave)
                        {
                            // Fire event for room leave
                            Events.FireRoomLeaveEvent(room.Key, room.Value);
                        }
                    }

                    if (response.Presense != null)
                    {
                        foreach (var evt in response.Presense.Events)
                        {
                            var actualEvent = evt as Responses.Events.Presence;
                            bool active = actualEvent.Content.CurrentlyActive;
                        }
                    }

                    if (response.AccountData != null)
                    {
                        foreach (var evt in response.AccountData.Events)
                        {
                            Debug.WriteLine("AccountData Event: " + evt.Type);
                            Events.FireAccountDataEvent(evt);
                        }
                    }

                    // Do stuff
                    IsConnected = true;
                }
            }
            catch (Exception e)
            {
                throw new MatrixException("Failed to parse ClientSync - " + e.Message);
            }
        }

        private string ParseMediaUpload(string resp)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(resp)))
                {
                    var ser = new DataContractJsonSerializer(typeof(Responses.Media.MediaUploadResponse));
                    Responses.Media.MediaUploadResponse response = (ser.ReadObject(stream) as Responses.Media.MediaUploadResponse);

                    return response.ContentUri;
                }
            }
            catch
            {
                throw new MatrixException("Failed to parse MediaUpload");
            }
        }

        private void ParseJoinedRooms(string resp)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(resp)))
                {
                    var ser = new DataContractJsonSerializer(typeof(Responses.JoinedRooms));
                    Responses.JoinedRooms response = (ser.ReadObject(stream) as Responses.JoinedRooms);

                    var thing = response.Rooms;
                }
            }
            catch
            {
                throw new MatrixException("Failed to parse JoinedRooms");
            }
        }

        private void ParseCreatedRoom(string resp)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(resp)))
                {
                    var ser = new DataContractJsonSerializer(typeof(Responses.Rooms.CreateRoom));
                    Responses.Rooms.CreateRoom response = (ser.ReadObject(stream) as Responses.Rooms.CreateRoom);

                    Events.FireRoomCreateEvent(response.RoomID);
                }
            }
            catch
            {
                throw new MatrixException("Failed to parse CreatedRoom");
            }
        }

        private void ParseNotifications(string resp)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(resp)))
                {
                    var ser = new DataContractJsonSerializer(typeof(Responses.Pushers.MatrixNotifications));
                    Responses.Pushers.MatrixNotifications response = (ser.ReadObject(stream) as Responses.Pushers.MatrixNotifications);

                    foreach (var notification in response.Notifications)
                    {
                        Events.FireNotificationEvent(notification);
                    }
                }
            }
            catch
            {

            }
        }


    }
}

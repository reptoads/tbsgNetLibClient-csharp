using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Configuration;
using Newtonsoft.Json;

namespace NetLib.WebAPI
{

    public struct Session
    {
        public Session(bool valid, string token, uint profileId, string error,string username)
        {
            this.valid = valid;
            this.token = token;
            this.profileId = profileId;
            this.error = error;
            this.username = username;
        }

        public bool Valid
        {
            get => valid;
            set => valid = value;
        }

        public string Token
        {
            get => token;
            set => token = value;
        }

        public uint ProfileId
        {
            get => profileId;
            set => profileId = value;
        }

        public string Error
        {
            get => error;
            set => error = value;
        }

        public string Username
        {
            get => username;
            set => username = value;
        }

        private bool valid;
        private string token;
        private uint profileId;
        private string error;
        private string username;
    }
    public static class Login
    {

        private struct TokenValid
        {
            public bool Valid
            {
                get => valid;
                set => valid = value;
            }

            public string Error
            {
                get => error;
                set => error = value;
            }

            private bool valid;
            private string error;
        }

        public static Session Request(string username, string password)
        {
            var loginRequest = APIRequest.Request(Service.Login,WebMethod.POST,new List<RequestData>()
            {
                new RequestData(){Key = "username",Value = username},
                new RequestData(){Key = "password",Value = password}
            });

            if (loginRequest.StatusCode == 200) { 
                var result = JsonConvert.DeserializeObject<Session>(loginRequest.Content);
                result.Username = username;
                return result;
            }

            return new Session(false, null, 0, loginRequest.Content, username);
        }
        public static bool ValidateToken(Session session)
        {
            return ValidateToken(session.Token);
        }
        public static bool ValidateToken(string token)
        {
            var response = APIRequest.Request(Service.ValidateToken, WebMethod.POST, new List<RequestData>()
            {
                new RequestData(){Key = "token",Value = token}
            });
            if (response.StatusCode == 200)
            {
                var isTokenValid = JsonConvert.DeserializeObject<TokenValid>(response.Content);
                return isTokenValid.Valid;
            }

            return false;
        }
        public static void SaveSessionToDisk(Session session,bool binary = false)
        {
            if (session.Valid)
            {
                if (binary)
                {
                    using (System.IO.BinaryWriter file =
                        new System.IO.BinaryWriter(File.Open(@SessionFile, FileMode.Create)))
                    {
                        file.Write(NetUtils.Base64Encode(session.Token));
                        file.Write(NetUtils.Base64Encode(session.Username));
                        file.Write(session.ProfileId);
                    }
                    return;
                }
                using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(@SessionFile,false))
                {
                    file.WriteLine(session.Token);
                    file.WriteLine(session.Username);
                    file.WriteLine(session.ProfileId);
                }
            }
        }
        public static Session LoadLocalSessionFromDisk(bool binary = false)
        {
            Session session = new Session();
            if (!File.Exists(@SessionFile))
            {
                session.Valid = false;
                return session;
            }

            if (binary)
            {
                using (BinaryReader reader = new BinaryReader(File.Open(@SessionFile, FileMode.Open)))
                {
                    session.Token = NetUtils.Base64Decode(reader.ReadString());
                    session.Username = NetUtils.Base64Decode(reader.ReadString());
                    session.ProfileId = (uint) reader.ReadInt32();
                }

                return session;
            }
            else
            {
                string line = "";
                uint counter = 0;
                System.IO.StreamReader file =
                    new System.IO.StreamReader(@SessionFile);
                while ((line = file.ReadLine()) != null)
                {
                    switch (counter)
                    {
                        case 0:
                            session.Token = line;
                            break;
                        case 1:
                            session.Username = line;
                            break;
                        case 2:
                            session.ProfileId = (uint) Int32.Parse(line);
                            break;
                    }

                    ++counter;
                }

                file.Close();
                //validate session
                return session;
            }

        }

        public static bool CanFindLocalSessionToken()
        {
            return File.Exists(@SessionFile);
        }

        public static string SessionFile
        {
            get => sessionFile;
            set => sessionFile = value;
        }

        private static string sessionFile;
    }

}

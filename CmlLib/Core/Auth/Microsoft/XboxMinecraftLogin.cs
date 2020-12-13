﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CmlLib.Core.Mojang;

namespace CmlLib.Core.Auth.Microsoft
{
    public class XboxMinecraftLogin
    {
        public const string RelyingParty = "rp://api.minecraftservices.com/";

        private void writeReq(WebRequest req, string data)
        {
            using(var reqStream = req.GetRequestStream())
            using(var sw = new StreamWriter(reqStream))
            {
                sw.Write(data);
            }
        }

        private string readRes(WebResponse res)
        {
            using (var resStream = res.GetResponseStream())
            using (var sr = new StreamReader(resStream))
            {
                return sr.ReadToEnd();
            }
        }

        // login_with_xbox
        public AuthenticationResponse LoginWithXbox(string uhs, string xstsToken)
        {
            var url = "https://api.minecraftservices.com/authentication/login_with_xbox";
            var req = WebRequest.CreateHttp(url);
            req.ContentType = "application/json";
            req.Method = "POST";

            var reqBody = $"{{\"identityToken\": \"XBL3.0 x={uhs};{xstsToken}\"}}";
            writeReq(req, reqBody);

            var res = req.GetResponse();
            var resBody = readRes(res);

            return JsonConvert.DeserializeObject<AuthenticationResponse>(resBody);
        }

        public MLoginResponse RequestSession(string accessToken)
        {
            try
            {
                if (!MojangAPI.CheckGameOwnership(accessToken))
                    return new MLoginResponse(MLoginResult.NoProfile, null, null, null);

                var profile = MojangAPI.GetProfileUsingToken(accessToken);
                var session = new MSession
                {
                    Username = profile.Name,
                    AccessToken = accessToken,
                    UUID = profile.UUID
                };

                return new MLoginResponse(MLoginResult.Success, session, null, null);
            }
            catch (Exception ex)
            {
                return new MLoginResponse(MLoginResult.UnknownError, null, ex.ToString(), null);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Plugins.LoginServer
{
    public class UAccountData : IO.BaseSerializer
    {
        [Rtti.Meta]
        public string UserName { get; set; }
        [Rtti.Meta]
        public string Password { get; set; }
        [Rtti.Meta]
        public Guid SessionId { get; set; }
    }
    public class UAccountManager
    {
        public Dictionary<string, UAccountData> Accounts { get; } = new Dictionary<string, UAccountData>();
        public UAccountData LoginAccount(string user, string psw)
        {
            UAccountData result;
            if (Accounts.TryGetValue(user, out result))
            {
                if (result.Password == psw)
                    return result;
                return null;
            }
            result = new UAccountData();
            result.UserName = user;
            result.Password = psw;
            //load from db
            result.SessionId = Guid.NewGuid();
            Accounts.Add(user, result);
            return result;
        }
        public bool LogoutAccount(string user)
        {
            UAccountData result;
            if (Accounts.TryGetValue(user, out result))
            {
                Accounts.Remove(user);
                return true;
            }
            return false;
        }
    }
}

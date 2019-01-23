using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StackExchange.Redis;


namespace CommonServerLib
{
    public class RedisLib
    {
        ConnectionMultiplexer Connection;
        IDatabase Db;


        public void Init(string address)
        {
            Connection = ConnectionMultiplexer.Connect(address);

            if(Connection.IsConnected)
            {
                Db = Connection.GetDatabase();
            }
        }

        
        public Task<RedisValue> GetString(string key)
        {
            return Db.StringGetAsync(key);
        }   
        

        
    }
}

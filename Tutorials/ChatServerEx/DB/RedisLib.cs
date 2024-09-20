using System.Threading.Tasks;

using StackExchange.Redis;


namespace DB;

public class RedisLib
{
    ConnectionMultiplexer _connection;
    IDatabase _db;


    public void Init(string address)
    {
        _connection = ConnectionMultiplexer.Connect(address);

        if(_connection.IsConnected)
        {
            _db = _connection.GetDatabase();
        }
    }

    
    public Task<RedisValue> GetString(string key)
    {
        return _db.StringGetAsync(key);
    }   
    

    
}

using System;

using MessagePack;

using CSBaseLib;


namespace DB;

class DBJobWorkHandler
{
    RedisLib RefRedis = null;

    public Tuple<ErrorCode,string> Init(RedisLib redis)
    {
        try
        {
            RefRedis = redis;

            // 미리 Redis와 연결이 되도록 여기서 더미 데이터를 요청한다.
            RefRedis.GetString("test");

            return new Tuple<ErrorCode, string>(ErrorCode.None, "");
        }
        catch(Exception ex)
        {
            return new Tuple<ErrorCode, string>(ErrorCode.RedisInitFail, ex.ToString());
        }
    }

    public DBResultQueue RequestLogin(DBQueue dbQueue)
    {
        var sessionID = dbQueue.SessionID;
        var sessionIndex = dbQueue.SessionIndex;
        var userID = "UnKnown";
        
        try
        {
            var reqData = MessagePackSerializer.Deserialize<DBReqLogin>(dbQueue.Datas);
            userID = reqData.UserID;

            // 필드 단위로 읽어 올 때는 꼭 Key가 있는지 확인 해야 한다!!!
            var redis = RefRedis.GetString(reqData.UserID);
            var value = redis.Result;                
            if (value.IsNullOrEmpty)
            {
                return RequestLoginValue(ErrorCode.DbLoginEmptyUser, userID, sessionID, sessionIndex);
            }
                                            
            if( reqData.AuthToken != value)
            {
                return RequestLoginValue(ErrorCode.DbLoginInvalidPassword, userID, sessionID, sessionIndex);
            }
            else
            {
                return RequestLoginValue(ErrorCode.None, userID, sessionID, sessionIndex);
            }
        }
        catch
        {
            return RequestLoginValue(ErrorCode.DbLoginException, userID, sessionID, sessionIndex);
        }
    }

    DBResultQueue RequestLoginValue(ErrorCode result, string userID, string sessionID, int sessionIndex)
    {
        var returnData = new DBResultQueue()
        {
            PacketID = PacketId.ResDbLogin,
            SessionID = sessionID,
            SessionIndex = sessionIndex
        };

        var resLoginData = new DBResLogin() { UserID = userID, Result = result };
        returnData.Datas = MessagePackSerializer.Serialize(resLoginData);
        
        return returnData;
    }

} // DBJobWorkHandler End

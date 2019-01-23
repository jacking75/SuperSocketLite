using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MessagePack;

using CSBaseLib;

namespace CommonServerLib
{
    class DBJobWorkHandler
    {
        RedisLib RefRedis = null;

        public Tuple<ERROR_CODE,string> Init(RedisLib redis)
        {
            try
            {
                RefRedis = redis;

                // 미리 Redis와 연결이 되도록 여기서 더미 데이터를 요청한다.
                RefRedis.GetString("test");

                return new Tuple<ERROR_CODE,string>(ERROR_CODE.NONE, "");
            }
            catch(Exception ex)
            {
                return new Tuple<ERROR_CODE, string>(ERROR_CODE.REDIS_INIT_FAIL, ex.ToString());
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
                    return RequestLoginValue(ERROR_CODE.DB_LOGIN_EMPTY_USER, userID, sessionID, sessionIndex);
                }
                                                
                if( reqData.AuthToken != value)
                {
                    return RequestLoginValue(ERROR_CODE.DB_LOGIN_INVALID_PASSWORD, userID, sessionID, sessionIndex);
                }
                else
                {
                    return RequestLoginValue(ERROR_CODE.NONE, userID, sessionID, sessionIndex);
                }
            }
            catch
            {
                return RequestLoginValue(ERROR_CODE.DB_LOGIN_EXCEPTION, userID, sessionID, sessionIndex);
            }
        }

        DBResultQueue RequestLoginValue(ERROR_CODE result, string userID, string sessionID, int sessionIndex)
        {
            var returnData = new DBResultQueue()
            {
                PacketID = PACKETID.RES_DB_LOGIN,
                SessionID = sessionID,
                SessionIndex = sessionIndex
            };

            var resLoginData = new DBResLogin() { UserID = userID, Result = result };
            returnData.Datas = MessagePackSerializer.Serialize(resLoginData);
            
            return returnData;
        }

    } // DBJobWorkHandler End
}

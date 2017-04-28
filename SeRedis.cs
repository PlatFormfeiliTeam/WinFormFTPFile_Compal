
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace WinFormFTPFile_Compal
{

    public static class SeRedis
    {

        private static string constr = ConfigurationManager.AppSettings["redisserver"];

        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            ConfigurationOptions co = new ConfigurationOptions()
            {
                SyncTimeout = 500000,
                EndPoints =
            {
                {constr,6379 }
            },
                AbortOnConnectFail = false // this prevents that error
            };
            return ConnectionMultiplexer.Connect(co);

        });

        public static ConnectionMultiplexer redis
        {
            get
            {
                return lazyConnection.Value;
            }
        }
    }
}
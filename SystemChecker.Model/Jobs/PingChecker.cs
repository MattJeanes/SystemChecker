using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using SystemChecker.Model.Data;
using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Enums;
using SystemChecker.Model.Helpers;
using SystemChecker.Model.Loggers;

namespace SystemChecker.Model.Jobs
{
    public class PingChecker : BaseChecker
    {
        public PingChecker(ICheckerHelper helper) : base(helper) { }
        private enum Settings
        {
            Server = 12,
            TimeoutMS = 13
        }

        public async override Task<CheckResult> PerformCheck(Check check, ISettings settings, ICheckLogger logger, CheckResult result)
        {
            string server = check.Data.TypeOptions[((int)Settings.Server).ToString()];
            int timeoutMS = check.Data.TypeOptions[((int)Settings.TimeoutMS).ToString()];

            if (string.IsNullOrEmpty(server))
            {
                throw new Exception("Server invalid");
            }
            logger.Info($"Pinging server {server}");

            var reply = await Ping(server, timeoutMS);

            result.TimeMS = (int)reply.RoundtripTime;

            if (reply.Status != IPStatus.Success)
            {
                logger.Error($"Ping failed: {reply.Status.ToString()}");
                if (reply.Status == IPStatus.TimedOut)
                {
                    logger.Error($"Timeout after {timeoutMS}ms");
                    result.Status = CheckResultStatus.Timeout;
                }
                else
                {
                    result.Status = CheckResultStatus.Failed;
                }
            }

            return result;
        }

        protected async Task<PingReply> Ping(string server, int timeoutMS)
        {
            var pingSender = new Ping();
            var options = new PingOptions
            {
                // Use the default Ttl value which is 128,
                // but change the fragmentation behavior.
                DontFragment = true
            };

            // Create a buffer of 32 bytes of data to be transmitted.
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            var reply = await pingSender.SendPingAsync(server, timeoutMS, buffer, options);
            return reply;
        }
    }
}

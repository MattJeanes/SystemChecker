using System;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Data.Enums;
using SystemChecker.Model.Helpers;

namespace SystemChecker.Model.Jobs
{
    public class PingChecker : BaseChecker
    {
        public PingChecker(ICheckerHelper helper) : base(helper) { }

        public class Settings
        {
            public string ServerAddress { get; set; }
            public int TimeoutMS { get; set; }
        }

        public override async Task<CheckResult> PerformCheck(CheckResult result)
        {
            var settings = _check.Data.GetTypeOptions<Settings>();
            var server = settings.ServerAddress;
            var timeoutMS = settings.TimeoutMS;

            if (string.IsNullOrEmpty(server))
            {
                throw new Exception("Server invalid");
            }
            _logger.Info($"Pinging server {server}");

            var reply = await Ping(server, timeoutMS);

            result.TimeMS = (int)reply.RoundtripTime;

            if (reply.Status != IPStatus.Success)
            {
                _logger.Error($"Ping failed: {reply.Status.ToString()}");
                if (reply.Status == IPStatus.TimedOut)
                {
                    _logger.Error($"Timeout after {timeoutMS}ms");
                    result.Status = _helper.GetResultStatus(ResultStatusEnum.Timeout);
                }
                else
                {
                    result.Status = _helper.GetResultStatus(ResultStatusEnum.Failed);
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
            var data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            var buffer = Encoding.ASCII.GetBytes(data);
            var reply = await pingSender.SendPingAsync(server, timeoutMS, buffer, options);
            return reply;
        }
    }
}

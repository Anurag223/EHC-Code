using System;
using System.Collections.Generic;
using System.Text;
using TLM.EHC.Common.Historian;
using TLM.EHC.Common.Models;

namespace TLM.EHC.API.HyperLinks
{
    public class HyperLinkBuilder
    {
        private const string ApiPrefix = "/v2";
        private const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
        private readonly List<string> _baseUrl;
        private readonly Dictionary<string, string> _parameters;


        public HyperLinkBuilder()
        {
            _baseUrl = new List<string>();
            _baseUrl.Add(ApiPrefix);
            _parameters = new Dictionary<string, string>();
        }


        public HyperLinkBuilder UseEquipment(string wkeid)
        {
            _baseUrl.Add("/equipment/" + wkeid);
            return this;
        }


        public HyperLinkBuilder UseChannels()
        {
            _baseUrl.Add("/channels");
            return this;
        }


        public HyperLinkBuilder UseReadings()
        {
            _baseUrl.Add("/readings");
            return this;
        }

        public HyperLinkBuilder UseEpisodicPoints()
        {
            _baseUrl.Add("/episodic-points");
            return this;
        }


        public HyperLinkBuilder UseSingleCode(string channelCode)
        {
            _baseUrl.Add("/" + channelCode);
            return this;
        }


        public HyperLinkBuilder SetMultipleCodes(string[] codes)
        {
            if (codes.Length > 0)
            {
                _parameters["codes"] = string.Join(',', codes);
            }
            
            return this;
        }


        public HyperLinkBuilder SetTimePeriod(TimePeriod timePeriod)
        {
            _parameters["start"] = timePeriod.Start.ToString(DateTimeFormat);
            _parameters["end"] = timePeriod.End.ToString(DateTimeFormat);
            return this;
        }

        public HyperLinkBuilder SetEpisodeId(string episodeId)
        {
            _parameters["episodeId"] = episodeId;
            return this;
        }


        public string GetLinkValue()
        {
            var result = new StringBuilder();

            foreach (string baseUrlPart in _baseUrl)
            {
                result.Append(baseUrlPart);
            }

            bool useAmpersand = false;

            foreach (var pair in _parameters)
            {
                result.Append(useAmpersand ? "&" : "?");
                result.Append(pair.Key).Append("=").Append(pair.Value);
                useAmpersand = true;
            }

            return result.ToString();
        }

    }
}

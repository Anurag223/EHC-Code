using System.Collections.Generic;

namespace TLM.EHC.API.ControllerModels
{
    /// <summary>
    /// Object for posting MultipleChannelRequest for channels and readings.
    /// </summary>
    public class MultipleChannelsRequest
    {
        /// <summary>
        /// Meta tag in RequestMeta format.
        /// </summary>
        public RequestMeta Meta { get; set; }
        /// <summary>
        /// List of rows for time series
        /// </summary>
        public List<List<object>> Rows { get; set; }
    }

    /// <summary>
    /// RequestMeta.
    /// </summary>
    public class RequestMeta
    {
        /// <summary>
        /// List of channels in ChannelRequest format.
        /// </summary>
        public List<ChannelRequest> Channels { get; set; }
    }
}
using System;
using System.Linq;
using TLM.EHC.API.ControllerModels;
using TLM.EHC.API.Controllers;
using TLM.EHC.Common.Historian;
using TLM.EHC.Common.Models;

namespace TLM.EHC.API.HyperLinks
{
    public class HyperLinksProvider : IHyperLinksProvider
    {
        public HyperLinkDictionary GetHyperLinks(RowsRequest rowsRequest, TimePeriod timePeriod)
        {
            var hyperLinks = new HyperLinkDictionary();

            var builder = GetBuilderSelf(rowsRequest);
            string self = builder.GetLinkValue();
            hyperLinks.Add("self", new HyperLink("self", self));

            if (timePeriod != null)
            {
                builder.SetTimePeriod(timePeriod.GetPrevious24h());
                string previousPeriod = builder.GetLinkValue();
                hyperLinks.Add("previousPeriod", new HyperLink("previousPeriod", previousPeriod));
            }
            

            string equipment = new HyperLinkBuilder().UseEquipment(rowsRequest.WKEid.Value).GetLinkValue();
            hyperLinks.Add("equipment", new HyperLink("equipment", equipment));

            return hyperLinks;
        }


        private HyperLinkBuilder GetBuilderSelf(RowsRequest rowsRequest)
        {
            var builder = new HyperLinkBuilder();
            builder.UseEquipment(rowsRequest.WKEid.Value);

            switch (rowsRequest.DataType)
            {
                case DataType.Channel:
                    builder.UseChannels();
                    break;

                case DataType.Reading:
                    builder.UseReadings();
                    break;

                case DataType.Episodic:
                    builder.UseEpisodicPoints();
                    break;

                default:
                    throw new ArgumentException("Unexpected value: " + rowsRequest.DataType);
            }

            switch (rowsRequest.QueryType)
            {
                case QueryType.SingleCode:
                    builder.UseSingleCode(rowsRequest.Codes.Single());
                    break;

                case QueryType.MultipleCodes:
                    builder.SetMultipleCodes(rowsRequest.Codes);
                    break;

                default:
                    throw new ArgumentException("Unexpected value: " + rowsRequest.DataType);
            }

            if (rowsRequest.TimePeriod != null)
            {
                builder.SetTimePeriod(rowsRequest.TimePeriod);
            }

            if (rowsRequest.EpisodeId != null)
            {
                builder.SetEpisodeId(rowsRequest.EpisodeId);
            }

            return builder;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TLM.EHC.Common.Historian;
using TLM.EHC.Common.Models;
using TLM.EHC.Common.Services;

namespace TLM.EHC.API.Services
{
    public interface IEpisodicPointService
    {
        Task<bool> AnyPointsExists(string episodeId, string equipmentWkeId);
    }


    public class EpisodicPointService : IEpisodicPointService
    {
        private readonly IHistorianClient _historianClient;
        private readonly IEquipmentProvider _equipmentProvider;
        private readonly IEquipmentModelProvider _equipmentModelProvider;

        public EpisodicPointService(
            IHistorianClient historianClient,
            IEquipmentProvider equipmentProvider,
            IEquipmentModelProvider equipmentModelProvider
        )
        {
            _historianClient = historianClient;
            _equipmentProvider = equipmentProvider;
            _equipmentModelProvider = equipmentModelProvider;
        }


        // SELECT * FROM "DXJ_WPS_BLENDING_EQUIPMENT"."autogen"."WS-63_STIMULATION_BLENDER_PROP.Episodic" WHERE "Episode"='5db34f1114d0de000156c0dc' AND "EquipmentInstance"='100196736:SBF62413A0180' LIMIT 1

        public async Task<bool> AnyPointsExists(string episodeId, string equipmentWkeId)
        {
            var equipment = await _equipmentProvider.GetEquipmentByWkeid(WellKnownEntityId.Parse(equipmentWkeId));

            if (equipment == null)
            {
                return false;
            }

            EquipmentModel equipmentModel = await _equipmentModelProvider.GetEquipmentModelByCode(equipment.EquipmentCode);

            var influxPath = InfluxPath.GetFromEquipmentModel(equipmentModel);

            var queryBuilder = new QueryBuilder()
                .UseTechnology(influxPath.Technology)
                .UseBrand(influxPath.Brand)
                .UseDataType(DataType.Episodic)
                .FilterByEpisodeId(episodeId)
                .FilterByWkeId(equipmentWkeId)
                .SelectAllFields();

            var query = queryBuilder.GetQueryForLatestTimestamp();
            var result = await _historianClient.PerformQuery(query);

            return (result != null);
        }
    }


}

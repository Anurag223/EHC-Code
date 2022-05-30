using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Tlm.Sdk.Core.Data;
using Tlm.Sdk.Core.Models.Querying;
using  TLM.EHC.API.ControllerModels.Separated;
using TLM.EHC.Common.Exceptions;

namespace TLM.EHC.API.Services
{
    public interface IEpisodeService
    {
        Task<string> CreateEpisode(Episode episode);
        Task UpdateEpisode(Episode episode);
        Task DeleteEpisode(string episodeId);
        Task<Episode> GetEpisodeById(string episodeId);
        Task DeleteOldChildEpisodes(Episode newChildEpisode);
    }

    public class EpisodeService : IEpisodeService
    {
        private static readonly ILogger Logger = Log.Logger.ForContext<EpisodeService>();

        private readonly IRepositoryHandler<Episode> _episodeRepositoryHandler;


        public EpisodeService(IRepositoryHandler<Episode> episodeRepositoryHandler)
        {
            _episodeRepositoryHandler = episodeRepositoryHandler;
        }


        // see https://slb-it.visualstudio.com/es-TLM-federation/_workitems/edit/897302

        private void MakeDictionaryKeysLowerCase<T>(Dictionary<string, T> dic)
        {
            var badKeys = dic.Keys.Where(key => key.ToLowerInvariant() != key).ToArray();

            foreach (string badKey in badKeys)
            {
                dic.Add(badKey.ToLowerInvariant(), dic[badKey]);
                dic.Remove(badKey);
            }
        }

        public async Task<string> CreateEpisode(Episode episode)
        {
            if (episode.Id != null)
            {
                throw new BadRequestException("EpisodeId should be null.");
            }

            MakeDictionaryKeysLowerCase(episode.Relationships);

            foreach (var rel in episode.Relationships.Values)
            {
                MakeDictionaryKeysLowerCase(rel.Data);
                MakeDictionaryKeysLowerCase(rel.Links);
            }

            await _episodeRepositoryHandler.UpdateAsync(episode);

            if (string.IsNullOrWhiteSpace(episode.Id))
            {
                throw new ServerErrorException("Got empty id after creating an episode.'");
            }

            return episode.Id;
        }

        public async Task UpdateEpisode(Episode episode)
        {
            await _episodeRepositoryHandler.UpdateAsync(episode);
        }

        public async Task DeleteEpisode(string episodeId)
        {
            long? count = await _episodeRepositoryHandler.DeleteManyAsync(DeleteSpec.ById(episodeId));

            if (count == null || count.Value == 0)
            {
                throw new NotFoundException("Episode not found: " + episodeId) { ErrorCode = ErrorCodes.EpisodeNotFound }; 
            }
        }

        public async Task<Episode> GetEpisodeById(string episodeId)
        {
            var list = await _episodeRepositoryHandler.GetAsync(x => x.Id == episodeId);
            
            if (list.Count == 0)
            {
                throw new NotFoundException("Episode not found: " + episodeId) { ErrorCode= ErrorCodes.EpisodeNotFound};
            }

            return list.Single();
        }


        public async Task DeleteOldChildEpisodes(Episode newChildEpisode)
        {
            string runIdTag = newChildEpisode.Tags.Single(x => x.StartsWith("avatar-run-id:"));

            Logger.Information("DeleteOldChildEpisodes: " + runIdTag);

            var list = await _episodeRepositoryHandler.GetAsync(x => x.ParentId == newChildEpisode.ParentId && !x.Tags.Contains(runIdTag));

            Logger.Information("Found old child episodes to delete: " + list.Count);

            if (list.Count == 0)
            {
                return;
            }

            long? count = await _episodeRepositoryHandler.DeleteManyAsync(DeleteSpec.ByIds(list.Select(x => x.Id)));
            Logger.Information("Deleted: " + count);
        }


    }
}

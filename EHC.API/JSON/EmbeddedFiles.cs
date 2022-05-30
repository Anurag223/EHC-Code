using System;
using System.IO;
using System.Reflection;

namespace TLM.EHC.API.JSON
{
    // after migrating to mateo framework with .net core 3.1
    // swagger json examples for POST are not displayed
    // this is a known issue of sdk
    // https://slb-it.visualstudio.com/es-TLM-federation/_workitems/edit/1081251


    public static class EmbeddedFiles
    {
        public static string EpisodeCreate { get; }
        public static string EpisodeUpdate { get; }
        public static string ChannelExplicit { get; }
        public static string EpisodicPointsExplicit { get; }
        public static string ChannelDefinitionCreate { get; }
        public static string ChannelDefinitionUpdate { get; }
        public static string ChannelSendBulk { get; }

        static EmbeddedFiles()
        {
            Assembly assembly = typeof(EmbeddedFiles).GetTypeInfo().Assembly;
            string allNames = string.Join(",", assembly.GetManifestResourceNames()); // to find names

            // just make json files 'Embedded resource' and 'Do not copy'

            EpisodeCreate = ReadFileContents(assembly, "TLM.EHC.API.JSON.Episodes.EpisodeCreate.json");
            EpisodeUpdate = ReadFileContents(assembly, "TLM.EHC.API.JSON.Episodes.EpisodeUpdate.json");
            ChannelExplicit = ReadFileContents(assembly, "TLM.EHC.API.JSON.ChannelData.Input.Explicit.json");
            ChannelSendBulk = ReadFileContents(assembly, "TLM.EHC.API.JSON.ChannelData.Input.SendBulk.json");
            EpisodicPointsExplicit = ReadFileContents(assembly, "TLM.EHC.API.JSON.EpisodicPoints.Input.Explicit.json");
            ChannelDefinitionCreate = ReadFileContents(assembly, "TLM.EHC.API.JSON.ChannelDefinitions.ChannelDefinitionCreate.json");
            ChannelDefinitionUpdate = ReadFileContents(assembly, "TLM.EHC.API.JSON.ChannelDefinitions.ChannelDefinitionUpdate.json");
        }


        private static string ReadFileContents(Assembly assembly, string name)
        {
            using (var stream = assembly.GetManifestResourceStream(name))
            {
                if (stream == null)
                {
                    throw new ArgumentException("Embedded resource not found: " + name);
                }

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

    }
}

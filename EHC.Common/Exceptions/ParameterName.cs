// ReSharper disable once CheckNamespace
namespace TLM.EHC.Common.Exceptions
{

    public static class ParameterName
    {
        public static string GetParameterName(ErrorCodes? errorCodes)
        {
            switch (errorCodes)
            {
                case ErrorCodes.InvalidEquipmentWkeId:
                case ErrorCodes.EquipmentNotFound:
                    return "equipmentWkeId";
                case ErrorCodes.InvalidStartDate:
                    return "start";
                case ErrorCodes.EpisodeNotFound:
                    return "episode";
                case ErrorCodes.ChannelDefinitionNotFound:
                case ErrorCodes.ChannelCodeNotFound:
                case ErrorCodes.InvalidCode:
                    return "code";
                case ErrorCodes.EquipmentCodeNotFound: 
                    return "equipmentcode";
                case ErrorCodes.InvalidFillValue:
                    return "fillValue";
                case ErrorCodes.InvalidThresholdTimestamp:
                    return "thresholdTimeStamp";
                case ErrorCodes.InvalidEndDate:
                    return "end";
                default:
                    return "no parameter detail";
            }

        }

    }
}

// ReSharper disable once CheckNamespace
namespace TLM.EHC.Common.Exceptions
{
    public static class EhcConstants
    {
        public const string PartialWriteError = "partial write";
        public const string WriteSuffix = "InfluxDB write error: ";
        public const string InvalidStartDate = "The requested start date is invalid";
        public const string InvalidEndDate = "The requested end date is invalid";
        public const string InvalidEquipmentWkeid = @"Invalid EquipmentWkeId {0},':' symbol not found";
        public const string InvalidChannelCode = @"Channel code {0} is invalid";
        public const string NegativePageSize = @"The requested page size is {0} which is less than one";
        public const string NegativePageNumber = @"The requested page number is {0} which is less than one";
        public const string PageSizeGreaterThanMax = @"The requested page size is {0} which is greater than the maximum size, {1}";
        public const string InvalidPageSize = @"The requested page size {0} is invalid";
        public const string EpisodeNotFound = @"Episode not found {0}";
        public const string ChannelCodeNotFound = @"Channel code not found: {0}";
        public const string ChannelDefinitionNotFound = @"Channel definition {0} not found";
        public const string EquipmentNotFound = @"Equipment not found: {0}";
        public const string InvalidThresholdTimestamp = @"Provided threshold timestamp is not valid:{0}";
        public const string EquipmentCodeAlreadyExists = "Equipment code already exists";
        public const string EquipmentCodeCannotBeNullOrEmpty = "Equipment code cannot be null or empty";
        public const string InfluxPathNotFound = "Influx path not found";
        public const string DbMapCreationFailure = "DBMap creation failure : Check InfluxDB connectivity.";
        public const string InactiveDbMap = "This DB is not active, please contact EHC on {0}. Equipment WKEID {1}, Equipment Code {2}.";
        public const string EquipmentMappingCannotBeFound = "Equipment mapping cannot be found";
        public const string ChannelCodeCannotBeFound = "Channel code cannot be found";
        public const string RecordUnavailableForThresholdValue ="Records not available for provided threshold timestamp value";
        public const string WkeIdNotFoundInInfluxDb = "Cannot find wkeid in influxdb :";
        public const string EquipmentCannotBeFound = "Equipment not found: ";
        public const string NonNullMultipleChannelsExpected = "Non-null MultipleChannels expected.";
        public const string UnexpectedQueryType = "Unexpected query type: ";
        public const string EhcSupportEmailId = "EquipmentHistorian@slb.com";
        public const string DbMappingUpdatedSuccessMessage = "DB mapping has been updated.";
        public const string DbMappingUpdatedErrorMessage = "Error while enabling/disabling DB mapping.";
        public const string InfluxDbCreationErrorMessage = "Influx DB creation failed.";
        public const string InfluxDbCreationSuccessMessage = "Influx DB is created";
        public const string EquipmentCodeNotFound = @"Equipment code not found: {0}";
        public const string EquipmentClassificationNotFound= "No classifications found in Equipment API";
        public const string IdShouldBeNull = "Id should be null";
        public const string A2RAuditLogCreationError= "Got empty id after creating an audit log";

        public const string EquipmentCodeNotInDBMap = "Equipment code not found in InfluxDbMapping";
        public const string ChannelDefinitionNotFoundForCode = "Channel definition not found for code: ";
        public const string InvalidFillValue = "Invalid Fill Value";
    }
}
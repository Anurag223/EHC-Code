// ReSharper disable once CheckNamespace
namespace TLM.EHC.Admin
{
    /// <summary>
    /// Enum to define different activities (actions) on A2R utils
    /// </summary>
    public enum A2RUtilsActivityType
    {
        /// <summary>
        /// Add Equipment Code on A2R Utils- Historian Management dashboard
        /// </summary>
        AddEquipmentCode = 0,
        /// <summary>
        ///  Update DB map status on A2R Utils- Historian Management dashboard
        /// </summary>
        UpdateDbMapStatus = 1
    }

    /// <summary>
    /// Enum to define different applications (dashboards) on A2R utils
    /// </summary>
    public enum A2RUtilsApplicationType
    {
        /// <summary>
        /// Db Map management application on A2R Utils
        /// </summary>
        DbMapManagement = 0
    }
}

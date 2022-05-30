using System;

namespace TLM.EHC.Common.Models
{
    public class WellKnownEntityId
    {
        // MaterialNumber and SerialNumber may change over time, but initial values are unique constant WKEID

        public string MaterialNumber { get; }
        public string SerialNumber { get; }

        public string Value => (MaterialNumber + ":" + SerialNumber);

        public override string ToString() => Value;


        public WellKnownEntityId(string materialNumber, string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(materialNumber))
            {
                throw new ArgumentException("Empty materialNumber in wkeid.");
            }

            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                throw new ArgumentException("Empty serialNumber in wkeid.");
            }

            // TO UPPERCASE?

            MaterialNumber = materialNumber.Trim();
            SerialNumber = serialNumber.Trim();
        }


        public static WellKnownEntityId Parse(string equipmentWkeId)
        {
            if (string.IsNullOrWhiteSpace(equipmentWkeId))
            {
                throw new ArgumentException("Empty wkeid.");
            }

            int index = equipmentWkeId.IndexOf(':');

            if (index < 0)
            {
                throw new ArgumentException($"Invalid wkeid '{equipmentWkeId}', ':' symbol not found.");
            }

            string materialNumber = equipmentWkeId.Substring(0, index);
            string serialNumber = equipmentWkeId.Substring(index + 1);

            return new WellKnownEntityId(materialNumber, serialNumber);
        }


    }
}

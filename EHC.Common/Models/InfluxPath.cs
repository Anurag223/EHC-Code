using System;
using System.Text;
using TLM.EHC.Admin;

namespace TLM.EHC.Common.Models
{
    public class InfluxPath
    {
        public string Technology { get; private set; }    // db
        public string Brand { get; private set; }         // measurement
        public string EquipmentCode { get; private set; } // not used, most specific

        public static InfluxPath GetFromEquipmentModel(EquipmentModel equipmentModel)
        {
            CheckArgument(equipmentModel);

            return new InfluxPath()
            {
                Technology = EscapeValue(equipmentModel.TechnologyCode + "_" + equipmentModel.TechnologyName),
                Brand = EscapeValue(equipmentModel.BrandCode + "_" + equipmentModel.BrandName),
                EquipmentCode = equipmentModel.EquipmentCode,
            };
        }

        public static InfluxPath GetFromInfluxDBMapping(InfluxDBMapping dbMapping, string code)
        {
            return new InfluxPath()
            {
                Technology = dbMapping.DbName,
                Brand = dbMapping.MeasurementName,
                EquipmentCode = code,
            };
        }

        private static string EscapeValue(string value)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char ch in value)
            {
                if (char.IsLetterOrDigit(ch) || ch == '_' || ch == '-')
                {
                    sb.Append(char.ToUpperInvariant(ch));
                }
                else
                {
                    sb.Append('_');
                }
            }

            return sb.ToString();
        }


        private static void CheckArgument(EquipmentModel equipmentModel)
        {
            if (string.IsNullOrWhiteSpace(equipmentModel.TechnologyCode))
            {
                throw new ArgumentException();
            }

            if (string.IsNullOrWhiteSpace(equipmentModel.TechnologyName))
            {
                throw new ArgumentException();
            }

            if (string.IsNullOrWhiteSpace(equipmentModel.BrandCode))
            {
                throw new ArgumentException();
            }

            if (string.IsNullOrWhiteSpace(equipmentModel.BrandName))
            {
                throw new ArgumentException();
            }

            if (string.IsNullOrWhiteSpace(equipmentModel.EquipmentCode))
            {
                throw new ArgumentException();
            }
        }


    }
}

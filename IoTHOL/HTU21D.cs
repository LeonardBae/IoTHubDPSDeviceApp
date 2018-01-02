using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace IoTHOL
{
    class HTU21D
    {
        // I2C Addresses
        public const ushort HTDU21D_I2C_ADDRESS = 0x0040;

        // HTDU21D I2C Commands
        private const byte SAMPLE_TEMPERATURE_HOLD = 0xE3;
        private const byte SAMPLE_HUMIDITY_HOLD = 0xE5;

        // I2C Devices
        private I2cDevice htdu21d;

        public HTU21D(ref I2cDevice I2CDevice)
        {
            this.htdu21d = I2CDevice; 
        }

        public float Humidity()
        {
            ushort raw_humidity_data = RawHumidity();
            double humidity_RH = (((125.0 * raw_humidity_data) / 65536) - 6.0);

            float humidity = Convert.ToSingle(humidity_RH);

            return humidity; 
        }

        public float Temperature()
        {
            ushort raw_temperature_data = RawTemperature();
            double temperature_C = (((175.72 * raw_temperature_data) / 65536) - 46.85);

            float temperature = Convert.ToSingle(temperature_C);

            return temperature; 
        }


        private ushort RawHumidity()
        {
            ushort humidity = 0;
            byte[] i2c_humidity_data = new byte[3];

            htdu21d.WriteRead(new byte[] { SAMPLE_HUMIDITY_HOLD }, i2c_humidity_data);

            humidity = (ushort)(i2c_humidity_data[0] << 8);
            humidity |= (ushort)(i2c_humidity_data[1] & 0xFC);

            bool humidity_data = (0x00 != (0x02 & i2c_humidity_data[1]));
            if (!humidity_data) { return 0; }

            bool valid_data = ValidHtdu21dCyclicRedundancyCheck(humidity, (byte)(i2c_humidity_data[2] ^ 0x62));
            if (!valid_data) { return 0; }

            return humidity;
        }

        private ushort RawTemperature()
        {
            ushort temperature = 0;
            byte[] i2c_temperature_data = new byte[3];

            htdu21d.WriteRead(new byte[] { SAMPLE_TEMPERATURE_HOLD }, i2c_temperature_data);

            temperature = (ushort)(i2c_temperature_data[0] << 8);
            temperature |= (ushort)(i2c_temperature_data[1] & 0xFC);

            bool temperature_data = (0x00 == (0x02 & i2c_temperature_data[1]));
            if (!temperature_data) { return 0; }

            bool valid_data = ValidHtdu21dCyclicRedundancyCheck(temperature, i2c_temperature_data[2]);
            if (!valid_data) { return 0; }

            return temperature;
        }



        private bool ValidHtdu21dCyclicRedundancyCheck(
                ushort data_,
                byte crc_
            )
        {
            const int CRC_BIT_LENGTH = 8;
            const int DATA_LENGTH = 16;
            const ushort GENERATOR_POLYNOMIAL = 0x0131;

            int crc_data = data_ << CRC_BIT_LENGTH;

            for (int i = (DATA_LENGTH - 1); 0 <= i; --i)
            {
                if (0 == (0x01 & (crc_data >> (CRC_BIT_LENGTH + i)))) { continue; }
                crc_data ^= (GENERATOR_POLYNOMIAL << i);
            }
            return (crc_ == crc_data);
        }
    }
}

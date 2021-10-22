using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Devices.Radios;

namespace RgbLed
{
    static class sinozeby
    {
        public async static Task<GattCharacteristic> BulbGetSvc(int iTyp, string sId)
        {
            // iTyp=1: LEDBLE (jak dotychczas), =2: Triones (nowe)

            Guid GuidSvc, GuidChar;
            switch (iTyp)
            {
                case 1:
                    {
                        GuidSvc = new Guid("{0000FFE5-0000-1000-8000-00805F9B34FB}");
                        GuidChar = new Guid("{0000FFE9-0000-1000-8000-00805F9B34FB}");
                        break;
                    }

                case 2:
                    {
                        GuidSvc = new Guid("{0000FFD5-0000-1000-8000-00805F9B34FB}"); // FFD5
                        GuidChar = new Guid("{0000FFD9-0000-1000-8000-00805F9B34FB}"); // FFD9
                        break;
                    }

                default:
                    {
                        return null;  // unknown type, nie umiemy obsluzyc
                    }
            }

            var oBTacc = DeviceAccessInformation.CreateFromId(sId);
            if (!(oBTacc.CurrentStatus == DeviceAccessStatus.Allowed | oBTacc.CurrentStatus == DeviceAccessStatus.Unspecified))
            {
                return null;
            }

            var oPilotBT = await BluetoothLEDevice.FromIdAsync(sId);
            if (oPilotBT is null)
                return null;
            GattDeviceServicesResult oSvc = null;
            // petla za https://stackoverflow.com/questions/44071592/device-gattservices-returns-an-empty-set-for-ble-devices-on-a-windows-universal
            for (int i = 1; i <= 10; i++)
            {
                oSvc = await oPilotBT.GetGattServicesForUuidAsync(GuidSvc);
                if (oSvc.Status != GattCommunicationStatus.Success)
                    return null;
                if (oSvc.Services.Count > 0)
                    break;
                await Task.Delay(100);
            }

            if (oSvc.Services.Count == 0)
                return null;

            // czyli mamy service, zakladam ze jeden
            GattCharacteristicsResult oChars = null;
            for (int i = 1; i <= 10; i++)
            {
                oChars = await oSvc.Services[0].GetCharacteristicsForUuidAsync(GuidChar);
                if (oChars.Status != GattCommunicationStatus.Success)
                    return null;
                if (oChars.Characteristics.Count > 0)
                    break;
                await Task.Delay(100);
            }

            if (oChars.Characteristics.Count == 0)
                return null;
            return oChars.Characteristics[0];
        }

        public async static Task<bool> IsNetBTavailable(bool bMsg)
        {

            // sprawdzamy czy jest Bluetooth w ogole dostepny
            bool bError = true;
            var oRadios = await Radio.GetRadiosAsync();
            foreach (Radio oRadio in oRadios)
            {
                if (oRadio.Kind == RadioKind.Bluetooth)
                {
                    bError = false;
                    break;
                }
            }

            if (bError)
            {
                if (bMsg)
                    RgbLed.pkar.DialogBox("ERROR: no Bluetooth available");
                return false;
            }

            bError = true;
            foreach (Radio oRadio in oRadios)
            {
                if (oRadio.Kind == RadioKind.Bluetooth & oRadio.State == RadioState.On)
                {
                    bError = false;
                    break;
                }
            }

            if (bError)
            {
                if (bMsg)
                    RgbLed.pkar.DialogBox("ERROR: Bluetooth is not enabled");
                return false;
            }

            return true;
        }
    }
}

// 2020.01.13
// * uruchomienie Triones
// * dowolnie dużo devices może być znalezione
// * nie wymaga uprzedniego Pair
// * ta sama komenda do wszystkich zaznaczonych devices
// * zapisywanie listy devices
// * zmiana numeracji wersji: poprzdnia 1.1.1, aktualna: 2.2001.1 [2: bo dwa typy]
// * na stronie About podaje numer wersji

// 2020.01.05
// * migracja do pkarModule.vb
// * przerzucenie funkcjonalnosci BT do sinozeby.vb (module)
// cel zmian: 
// 1) obsluga takze paska LEDowego, oraz zarowki nowej (troche inne komendy)
// 2) jedna komenda do kilku device do wyslania
// * back button

using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace RgbLed
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        // https://medium.com/@urish/reverse-engineering-a-bluetooth-lightbulb-56580fcb7546

        private async Task BtSendCommandLEDBLE(string sBulbId, bool bWhite, int iRed, int iGreen, int iBlue, int iWhite)
        {

            // wyslij do sAddr = LEDBLE-786036C2 , F8:1D:78:60:36:C2
            // service 0000FFE5-0000-1000-8000-00805F9B34FB
            // characteristic 0000FFE9-0000-1000-8000-00805F9B34FB
            // 56 RR GG BB F0 AA
            // 56 00 00 00 0F AA
            // BB ii ss 44   : miganie typ ii (25..38), szybkosc ss * 200 ms 

            var oChar = await sinozeby.BulbGetSvc(1, sBulbId);
            var oWriter = new Windows.Storage.Streams.DataWriter();
            oWriter.WriteByte(0x56);
            if (bWhite)
            {
                oWriter.WriteByte(0);
                oWriter.WriteByte(0);
                oWriter.WriteByte(0);
                oWriter.WriteByte((byte)iWhite);
                oWriter.WriteByte(0xF);
            }
            else
            {
                oWriter.WriteByte((byte)iRed);
                oWriter.WriteByte((byte)iGreen);
                oWriter.WriteByte((byte)iBlue);
                oWriter.WriteByte(0);
                oWriter.WriteByte(0xF0);
            }

            oWriter.WriteByte(0xAA);
            await oChar.WriteValueAsync(oWriter.DetachBuffer(), Windows.Devices.Bluetooth.GenericAttributeProfile.GattWriteOption.WriteWithoutResponse);
        }

        private async Task BtSendCommandTriones(string sBulbId, bool bWhite, int iRed, int iGreen, int iBlue, int iWhite)
        {
            var oChar = await RgbLed.sinozeby.BulbGetSvc(2, sBulbId);
            var oWriter = new Windows.Storage.Streams.DataWriter();
            oWriter.WriteByte(0x56);
            // RGB: 0x56, r, g, b, 0x00, 0xf0, 0xaa
            if (bWhite)
            {
                oWriter.WriteByte(0);
                oWriter.WriteByte(0);
                oWriter.WriteByte(0);
                oWriter.WriteByte((byte)iWhite);
                oWriter.WriteByte(0xF);
            }
            else
            {
                oWriter.WriteByte((byte)iRed);
                oWriter.WriteByte((byte)iGreen);
                oWriter.WriteByte((byte)iBlue);
                oWriter.WriteByte(0);
                oWriter.WriteByte(0xF0);
            }

            oWriter.WriteByte(0xAA);
            await oChar.WriteValueAsync(oWriter.DetachBuffer(), Windows.Devices.Bluetooth.GenericAttributeProfile.GattWriteOption.WriteWithoutResponse);
        }

        private async Task BtSendCommand(RgbLed.JedenDevice oItem, bool bWhite, int iRed, int iGreen, int iBlue, int iWhite)
        {
            var switchExpr = oItem.iTyp;
            switch (switchExpr)
            {
                case 1:
                    {
                        await this.BtSendCommandLEDBLE(oItem.sId, bWhite, iRed, iGreen, iBlue, iWhite);
                        break;
                    }

                case 2:
                    {
                        await this.BtSendCommandTriones(oItem.sId, bWhite, iRed, iGreen, iBlue, iWhite);
                        break;
                    }
            }
        }

        private async void uiRgbSet_Click(object sender, RoutedEventArgs e)
        {
            foreach (ToggleMenuFlyoutItem oMFI in uiDevicesy.Items)
            {
                if (oMFI.IsChecked)
                {
                    RgbLed.JedenDevice oItem = oMFI.DataContext as RgbLed.JedenDevice;
                    if (oItem is object)
                    {
                        await BtSendCommand(oItem, false, (int)uiRed.Value, (int)uiGreen.Value, (int)uiBlue.Value, 0);
                    }
                }
            }
        }

        private async void uiWhiteSet_Click(object sender, RoutedEventArgs e)
        {
            foreach (ToggleMenuFlyoutItem oMFI in uiDevicesy.Items)
            {
                if (oMFI.IsChecked)
                {
                    RgbLed.JedenDevice oItem = oMFI.DataContext as RgbLed.JedenDevice;
                    if (oItem is object)
                    {
                        await BtSendCommand(oItem, true, 0, 0, 0, (int)uiWhite.Value);
                    }
                }
            }
        }

        private void uiSettings_Click(object sender, RoutedEventArgs e)
        {
            // ustawienie adresu
            Frame.Navigate(typeof(RgbLed.SelectBulb));
            // App.sBulbId -> gattsvc Public Shared oBulbChar As GattCharacteristic

        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {

            // BeforeUno.Windows.Storage.Streams.TestDataWriterReader.testCase();

            await RgbLed.sinozeby.IsNetBTavailable(true);
            App.msLastBulbId = p.k.GetSettingsString("BulbId", "");
            await App.moItemy.Load();
            // dodaj to do menu uiDevicesy, z bSelected=false dla wszystkich poza App.msLastBulbId

            // kolejne zabezpieczenie przed powtórkami
            string sLista = "";
            foreach (var oItem in RgbLed.App.moItemy.GetList())
            {
                if (!sLista.Contains(oItem.sName + "|"))
                {
                    var oMFI = new ToggleMenuFlyoutItem();
                    oMFI.Text = oItem.sName;
                    oMFI.IsChecked = oItem.bSelected;
                    oMFI.DataContext = oItem;
                    if (RgbLed.App.msLastBulbId.Contains(oItem.sId))
                        oMFI.IsChecked = true;
                    this.uiDevicesy.Items.Add(oMFI);
                    sLista = sLista + oItem.sName + "|";
                }
            }
        }

        private void uiAbout_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RgbLed.About));
        }
    }
}
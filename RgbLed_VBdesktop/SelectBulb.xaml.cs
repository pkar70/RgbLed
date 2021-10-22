using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
// Imports Windows.Devices.Bluetooth.GenericAttributeProfile
using Windows.Devices.Enumeration;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace RgbLed
{
    public sealed partial class SelectBulb : Page
    {
        public SelectBulb()
        {
            InitializeComponent();
        }

        private bool mbSkanuje = false;
        public DeviceWatcher moWatcher = null;
        public BluetoothLEAdvertisementWatcher moBLEWatcher = null;
        private DispatcherTimer oTimer;
        private int iTimerCnt = 30;

        /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia *//* TODO ERROR: Skipped EndIfDirectiveTrivia */

        private void ScanSinozeby()
        {
            // https://stackoverflow.com/questions/40950482/search-for-devices-in-range-of-bluetooth-uwp
            // GetDeviceSelector = paired

            // If Not uiFindAll.IsOn Then
            // Dim sAQS As String = "System.Devices.DevObjectType:=5 AND System.Devices.Aep.ProtocolId:=""{E0CBF06C-CD8B-4647-BB8A-263B43F0F974}"""

            // 'Dim str1 As String = BluetoothDevice.GetDeviceSelectorFromPairingState(False)
            // '' "System.Devices.DevObjectType:=5 AND System.Devices.Aep.ProtocolId:=""{E0CBF06C-CD8B-4647-BB8A-263B43F0F974}"" AND (System.Devices.Aep.IsPaired:=System.StructuredQueryType.Boolean#False OR System.Devices.Aep.Bluetooth.IssueInquiry:=System.StructuredQueryType.Boolean#True)"
            // 'Dim str2 As String = BluetoothDevice.GetDeviceSelectorFromPairingState(True)
            // '' "System.Devices.DevObjectType:=5 AND System.Devices.Aep.ProtocolId:=""{E0CBF06C-CD8B-4647-BB8A-263B43F0F974}"" AND (System.Devices.Aep.IsPaired:=System.StructuredQueryType.Boolean#True OR System.Devices.Aep.Bluetooth.IssueInquiry:=System.StructuredQueryType.Boolean#False)"
            // 'Dim str3 As String = BluetoothDevice.GetDeviceSelector
            // '' str3 == str2

            // sAQS = BluetoothDevice.GetDeviceSelectorFromPairingState(True)

            // moWatcher = DeviceInformation.CreateWatcher(sAQS)
            // AddHandler moWatcher.Added, AddressOf bt_Added
            // AddHandler moWatcher.EnumerationCompleted, AddressOf bt_Koniec
            // moWatcher.Start()
            // Else
            moBLEWatcher = new BluetoothLEAdvertisementWatcher(); // DeviceInformation.CreateWatcher(sAQS)
            moBLEWatcher.ScanningMode = (BluetoothLEScanningMode)1;   // tylko czeka, 1: żąda wysłania adv
            moBLEWatcher.Received += BTwatch_Received;
            moBLEWatcher.Start();
            // End If


            // Dim iCnt As Integer = 0

            // Dim oPiloty As DeviceInformationCollection
            // If uiPaired.IsOn Then
            // oPiloty = Await DeviceInformation.FindAllAsync(BluetoothLEDevice.GetDeviceSelectorFromPairingState(True))
            // Else
            // oPiloty = Await DeviceInformation.FindAllAsync(BluetoothLEDevice.GetDeviceSelector)
            // End If

            // For Each oPilotDI As DeviceInformation In oPiloty
            // Dim sTmp As String
            // sTmp = oPilotDI.Name
            // If oPilotDI.Pairing.IsPaired Then sTmp &= " (paired)"

            // Select Case iCnt
            // Case 0
            // uiBulb1.Content = sTmp
            // uiBulb1.Visibility = Visibility.Visible
            // Case 1
            // uiBulb2.Content = sTmp
            // uiBulb2.Visibility = Visibility.Visible
            // Case 2
            // uiBulb3.Content = sTmp
            // uiBulb3.Visibility = Visibility.Visible
            // Case 3
            // uiBulb4.Content = sTmp
            // uiBulb4.Visibility = Visibility.Visible
            // End Select
            // iCnt = iCnt + 1
            // msBTLEids(iCnt) = oPilotDI.Id

            // Dim oChar As GattCharacteristic = Await BulbGetSvc(oPilotDI.Id)

            // If oChar Is Nothing Then Continue For

            // Select Case iCnt
            // ' iCnt jest +1 wzgledem poprzedniego Select Case
            // Case 1
            // uiBulb1.IsEnabled = True
            // Case 2
            // uiBulb2.IsEnabled = True
            // Case 3
            // uiBulb3.IsEnabled = True
            // Case 4
            // uiBulb4.IsEnabled = True
            // End Select

            // Next

            // uiScan.IsEnabled = True
        }

        private bool SprobujDodac(RgbLed.JedenDevice oNew)
        {

            // Debug.WriteLine("sprobuj dodac")
            // wewnetrzne zabezpieczenie przed powtorkami - bo czesto wyskakuje blad przy ForEach, ze sie zmienila Collection
            string sTmp = "|" + oNew.sName.ToLower() + "|";
            if (msAllDevNames.Contains(sTmp))
                return false;
            msAllDevNames += sTmp;

            // Debug.WriteLine("jeszcze nie mam")

            if (oNew.sName.ToLower().StartsWith("ledble"))
                oNew.iTyp = 1;
            if (oNew.sName.ToLower().StartsWith("triones"))
                oNew.iTyp = 2;

            // Debug.WriteLine("typ=" & oNew.iTyp)

            if (oNew.iTyp > 0)
            {
                oNew.bEnabled = true;
                oNew.bSelected = true;
                oNew.bSave = true;
            }
            else
            {
                oNew.bEnabled = false;
                oNew.bSelected = false;
                oNew.bSave = false;
            }

            // to czesto daje "Collection was modified; enumeration operation may not execute", wiec wprowadzam wlasne (na poczatku funkcji)
            // For Each oItem In App.moItemy.GetList
            // If oItem.sAddr = oNew.sAddr Then Return False
            // Next

            Debug.WriteLine("adding: " + oNew.sDisplayName);
            RgbLed.App.moItemy.Add(oNew);
            return true;
        }

        private async void BTwatch_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            var oNew = new RgbLed.JedenDevice();
            // Debug.WriteLine("watch_received, addr=" & args.BluetoothAddress)
            oNew.sAddr = args.BluetoothAddress.ToString();
            BluetoothLEDevice oDev;
            oDev = await BluetoothLEDevice.FromBluetoothAddressAsync(args.BluetoothAddress);
            if (oDev is null)
            {
                oNew.sName = "unnamed";
                oNew.sDisplayName = "unnamed - " + oNew.sAddr;
                oNew.sId = "";
            }
            else
            {
                oNew.sName = oDev.Name;
                oNew.sDisplayName = oNew.sName + " - " + oNew.sAddr;
                oNew.sId = oDev.DeviceId;
                // Debug.WriteLine("name=" & oDev.Name)
            }
            // End If
            // oNew.sId = args.Id
            if (SprobujDodac(oNew))
                toDispatch();
        }

        private static bool bInside = false;

        public async void fromDispatchShowItems()
        {
            if (bInside)
            {
                Debug.WriteLine("czekam");
                for (int i = 1; i <= 10; i++)
                {
                    await Task.Delay(10);
                    if (!bInside)
                        break;
                }

                bInside = true;
            }
            // ListaItems.ItemsSource = From c In App.moLista Where c.iCntTillDeath > 0 Distinct
            Debug.WriteLine("nowa lista, count=" + RgbLed.App.moItemy.Count());
            this.uiListItems.ItemsSource = RgbLed.App.moItemy.GetList();
            bInside = false;
        }

        public async void toDispatch()
        {
            await this.uiListItems.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, this.fromDispatchShowItems);
        }

        private async void bt_Koniec(DeviceWatcher sender, object args)
        {
            // StopScan()
            await this.uiScan.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, this.StopScan);
        }

        private void bt_Added(DeviceWatcher sender, DeviceInformation args)
        {
            var oNew = new RgbLed.JedenDevice();
            Debug.WriteLine("bt_Added, name=" + args.Name);
            oNew.sName = args.Name;
            if (args.Pairing.IsPaired)
            {
                oNew.sDisplayName = args.Name + " (paired)";
            }
            else
            {
                oNew.sDisplayName = args.Name;
            }

            oNew.sId = args.Id;
            if (SprobujDodac(oNew))
                toDispatch();
        }

        private void Procesuje(bool bStart)
        {
            if (bStart)
            {
                double dVal;
                dVal = Math.Min(this.uiGrid.ActualHeight, this.uiGrid.ActualWidth) / 2;
                this.uiProcesuje.Width = dVal;
                this.uiProcesuje.Height = dVal;
                this.uiProcesuje.Visibility = Visibility.Visible;
                this.uiProcesuje.IsActive = true;
            }
            else
            {
                this.uiProcesuje.IsActive = false;
                this.uiProcesuje.Visibility = Visibility.Collapsed;
            }
        }

        private string msAllDevNames = "";

        private async void StartScan()
        {
            if (await RgbLed.sinozeby.IsNetBTavailable(true) == false)
                return;
            msAllDevNames = "";
            foreach (var oItem in RgbLed.App.moItemy.GetList())
                msAllDevNames = msAllDevNames + "|" + oItem.sName.ToLower() + "|";
            oTimer.Interval = new TimeSpan(0, 0, 1);    // 30 sekund na szukanie
            iTimerCnt = 30;
            oTimer.Start();
            Procesuje(true);
            // App.moDevicesy = New Collection(Of JedenDevice) - nieprawda! korzystamy z dotychczasowych danych!
            this.uiScan.Content = "Stop";
            ScanSinozeby();
        }

        private void StopScan()
        {
            oTimer.Stop();
            if (moWatcher is object && moWatcher.Status != DeviceWatcherStatus.Stopped)
            {
                moWatcher.Stop();
            }

            if (moBLEWatcher is object && moBLEWatcher.Status != BluetoothLEAdvertisementWatcherStatus.Stopped)
            {
                moBLEWatcher.Stop();
            }

            this.uiScan.Content = "Scan";
            Procesuje(false);
        }

        private void uiScan_Click(object sender, RoutedEventArgs e)
        {
            if (mbSkanuje)
            {
                StopScan();
                mbSkanuje = false;
            }
            else
            {
                mbSkanuje = true;
                StartScan();
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (RgbLed.App.moItemy.Count() < 1)
                await RgbLed.App.moItemy.Load();
            // uiListItems.ItemsSource = App.moItemy.GetList

            foreach (var oItem in RgbLed.App.moItemy.GetList())
                oItem.bSelected = true;
            oTimer = new DispatcherTimer();
            oTimer.Interval = new TimeSpan(0, 0, 30);
            oTimer.Tick += TimerTick;
        }

        private void TimerTick(object sender, object e)
        {
            iTimerCnt = iTimerCnt - 1;
            if (iTimerCnt < 1)
                StopScan();
            // toDispatch()
        }

        private async void uiSave_Click(object sender, RoutedEventArgs e)
        {
            await RgbLed.App.moItemy.Save();
            Frame.GoBack();
        }

        private void Page_Unload(object sender, RoutedEventArgs e)
        {
            // zatrzymaj skanowanie!
            StopScan();
        }
    }
}
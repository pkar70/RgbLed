
Imports Windows.Devices.Bluetooth
Imports Windows.Devices.Bluetooth.Advertisement
'Imports Windows.Devices.Bluetooth.GenericAttributeProfile
Imports Windows.Devices.Enumeration


Public NotInheritable Class SelectBulb
    Inherits Page

    Dim mbSkanuje As Boolean = False
    Public moWatcher As DeviceWatcher = Nothing
    Public moBLEWatcher As Advertisement.BluetoothLEAdvertisementWatcher = Nothing
    Private oTimer As DispatcherTimer
    Private iTimerCnt As Integer = 30

#If False Then
    Private Sub uiSelect_Click(sender As Object, e As RoutedEventArgs)
        If uiBulb1.IsChecked = True Then
            App.msLastBulbId = msBTLEids(1)
        End If
        If uiBulb2.IsChecked = True Then
            App.msLastBulbId = msBTLEids(2)
        End If
        If uiBulb3.IsChecked = True Then
            App.msLastBulbId = msBTLEids(3)
        End If
        If uiBulb4.IsChecked = True Then
            App.msLastBulbId = msBTLEids(4)
        End If

        SetSettingsString("BulbId", App.msLastBulbId)

        Frame.GoBack()
    End Sub
#End If


    Private Sub ScanSinozeby()
        ' https://stackoverflow.com/questions/40950482/search-for-devices-in-range-of-bluetooth-uwp
        ' GetDeviceSelector = paired

        'If Not uiFindAll.IsOn Then
        '    Dim sAQS As String = "System.Devices.DevObjectType:=5 AND System.Devices.Aep.ProtocolId:=""{E0CBF06C-CD8B-4647-BB8A-263B43F0F974}"""

        '    'Dim str1 As String = BluetoothDevice.GetDeviceSelectorFromPairingState(False)
        '    '' "System.Devices.DevObjectType:=5 AND System.Devices.Aep.ProtocolId:=""{E0CBF06C-CD8B-4647-BB8A-263B43F0F974}"" AND (System.Devices.Aep.IsPaired:=System.StructuredQueryType.Boolean#False OR System.Devices.Aep.Bluetooth.IssueInquiry:=System.StructuredQueryType.Boolean#True)"
        '    'Dim str2 As String = BluetoothDevice.GetDeviceSelectorFromPairingState(True)
        '    '' "System.Devices.DevObjectType:=5 AND System.Devices.Aep.ProtocolId:=""{E0CBF06C-CD8B-4647-BB8A-263B43F0F974}"" AND (System.Devices.Aep.IsPaired:=System.StructuredQueryType.Boolean#True OR System.Devices.Aep.Bluetooth.IssueInquiry:=System.StructuredQueryType.Boolean#False)"
        '    'Dim str3 As String = BluetoothDevice.GetDeviceSelector
        '    '' str3 == str2

        '    sAQS = BluetoothDevice.GetDeviceSelectorFromPairingState(True)

        '    moWatcher = DeviceInformation.CreateWatcher(sAQS)
        '    AddHandler moWatcher.Added, AddressOf bt_Added
        '    AddHandler moWatcher.EnumerationCompleted, AddressOf bt_Koniec
        '    moWatcher.Start()
        'Else
        moBLEWatcher = New Advertisement.BluetoothLEAdvertisementWatcher ' DeviceInformation.CreateWatcher(sAQS)
        moBLEWatcher.ScanningMode = 1   ' tylko czeka, 1: żąda wysłania adv
        AddHandler moBLEWatcher.Received, AddressOf BTwatch_Received
        moBLEWatcher.Start()
        ' End If

#If False Then
        'Dim iCnt As Integer = 0

        'Dim oPiloty As DeviceInformationCollection
        'If uiPaired.IsOn Then
        '    oPiloty = Await DeviceInformation.FindAllAsync(BluetoothLEDevice.GetDeviceSelectorFromPairingState(True))
        'Else
        '    oPiloty = Await DeviceInformation.FindAllAsync(BluetoothLEDevice.GetDeviceSelector)
        'End If

        'For Each oPilotDI As DeviceInformation In oPiloty
        '    Dim sTmp As String
        '    sTmp = oPilotDI.Name
        '    If oPilotDI.Pairing.IsPaired Then sTmp &= " (paired)"

        '    Select Case iCnt
        '        Case 0
        '            uiBulb1.Content = sTmp
        '            uiBulb1.Visibility = Visibility.Visible
        '        Case 1
        '            uiBulb2.Content = sTmp
        '            uiBulb2.Visibility = Visibility.Visible
        '        Case 2
        '            uiBulb3.Content = sTmp
        '            uiBulb3.Visibility = Visibility.Visible
        '        Case 3
        '            uiBulb4.Content = sTmp
        '            uiBulb4.Visibility = Visibility.Visible
        '    End Select
        '    iCnt = iCnt + 1
        '    msBTLEids(iCnt) = oPilotDI.Id

        '    Dim oChar As GattCharacteristic = Await BulbGetSvc(oPilotDI.Id)

        '    If oChar Is Nothing Then Continue For

        '    Select Case iCnt
        '        ' iCnt jest +1 wzgledem poprzedniego Select Case
        '        Case 1
        '            uiBulb1.IsEnabled = True
        '        Case 2
        '            uiBulb2.IsEnabled = True
        '        Case 3
        '            uiBulb3.IsEnabled = True
        '        Case 4
        '            uiBulb4.IsEnabled = True
        '    End Select

        'Next

        'uiScan.IsEnabled = True
#End If

    End Sub

    Private Function SprobujDodac(oNew As JedenDevice) As Boolean

        'Debug.WriteLine("sprobuj dodac")
        ' wewnetrzne zabezpieczenie przed powtorkami - bo czesto wyskakuje blad przy ForEach, ze sie zmienila Collection
        Dim sTmp As String = "|" & oNew.sName.ToLower & "|"
        If msAllDevNames.Contains(sTmp) Then Return False
        msAllDevNames &= sTmp

        'Debug.WriteLine("jeszcze nie mam")

        If oNew.sName.ToLower.StartsWith("ledble") Then oNew.iTyp = BulbType.LEDBLE
        If oNew.sName.ToLower.StartsWith("triones") Then oNew.iTyp = BulbType.Tricolor
        If oNew.sName.ToLower.StartsWith("elk-bledom") Then oNew.iTyp = BulbType.ELKBLEDOM

        'Debug.WriteLine("typ=" & oNew.iTyp)

        If oNew.iTyp > 0 Then
            oNew.bEnabled = True
            oNew.bSelected = True
            oNew.bSave = True
        Else
            oNew.bEnabled = False
            oNew.bSelected = False
            oNew.bSave = False
        End If

        ' to czesto daje "Collection was modified; enumeration operation may not execute", wiec wprowadzam wlasne (na poczatku funkcji)
        'For Each oItem In App.moItemy.GetList
        '    If oItem.sAddr = oNew.sAddr Then Return False
        'Next

        Debug.WriteLine("adding: " & oNew.sDisplayName)

        ' App.moItemy.Add(oNew)
        toDispatchAddShowItems(oNew)
        Return True
    End Function


    Private Async Sub BTwatch_Received(sender As BluetoothLEAdvertisementWatcher, args As BluetoothLEAdvertisementReceivedEventArgs)
        Dim oNew As JedenDevice = New JedenDevice
        'Debug.WriteLine("watch_received, addr=" & args.BluetoothAddress)
        oNew.sAddr = args.BluetoothAddress
        Dim oDev As BluetoothLEDevice
        oDev = Await BluetoothLEDevice.FromBluetoothAddressAsync(args.BluetoothAddress)
        If oDev Is Nothing Then
            oNew.sName = "unnamed"
            oNew.sDisplayName = "unnamed - " & oNew.sAddr
            oNew.sId = ""
        Else
            oNew.sName = oDev.Name
            oNew.sDisplayName = oNew.sName & " - " & oNew.sAddr
            oNew.sId = oDev.DeviceId
            'Debug.WriteLine("name=" & oDev.Name)
        End If
        'End If
        'oNew.sId = args.Id
        ' If SprobujDodac(oNew) Then toDispatchShowItems()
        SprobujDodac(oNew)
    End Sub

    Private Shared bInside As Boolean = False

    Public Async Sub fromDispatchShowItems()
        If bInside Then
            Debug.WriteLine("czekam")
            For i As Integer = 1 To 10
                Await Task.Delay(10)
                If Not bInside Then Exit For
            Next
            bInside = True
        End If
        '        ListaItems.ItemsSource = From c In App.moLista Where c.iCntTillDeath > 0 Distinct
        Debug.WriteLine("nowa lista, count=" & App.moItemy.Count)
        uiListItems.ItemsSource = App.moItemy.GetList

        bInside = False
    End Sub


    Public Async Sub toDispatchAddShowItems(oNew As JedenDevice)
        ' App.moItemy.Add(oNew)   ' marshalling bad thread. Ale tak samo jest w AirBox etc., i tam jest OK??
        Await uiListItems.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
            Sub()
                App.moItemy.Add(oNew)
                fromDispatchShowItems()
            End Sub
            )
    End Sub

    'Public Async Sub fromDispatchAddShowItems()
    '    'App.moItemy.Add(oNew)
    '    fromDispatchShowItems()
    'End Sub


    Public Async Sub toDispatchShowItems()
        Await uiListItems.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, AddressOf fromDispatchShowItems)
    End Sub


    Private Async Sub bt_Koniec(sender As DeviceWatcher, args As Object)
        ' StopScan()
        Await uiScan.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, AddressOf StopScan)
    End Sub

    'Private Sub bt_Added(sender As DeviceWatcher, args As DeviceInformation)
    '    Dim oNew As JedenDevice = New JedenDevice
    '    Debug.WriteLine("bt_Added, name=" & args.Name)

    '    oNew.sName = args.Name
    '    If args.Pairing.IsPaired Then
    '        oNew.sDisplayName = args.Name & " (paired)"
    '    Else
    '        oNew.sDisplayName = args.Name
    '    End If
    '    oNew.sId = args.Id

    '    If SprobujDodac(oNew) Then toDispatchShowItems()

    'End Sub

    Dim msAllDevNames As String = ""

    Private Async Sub StartScan()
        If Await NetIsBTavailableAsync(True) = False Then Return

        msAllDevNames = ""

        For Each oItem In App.moItemy.GetList
            msAllDevNames = msAllDevNames & "|" & oItem.sName.ToLower & "|"
        Next

        oTimer.Interval = New TimeSpan(0, 0, 1)    ' 30 sekund na szukanie
        iTimerCnt = 30
        oTimer.Start()

        ProgRingShow(True, False, 0, 30)
        ' App.moDevicesy = New Collection(Of JedenDevice) - nieprawda! korzystamy z dotychczasowych danych!
        uiScan.Content = "Stop"
        ScanSinozeby()
    End Sub

    Private Sub StopScan()

        oTimer.Stop()

        If moWatcher IsNot Nothing AndAlso moWatcher.Status <> DeviceWatcherStatus.Stopped Then
            moWatcher.Stop()
        End If

        If moBLEWatcher IsNot Nothing AndAlso moBLEWatcher.Status <> BluetoothLEAdvertisementWatcherStatus.Stopped Then
            moBLEWatcher.Stop()
        End If

        uiScan.Content = "Scan"
        ProgRingShow(False)
    End Sub

    Private Sub uiScan_Click(sender As Object, e As RoutedEventArgs)
        If mbSkanuje Then
            StopScan()
            mbSkanuje = False
        Else
            mbSkanuje = True
            StartScan()
        End If
    End Sub

    Private Async Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        If App.moItemy.Count < 1 Then Await App.moItemy.Load()
        ' uiListItems.ItemsSource = App.moItemy.GetList

        For Each oItem In App.moItemy.GetList
            oItem.bSelected = True
        Next
        fromDispatchShowItems()

        oTimer = New DispatcherTimer()
        oTimer.Interval = New TimeSpan(0, 0, 30)
        AddHandler oTimer.Tick, AddressOf TimerTick

        ProgRingInit(True, True)

    End Sub

    Private Sub TimerTick(sender As Object, e As Object)
        iTimerCnt = iTimerCnt - 1
        If iTimerCnt < 1 Then StopScan()

        ProgRingInc()
        toDispatchShowItems()
    End Sub

    Private Async Sub uiSave_Click(sender As Object, e As RoutedEventArgs)

        Await App.moItemy.Save()
        Me.Frame.GoBack()
    End Sub

    Private Sub Page_Unload(sender As Object, e As RoutedEventArgs)
        ' zatrzymaj skanowanie!
        StopScan()
    End Sub

    Private Sub uiExport_Click(sender As Object, e As RoutedEventArgs)

        Dim sTxt As String = ""
        For Each oItem As JedenDevice In App.moItemy.GetList
            Dim uMAC As ULong = 0
            If ULong.TryParse(oItem.sAddr, uMAC) Then
                sTxt = sTxt & uMAC.ToHexBytesString()
            Else
                sTxt = sTxt & "???"
            End If
            sTxt = sTxt & vbTab & oItem.sName & vbCrLf
        Next

        ClipPut(sTxt)
        DialogBox("List of devices was exported to ClipBoard")
    End Sub
End Class

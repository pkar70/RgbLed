

Imports Windows.Devices.Bluetooth
Imports Windows.Devices.Bluetooth.GenericAttributeProfile
Imports Windows.Devices.Enumeration
Imports Windows.Devices.Radios



Module sinozeby

    Public Async Function BulbGetSvc(iTyp As Integer, sId As String) As Task(Of GattCharacteristic)
        ' iTyp=1: LEDBLE (jak dotychczas), =2: Triones (nowe)

        Dim GuidSvc, GuidChar As Guid

        Select Case iTyp
            Case 1 ' LEDBLE
                GuidSvc = New Guid("{0000FFE5-0000-1000-8000-00805F9B34FB}")
                GuidChar = New Guid("{0000FFE9-0000-1000-8000-00805F9B34FB}")
            Case 2 ' Triones
                GuidSvc = New Guid("{0000FFD5-0000-1000-8000-00805F9B34FB}") ' FFD5
                GuidChar = New Guid("{0000FFD9-0000-1000-8000-00805F9B34FB}") ' FFD9
            Case Else
                Return Nothing  ' unknown type, nie umiemy obsluzyc
        End Select

        Dim oBTacc As DeviceAccessInformation = DeviceAccessInformation.CreateFromId(sId)
        If Not (oBTacc.CurrentStatus = DeviceAccessStatus.Allowed Or oBTacc.CurrentStatus = DeviceAccessStatus.Unspecified) Then
            MakeToast("BulbGetSvc oBTacc null")
            Return Nothing
        End If

        Dim oPilotBT As BluetoothLEDevice = Nothing
        oPilotBT = Await BluetoothLEDevice.FromIdAsync(sId)
        If oPilotBT Is Nothing Then
            MakeToast("BulbGetSvc oPilotBT null")
            Return Nothing
        End If

#If DEBUG Then
        Await DebugBTdeviceAsync(oPilotBT)
#End If

        Dim oSvc As GattDeviceServicesResult = Nothing
        ' petla za https://stackoverflow.com/questions/44071592/device-gattservices-returns-an-empty-set-for-ble-devices-on-a-windows-universal
        For i As Integer = 1 To 10
            oSvc = Await oPilotBT.GetGattServicesForUuidAsync(GuidSvc)
            If oSvc.Status <> GattCommunicationStatus.Success Then
                MakeToast("BulbGetSvc oSvc.Status error")
                Return Nothing
            End If
            If oSvc.Services.Count > 0 Then Exit For
            Await Task.Delay(100)
        Next
        If oSvc.Services.Count = 0 Then
            MakeToast("BulbGetSvc svc count 0")
            Return Nothing
        End If

        ' czyli mamy service, zakladam ze jeden
        Dim oChars As GattCharacteristicsResult = Nothing
        For i As Integer = 1 To 10
            oChars = Await oSvc.Services.Item(0).GetCharacteristicsForUuidAsync(GuidChar)
            If oChars.Status <> GattCommunicationStatus.Success Then Return Nothing
            If oChars.Characteristics.Count > 0 Then Exit For
            Await Task.Delay(100)
        Next
        If oChars.Characteristics.Count = 0 Then
            MakeToast("BulbGetSvc chars count 0")
            Return Nothing
        End If

        Return oChars.Characteristics.Item(0)

    End Function


End Module

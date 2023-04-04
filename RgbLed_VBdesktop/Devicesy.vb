Public Class JedenDevice
    <System.Xml.Serialization.XmlAttribute()>
    Public Property sId As String
    <System.Xml.Serialization.XmlAttribute()>
    Public Property sName As String
    <System.Xml.Serialization.XmlAttribute()>
    Public Property sAddr As String
    <System.Xml.Serialization.XmlAttribute()>
    Public Property bWhite As Boolean
    <System.Xml.Serialization.XmlAttribute()>
    Public Property iTyp As Integer ' BulbType = BulbType.Unknown ' 1: LEDBLE, 2: Tricolor, 3:ELK-BLEDOM
    <System.Xml.Serialization.XmlAttribute>
    Public Property bEnabled As Boolean     ' enabled na liscie - tzn. pasuje jako sterowalny
    <System.Xml.Serialization.XmlIgnore>
    Public Property bSelected As Boolean    ' czy ma byc wykorzystany aktualnie
    <System.Xml.Serialization.XmlIgnore>
    Public Property sDisplayName As String
    <System.Xml.Serialization.XmlIgnore>
    Public Property bSave As Boolean    ' czy ma byc zapisany
End Class

Public Class BulbType
    Public Shared Property Unknown As Integer = 0
    Public Shared Property LEDBLE As Integer = 1
    Public Shared Property Tricolor As Integer = 2
    Public Shared Property ELKBLEDOM As Integer = 3
End Class

Public Class Devicesy
    Private moItemy As ObservableCollection(Of JedenDevice) = New ObservableCollection(Of JedenDevice)
    Const FILENAME As String = "devicesy.xml"

    Public Function Count() As Integer
        Return moItemy.Count
    End Function

    Public Function GetList() As ObservableCollection(Of JedenDevice)
        Return moItemy
    End Function

    Public Sub Add(oNew As JedenDevice)
        moItemy.Add(oNew)
    End Sub


    'Public Sub Add(sId As String, sName As String, sAddr As String, bWhite As Boolean, iTyp As Integer,
    '                bSelected As Boolean, bEnabled As Boolean, bSave As Boolean)
    '    Dim oNew As JedenDevice = New JedenDevice
    '    oNew.sId = sId
    '    oNew.sName = sName
    '    oNew.sAddr = sAddr
    '    oNew.bWhite = bWhite
    '    oNew.iTyp = iTyp
    '    oNew.bEnabled = bEnabled
    '    oNew.bSelected = bSelected
    '    oNew.bSave = bSave
    '    Add(oNew)
    'End Sub
    ' Delete
    ' New
    Public Async Function Save(Optional bAll As Boolean = False) As Task

        ' 2021.01.16: migracja pliku z LocalCache do LocalFolder
        Dim oFile As Windows.Storage.StorageFile =
            Await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync(
                FILENAME, Windows.Storage.CreationCollisionOption.ReplaceExisting)

        If oFile Is Nothing Then Exit Function

        Dim oSer As System.Xml.Serialization.XmlSerializer = New System.Xml.Serialization.XmlSerializer(moItemy.GetType)
        Dim oStream As Stream = Await oFile.OpenStreamForWriteAsync
        If bAll Then
            oSer.Serialize(oStream, moItemy)
        Else
            Dim oTemp As ObservableCollection(Of JedenDevice) = New ObservableCollection(Of JedenDevice)
            For Each oItem In moItemy
                If oItem.bSelected = True Then oTemp.Add(oItem)
            Next

            oSer.Serialize(oStream, oTemp)
        End If
        oStream.Dispose()   ' == fclose
    End Function

    ' Load
    Public Async Function Load() As Task(Of Boolean)
        ' ret=false gdy nie jest wczytane

        Dim oObj As Windows.Storage.StorageFile

        ' 2021.01.16: migracja pliku z LocalCache do LocalFolder
        oObj = Await Windows.Storage.ApplicationData.Current.LocalFolder.TryGetItemAsync(FILENAME)
        If oObj Is Nothing Then oObj = Await Windows.Storage.ApplicationData.Current.LocalCacheFolder.TryGetItemAsync(FILENAME)

        If oObj Is Nothing Then Return False
        Dim oFile As Windows.Storage.StorageFile = TryCast(oObj, Windows.Storage.StorageFile)

        Dim oSer As System.Xml.Serialization.XmlSerializer = New System.Xml.Serialization.XmlSerializer(moItemy.GetType)
        Dim oStream As Stream = Await oFile.OpenStreamForReadAsync

        Try
            moItemy = TryCast(oSer.Deserialize(oStream), Collection(Of JedenDevice))
        Catch
            Return False
        End Try

        For Each oItem In moItemy
            oItem.bSave = True
        Next

        Return True
    End Function


End Class

Public Module DostepBluetooth
    Public Async Function BtSendCommandLEDBLE(sBulbId As String, bWhite As Boolean, iRed As Integer, iGreen As Integer, iBlue As Integer, iWhite As Integer) As Task

        ' wyslij do sAddr = LEDBLE-786036C2 , F8:1D:78:60:36:C2
        ' service 0000FFE5-0000-1000-8000-00805F9B34FB
        ' characteristic 0000FFE9-0000-1000-8000-00805F9B34FB
        ' 56 RR GG BB F0 AA
        ' 56 00 00 00 0F AA
        ' BB ii ss 44   : miganie typ ii (25..38), szybkosc ss * 200 ms 

        Dim oChar As Windows.Devices.Bluetooth.GenericAttributeProfile.GattCharacteristic = Await BulbGetSvc(1, sBulbId)
        If oChar Is Nothing Then
            MakeToast("BtSendCommandLEDBLE oChar null")
            Return ' error, brak kontaktu zapewne
        End If
        Dim oWriter As Windows.Storage.Streams.DataWriter = New Windows.Storage.Streams.DataWriter
        oWriter.WriteByte(&H56)
        If bWhite Then
            oWriter.WriteByte(0)
            oWriter.WriteByte(0)
            oWriter.WriteByte(0)
            oWriter.WriteByte(iWhite)
            oWriter.WriteByte(&HF)
        Else
            oWriter.WriteByte(iRed)
            oWriter.WriteByte(iGreen)
            oWriter.WriteByte(iBlue)
            oWriter.WriteByte(0)
            oWriter.WriteByte(&HF0)
        End If
        oWriter.WriteByte(&HAA)
        'MakeToast($"BtSendCommandLEDBLE: {iRed}, {iGreen}, {iBlue}")
        Dim oResp = Await oChar.WriteValueAsync(oWriter.DetachBuffer, Windows.Devices.Bluetooth.GenericAttributeProfile.GattWriteOption.WriteWithoutResponse)

    End Function

    Public Async Function BtSendCommandTriones(sBulbId As String, bWhite As Boolean, iRed As Integer, iGreen As Integer, iBlue As Integer, iWhite As Integer) As Task

        Dim oChar As Windows.Devices.Bluetooth.GenericAttributeProfile.GattCharacteristic = Await BulbGetSvc(2, sBulbId)
        If oChar Is Nothing Then Return ' error, brak kontaktu zapewne

        Dim oWriter As Windows.Storage.Streams.DataWriter = New Windows.Storage.Streams.DataWriter
        oWriter.WriteByte(&H56)
        ' RGB: 0x56, r, g, b, 0x00, 0xf0, 0xaa
        If bWhite Then
            oWriter.WriteByte(0)
            oWriter.WriteByte(0)
            oWriter.WriteByte(0)
            oWriter.WriteByte(iWhite)
            oWriter.WriteByte(&HF)
        Else
            oWriter.WriteByte(iRed)
            oWriter.WriteByte(iGreen)
            oWriter.WriteByte(iBlue)
            oWriter.WriteByte(0)
            oWriter.WriteByte(&HF0)
        End If
        oWriter.WriteByte(&HAA)

        Await oChar.WriteValueAsync(oWriter.DetachBuffer, Windows.Devices.Bluetooth.GenericAttributeProfile.GattWriteOption.WriteWithoutResponse)

    End Function

    Private Async Function BtSendCommandBLEDOM(sBulbId As String, bWhite As Boolean, iRed As Integer, iGreen As Integer, iBlue As Integer, iWhite As Integer) As Task
        Dim oChar As Windows.Devices.Bluetooth.GenericAttributeProfile.GattCharacteristic = Await BulbGetSvc(3, sBulbId)
        If oChar Is Nothing Then Return ' error, brak kontaktu zapewne

        Dim oWriter As Windows.Storage.Streams.DataWriter = New Windows.Storage.Streams.DataWriter
        oWriter.WriteByte(&H7E)
        oWriter.WriteByte(0)
        oWriter.WriteByte(5)
        oWriter.WriteByte(3)
        ' RGB: 0x56, r, g, b, 0x00, 0xf0, 0xaa
        If bWhite Then
            oWriter.WriteByte(iWhite)
            oWriter.WriteByte(iWhite)
            oWriter.WriteByte(iWhite)
        Else
            oWriter.WriteByte(iRed)
            oWriter.WriteByte(iGreen)
            oWriter.WriteByte(iBlue)
        End If
        oWriter.WriteByte(0)
        oWriter.WriteByte(&HEF)

        Await oChar.WriteValueAsync(oWriter.DetachBuffer, Windows.Devices.Bluetooth.GenericAttributeProfile.GattWriteOption.WriteWithoutResponse)
    End Function

    Public Async Function BtSendCommand(oItem As JedenDevice, bWhite As Boolean, iRed As Integer, iGreen As Integer, iBlue As Integer, iWhite As Integer) As Task
        Select Case oItem.iTyp
            Case BulbType.LEDBLE
                Await BtSendCommandLEDBLE(oItem.sId, bWhite, iRed, iGreen, iBlue, iWhite)
            Case BulbType.Tricolor
                Await BtSendCommandTriones(oItem.sId, bWhite, iRed, iGreen, iBlue, iWhite)
            Case BulbType.ELKBLEDOM
                Await BtSendCommandBLEDOM(oItem.sId, bWhite, iRed, iGreen, iBlue, iWhite)
        End Select
    End Function


End Module

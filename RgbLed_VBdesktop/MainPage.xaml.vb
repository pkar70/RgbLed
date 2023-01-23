

' 2020.01.16
'   * rozdzielenie na dwie wersje (dwa Project): mobile (=15063) i desktop (>=16299)
'       wersja desktop: z obsługą cmdline, build PKAR_CMDLINE
'       nie może być ta sama, bo Manifest jest nierozumiany przez telefon
'       wiekszosc plików: MkLink, ale *pfx musiały być copy, bo link nie działał

'   * Desktop: obsługa CommandLine:
'       rgbbulb MACADDR p1 [p2 p3]
'       MACADDR: może być z : - . jako separatorami części
'       p1: gdy samo, to set white, gdy razem z p2 i p3 to RED
'       p2, p3: gdy istnieją (oba!), to GREEN i BLUE
'   * SelectBulb: pokazuje na Loaded istniejącą listę devicesów
'   * SelectBulb: dodałem BottomBar z copy/export i Save
'   * MainPage: dla Desktop, pokazuje dwa dodatkowe guziki, Copy - do tworzenia skryptow
'   * version: 3.2101

' 2020.01.13
'   * uruchomienie Triones
'   * dowolnie dużo devices może być znalezione
'   * nie wymaga uprzedniego Pair
'   * ta sama komenda do wszystkich zaznaczonych devices
'   * zapisywanie listy devices
'   * zmiana numeracji wersji: poprzdnia 1.1.1, aktualna: 2.2001.1 [2: bo dwa typy]
'   * na stronie About podaje numer wersji

' 2020.01.05
'   * migracja do pkarModule.vb
'   * przerzucenie funkcjonalnosci BT do sinozeby.vb (module)
'   cel zmian: 
'       1) obsluga takze paska LEDowego, oraz zarowki nowej (troche inne komendy)
'       2) jedna komenda do kilku device do wyslania
'   * back button

Public NotInheritable Class MainPage
    Inherits Page

    ' https://medium.com/@urish/reverse-engineering-a-bluetooth-lightbulb-56580fcb7546

#Region "Bluetooth"


    'Private Async Function BtSendCommandLEDBLE(sBulbId As String, bWhite As Boolean, iRed As Integer, iGreen As Integer, iBlue As Integer, iWhite As Integer) As Task

    '    ' wyslij do sAddr = LEDBLE-786036C2 , F8:1D:78:60:36:C2
    '    ' service 0000FFE5-0000-1000-8000-00805F9B34FB
    '    ' characteristic 0000FFE9-0000-1000-8000-00805F9B34FB
    '    ' 56 RR GG BB F0 AA
    '    ' 56 00 00 00 0F AA
    '    ' BB ii ss 44   : miganie typ ii (25..38), szybkosc ss * 200 ms 

    '    Dim oChar As Windows.Devices.Bluetooth.GenericAttributeProfile.GattCharacteristic = Await BulbGetSvc(1, sBulbId)
    '    Dim oWriter As Windows.Storage.Streams.DataWriter = New Windows.Storage.Streams.DataWriter
    '    oWriter.WriteByte(&H56)
    '    If bWhite Then
    '        oWriter.WriteByte(0)
    '        oWriter.WriteByte(0)
    '        oWriter.WriteByte(0)
    '        oWriter.WriteByte(iWhite)
    '        oWriter.WriteByte(&HF)
    '    Else
    '        oWriter.WriteByte(iRed)
    '        oWriter.WriteByte(iGreen)
    '        oWriter.WriteByte(iBlue)
    '        oWriter.WriteByte(0)
    '        oWriter.WriteByte(&HF0)
    '    End If
    '    oWriter.WriteByte(&HAA)

    '    Dim oResp = Await oChar.WriteValueAsync(oWriter.DetachBuffer, Windows.Devices.Bluetooth.GenericAttributeProfile.GattWriteOption.WriteWithoutResponse)

    'End Function

    'Private Async Function BtSendCommandTriones(sBulbId As String, bWhite As Boolean, iRed As Integer, iGreen As Integer, iBlue As Integer, iWhite As Integer) As Task

    '    Dim oChar As Windows.Devices.Bluetooth.GenericAttributeProfile.GattCharacteristic = Await BulbGetSvc(2, sBulbId)
    '    If oChar Is Nothing Then Return ' error, brak kontaktu zapewne

    '    Dim oWriter As Windows.Storage.Streams.DataWriter = New Windows.Storage.Streams.DataWriter
    '    oWriter.WriteByte(&H56)
    '    ' RGB: 0x56, r, g, b, 0x00, 0xf0, 0xaa
    '    If bWhite Then
    '        oWriter.WriteByte(0)
    '        oWriter.WriteByte(0)
    '        oWriter.WriteByte(0)
    '        oWriter.WriteByte(iWhite)
    '        oWriter.WriteByte(&HF)
    '    Else
    '        oWriter.WriteByte(iRed)
    '        oWriter.WriteByte(iGreen)
    '        oWriter.WriteByte(iBlue)
    '        oWriter.WriteByte(0)
    '        oWriter.WriteByte(&HF0)
    '    End If
    '    oWriter.WriteByte(&HAA)

    '    Await oChar.WriteValueAsync(oWriter.DetachBuffer, Windows.Devices.Bluetooth.GenericAttributeProfile.GattWriteOption.WriteWithoutResponse)

    'End Function

    'Private Async Function BtSendCommand(oItem As JedenDevice, bWhite As Boolean, iRed As Integer, iGreen As Integer, iBlue As Integer, iWhite As Integer) As Task
    '    Select Case oItem.iTyp
    '        Case 1
    '            Await BtSendCommandLEDBLE(oItem.sId, bWhite, iRed, iGreen, iBlue, iWhite)
    '        Case 2
    '            Await BtSendCommandTriones(oItem.sId, bWhite, iRed, iGreen, iBlue, iWhite)
    '    End Select
    'End Function

#End Region

    Private Function SetNotCopy(sender As Object) As Boolean
        Dim oBut As Button = TryCast(sender, Button)
        If oBut Is Nothing Then Return False

        If oBut.Name.Contains("Export") Then Return False

        Return True
    End Function

    Public Async Function BtSendCommandOrCopy(oItem As JedenDevice, bWhite As Boolean, iRed As Integer, iGreen As Integer, iBlue As Integer, iWhite As Integer, sender As Object) As Task
        If SetNotCopy(sender) Then
            Await BtSendCommand(oItem, bWhite, iRed, iGreen, iBlue, iWhite)
        Else
            Dim sTxt As String = "RGBbulb "
            Dim uMAC As ULong = 0
            If ULong.TryParse(oItem.sAddr, uMAC) Then
                sTxt = sTxt & uMAC.ToHexBytesString()
            Else
                sTxt = sTxt & "???"
            End If

            If bWhite Then
                sTxt = sTxt & " " & iWhite
            Else
                sTxt = sTxt & " " & iRed & " " & iGreen & " " & iBlue
            End If

            ClipPut(sTxt)
            DialogBox("command set to ClipBoard")

        End If
    End Function

    Private Async Sub uiRgbSet_Click(sender As Object, e As RoutedEventArgs)

        ProgRingShow(True)

        For Each oMFI As ToggleMenuFlyoutItem In uiDevicesy.Items
            If oMFI.IsChecked Then
                Dim oItem As JedenDevice = TryCast(oMFI.DataContext, JedenDevice)
                If oItem IsNot Nothing Then
                    Await BtSendCommandOrCopy(oItem, False, uiRed.Value, uiGreen.Value, uiBlue.Value, 0, sender)
                End If
            End If
        Next

        ProgRingShow(False)

    End Sub

    Private Async Sub uiWhiteSet_Click(sender As Object, e As RoutedEventArgs)

        ProgRingShow(True)

        For Each oMFI As ToggleMenuFlyoutItem In uiDevicesy.Items
            If oMFI.IsChecked Then
                Dim oItem As JedenDevice = TryCast(oMFI.DataContext, JedenDevice)
                If oItem IsNot Nothing Then
                    Await BtSendCommandOrCopy(oItem, True, 0, 0, 0, uiWhite.Value, sender)
                End If
            End If
        Next

        ProgRingShow(False)

    End Sub

    Private Sub uiSettings_Click(sender As Object, e As RoutedEventArgs)
        ' ustawienie adresu
        Me.Frame.Navigate(GetType(SelectBulb))
        '  App.sBulbId -> gattsvc Public Shared oBulbChar As GattCharacteristic

    End Sub

    Private Async Sub Page_Loaded(sender As Object, e As RoutedEventArgs)


        If IsFamilyDesktop() Then
            uiWhiteExport.Visibility = Visibility.Visible
            uiRgbExport.Visibility = Visibility.Visible
        Else
            uiWhiteExport.Visibility = Visibility.Collapsed
            uiRgbExport.Visibility = Visibility.Collapsed
        End If

        Await NetIsBTavailableAsync(True)
        ProgRingInit(True, False)
        Me.ShowAppVers()

        App.msLastBulbId = pkar.GetSettingsString("BulbId")

        Await App.moItemy.Load
        ' dodaj to do menu uiDevicesy, z bSelected=false dla wszystkich poza App.msLastBulbId

        ' kolejne zabezpieczenie przed powtórkami
        Dim sLista As String = ""

        For Each oItem In App.moItemy.GetList
            If Not sLista.Contains(oItem.sName & "|") Then
                Dim oMFI As ToggleMenuFlyoutItem = New ToggleMenuFlyoutItem
                oMFI.Text = oItem.sName
                oMFI.IsChecked = oItem.bSelected
                oMFI.DataContext = oItem
                If App.msLastBulbId.Contains(oItem.sId) Then oMFI.IsChecked = True
                uiDevicesy.Items.Add(oMFI)
                sLista = sLista & oItem.sName & "|"
            End If
        Next

        'If Await CanRegisterTriggersAsync() Then
        '    UnregisterTriggers("Rgbled")
        '    DebugOut("MainPage_Loaded registering trigger")
        '    'RegisterTimerTrigger("Rgbled_Alarm", 60, True)
        'Else
        '    DebugOut("MainPage_Loaded no permission dla Background")
        'End If


    End Sub

    Private Sub uiAbout_Click(sender As Object, e As RoutedEventArgs)
        Me.Frame.Navigate(GetType(About))
    End Sub


End Class

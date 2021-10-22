




NotInheritable Class App
    Inherits Application



#Region "By wizard"

    ''' <summary>
    ''' Invoked when the application is launched normally by the end user.  Other entry points
    ''' will be used when the application is launched to open a specific file, to display
    ''' search results, and so forth.
    ''' </summary>
    ''' <param name="e">Details about the launch request and process.</param>
    Protected Overrides Sub OnLaunched(e As Windows.ApplicationModel.Activation.LaunchActivatedEventArgs)
        Dim rootFrame As Frame = OnLaunchFragment(e.PreviousExecutionState)

        If e.PrelaunchActivated = False Then
            If rootFrame.Content Is Nothing Then
                ' When the navigation stack isn't restored navigate to the first page,
                ' configuring the new page by passing required information as a navigation
                ' parameter
                rootFrame.Navigate(GetType(MainPage), e.Arguments)
            End If

            ' Ensure the current window is active
            Window.Current.Activate()
        End If

    End Sub

    Protected Function OnLaunchFragment(aes As ApplicationExecutionState) As Frame
        Dim mRootFrame As Frame = TryCast(Window.Current.Content, Frame)

        ' Do not repeat app initialization when the Window already has content,
        ' just ensure that the window is active

        If mRootFrame Is Nothing Then
            ' Create a Frame to act as the navigation context and navigate to the first page
            mRootFrame = New Frame()

            AddHandler mRootFrame.NavigationFailed, AddressOf OnNavigationFailed

            ' PKAR added wedle https://stackoverflow.com/questions/39262926/uwp-hardware-back-press-work-correctly-in-mobile-but-error-with-pc
            AddHandler mRootFrame.Navigated, AddressOf OnNavigatedAddBackButton
            AddHandler Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested, AddressOf OnBackButtonPressed

            ' Place the frame in the current Window
            Window.Current.Content = mRootFrame
        End If

        Return mRootFrame
    End Function

    ''' <summary>
    ''' Invoked when Navigation to a certain page fails
    ''' </summary>
    ''' <param name="sender">The Frame which failed navigation</param>
    ''' <param name="e">Details about the navigation failure</param>
    Private Sub OnNavigationFailed(sender As Object, e As NavigationFailedEventArgs)
        Throw New Exception("Failed to load Page " + e.SourcePageType.FullName)
    End Sub

    ''' <summary>
    ''' Invoked when application execution is being suspended.  Application state is saved
    ''' without knowing whether the application will be terminated or resumed with the contents
    ''' of memory still intact.
    ''' </summary>
    ''' <param name="sender">The source of the suspend request.</param>
    ''' <param name="e">Details about the suspend request.</param>
    Private Sub OnSuspending(sender As Object, e As SuspendingEventArgs) Handles Me.Suspending
        Dim deferral As SuspendingDeferral = e.SuspendingOperation.GetDeferral()
        ' TODO: Save application state and stop any background activity
        deferral.Complete()
    End Sub

#End Region

    Public Shared msLastBulbId As String

    Public Shared moItemy As Devicesy = New Devicesy

    Protected Overrides Async Sub OnBackgroundActivated(args As BackgroundActivatedEventArgs)
        moTaskDeferal = args.TaskInstance.GetDeferral() ' w pkarmodule.App

        Dim bNoComplete As Boolean = False
        Dim bObsluzone As Boolean = False

        ' lista komend danej aplikacji
        '''' !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!    *TODO* rzeczywista lista komend
        Dim sLocalCmds As String = "[MAC] [whiteValue]" & vbTab & "change RGB white" & vbCrLf &
            "[MAC] [Red_Value] [Green_Value] [Blue_Value]" & vbTab & "change RGB color"

        ' zwroci false gdy to nie jest RemoteSystem; gdy true, to zainicjalizowało odbieranie
        If Not bObsluzone Then bNoComplete = RemSysInit(args, sLocalCmds)

        If Not bNoComplete Then moTaskDeferal.Complete()


        ' timer to była tylko próba, z której się potem wycofałem.

        ' tile update / warnings
        'Dim oTimerDeferal As Background.BackgroundTaskDeferral
        'oTimerDeferal = args.TaskInstance.GetDeferral()

        ''MakeToast("RGBLED In timer")

        'Select Case args.TaskInstance.Task.Name
        '    Case "Rgbled_Alarm"
        '        ' MakeToast("to timer")
        '        Await App.moItemy.Load
        '        'MakeToast("wczytalem devicesow " & App.moItemy.Count)
        '        For Each oItem As JedenDevice In moItemy.GetList
        '            If oItem.sName.Contains("LEDBLE") Then
        '                'MakeToast("trying to send")
        '                Await BtSendCommand(oItem, True, 0, 0, 0, 200)
        '            End If
        '        Next
        'End Select

        'oTimerDeferal.Complete()

    End Sub

    ' CommandLine, Toasts
    Protected Overrides Async Sub OnActivated(args As IActivatedEventArgs)
        ' to jest m.in. dla Toast i tak dalej?

        ' próba czy to commandline
        If args.Kind = ActivationKind.CommandLineLaunch Then

            Dim commandLine As CommandLineActivatedEventArgs = TryCast(args, CommandLineActivatedEventArgs)
            Dim operation As CommandLineActivationOperation = commandLine?.Operation
            Dim strArgs As String = operation?.Arguments

            If Not String.IsNullOrEmpty(strArgs) Then
                ' Await ObsluzParam(strArgs, 1)   ' z wersji poprzedniej
                Await ObsluzCommandLine(strArgs) ' z wersji nowej
                Window.Current.Close()
                Return
            End If
        End If

        ' jesli nie cmdline (a np. toast), albo cmdline bez parametrow, to pokazujemy okno
        Dim rootFrame As Frame = OnLaunchFragment(args.PreviousExecutionState)

        If args.Kind = ActivationKind.ToastNotification Then
            rootFrame.Navigate(GetType(MainPage))
        End If

        Window.Current.Activate()

    End Sub

    Private Async Function AppServiceLocalCommand(sCommand As String) As Task(Of String)
        Await ObsluzParam(sCommand, 1)   ' przeskok do wersji poprzedniej, żeby nie kombinować
        Return "OK" ' powinno być jakoś z błędami etc.
    End Function


    Private Function GetRequestedDeviceItem(sParam0 As String) As JedenDevice

        Dim uAddr As ULong = 0
        Dim sTmpAddr As String = sParam0.Replace("-", ":").Replace(".", ":")
        'Await DebugLogFileOutAsync("Param " & sParam0 & " should be address")
        Dim aAddr As String() = sTmpAddr.Split(":")
        If aAddr.Count <> 6 Then
            'Await DebugLogFileOutAsync("Cannot decompose address, count=" & aAddr.Count)
            Return Nothing
        End If
        sTmpAddr = sTmpAddr.Replace(":", "")
        'Await DebugLogFileOutAsync("after replace, sTmpAddr=" & sTmpAddr)   ' f81d786036c2
        uAddr = Convert.ToInt64(sTmpAddr, 16)

        For Each oTmpItem As JedenDevice In App.moItemy.GetList
            If oTmpItem.sAddr = uAddr Then Return oTmpItem
        Next

        'Await DebugLogFileOutAsync("Cannot find such address")

        Return Nothing

    End Function

    Private Async Function ObsluzParamMain(sParam As String) As Task(Of String)
        ' nic nie mozemy zrobic, bo nie mamy w ogóle zapamiętanych devicesów
        If App.moItemy.Count < 1 Then Return "ERROR: no RGBbulb defined"

        Dim aParams As String() = sParam.Split(" ")
        If aParams.Count <> 2 AndAlso aParams.Count <> 4 Then
            Return "ERROR: Too many or too little paramaters"
        End If

        Dim oItem As JedenDevice = GetRequestedDeviceItem(aParams.ElementAt(0))
        If oItem Is Nothing Then
            Return "ERROR: Nieudane oItem"
        End If

        'Await DebugLogFileOutAsync("Found: name=" & oItem.sName & ", iTyp=" & oItem.iTyp)

        Dim iPar1 As Integer
        If Not Integer.TryParse(aParams.ElementAt(1), iPar1) Then
            Return "ERROR: par1 parse failed, =" & aParams.ElementAt(1)
        End If
        iPar1 = Math.Min(Math.Max(iPar1, 0), 255)

        If aParams.Count > 2 Then

            Dim iPar2 As Integer
            If Not Integer.TryParse(aParams.ElementAt(2), iPar2) Then
                Return "ERROR: par2 parse failed, =" & aParams.ElementAt(2)
            End If
            iPar2 = Math.Min(Math.Max(iPar2, 0), 255)

            Dim iPar3 As Integer
            If Not Integer.TryParse(aParams.ElementAt(3), iPar1) Then
                Return "ERROR: par3 parse failed, =" & aParams.ElementAt(3)
            End If
            iPar3 = Math.Min(Math.Max(iPar3, 0), 255)

            'Await DebugLogFileOutAsync("Setting RGB " & iPar1 & ", " & iPar2 & ", " & iPar3 & ", ")
            Await BtSendCommand(oItem, False, iPar1, iPar2, iPar3, 0)
        Else

            'Await DebugLogFileOutAsync("Setting White " & iPar1)
            Await BtSendCommand(oItem, True, 0, 0, 0, iPar1)
        End If

        Return "OK"
    End Function

    Private Async Function ObsluzParam(sParam As String, iTyp As Integer) As Task(Of String)
        ' iTyp= 1: OnActivated , 2: OnLaunched

        'Await DebugLogFileOutAsync("Uruchomienie z cmdline, sParam=" & sParam & ", iTyp=" & iTyp)

        ' nie mozna zadzialac jak jest app uruchomiona - bo może być Conflict na BlueTooth!
        If App.moItemy.Count > 0 Then
            'Await DebugLogFileOutAsync("FAIL: app działa, więc idę sobie")
            Return "ERROR: app działa, więc idę sobie (żeby nie było podwójnego Bluetooth)"
        End If

        Await App.moItemy.Load
        'Await DebugLogFileOutAsync("Devices.Count=" & App.moItemy.Count)

        Dim sRet As String = Await ObsluzParamMain(sParam)
        If sRet <> "OK" Then Return sRet    ' error

        Application.Current.Exit()

    End Function



End Class

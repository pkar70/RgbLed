
Public NotInheritable Class About
    Inherits Page

    Private Sub uiInfoOk_Click(sender As Object, e As RoutedEventArgs)
        Frame.GoBack()
    End Sub

    Private Async Sub uiRate_Click(sender As Object, e As RoutedEventArgs)
        Dim sUri As New Uri("ms-windows-store://review/?PFN=" & Package.Current.Id.FamilyName)
        Await Windows.System.Launcher.LaunchUriAsync(sUri)

    End Sub

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        uiVers.Text = Windows.ApplicationModel.Package.Current.Id.Version.Major & "." &
                Windows.ApplicationModel.Package.Current.Id.Version.Minor & "." &
                Windows.ApplicationModel.Package.Current.Id.Version.Build
    End Sub
End Class

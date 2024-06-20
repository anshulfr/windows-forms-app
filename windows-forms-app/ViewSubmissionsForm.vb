Imports System.ComponentModel
Imports System.IO
Imports System.Net
Imports Newtonsoft.Json

Public Class ViewSubmissionsForm
    Private submissions As List(Of Submission)
    Private currentIndex As Integer = 0
    Private WithEvents bgWorker As New BackgroundWorker()

    Private Sub ViewSubmissionsForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        bgWorker.WorkerSupportsCancellation = True

        bgWorker.RunWorkerAsync()

        Me.KeyPreview = True

        SetTextboxesReadOnly(True)
    End Sub

    Private Sub bgWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles bgWorker.DoWork
        LoadSubmissions()
    End Sub

    Private Sub bgWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles bgWorker.RunWorkerCompleted
        If submissions IsNot Nothing AndAlso submissions.Count > 0 Then
            DisplaySubmission(currentIndex)
        Else
            MessageBox.Show("No submissions found.")
        End If
    End Sub

    Private Sub LoadSubmissions()
        Try
            Dim request As HttpWebRequest = WebRequest.Create("http://localhost:3000/read")
            request.Method = "GET"
            request.ContentType = "application/json"

            Using response As HttpWebResponse = CType(request.GetResponse(), HttpWebResponse)
                If response.StatusCode = HttpStatusCode.OK Then
                    Dim reader As New StreamReader(response.GetResponseStream())
                    Dim json As String = reader.ReadToEnd()
                    submissions = JsonConvert.DeserializeObject(Of List(Of Submission))(json)
                    reader.Close()
                Else
                    MessageBox.Show("Failed to retrieve submissions. Status code: " & response.StatusCode)
                End If
            End Using
        Catch ex As Exception
            MessageBox.Show("Error loading submissions: " & ex.Message)
        End Try
    End Sub

    Private Sub DisplaySubmission(index As Integer)
        If submissions IsNot Nothing AndAlso index >= 0 AndAlso index < submissions.Count Then
            Dim submission As Submission = submissions(index)
            txtName.Text = submission.Name
            txtEmail.Text = submission.Email
            txtPhoneNum.Text = submission.Phone
            txtGithubLink.Text = submission.GithubLink
            txtStopwatchTime.Text = submission.StopwatchTime

            SetTextboxesReadOnly(True)
        End If
    End Sub

    Private Sub SetTextboxesReadOnly(ByVal readonlyMode As Boolean)
        txtName.ReadOnly = readonlyMode
        txtEmail.ReadOnly = readonlyMode
        txtPhoneNum.ReadOnly = readonlyMode
        txtGithubLink.ReadOnly = readonlyMode
        txtStopwatchTime.ReadOnly = readonlyMode
    End Sub

    Private Sub btnPrevious_Click(sender As Object, e As EventArgs) Handles btnPrevious.Click
        SaveChanges()
        If currentIndex > 0 Then
            currentIndex -= 1
            DisplaySubmission(currentIndex)
        End If
    End Sub

    Private Sub btnNext_Click(sender As Object, e As EventArgs) Handles btnNext.Click
        SaveChanges()
        If currentIndex < submissions.Count - 1 Then
            currentIndex += 1
            DisplaySubmission(currentIndex)
        End If
    End Sub

    Private Sub ViewSubmissionsForm_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If e.Control AndAlso e.KeyCode = Keys.P Then
            btnPrevious.PerformClick()
        ElseIf e.Control AndAlso e.KeyCode = Keys.N Then
            btnNext.PerformClick()
        End If
    End Sub

    Private Sub txtName_MouseClick(sender As Object, e As MouseEventArgs) Handles txtName.MouseClick
        SetTextboxesReadOnly(False)
    End Sub

    Private Sub txtEmail_MouseClick(sender As Object, e As MouseEventArgs) Handles txtEmail.MouseClick
        SetTextboxesReadOnly(False)
    End Sub

    Private Sub txtPhoneNum_MouseClick(sender As Object, e As MouseEventArgs) Handles txtPhoneNum.MouseClick
        SetTextboxesReadOnly(False)
    End Sub

    Private Sub txtGithubLink_MouseClick(sender As Object, e As MouseEventArgs) Handles txtGithubLink.MouseClick
        SetTextboxesReadOnly(False)
    End Sub

    Private Sub txtStopwatchTime_MouseClick(sender As Object, e As MouseEventArgs) Handles txtStopwatchTime.MouseClick
        SetTextboxesReadOnly(False)
    End Sub

    Private Sub SaveChanges()
        If submissions IsNot Nothing AndAlso currentIndex >= 0 AndAlso currentIndex < submissions.Count Then
            Dim submission = submissions(currentIndex)

            submission.Name = txtName.Text
            submission.Email = txtEmail.Text
            submission.Phone = txtPhoneNum.Text
            submission.GithubLink = txtGithubLink.Text
            submission.StopwatchTime = txtStopwatchTime.Text

            Try
                Dim jsonSubmission As String = JsonConvert.SerializeObject(submission)
                Dim request As HttpWebRequest = WebRequest.Create($"http://localhost:3000/update/{currentIndex}")
                request.Method = "PUT"
                request.ContentType = "application/json"

                Using writer As New StreamWriter(request.GetRequestStream())
                    writer.Write(jsonSubmission)
                End Using

                Dim response As HttpWebResponse = CType(request.GetResponse(), HttpWebResponse)

                If response.StatusCode = HttpStatusCode.OK Then
                Else
                    MessageBox.Show($"Failed to update submission. Status code: {response.StatusCode}")
                End If
            Catch ex As Exception
                MessageBox.Show($"Error updating submission: {ex.Message}")
            End Try
        End If
    End Sub

    Private Sub btnDelete_Click(sender As Object, e As EventArgs) Handles btnDelete.Click
        If submissions IsNot Nothing AndAlso currentIndex >= 0 AndAlso currentIndex < submissions.Count Then
            Try
                Dim request As HttpWebRequest = WebRequest.Create($"http://localhost:3000/delete/{currentIndex}")
                request.Method = "DELETE"
                request.ContentType = "application/json"

                Dim response As HttpWebResponse = CType(request.GetResponse(), HttpWebResponse)

                If response.StatusCode = HttpStatusCode.OK Then
                    submissions.RemoveAt(currentIndex)

                    If currentIndex >= submissions.Count Then
                        currentIndex = submissions.Count - 1
                    End If

                    DisplaySubmission(currentIndex)
                    MessageBox.Show("Submission deleted successfully.")
                Else
                    MessageBox.Show($"Failed to delete submission. Status code: {response.StatusCode}")
                End If
            Catch ex As Exception
                MessageBox.Show($"Error deleting submission: {ex.Message}")
            End Try
        End If
    End Sub
End Class

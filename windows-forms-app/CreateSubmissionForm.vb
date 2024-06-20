Imports Newtonsoft.Json
Imports System.Net
Imports System.IO

Public Class CreateSubmissionForm
    Private stopwatch As New Stopwatch()
    Private stopwatchTimer As New Timer()

    Private Sub CreateSubmissionForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.KeyPreview = True
        AddHandler stopwatchTimer.Tick, AddressOf UpdateStopwatchTime
        stopwatchTimer.Interval = 1000
    End Sub

    Private Sub btnToggleStopwatch_Click(sender As Object, e As EventArgs) Handles btnToggleStopWatch.Click
        If stopwatch.IsRunning Then
            stopwatch.Stop()
            stopwatchTimer.Stop()
        Else
            stopwatch.Start()
            stopwatchTimer.Start()
        End If
        UpdateStopwatchTime()
    End Sub

    Private Sub UpdateStopwatchTime()
        txtStopwatchTime.Text = stopwatch.Elapsed.ToString("hh\:mm\:ss")
    End Sub

    Private Sub btnSubmit_Click(sender As Object, e As EventArgs) Handles btnSubmit.Click
        Dim submission As New Submission With {
            .Name = txtName.Text,
            .Email = txtEmail.Text,
            .Phone = txtPhoneNum.Text,
            .GithubLink = txtGithubLink.Text,
            .StopwatchTime = txtStopwatchTime.Text
        }

        Try
            Dim jsonSubmission As String = JsonConvert.SerializeObject(submission)
            Dim request As HttpWebRequest = WebRequest.Create("http://localhost:3000/submit")
            request.Method = "POST"
            request.ContentType = "application/json"

            Using writer As New StreamWriter(request.GetRequestStream())
                writer.Write(jsonSubmission)
            End Using

            Dim response As HttpWebResponse = CType(request.GetResponse(), HttpWebResponse)

            If response.StatusCode = HttpStatusCode.OK Then
                MessageBox.Show("Submission sent to the backend.")
                ClearForm()
            Else
                MessageBox.Show("Failed to submit form. Status code: " & response.StatusCode)
            End If
        Catch ex As Exception
            MessageBox.Show("Error submitting form: " & ex.Message)
        End Try
    End Sub

    Private Sub ClearForm()
        txtName.Clear()
        txtEmail.Clear()
        txtPhoneNum.Clear()
        txtGithubLink.Clear()
        txtStopwatchTime.Clear()
        stopwatch.Reset()
        stopwatchTimer.Stop()
    End Sub

    Private Sub CreateSubmissionForm_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If e.Control AndAlso e.KeyCode = Keys.T Then
            btnToggleStopWatch.PerformClick()
        ElseIf e.Control AndAlso e.KeyCode = Keys.S Then
            btnSubmit.PerformClick()
        End If
    End Sub
End Class

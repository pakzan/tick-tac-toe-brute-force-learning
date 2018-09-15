Imports System
Imports System.IO
Imports System.Text
Public Class Form1
    Dim b As String
    Dim RND As New Random

    Dim box() As Integer = {0, 0, 0, 0, 0, 0, 0, 0, 0}
    Public Shared Turn As Integer
    Dim statistic As New List(Of List(Of String))
    Dim movement As String
    Dim path As String = "c:\temp\MyTest.txt"
    Dim found As Boolean

    Private Sub Form1_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed

        b = MsgBox("Are you sure you want to quit?", MsgBoxStyle.YesNo + MsgBoxStyle.Question, "Quit?")
        If b = MsgBoxResult.No Then
            Form2.Visible = True
        End If

    End Sub

    Private Sub PlayerTurn(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Label1.Click, Label2.Click, Label3.Click, Label4.Click, Label5.Click, Label6.Click, Label7.Click, Label8.Click, Label9.Click
        Dim clickedLabel = TryCast(sender, Label)

        If clickedLabel IsNot Nothing Then
            If clickedLabel.Text = "" Then
                clickedLabel.Text = "X"
                If Not CheckWin() Then
                    ComputerTurn()
                End If
            End If
        End If

    End Sub

    Private Sub ResetBtn(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Form2.Visible = True
        Reset()
    End Sub

    Private Sub RecordMovement(ByVal sender As Object, ByVal e As System.EventArgs) Handles Label1.TextChanged, Label2.TextChanged, Label3.TextChanged, Label4.TextChanged, Label5.TextChanged, Label6.TextChanged, Label7.TextChanged, Label8.TextChanged, Label9.TextChanged
        Dim ChangedLabel = TryCast(sender, Label)

        If ChangedLabel IsNot Nothing Then
            movement += Replace(ChangedLabel.Name, "Label", "")
            If ChangedLabel.Text = "X" Then
                box(Replace(ChangedLabel.Name, "Label", "") - 1) = 1
            ElseIf ChangedLabel.Text = "O" Then
                box(Replace(ChangedLabel.Name, "Label", "") - 1) = 2
            End If
        End If
    End Sub

    Public Sub ComputerTurn()
        Dim compLabel As Label
        Dim potential As Integer
        Dim mark As Integer = 0
        found = False

        For i = 1 To statistic(0).Count - 1
            If Integer.Parse(statistic(0)(i)(0)) = Turn And statistic(0)(i).Length - 1 > movement.Length And statistic(0)(i).Contains(movement) And mark <= Integer.Parse(statistic(1)(i)) Then
                mark = Integer.Parse(statistic(1)(i))
                potential = Integer.Parse(statistic(0)(i)(movement.Length + 1))
                found = True
            End If
        Next

        If found Then
            compLabel = Me.Controls("Label" & potential)
        Else
            Dim number As Integer = RND.Next(1, 9)
            For i = 0 To 8
                number = ((number + 1) Mod 9) + 1
                compLabel = Me.Controls("Label" & number)
                If compLabel.Text = "" Then
                    GoTo end_of_for
                End If
            Next
        End If
end_of_for:

        compLabel.Text = "O"
        CheckWin()
        'If Not CheckWin() Then
        '    ComputerTurn1()
        'End If
    End Sub

    Public Sub ComputerTurn1()
        Dim compLabel As Label
        Dim found As Boolean = False

'        For i = 1 To statistic.Length - 1
'            If statistic(i)(0) = Convert.ToString(Turn) Then
'                If statistic(i).Length - 1 < movement.Length Then
'                    GoTo end_of_for
'                End If
'                For j = 0 To movement.Length - 1
'                    If statistic(i)(j + 1) <> movement(j) Then
'                        GoTo end_of_for
'                    End If
'                Next
'                compLabel = Me.Controls("Label" & statistic(i)(movement.Length + 1))
'                found = True
'            End If
'end_of_for:
'        Next

        If Not found Then
            Dim number As Integer = RND.Next(1, 9)
            Do
                number = ((number + 1) Mod 9) + 1
                compLabel = Me.Controls("Label" & number)
            Loop Until compLabel.Text = ""
        End If

        compLabel.Text = "X"
        If Not CheckWin() Then
            ComputerTurn()
        End If
    End Sub

    Private Function CheckWin() As Boolean
        Dim win As Short = 0

        For j = 1 To 2
            If box(0) = j And box(4) = j And box(8) = j Then
                win = j
            ElseIf box(2) = j And box(4) = j And box(6) = j Then
                win = j
            Else
                For i As Integer = 0 To 2
                    If box(i) = j And box(i + 3) = j And box(i + 6) = j Then
                        win = j
                    ElseIf box(3 * i) = j And box(3 * i + 1) = j And box(3 * i + 2) = j Then
                        win = j
                    End If
                Next
            End If
        Next

        If win = 0 And Array.IndexOf(box, 0) = -1 Then
            win = 3
        End If

        If win = 0 Then
            Return False
        ElseIf win = 1 Then
            'b = MsgBox("You win!", MsgBoxStyle.OkOnly, "Congratulation!")
        ElseIf win = 2 Then
            'b = MsgBox("Try again later.", MsgBoxStyle.OkOnly, "You lose!")
        ElseIf win = 3 Then
            'b = MsgBox("It's a draw", MsgBoxStyle.OkOnly, "Draw")
        End If

        If statistic(0).Contains(Turn & movement) Then
            For i = 0 To statistic(0).Count() - 1
                If statistic(0)(i).Equals(Turn & movement) Then
                    Dim value = Integer.Parse(statistic(1)(i)) + win - 2
                    statistic(1)(i) = value.ToString
                End If
            Next
        Else
            statistic(0).Add(Turn & movement)
            statistic(1).Add(win - 2)
        End If

        If statistic.Count > 0 Then
            Dim tempStatistic As New List(Of String)
            For i = 0 To statistic(0).Count - 1
                tempStatistic.Add(statistic(0)(i) + "," + statistic(1)(i))
            Next
            File.WriteAllLines(path, tempStatistic.ToArray())
        End If
        Static a As Integer = 0
        'a += 1
        'If a > 100 Then
        '    Return False
        'End If

        'If b = MsgBoxResult.Ok Then
        '    b = 0
        '    Reset()
        'End If

        Reset()

        Return True
    End Function

    Public Sub Reset()
        Dim Label As Label
        For i = 1 To 9
            Label = Me.Controls("Label" & i)
            Label.Text = ""
            Label.Visible = True
        Next
        box = {0, 0, 0, 0, 0, 0, 0, 0, 0}
        movement = ""
        statistic.Clear()
        statistic.Add(New List(Of String))
        statistic.Add(New List(Of String))

        Dim tempStatistic As New List(Of String)
        tempStatistic.AddRange(File.ReadAllLines(path))
        For Each i In tempStatistic
            Dim s As String() = i.Split(",")
            statistic(0).Add(s(0))
            statistic(1).Add(s(1))
        Next
        If Turn = 2 Then
            ComputerTurn()
        Else
            'ComputerTurn1()
        End If
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Not File.Exists(path) Then
            File.WriteAllText(path, "")
        End If
    End Sub
End Class

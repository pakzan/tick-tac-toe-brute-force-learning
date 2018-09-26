Imports System
Imports System.IO
Imports System.Text
Public Class Form1
    Dim b As String
    Dim RND As New Random

    Dim box() As Integer = {0, 0, 0, 0, 0, 0, 0, 0, 0}
    Public Shared Turn As Integer
    Dim statistics As New Dictionary(Of String, Single)
    Dim movement As String
    Dim path As String = "..\data.txt"
    Dim found As Boolean
    Dim deep As Short = 3
    Dim branch As Short = 3

    Private Sub Form1_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed

        b = MsgBox("Are you sure you want to quit?", MsgBoxStyle.YesNo + MsgBoxStyle.Question, "Quit?")
        If b = MsgBoxResult.No Then
            Form2.Visible = True
        End If

    End Sub

    Private Sub PlayerTurn(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Label1.Click, Label2.Click, Label3.Click, Label4.Click, Label5.Click, Label6.Click, Label7.Click, Label8.Click, Label9.Click
        'cast Sender Object into Label
        Dim clickedLabel = TryCast(sender, Label)

        'check if the label is empty
        If clickedLabel IsNot Nothing Then
            If clickedLabel.Text = "" Then
                clickedLabel.Text = "X"
                'switch to computer turn if player havent win
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
        'cast Sender Object into Label
        Dim ChangedLabel = TryCast(sender, Label)


        If ChangedLabel IsNot Nothing Then
            Dim label_no = Replace(ChangedLabel.Name, "Label", "")
            movement += label_no
            If ChangedLabel.Text = "X" Then
                box(label_no - 1) = 1
            ElseIf ChangedLabel.Text = "O" Then
                box(label_no - 1) = 2
            End If
        End If
    End Sub

    Public Sub ComputerTurn()
        Dim compLabel As Label
        Dim potential As Integer
        Dim reward As Single = 0
        found = False

        'check database to find similar record, follow the record with best result
        'found: check if found good result(reward > 0)
        'reward: result of previous record
        'potential: movement of previous record
        For Each statistic In statistics
            If statistic.Key.Substring(0, statistic.Key.Length - 2).Equals(Turn & movement) And reward <= statistic.Value Then
                reward = statistic.Value
                potential = Integer.Parse(statistic.Key(movement.Length + 1))
                found = True
            End If
        Next

        'add greedy factor to encourage further exploration
        Dim greedy As Integer = RND.Next(0, 10)
        If Not found Or greedy < 7 Then
            potential = Minimax(box, branch, deep, False) + 1
        End If

        compLabel = Me.Controls("Label" & potential)
        compLabel.Text = "O"
        CheckWin()

        'Reinforcement Learning
        If Not CheckWin() Then
            ''ComputerTurn1()
        End If
    End Sub

    Public Sub ComputerTurn1()
        Dim compLabel As Label
        Dim potential As Integer
        Dim reward As Single = 0
        found = False

        'check database to find similar record, follow the record with best result
        'found: check if found good result(reward > 0)
        'reward: result of previous record
        'potential: movement of previous record
        For Each statistic In statistics
            If statistic.Key.Substring(0, statistic.Key.Length - 2).Equals(((Turn + 1) Mod 2) & movement) And reward <= statistic.Value Then
                reward = statistic.Value
                potential = Integer.Parse(statistic.Key(movement.Length + 1))
                found = True
            End If
        Next

        'add greedy factor to encourage further exploration
        Dim greedy As Integer = RND.Next(0, 10)
        If Not found Or greedy < 7 Then
            potential = Minimax(box, branch, deep, True) + 1
        End If

        compLabel = Me.Controls("Label" & potential)
        compLabel.Text = "X"
        CheckWin()

        'Reinforcement Learning
        If Not CheckWin() Then
            ComputerTurn()
        End If
    End Sub

    Private Function Minimax(box() As Integer, branch As Short, deep As Short, is_player_turn As Boolean) As Short
        Dim reward As Short = GetReward(box)
        'end recursive loop if got winner, or reached maximum deep
        If reward <> 0 Or deep = 0 Then
            Return reward
        End If

        Dim rewards As New List(Of Short)
        Dim max_reward_path As New Dictionary(Of Short, Short)

        Dim number As Short = RND.Next(1, 9)

        For i = 0 To Len(branch)
            'copy array ByVal
            Dim tempbox(8) As Integer
            Array.Copy(box, tempbox, tempbox.Length)

            number = GetRnd(number, tempbox)

            'is_player_turn: true = -1, false = 0
            tempbox(number) = is_player_turn + 2

            'continue running recursive function, and switch turn
            rewards.Add(Minimax(tempbox, branch, deep - 1, Not is_player_turn))

            If Not max_reward_path.Keys.Contains(number) Then
                max_reward_path.Add(number, rewards(i))
            End If
        Next

        'if finished searching, return the value of position of the highest reward 
        If deep = Me.deep Then
            If is_player_turn Then
                Return max_reward_path.FirstOrDefault(Function(n) n.Value = max_reward_path.Values.Min()).Key
            Else
                Return max_reward_path.FirstOrDefault(Function(n) n.Value = max_reward_path.Values.Max()).Key
            End If
        End If

        'apply minimax theory
        If is_player_turn Then
            Return rewards.Min
        Else
            Return rewards.Max
        End If

    End Function

    Private Function GetRnd(number As Short, box() As Integer) As Short
        For i = 0 To 8
            'pick a random label, choose another one if the label is filled
            number = ((number + 1) Mod 9)
            If box(number) = 0 Then
                Return number
            End If
        Next
    End Function

    Private Function GetWinner(box() As Integer) As Short
        Dim winner As Short = -1
        'check diagonal win status
        If (box(0) = box(4) And box(4) = box(8)) Or (box(2) = box(4) And box(4) = box(6)) Then
            winner = box(4)
        Else
            For i As Integer = 0 To 2
                'check column and row win status
                If box(i) = box(i + 3) And box(i + 3) = box(i + 6) Then
                    winner = box(i)
                ElseIf box(3 * i) = box(3 * i + 1) And box(3 * i + 1) = box(3 * i + 2) Then
                    winner = box(3 * i)
                End If
            Next
        End If
        Return winner
    End Function

    Private Function GetReward(box() As Integer) As Short
        Dim winner As Short = GetWinner(box)
        Dim reward As Short = 0

        If winner = 1 Then
            'player wins
            reward = -1
            'b = MsgBox("You win!", MsgBoxStyle.OkOnly, "Congratulation!")
        ElseIf winner = 2 Then
            'computer wins
            reward = 2
            'b = MsgBox("Try again later.", MsgBoxStyle.OkOnly, "You lose!")
        ElseIf winner = -1 And Array.IndexOf(box, 0) = -1 Then
            'check if no winner and all label is filled
            reward = 1
            'b = MsgBox("It's a draw!", MsgBoxStyle.OkOnly, "Draw!")
        End If

        Return reward

    End Function

    Private Function CheckWin() As Boolean
        'set reward for training computer
        Dim reward As Single = GetReward(box)

        If reward = 0 Then
            'continue playing
            Return False
        End If

        'game ended, store record to database
        'apply Q-learning
        Dim newKey As String = Turn & movement
        'make sure the last value is the value made by AI
        If Not (newKey.Length + Turn) Mod 2 = 0 Then
            newKey = newKey.Remove(newKey.Length - 1)
        End If

        Dim learn_rate As Single = 0.9
        While Not newKey.Length <= 1
            If statistics.ContainsKey(newKey) Then
                'update database if record exist
                reward = (1 - learn_rate) * statistics(newKey) + learn_rate * reward
                statistics(newKey) = reward
            Else
                'otherwise add new record to database
                reward = learn_rate * reward
                statistics.Add(newKey, reward)
            End If
            'add record for all of the moves made by AI
            newKey = newKey.Remove(newKey.Length - 2)
        End While

        'store database into file
        If statistics.Count > 0 Then
            Dim tempstatistics As New List(Of String)
            For Each statistic In statistics
                tempstatistics.Add(statistic.Key + "," + statistic.Value.ToString)
            Next
            File.WriteAllLines(path, tempstatistics.ToArray())
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
        statistics.Clear()

        'retrive previous playing records from database
        Dim tempstatistics As New List(Of String)
        tempstatistics.AddRange(File.ReadAllLines(path))
        For Each i In tempstatistics
            'store playing movements and points into statistics
            'statistics Key: movements; Value: reward
            Dim s As String() = i.Split(",")
            statistics.Add(s(0), s(1))
        Next

        'check whos turn before start game
        If Turn = 2 Then
            ComputerTurn()
        Else
            ''ComputerTurn1()
        End If
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'create database if databases not exist
        If Not File.Exists(path) Then
            File.WriteAllText(path, "")
        End If
    End Sub
End Class

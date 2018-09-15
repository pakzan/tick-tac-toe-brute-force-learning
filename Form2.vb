Public Class Form2
   
    Private Sub Label1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Label1.Click

    End Sub

    Private Sub ComputerFirst(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Form1.Visible = True
        Form1.Reset()
        Form1.Turn = 2
        Form1.ComputerTurn()
        Me.Visible = False
    End Sub

    Private Sub PlayerFirst(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Form1.Visible = True
        Form1.Reset()
        Form1.Turn = 1
        Me.Visible = False
    End Sub

    Private Sub Form2_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

    End Sub
End Class
Imports System
Imports System.Reactive.Linq
Imports Firebase.Database
Imports Firebase.Database.Query

Module Program
    Sub Main(args As String())
        Run().Wait()
    End Sub

    Async Function Run() As Task
        Console.WriteLine("You can run this application multiple times to simulate chat between multiple users")
        Console.Write("What's your name? ")

        Dim name = Console.ReadLine()

        Console.WriteLine("*******************************************************")

        Dim client = New FirebaseClient("https://torrid-inferno-3642.firebaseio.com/")
        Dim child = client.Child("messages")

        Dim observable = child.AsObservable(Of InboundMessage)

        ' delete entire conversation list
        Await child.DeleteAsync()

        Console.WriteLine("Start chatting")

        ' subscribe to messages comming in, ignoring the ones that are from me
        Dim subscription = observable _
        .Where(Function(f) Not String.IsNullOrEmpty(f.Key)) _ ' you Get empty Key When there are no data On the server For specified node
        .Where(Function(f) f.Object?.Author IsNot name) _
        .Subscribe(Sub(f) Console.WriteLine($"{f.Object.Author}: {f.Object.Content}"))

        While (True)

            Dim message = Console.ReadLine()

            If (message?.ToLower() Is "q") Then
                Exit While
            Else
                Dim msg = New OutboundMessage
                msg.Author = name
                msg.Content = message
                Await child.PostAsync(msg)
            End If
        End While

        subscription.Dispose()
    End Function


End Module

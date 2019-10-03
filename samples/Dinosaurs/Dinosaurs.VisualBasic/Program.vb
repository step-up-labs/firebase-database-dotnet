Imports Firebase.Database
Imports Firebase.Database.Query

Module Program
    Sub Main(args As String())
        Run().Wait()
    End Sub

    Async Function Run() As Task

        ' Since the dinosaur-facts repo no longer works, populate your own one with sample data
        ' in "sample.json"
        Dim client = New FirebaseClient("https://dinosaur-facts.firebaseio.com/")

        Dim dinos = Await client _
            .Child("dinosaurs") _
            .OrderByKey() _
            .StartAt("pterodactyl") _
            .LimitToFirst(2) _
            .OnceAsync(Of Dinosaur)

        For Each dino In dinos
            Console.WriteLine($"{dino.Key} is {dino.Object.Height}m high.")
        Next

    End Function
End Module

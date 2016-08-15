# FirebaseDatabase.net
[![AppVeyor Build status](https://ci.appveyor.com/api/projects/status/ep8xw22cexktghba?svg=true)](https://ci.appveyor.com/project/bezysoftware/firebase-database-dotnet)

Simple wrapper on top of [Firebase Realtime Database REST API](https://firebase.google.com/docs/database/). Among others it supports streaming API which you can use for realtime notifications.

Authentication with Firebase tokens is also supported, checkout the [Firebase Authentication library](https://github.com/step-up-labs/firebase-authentication-dotnet) and related [blog post](http://blog.bezysoftware.net/firebase-authentication-csharp-library/)

## Installation
```csharp
// Install release version
Install-Package FirebaseDatabase.net

// Install pre-release version
Install-Package FirebaseDatabase.net -pre
```

## Supported frameworks
* .NET 4.5+
* Windows 8.x
* UWP
* Windows Phone 8.1
* CoreCLR

## Usage

### Querying

```csharp
var firebase = new FirebaseClient("https://dinosaur-facts.firebaseio.com/");
var dinos = await firebase
  .Child("dinosaurs")
  .OrderByKey()
  .StartAt("pterodactyl")
  .LimitToFirst(2)
  .OnceAsync<Dinosaur>();
  
foreach (var dino in dinos)
{
  Console.WriteLine($"{dino.Key} is {dino.Object.Height}m high.");
}
```

### Saving data

```csharp
var firebase = new FirebaseClient("https://dinosaur-facts.firebaseio.com/");

// add new item to list of data and let the client generate new key for you (done offline)
var dino = await firebase
  .Child("dinosaurs")
  .PostAsync(new Dinosaur());
  
// note that there is another overload for the PostAsync method which delegates the new key generation to the firebase server
  
Console.WriteLine($"Key for the new dinosaur: {dino.Key}");  

// add new item directly to the specified location (this will overwrite whatever data already exists at that location)
var dino = await firebase
  .Child("dinosaurs")
  .Child("t-rex")
  .PutAsync(new Dinosaur());

```

### Realtime streaming

```csharp
var firebase = new FirebaseClient("https://dinosaur-facts.firebaseio.com/");
var observable = firebase
  .Child("dinosaurs")
  .AsObservable<Dinosaur>()
  .Subscribe(d => Console.WriteLine(d.Key));
  
```

```AsObservable<T>``` methods returns an ```IObservable<T>``` which you can take advantage of using [Reactive Extensions](https://github.com/Reactive-Extensions/Rx.NET)

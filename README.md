# FirebaseDatabase.net
[![AppVeyor Build status](https://ci.appveyor.com/api/projects/status/ep8xw22cexktghba?svg=true)](https://ci.appveyor.com/project/bezysoftware/firebase-database-dotnet)

Simple wrapper on top of [Firebase Realtime Database REST API](https://firebase.google.com/docs/database/). Among others it supports streaming API which you can use for realtime notifications.

For Authenticating with Firebase checkout the [Firebase Authentication library](https://github.com/step-up-labs/firebase-authentication-dotnet) and related [blog post](https://medium.com/step-up-labs/firebase-authentication-c-library-8e5e1c30acc2)

To upload files to Firebase Storage checkout the [Firebase Storage library](https://github.com/step-up-labs/firebase-storage-dotnet) and related [blog post](https://medium.com/step-up-labs/firebase-storage-c-library-d1656cc8b3c3)

## Installation
```csharp
// Install release version
Install-Package FirebaseDatabase.net

// Install pre-release version
Install-Package FirebaseDatabase.net -pre
```

## Supported frameworks
.NET Standard 1.1 - see https://github.com/dotnet/standard/blob/master/docs/versions.md for compatibility matrix

## Usage

### Authentication

The simplest solution where you only use your app secret is as follows:

```
var auth = "ABCDE"; // your app secret
var firebaseClient = new FirebaseClient(
  "<URL>",
  new FirebaseOptions
  {
    AuthTokenAsyncFactory = () => Task.FromResult(auth) 
  });
```

Note that using app secret can only be done for server-side scenarios. Otherwise you should use some sort of third-party login. 

```
var firebaseClient = new FirebaseClient(
  "<URL>",
  new FirebaseOptions
  {
    AuthTokenAsyncFactory = () => LoginAsync()
  });

...

public static async Task<string> LoginAsync()
{
  // manage oauth login to Google / Facebook etc.
  // call FirebaseAuthentication.net library to get the Firebase Token
  // return the token
}
  
```

As you can se, the AuthTokenAsyncFactory is of type `Func<Task<string>>`. This is to allow refreshing the expired token in streaming scenarios, in which case the func is called to get a fresh token.

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

### Saving & deleting data

```csharp
var firebase = new FirebaseClient("https://dinosaur-facts.firebaseio.com/");

// add new item to list of data and let the client generate new key for you (done offline)
var dino = await firebase
  .Child("dinosaurs")
  .PostAsync(new Dinosaur());
  
// note that there is another overload for the PostAsync method which delegates the new key generation to the firebase server
  
Console.WriteLine($"Key for the new dinosaur: {dino.Key}");  

// add new item directly to the specified location (this will overwrite whatever data already exists at that location)
await firebase
  .Child("dinosaurs")
  .Child("t-rex")
  .PutAsync(new Dinosaur());

// delete given child node
await firebase
  .Child("dinosaurs")
  .Child("t-rex")
  .DeleteAsync();
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

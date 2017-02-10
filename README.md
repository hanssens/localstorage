# LocalStorage for .NET

## About LocalStorage

The LocalStorage tool allows you to store and access objects in a simple, frictionless way. It is designed to serve one single purpose: having a **simple solution for persisting objects**, even between sessions.

It should be noted that LocalStorage is in no way useful as a durable, redundant or distributed storage system. It is a **lightweight tool for persisting data** - and it should be treated as such. 

The inspiration behind LocalStorage is the [Window.localStorage as you may know if from Javascript](https://developer.mozilla.org/nl/docs/Web/API/Window/localStorage). It is lean and mean. It works. We use it all the time. But, like like Javascript's localStorage, the LocalStorage for .NET is not meant as a replacement for a dedicated caching or database mechanism. Keep in mind, it is a **lightweight tool for persisting data** - and it should be treated as such.


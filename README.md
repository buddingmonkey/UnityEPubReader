# UnityEPubReader
ePub reader for Unity3d

### What is this? Why would somebody make this?
This is an ePub reader for the Unity 3D game engine. What is ePub? It is a format for ebooks, and you can read more about the standard [here](http://idpf.org/epub). It can read the book and provide the HTML for each chapter. It currently supports "Linear" books.

**Why?**

Well I thought it would be funny to have ebooks available in the Unity Engine, so I made this library. Maybe someone will find it useful someday, maybe not...

## Notes
First, this library *does not* render ebooks. It is merely a parser to provide you the data which you could write a renderer for.
	
Since ePub is a compressed format the library uses UnityZip and your ``` Application.temporaryCachePath``` to store the uncompressed book. In production it's important to check that there is enough free space on the device before opening a book.

Also, this library will unpack the entire HTML of the book to memory (but not anything the HTML links to e.g. images). This is done for speed and ease of access. For most books this will only be a few KB to a few MB at most. Again, be sure that your app can handle having the book text in memory. Feel free to modify the code if you want to stream the HTML at runtime.## Usage
It's pretty simple. Load the book via the constructor and you should be all set.

Example:

```csharp
var epub = new UEPubReader ("Assets/Books/austen-pride-and-prejudice-illustrations.epub");
```

## Thanks
Special thanks to the [UnityZip Library](https://github.com/tsubaki/UnityZip)!
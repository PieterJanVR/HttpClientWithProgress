# HttpClientWithProgress
.NET Core HttpClient with built in progress updates

## Usage

The most simple usage:

```C#
var httpClient = new HttpClientWithProgress();

var bytes = await httpClient.GetBytesAsync(Url, progress => {
    Console.WriteLine(progress.PercentageDone.ToString());
});
```

The progress delegate will give you an update every 8kB, or max every 10ms

## What's available in a progress update?
These are a few of the data properties that are available on a progress update:
1. ➗ Percentage downloaded
2. ⏳ Estimated time still necessary to finish the download
3. 🚄 The current download speed 
4. ...
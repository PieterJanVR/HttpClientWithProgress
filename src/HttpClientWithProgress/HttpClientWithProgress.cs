using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HttpClientWithProgress.Models;

namespace HttpClientWithProgress
{
    public interface IHttpClientWithProgress
    {
        /// <summary>
        /// Gets the byte array of the requestedUri and triggers the progressAction delegate with progress updates
        /// </summary>
        /// <param name="requestUri">The Uri the request is sent to.</param>
        /// <param name="progressAction">A delegate which will be triggered with progress updates and passes an HttpProgress object</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The downloaded byte array</returns>
        /// <exception cref="ProgressException">The progressAction delegate threw an exception</exception>
        /// <exception cref="HttpRequestException">The HTTP response is unsuccessful.</exception>
        Task<byte[]> GetBytesAsync(string? requestUri, Action<HttpProgress> progressAction, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the byte array of the requestedUri and triggers the progressAction delegate with progress updates
        /// </summary>
        /// <param name="requestUri">The Uri the request is sent to.</param>
        /// <param name="progressAction">A delegate which will be triggered with progress updates and passes an HttpProgress object</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The downloaded byte array</returns>
        /// <exception cref="ProgressException">The progressAction delegate threw an exception</exception>
        /// <exception cref="HttpRequestException">The HTTP response is unsuccessful.</exception>
        Task<byte[]> GetBytesAsync(Uri? requestUri, Action<HttpProgress> progressAction, CancellationToken cancellationToken = default);
    }

    public class HttpClientWithProgress : IHttpClientWithProgress
    {
        private readonly HttpClient _httpClient;
        
        public HttpClientWithProgress(HttpClient httpClient)
        {
            this._httpClient = httpClient;
        }

        public HttpClientWithProgress() : this(new HttpClient()) { }

        public Task<byte[]> GetBytesAsync(string? requestUri, Action<HttpProgress> progressAction, CancellationToken cancellationToken = default) =>
            GetBytesAsync(CreateUri(requestUri), progressAction, cancellationToken);

        /// <summary>
        /// Gets the byte array of the requestedUri and triggers the progressAction delegate with progress updates
        /// </summary>
        /// <param name="requestUri">The Uri the request is sent to.</param>
        /// <param name="progressAction">A delegate which will be triggered with progress updates and passes an HttpProgress object</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The downloaded byte array</returns>
        /// <exception cref="ProgressException">The progressAction delegate threw an exception</exception>
        /// <exception cref="HttpRequestException">The HTTP response is unsuccessful.</exception>
        public async Task<byte[]> GetBytesAsync(Uri? requestUri, Action<HttpProgress> progressAction, CancellationToken cancellationToken = default)
        {
            await using var memoryStream = new MemoryStream();
            await GetAsMemoryStreamWithProgress(requestUri, memoryStream, progressAction, cancellationToken);
            return memoryStream.ToArray();
        }

        private async Task GetAsMemoryStreamWithProgress(Uri? requestUri, MemoryStream memoryStream, Action<HttpProgress> progressAction, CancellationToken cancellationToken)
        {
            var startTime = DateTime.Now;
            using var response = await _httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var contentLength = response.Content.Headers.ContentLength;

            await using Stream contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var totalBytesRead = 0L;
            var buffer = new byte[8192];
            var contentStreamHasMoreToRead = true;
            DateTime? lastProgress = null;

            do
            {
                var read = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
                if (read == 0)
                {
                    contentStreamHasMoreToRead = false;
                }
                else
                {
                    await memoryStream.WriteAsync(buffer, 0, read, cancellationToken).ConfigureAwait(false);

                    totalBytesRead += read;

                    if (lastProgress.HasValue && (DateTime.Now - lastProgress.Value).TotalMilliseconds < 10)
                        continue;

                    lastProgress = DateTime.Now;
                    try
                    {
                        progressAction(new HttpProgress(totalBytesRead, contentLength, startTime));
                    }
                    catch (Exception e)
                    {
                        throw new ProgressException(e);
                    }
                }
            }
            while (contentStreamHasMoreToRead);
                    
            try
            {
                progressAction(new HttpProgress(totalBytesRead, contentLength, startTime, true));
            }
            catch (Exception e)
            {
                throw new ProgressException(e);
            }
        }

        private Uri? CreateUri(string? uri) => string.IsNullOrEmpty(uri) ? null : new Uri(uri, UriKind.RelativeOrAbsolute);
    }
}
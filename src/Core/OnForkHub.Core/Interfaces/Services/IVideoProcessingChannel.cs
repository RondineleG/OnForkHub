namespace OnForkHub.Core.Interfaces.Services;

/// <summary>
/// In-memory queue that decouples chunk assembly from video processing (transcoding/thumbnails).
/// Implementations must be thread-safe singletons; items are at-most-once and are
/// recovered on startup by sweeping uploads stuck in the Processing state.
/// </summary>
public interface IVideoProcessingChannel
{
    ValueTask EnqueueAsync(Guid uploadId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<Guid> ReadAllAsync(CancellationToken cancellationToken);
}

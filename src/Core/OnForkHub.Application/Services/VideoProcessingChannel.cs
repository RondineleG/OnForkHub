namespace OnForkHub.Application.Services;

using System.Threading.Channels;

using OnForkHub.Core.Interfaces.Services;

public sealed class VideoProcessingChannel : IVideoProcessingChannel
{
    private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>(
        new UnboundedChannelOptions { SingleReader = true, SingleWriter = false }
    );

    public async ValueTask EnqueueAsync(Guid uploadId, CancellationToken cancellationToken = default)
    {
        await _channel.Writer.WriteAsync(uploadId, cancellationToken);
    }

    public IAsyncEnumerable<Guid> ReadAllAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}

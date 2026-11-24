namespace OnForkHub.CrossCutting.Storage;

/// <summary>
/// Validates video file signatures (magic bytes) of the supported container formats.
/// Client-supplied file names and content types are not trustworthy; the file header is.
/// </summary>
public static class VideoFileSignatureValidator
{
    private static readonly byte[] EbmlSignature = [0x1A, 0x45, 0xDF, 0xA3]; // webm, mkv
    private static readonly byte[] MpegProgramStream = [0x00, 0x00, 0x01, 0xBA];
    private static readonly byte[] MpegVideoStream = [0x00, 0x00, 0x01, 0xB3];

    /// <summary>
    /// Checks whether the first bytes of the stream match a known video container signature.
    /// The stream position is restored when possible.
    /// </summary>
    /// <param name="stream">The stream to inspect.</param>
    /// <returns>True when the header matches mp4/mov, webm/mkv, avi, wmv or mpeg signatures.</returns>
    public static bool HasValidVideoSignature(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanRead)
        {
            return false;
        }

        var originalPosition = stream.CanSeek ? stream.Position : 0;
        try
        {
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            Span<byte> header = stackalloc byte[16];
            var read = stream.ReadAtLeast(header, 8, throwOnEndOfStream: false);
            if (read < 8)
            {
                return false;
            }

            // MP4/MOV/M4V: bytes 4..8 must be "ftyp"
            if (header[4] == (byte)'f' && header[5] == (byte)'t' && header[6] == (byte)'y' && header[7] == (byte)'p')
            {
                return true;
            }

            // EBML container: webm and matroska (mkv)
            if (header.StartsWith(EbmlSignature))
            {
                return true;
            }

            // AVI and WMV/ASF both start with RIFF or the ASF GUID; AVI: "RIFF????AVI "
            if (
                header[0] == (byte)'R'
                && header[1] == (byte)'I'
                && header[2] == (byte)'F'
                && header[3] == (byte)'F'
                && read >= 12
                && header[8] == (byte)'A'
                && header[9] == (byte)'V'
                && header[10] == (byte)'I'
                && header[11] == (byte)' '
            )
            {
                return true;
            }

            // ASF/WMV GUID: 30 26 B2 75 8E 66 CF 11
            if (header[0] == 0x30 && header[1] == 0x26 && header[2] == 0xB2 && header[3] == 0x75)
            {
                return true;
            }

            // MPEG program/video streams
            if (header.StartsWith(MpegProgramStream) || header.StartsWith(MpegVideoStream))
            {
                return true;
            }

            return false;
        }
        finally
        {
            if (stream.CanSeek)
            {
                stream.Position = originalPosition;
            }
        }
    }
}

using TagLib;
using File = System.IO.File;

namespace Noxy.NET.Test.MediaPlayer.Fixtures;

public static class MediaFileFixture
{
    // A minimal valid mono PCM wave file whose audio data is a repeating byte pattern, so a test can find it again.
    public static byte[] CreateWav(int dataBytes = 1000)
    {
        byte[] pattern = Pattern(dataBytes);

        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream);
        writer.Write("RIFF"u8);
        writer.Write(36 + dataBytes);
        writer.Write("WAVE"u8);
        writer.Write("fmt "u8);
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)1);
        writer.Write(8000);
        writer.Write(16000);
        writer.Write((short)2);
        writer.Write((short)16);
        writer.Write("data"u8);
        writer.Write(dataBytes);
        writer.Write(pattern);
        writer.Flush();

        return stream.ToArray();
    }

    // A few MPEG-1 Layer III frames (128 kbit/s, 44.1 kHz, stereo, 417 bytes each) of silence: enough for a tag library to accept it as an mp3.
    public static byte[] CreateMp3(int frameCount = 10)
    {
        using MemoryStream stream = new();
        for (int index = 0; index < frameCount; index++)
        {
            byte[] frame = new byte[417];
            frame[0] = 0xFF;
            frame[1] = 0xFB;
            frame[2] = 0x90;
            frame[3] = 0x00;
            stream.Write(frame);
        }

        return stream.ToArray();
    }

    // The flac signature, a STREAMINFO block for two seconds of 44.1 kHz stereo and a little data after it: enough for a tag library to accept it as flac.
    public static byte[] CreateFlac()
    {
        const ulong sampleRate = 44100;
        const ulong totalSamples = 88200;

        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream);
        writer.Write("fLaC"u8);
        writer.Write([0x80, 0x00, 0x00, 0x22]);
        writer.Write([0x10, 0x00, 0x10, 0x00]);
        writer.Write(new byte[6]);

        ulong packed = (sampleRate << 44) | (1UL << 41) | (15UL << 36) | totalSamples;
        byte[] packedBytes = BitConverter.GetBytes(packed);
        Array.Reverse(packedBytes);
        writer.Write(packedBytes);
        writer.Write(new byte[16]);
        writer.Write(new byte[200]);
        writer.Flush();

        return stream.ToArray();
    }

    // An mp3 with the tags the callback sets, as the bytes of the file.
    public static byte[] CreateTaggedMp3(Action<Tag> apply)
    {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.CreateVersion7()}.mp3");
        try
        {
            File.WriteAllBytes(path, CreateMp3());
            using (TagLib.File file = TagLib.File.Create(path))
            {
                apply(file.Tag);
                file.Save();
            }

            return File.ReadAllBytes(path);
        }
        finally
        {
            File.Delete(path);
        }
    }

    public static byte[] Pattern(int dataBytes = 1000)
    {
        byte[] pattern = new byte[dataBytes];
        for (int index = 0; index < dataBytes; index++)
        {
            pattern[index] = (byte)(index % 251);
        }

        return pattern;
    }
}

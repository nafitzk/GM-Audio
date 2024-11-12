using Concentus;
using Concentus.Oggfile;
using Concentus.Structs;
using Discord.Audio.Streams;
using NAudio.Utils;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Opus
{
    public class OpusFileReader : WaveStream, ISampleProvider
    {
        private Stream _fileStream;

        private IOpusDecoder _decoder;
        private OpusOggReadStream _stream;

        private CircularBuffer _buffer = new CircularBuffer(1024*1024);
        private ArrayPool<byte> _bufferPool = ArrayPool<byte>.Shared;

        private Pcm16BitToSampleProvider _converter;

        private long _position = 0;

        public OpusFileReader(string fileName) : this(System.IO.File.OpenRead(fileName)) { }

        public OpusFileReader(System.IO.Stream stream, bool closeWhenDone = false)
        {
            _fileStream = stream;
            _format = new WaveFormat(48000, 2);

            _converter = new Pcm16BitToSampleProvider(this);

            _decoder = OpusCodecFactory.CreateDecoder(_format.SampleRate, _format.Channels);
            _stream = new OpusOggReadStream(_decoder, stream);
        }

        private WaveFormat _format;
        public override WaveFormat WaveFormat => _format;

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override bool CanSeek => false;

        // if float samples are needed, they must be converted
        public int Read(float[] buffer, int offset, int count)
        {
            return _converter.Read(buffer, offset, count);
        }

        // decode packets from opus and then provide them as pcm16
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_position == 0)
            {
                int x = 1;
            }

            int position = _buffer.Count;
            while (_stream.HasNextPacket && position < count)
            {
                short[] packets = _stream.DecodeNextPacket();
                if(packets != null && packets.Length > 0)
                {
                    int byteLength = packets.Length * 2;
                    byte[] data = _bufferPool.Rent(byteLength);
                    try
                    {
                        Buffer.BlockCopy(packets, 0, data, 0, byteLength);
                        position += byteLength;
                        _buffer.Write(data, 0, byteLength);
                    }
                    finally
                    {
                        _bufferPool.Return(data);
                    }
                }
            }

            int numBytesRead = _buffer.Read(buffer, offset, count);
            _position += numBytesRead;
            return numBytesRead;
        }
    }
}

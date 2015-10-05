using System;
using System.IO;

namespace PagarMe.Mpos
{
    internal class AbecsStream
    {
        private readonly Stream _baseStream;
        private readonly Abecs.Stream _nativeStream;

        public Stream BaseStream { get { return _baseStream; } }
        public Abecs.Stream NativeStream { get { return _nativeStream; } }

        public AbecsStream(Stream baseStream)
        {
            _baseStream = baseStream;
        }
    }
}


namespace SwiftTrueRandom.Models
{

    public unsafe struct WAVHeaderModel
    {
        public fixed byte ChunkID[4];           //Spells out RIFF in ASCII
        public uint ChunkSize = 0;                 //Size of file in bytes ignoring this field and ChunkID
        public fixed byte Format[4];            //Spells out WAVE in ASCII

        public fixed byte SubChunk1ID[4];       //Spells out fmt in ASCII
        public uint SubChunk1Size = 0;             //Size of rest  of subchunk
        public ushort AudioFormat = 0;               //Format of audio data
        public ushort ChannelCount = 0;              //Number of channels in data
        public uint SampleRate = 0;                //Rate of Samples in Data
        public uint ByteRate = 0;                  //SampleRate * NumChannels * BitsPerSample/8
        public ushort BlockAlign = 0;                //Number of bytes for one sample (4 = 16bit stereo)
        public ushort BitsPerSample = 0;             //Bits for each sample in data

        public fixed byte SubChunk2ID[4];       //Contains data in ASCII
        public uint SubChunk2Size = 0;             //NumSamples * NumChannels * BitsPerSample/8, Number of bytes in data section

        public WAVHeaderModel()
        {
        }
    }
}

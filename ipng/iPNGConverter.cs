using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Crc;
using Ionic.Zlib;

namespace IPNGConverter
{
    public class PngConverter
    {
        private static List<PngTrunk> _trunks;

        private static PngTrunk getTrunk(String szName)
        {
            if (_trunks == null)
            {
                return null;
            }
            return _trunks.FirstOrDefault(trunktmp => trunktmp.getName().Equals(szName, StringComparison.OrdinalIgnoreCase));
        }

        private static bool convertPngFile(string pngFile, string targetFile)
        {
            readTrunks(pngFile);

            if (getTrunk("CgBI") != null)
            {
                var ihdrTrunk = (PngihdrTrunk) getTrunk("IHDR");

                int nMaxInflateBuffer = 4*(ihdrTrunk.MnWidth + 1)*ihdrTrunk.MnHeight;
                var outputBuffer = new byte[nMaxInflateBuffer];

                convertDataTrunk(ihdrTrunk, outputBuffer, nMaxInflateBuffer);
                writePng(targetFile);
                return true;

            } 
                // Likely a standard PNG
                return false;
        }

        private static void inflate(out byte[] conversionBuffer)
        {
            byte[] bufferin = null;
            foreach (PngTrunk dataTrunk in _trunks)
            {
                if (dataTrunk.getName().Equals("IDAT", StringComparison.OrdinalIgnoreCase))
                {
                    bufferin = dataTrunk.getData();

                }
            }
            var ms = new MemoryStream();
            var deflateStream = new DeflateStream(ms,CompressionMode.Decompress,true);
            if (bufferin != null) deflateStream.Write(bufferin,0,bufferin.Length);
            conversionBuffer = ms.GetBuffer();
        }

        private static ZlibCodec deflate(byte[] buffer, int nMaxInflateBuffer)
        {
            var deflater = new ZlibCodec(CompressionMode.Compress);
            deflater.InitializeDeflate(CompressionLevel.BestCompression);
            deflater.InputBuffer = buffer;
            deflater.AvailableBytesIn = buffer.Length;
            deflater.NextIn = 0;
            //deflater.setInput(buffer, 0, length, false);

            int nMaxDeflateBuffer = nMaxInflateBuffer + 1024;
            var deBuffer = new byte[nMaxDeflateBuffer];
            deflater.OutputBuffer = deBuffer;
            deflater.AvailableBytesOut = deBuffer.Length;
            deflater.NextOut = 0;


            int nResult = deflater.Deflate(FlushType.Finish);
            checkResultStatus(nResult);

            if (deflater.TotalBytesOut > nMaxDeflateBuffer)
            {
                throw new Exception("deflater output buffer was too small");
            }


            return deflater;
        }

        private static void checkResultStatus(int nResult)
        {
            switch (nResult)
            {
                case ZlibConstants.Z_OK:
                case ZlibConstants.Z_STREAM_END:
                    break;

                case ZlibConstants.Z_NEED_DICT:
                    throw new Exception("Z_NEED_DICT - " + nResult);
                case ZlibConstants.Z_DATA_ERROR:
                    throw new Exception("Z_DATA_ERROR - " + nResult);
                case ZlibConstants.Z_STREAM_ERROR:
                    throw new Exception("Z_STREAM_ERROR - " + nResult);
                case ZlibConstants.Z_BUF_ERROR:
                    throw new Exception("Z_BUF_ERROR - " + nResult);
                default:
                    throw new Exception("inflater error: " + nResult);
            }
        }

        private static void convertDataTrunk(PngihdrTrunk ihdrTrunk, byte[] conversionBuffer, int nMaxInflateBuffer)
        {
            if (conversionBuffer == null) throw new ArgumentNullException("conversionBuffer");
            inflate(out conversionBuffer);

            // Switch the color
            int nIndex = 0;
            for (int y = 0; y < ihdrTrunk.MnHeight; y++)
            {
                nIndex++;
                for (int x = 0; x < ihdrTrunk.MnWidth; x++)
                {
                    byte nTemp = conversionBuffer[nIndex];
                    conversionBuffer[nIndex] = conversionBuffer[nIndex + 2];
                    conversionBuffer[nIndex + 2] = nTemp;
                    nIndex += 4;
                }
            }

            ZlibCodec deflater = deflate(conversionBuffer, nMaxInflateBuffer);

            // Put the result in the first IDAT chunk (the only one to be written out)
            PngTrunk firstDataTrunk = getTrunk("IDAT");

            var crc32 = new CRC32();
            foreach (byte b in System.Text.Encoding.UTF8.GetBytes(firstDataTrunk.getName()))
            {
                crc32.UpdateCRC(b);
            }
                crc32.UpdateCRC((byte)deflater.NextOut,(int)deflater.TotalBytesOut);
            long lCrcValue = crc32.Crc32Result;

            firstDataTrunk.MnData = deflater.OutputBuffer;
            firstDataTrunk.MnCrc[0] = (byte) ((lCrcValue & 0xFF000000) >> 24);
            firstDataTrunk.MnCrc[1] = (byte) ((lCrcValue & 0xFF0000) >> 16);
            firstDataTrunk.MnCrc[2] = (byte) ((lCrcValue & 0xFF00) >> 8);
            firstDataTrunk.MnCrc[3] = (byte) (lCrcValue & 0xFF);
            firstDataTrunk.MnSize = (int) deflater.TotalBytesOut;
        }

        private static void writePng(string newFileName)
        {
            var outStream = new FileStream(newFileName,FileMode.OpenOrCreate);
            try
            {
                byte[] pngHeader = {unchecked((byte)-119), 80, 78, 71, 13, 10, 26, 10};
                outStream.Write(pngHeader,0,pngHeader.Length);
                bool dataWritten = false;
                foreach (PngTrunk trunk in _trunks)
                {
                    if (trunk.getName().Equals("CgBI",StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    if (trunk.getName().Equals("IDAT", StringComparison.OrdinalIgnoreCase))
                    {
                        if (dataWritten)
                        {
                            continue;
                        }
                        dataWritten = true;
                    }

                    trunk.writeToStream(outStream);
                }
                outStream.Flush();

            }
            finally
            {
                outStream.Close();
            }
        }

        private static void readTrunks(string pngFile)
        {
            var input = new BinaryReader(new FileStream(pngFile,FileMode.OpenOrCreate));
            try
            {
                byte[] nPngHeader = input.ReadBytes(8);

                _trunks = new List<PngTrunk>();
                if ((nPngHeader[0] == unchecked((byte)(-119))) && (nPngHeader[1] == 0x50)
                    && (nPngHeader[2] == 0x4e) && (nPngHeader[3] == 0x47)
                    && (nPngHeader[4] == 0x0d) && (nPngHeader[5] == 0x0a)
                    && (nPngHeader[6] == 0x1a) && (nPngHeader[7] == 0x0a))
                {
                    while(true)
                    {
                        PngTrunk trunk = PngTrunk.generateTrunk(input);
                        _trunks.Add(trunk);
                        if (trunk.getName().Equals("IEND", StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                        }
                    }
                }

            }
            finally
            {
                input.Close();
            }
        }

        public static bool convert(string sourceFile)
        {
            string targetFile = String.Format("{0}-new.png", sourceFile.Substring(0, sourceFile.Length - 4));
            if(!convertPngFile(sourceFile, targetFile))
            {
                return false;
            }
            return true;
        }
    }
}
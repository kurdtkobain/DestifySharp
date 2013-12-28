using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Crc;
using Ionic.Zlib;

namespace IPNGConverter
{
    public class iPNGConverter
    {
        private List<PNGTrunk> trunks = null;

        private PNGTrunk getTrunk(String szName)
        {
            if (trunks == null)
            {
                return null;
            }
            foreach (PNGTrunk trunktmp in trunks)
            {
                if(trunktmp.getName().Equals(szName,StringComparison.OrdinalIgnoreCase))
                {
                    return trunktmp;
                }
                
            }
            return null;
        }

        private bool convertPngFile(string pngFile, string targetFile)
        {
            readTrunks(pngFile);

            if (getTrunk("CgBI") != null)
            {
                PNGIHDRTrunk ihdrTrunk = (PNGIHDRTrunk) getTrunk("IHDR");

                int nMaxInflateBuffer = 4*(ihdrTrunk.m_nWidth + 1)*ihdrTrunk.m_nHeight;
                byte[] outputBuffer = new byte[nMaxInflateBuffer];

                convertDataTrunk(ihdrTrunk, outputBuffer, nMaxInflateBuffer);
                writePng(targetFile);
                return true;

            } 
                // Likely a standard PNG
                return false;
        }

        private long inflate(out byte[] conversionBuffer, int nMaxInflateBuffer)
        {
            byte[] bufferin = null;
            foreach (PNGTrunk dataTrunk in trunks)
            {
                if (dataTrunk.getName().Equals("IDAT", StringComparison.OrdinalIgnoreCase))
                {
                    bufferin = new byte[dataTrunk.getData().Length];
                    bufferin = dataTrunk.getData();

                }
            }
            MemoryStream ms = new MemoryStream();
            DeflateStream deflateStream = new DeflateStream(ms,CompressionMode.Decompress,true);
            deflateStream.Write(bufferin,0,bufferin.Length);
            conversionBuffer = ms.GetBuffer();
            return deflateStream.TotalOut;
        }

        private ZlibCodec deflate(byte[] buffer, int length, int nMaxInflateBuffer)
        {
            ZlibCodec deflater = new ZlibCodec(CompressionMode.Compress);
            deflater.InitializeDeflate(CompressionLevel.BestCompression);
            deflater.InputBuffer = buffer;
            deflater.AvailableBytesIn = buffer.Length;
            deflater.NextIn = 0;
            //deflater.setInput(buffer, 0, length, false);

            int nMaxDeflateBuffer = nMaxInflateBuffer + 1024;
            byte[] deBuffer = new byte[nMaxDeflateBuffer];
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

        private void checkResultStatus(int nResult)
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

        private bool convertDataTrunk(PNGIHDRTrunk ihdrTrunk, byte[] conversionBuffer, int nMaxInflateBuffer)
        {
            long inflatedSize = inflate(out conversionBuffer, nMaxInflateBuffer);

            // Switch the color
            int nIndex = 0;
            byte nTemp;
            for (int y = 0; y < ihdrTrunk.m_nHeight; y++)
            {
                nIndex++;
                for (int x = 0; x < ihdrTrunk.m_nWidth; x++)
                {
                    nTemp = conversionBuffer[nIndex];
                    conversionBuffer[nIndex] = conversionBuffer[nIndex + 2];
                    conversionBuffer[nIndex + 2] = nTemp;
                    nIndex += 4;
                }
            }

            ZlibCodec deflater = deflate(conversionBuffer, (int)inflatedSize, nMaxInflateBuffer);

            // Put the result in the first IDAT chunk (the only one to be written out)
            PNGTrunk firstDataTrunk = getTrunk("IDAT");

            CRC32 crc32 = new CRC32();
            foreach (byte b in System.Text.Encoding.UTF8.GetBytes(firstDataTrunk.getName()))
            {
                crc32.UpdateCRC(b);
            }
                crc32.UpdateCRC((byte)deflater.NextOut,(int)deflater.TotalBytesOut);
            long lCRCValue = crc32.Crc32Result;

            firstDataTrunk.m_nData = deflater.OutputBuffer;
            firstDataTrunk.m_nCRC[0] = (byte) ((lCRCValue & 0xFF000000) >> 24);
            firstDataTrunk.m_nCRC[1] = (byte) ((lCRCValue & 0xFF0000) >> 16);
            firstDataTrunk.m_nCRC[2] = (byte) ((lCRCValue & 0xFF00) >> 8);
            firstDataTrunk.m_nCRC[3] = (byte) (lCRCValue & 0xFF);
            firstDataTrunk.m_nSize = (int) deflater.TotalBytesOut;

            return false;
        }

        private void writePng(string newFileName)
        {
            FileStream outStream = new FileStream(newFileName,FileMode.OpenOrCreate);
            try
            {
                byte[] pngHeader = {unchecked((byte)-119), 80, 78, 71, 13, 10, 26, 10};
                outStream.Write(pngHeader,0,pngHeader.Length);
                bool dataWritten = false;
                foreach (PNGTrunk trunk in trunks)
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
                        else
                        {
                            dataWritten = true;
                        }
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

        private void readTrunks(string pngFile)
        {
            BinaryReader input = new BinaryReader(new FileStream(pngFile,FileMode.OpenOrCreate));
            try
            {
                byte[] nPNGHeader = new byte[8];
                nPNGHeader=input.ReadBytes(8);

                trunks = new List<PNGTrunk>();
                if ((nPNGHeader[0] == unchecked((byte)(-119))) && (nPNGHeader[1] == 0x50)
                    && (nPNGHeader[2] == 0x4e) && (nPNGHeader[3] == 0x47)
                    && (nPNGHeader[4] == 0x0d) && (nPNGHeader[5] == 0x0a)
                    && (nPNGHeader[6] == 0x1a) && (nPNGHeader[7] == 0x0a))
                {

                    PNGTrunk trunk;
                    while(true)
                    {
                        trunk = PNGTrunk.generateTrunk(input);
                        trunks.Add(trunk);
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

        public bool convert(string sourceFile)
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
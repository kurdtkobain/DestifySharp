using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IPNGConverter
{
    public class PNGTrunk
    {
        public int m_nSize;
	protected String m_szName;
        public byte[] m_nData;
        public byte[] m_nCRC;

    public static PNGTrunk generateTrunk(BinaryReader input)
    {
		int nSize = readPngInt(input);
		byte[] nData = new byte[4];
        input.Read(nData, 0, nData.Length);
        String szName = System.Text.Encoding.Default.GetString(nData);

		byte[] nDataBuffer = new byte[nSize];
        input.Read(nDataBuffer, 0, nSize);

		byte[] nCRC = new byte[4];
        input.Read(nCRC, 0, nCRC.Length);

		if (szName.Equals("IHDR",StringComparison.OrdinalIgnoreCase)) {
			return new PNGIHDRTrunk(nSize, szName, nDataBuffer, nCRC);
		}

		return new PNGTrunk(nSize, szName, nDataBuffer, nCRC);
	}

	protected PNGTrunk(int nSize, String szName, byte[] nCRC) {
		m_nSize = nSize;
		m_szName = szName;
		m_nCRC = nCRC;
	}

	protected PNGTrunk(int nSize, String szName, byte[] nData, byte[] nCRC) {
        m_nSize = nSize;
        m_szName = szName;
        m_nCRC = nCRC;
		m_nData = nData;
	}

	public int getSize() {
		return m_nSize;
	}

	public String getName() {
		return m_szName;
	}

	public byte[] getData() {
		return m_nData;
	}

	public byte[] getCRC() {
		return m_nCRC;
	}

	public void writeToStream(FileStream outStream){
		byte[] nSize = new byte[4];
		nSize[0] = (byte) ((m_nSize & 0xFF000000) >> 24);
		nSize[1] = (byte) ((m_nSize & 0xFF0000) >> 16);
		nSize[2] = (byte) ((m_nSize & 0xFF00) >> 8);
		nSize[3] = (byte) (m_nSize & 0xFF);

		outStream.Write(nSize,0,4);
        outStream.Write(Encoding.ASCII.GetBytes(m_szName.ToCharArray()), 0, Encoding.ASCII.GetBytes(m_szName.ToCharArray()).Length);
		outStream.Write(m_nData, 0, m_nSize);
		outStream.Write(m_nCRC,0,m_nCRC.Length);
	}

	public static void writeInt(byte[] nDes, int nPos, int nVal) {
		nDes[nPos] = (byte) ((nVal & 0xff000000) >> 24);
		nDes[nPos + 1] = (byte) ((nVal & 0xff0000) >> 16);
		nDes[nPos + 2] = (byte) ((nVal & 0xff00) >> 8);
		nDes[nPos + 3] = (byte) (nVal & 0xff);
	}

	public static int readPngInt(BinaryReader input){
		 byte[] buffer = new byte[4];
	    input.Read(buffer,0,buffer.Length);
		return readInt(buffer, 0);
	}

	public static int readInt(byte[] nDest, int nPos) {
		return ((nDest[nPos++] & 0xFF) << 24) | ((nDest[nPos++] & 0xFF) << 16) | ((nDest[nPos++] & 0xFF) << 8) | (nDest[nPos] & 0xFF);
	}

	public static void writeCRC(byte[] nData, int nPos) {
		int chunklen = readInt(nData, nPos);
		int sum = (int)(CRCChecksum(nData, nPos + 4, 4 + chunklen) ^ 0xffffffff);
		writeInt(nData, nPos + 8 + chunklen, sum);
	}

	public static int[] crc_table = null;

	public static int CRCChecksum(byte[] nBuffer, int nOffset, int nLength) {
		uint c = 0xffffffff;
		int n;
		if (crc_table == null) {
			int mkc;
			int mkn, mkk;
			crc_table = new int[256];
			for (mkn = 0; mkn < 256; mkn++) {
				mkc = mkn;
				for (mkk = 0; mkk < 8; mkk++) {
					if ((mkc & 1) == 1) {
						mkc = (int)(0xedb88320 ^ (mkc >> 1));
					} else {
						mkc = mkc >> 1;
					}
				}
				crc_table[mkn] = mkc;
			}
		}
		for (n = nOffset; n < nLength + nOffset; n++) {
			c = (uint)(crc_table[(c ^ nBuffer[n]) & 0xff] ^ (c >> 8));
		}
		return (int)c;
	}
    }
}

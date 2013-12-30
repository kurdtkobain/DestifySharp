using System;
using System.IO;
using System.Text;

namespace IPNGConverter
{
    public class PngTrunk
    {
        public int MnSize;
	protected String MSzName;
        public byte[] MnData;
        public byte[] MnCrc;

    public static PngTrunk generateTrunk(BinaryReader input)
    {
		int nSize = readPngInt(input);
		var nData = new byte[4];
        input.Read(nData, 0, nData.Length);
        String szName = Encoding.Default.GetString(nData);

		var nDataBuffer = new byte[nSize];
        input.Read(nDataBuffer, 0, nSize);

		var nCrc = new byte[4];
        input.Read(nCrc, 0, nCrc.Length);

		if (szName.Equals("IHDR",StringComparison.OrdinalIgnoreCase)) {
			return new PngihdrTrunk(nSize, szName, nDataBuffer, nCrc);
		}

		return new PngTrunk(nSize, szName, nDataBuffer, nCrc);
	}

	protected PngTrunk(int nSize, String szName, byte[] nCrc) {
		MnSize = nSize;
		MSzName = szName;
		MnCrc = nCrc;
	}

	protected PngTrunk(int nSize, String szName, byte[] nData, byte[] nCrc) {
        MnSize = nSize;
        MSzName = szName;
        MnCrc = nCrc;
		MnData = nData;
	}

	public int getSize() {
		return MnSize;
	}

	public String getName() {
		return MSzName;
	}

	public byte[] getData() {
		return MnData;
	}

	public byte[] getCrc() {
		return MnCrc;
	}

	public void writeToStream(FileStream outStream){
		var nSize = new byte[4];
		nSize[0] = (byte) ((MnSize & 0xFF000000) >> 24);
		nSize[1] = (byte) ((MnSize & 0xFF0000) >> 16);
		nSize[2] = (byte) ((MnSize & 0xFF00) >> 8);
		nSize[3] = (byte) (MnSize & 0xFF);

		outStream.Write(nSize,0,4);
        outStream.Write(Encoding.ASCII.GetBytes(MSzName.ToCharArray()), 0, Encoding.ASCII.GetBytes(MSzName.ToCharArray()).Length);
		outStream.Write(MnData, 0, MnSize);
		outStream.Write(MnCrc,0,MnCrc.Length);
	}

	public static void writeInt(byte[] nDes, int nPos, int nVal) {
		nDes[nPos] = (byte) ((nVal & 0xff000000) >> 24);
		nDes[nPos + 1] = (byte) ((nVal & 0xff0000) >> 16);
		nDes[nPos + 2] = (byte) ((nVal & 0xff00) >> 8);
		nDes[nPos + 3] = (byte) (nVal & 0xff);
	}

	public static int readPngInt(BinaryReader input){
		 var buffer = new byte[4];
	    input.Read(buffer,0,buffer.Length);
		return readInt(buffer, 0);
	}

	public static int readInt(byte[] nDest, int nPos) {
		return ((nDest[nPos++] & 0xFF) << 24) | ((nDest[nPos++] & 0xFF) << 16) | ((nDest[nPos++] & 0xFF) << 8) | (nDest[nPos] & 0xFF);
	}

	public static void writeCrc(byte[] nData, int nPos) {
		int chunklen = readInt(nData, nPos);
		var sum = (int)(crcChecksum(nData, nPos + 4, 4 + chunklen) ^ 0xffffffff);
		writeInt(nData, nPos + 8 + chunklen, sum);
	}

	public static int[] CrcTable;

	public static int crcChecksum(byte[] nBuffer, int nOffset, int nLength) {
		uint c = 0xffffffff;
		int n;
		if (CrcTable == null) {
		    int mkn;
		    CrcTable = new int[256];
			for (mkn = 0; mkn < 256; mkn++) {
			    int mkc = mkn;
			    int mkk;
			    for (mkk = 0; mkk < 8; mkk++) {
					if ((mkc & 1) == 1) {
						mkc = (int)(0xedb88320 ^ (mkc >> 1));
					} else {
						mkc = mkc >> 1;
					}
				}
				CrcTable[mkn] = mkc;
			}
		}
		for (n = nOffset; n < nLength + nOffset; n++) {
			c = (uint)(CrcTable[(c ^ nBuffer[n]) & 0xff] ^ (c >> 8));
		}
		return (int)c;
	}
    }
}

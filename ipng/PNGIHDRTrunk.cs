using System;
using System.Collections.Generic;
using System.Text;

namespace IPNGConverter
{
    public class PngihdrTrunk:PngTrunk
{
    public int MnWidth;
    public int MnHeight;

    public PngihdrTrunk(int nSize, String szName, byte[] nData, byte[] nCRC)
        : base(nSize,szName,nData,nCRC)
    {
        MnWidth = readInt(nData, 0);
        MnHeight = readInt(nData, 4);
    }
}
}

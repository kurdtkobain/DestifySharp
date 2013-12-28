using System;
using System.Collections.Generic;
using System.Text;

namespace IPNGConverter
{
    public class PNGIHDRTrunk:PNGTrunk
{
    public int m_nWidth;
    public int m_nHeight;

    public PNGIHDRTrunk(int nSize, String szName, byte[] nData, byte[] nCRC)
        : base(nSize,szName,nData,nCRC)
    {
        m_nWidth = readInt(nData, 0);
        m_nHeight = readInt(nData, 4);
    }
}
}

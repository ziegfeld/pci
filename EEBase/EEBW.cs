using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

using System.Runtime.InteropServices;

namespace Enterprise
{

	[ComVisible(false), ClassInterface(ClassInterfaceType.None)]
	public class EEBitwise
	{

		// Returns true if the bit specified in intBit is true, false if not.
        public bool ExamineBit(byte bytToCheck, int intBit)
		{
            byte BitMask = Convert.ToByte(1 << (intBit - 1));
			return ((bytToCheck & BitMask) != 0);
		}

		// Clears the bit specified in intBit in the byte given in bytByte
		public void ClearBit(ref byte bytByte, int intBit)
		{
            byte BitMaskComplement = Convert.ToByte(~ (1 << (intBit - 1)));
            bytByte &= BitMaskComplement;// Convert.ToByte(~BitMask);
		}

		// Sets the bit specified in intBit in the byte given in bytByte
		public void SetBit(ref byte bytByte, int intBit)
		{
            byte BitMask = Convert.ToByte(1 << (intBit - 1));
            bytByte |= BitMask;
		}

		// Toggles the bit specified in intBit in the byte given in bytByte
        public void ToggleBit(ref byte bytByte, int intBit)
		{
            byte BitMask = Convert.ToByte(1 << (intBit - 1));
			bytByte ^= BitMask;
		}

	}

}

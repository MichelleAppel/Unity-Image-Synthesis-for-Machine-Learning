using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class ColorEncoding
{
	public static int SparsifyBits(byte value, int sparse)
	{
		int retVal = 0;
		for (int bits = 0; bits < 8; bits++, value >>= 1)
		{
			retVal |= (value & 1);
			retVal <<= sparse;
		}
		return retVal >> sparse;
	}

	public static Color EncodeIDAsColor(int instanceId)
	{
		var uid = instanceId * 2;
		if (uid < 0)
			uid = -uid + 1;

		var sid =
			(SparsifyBits((byte)(uid >> 16), 3) << 2) |
			(SparsifyBits((byte)(uid >>  8), 3) << 1) |
			 SparsifyBits((byte)(uid      ), 3);
		//Debug.Log(uid + " >>> " + System.Convert.ToString(sid, 2).PadLeft(24, '0'));
	
		var r = (byte)(sid >> 8);
		var g = (byte)(sid >> 16);
		var b = (byte)(sid);
		
		//Debug.Log(r + " " + g + " " + b);
		return new Color32 (r, g, b, 255);
	}

	public static Color EncodeNameAsColor(string name)
	{
		var category = NameToCategory(name);

		var hash = category.GetHashCode();
		// var a = (byte)(hash >> 24);
		var r = (byte)(hash >> 16);
		var g = (byte)(hash >> 8);
		var b = (byte)(hash);
		return new Color32 (r, g, b, 255);
	}

	private static string NameToCategory(string name)
	{
		return name;
	}
}

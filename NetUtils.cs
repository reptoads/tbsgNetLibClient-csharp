using System;
using System.Runtime.InteropServices;
using System.Text;

namespace tbsgNetLib{

public class NetUtils{

    [DllImport("kernel32.dll")]
    private static extern Int32 WideCharToMultiByte(UInt32 CodePage, UInt32 dwFlags, [MarshalAs(UnmanagedType.LPWStr)] String lpWideCharStr, Int32 cchWideChar, [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder lpMultiByteStr, Int32 cbMultiByte, IntPtr lpDefaultChar, IntPtr lpUsedDefaultChar);

    public static string Utf16ToUtf8(string utf16String)
    {
        Int32 iNewDataLen = WideCharToMultiByte(Convert.ToUInt32(Encoding.UTF8.CodePage), 0, utf16String, utf16String.Length, null, 0, IntPtr.Zero, IntPtr.Zero);
        if (iNewDataLen > 1)
        {
            StringBuilder utf8String = new StringBuilder(iNewDataLen);
            WideCharToMultiByte(Convert.ToUInt32(Encoding.UTF8.CodePage), 0, utf16String, -1, utf8String, utf8String.Capacity, IntPtr.Zero, IntPtr.Zero);

            return utf8String.ToString();
        }
        else
        {
            return String.Empty;
        }
    }

    [DllImport("kernel32.dll")]
    private static extern Int32 MultiByteToWideChar(UInt32 CodePage, UInt32 dwFlags, [MarshalAs(UnmanagedType.LPStr)] String lpMultiByteStr, Int32 cbMultiByte, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpWideCharStr, Int32 cchWideChar);

    public static string Utf8ToUtf16(string utf8String)
    {
        Int32 iNewDataLen = MultiByteToWideChar(Convert.ToUInt32(Encoding.UTF8.CodePage), 0, utf8String, -1, null, 0);
        if (iNewDataLen > 1)
        {
            StringBuilder utf16String = new StringBuilder(iNewDataLen);
            MultiByteToWideChar(Convert.ToUInt32(Encoding.UTF8.CodePage), 0, utf8String, -1, utf16String, utf16String.Capacity);

            return utf16String.ToString();
        }
        else
        {
            return String.Empty;
        }
    }

    public static string Base64Encode(string plainText)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return System.Convert.ToBase64String(plainTextBytes);
    }

    public static string Base64Decode(string base64EncodedData)
    {
        var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
        return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
    }

    }
}
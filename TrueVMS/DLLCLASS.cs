using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TrueVMS
{
    public class DLLCLASS
    {
        //UInt32 HandCom;

        [DllImport("CRT_571.dll", CharSet = CharSet.Ansi)]
        public static extern UInt32 CommOpen(String ComStr);


        [DllImport("CRT_571.dll", CharSet = CharSet.Ansi)]
        public static extern UInt32 CommOpenWithBaut(String ComStr, UInt32 Handle);

        [DllImport("CRT_571.dll", CharSet = CharSet.Ansi)]
        public static extern UInt32 CommClose(UInt32 Handle);


        [DllImport("CRT_571.dll", CharSet = CharSet.Ansi)]
        public static extern Int32 ExecuteCommand(UInt32 Handle, Byte TxAddr, Byte TxCmCode, Byte TxCpCode, UInt16 TxDataLen, Byte TxData,
                                          ref byte RxReplyType, ref byte RxStCode0, ref byte RxStCode1, ref byte RxStCode2, ref UInt16 RxDataLen, ref byte RxData); 


    }
}

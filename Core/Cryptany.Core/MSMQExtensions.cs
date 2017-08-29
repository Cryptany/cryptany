/*
   Copyright 2006-2017 Cryptany, Inc.

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;
using System.Messaging;
using System.Runtime.InteropServices;

namespace Cryptany.Core
{
    public static class MessageQueueExtensions
    {

        [DllImport("mqrt.dll")]
        private static extern int MQMgmtGetInfo([MarshalAs(UnmanagedType.BStr)]string computerName, [MarshalAs(UnmanagedType.BStr)]string objectName, ref MQMGMTPROPS mgmtProps);

        private const byte VT_NULL = 1;
        private const byte VT_UI4 = 19;
        private const int PROPID_MGMT_QUEUE_MESSAGE_COUNT = 7;

        //size must be 16
        [StructLayout(LayoutKind.Sequential)]
        private struct MQPROPVariant
        {
            public byte vt;       //0
            public byte spacer;   //1
            public short spacer2; //2
            public int spacer3;   //4
            public uint ulVal;    //8
            public int spacer4;   //12
        }

        //size must be 16 in x86 and 28 in x64
        [StructLayout(LayoutKind.Sequential)]
        private struct MQMGMTPROPS
        {
            public uint cProp;
            public IntPtr aPropID;
            public IntPtr aPropVar;
            public IntPtr status;
        }

        public static uint GetCount(MessageQueue queue)
        {
            return GetCount(queue.Path);
        }

        private static uint GetCount(string path)
        {
            if (!MessageQueue.Exists(path))
            {
                return 0;
            }

            MQMGMTPROPS props = new MQMGMTPROPS();
            props.cProp = 1;
            try
            {
                props.aPropID = Marshal.AllocHGlobal(sizeof(int));
                Marshal.WriteInt32(props.aPropID, PROPID_MGMT_QUEUE_MESSAGE_COUNT);

                props.aPropVar = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MQPROPVariant)));
                MQPROPVariant vtnull = new MQPROPVariant();// { vt = VT_NULL }
                vtnull.vt = VT_NULL;
                Marshal.StructureToPtr(vtnull, props.aPropVar, false);

                props.status = Marshal.AllocHGlobal(sizeof(int));
                Marshal.WriteInt32(props.status, 0);

                int result = MQMgmtGetInfo(null, "queue=Direct=OS:" + path, ref props);
                if (result != 0 || Marshal.ReadInt32(props.status) != 0)
                {
                    return 0;
                }

                MQPROPVariant propVar = (MQPROPVariant)Marshal.PtrToStructure(props.aPropVar, typeof(MQPROPVariant));
                if (propVar.vt != VT_UI4)
                {
                    return 0;
                }
                else
                {
                    return propVar.ulVal;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(props.aPropID);
                Marshal.FreeHGlobal(props.aPropVar);
                Marshal.FreeHGlobal(props.status);
            }
        }
    }
}
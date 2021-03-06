﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using YanZhiwei.DotNet.ModbusProtocol.Utilities.Model;

namespace YanZhiwei.DotNet.ModbusProtocol.Utilities.Tests
{
    [TestClass()]
    public class RTUModBusUnPackageTests
    {
        [TestMethod()]
        public void BuilderObjFromBytesTest()
        {
            byte[] _orderCmd0x01 = { 0x02, 0x01, 0x02, 0x1F, 0x00, 0xF5, 0xCC };
            ModBusRTUUnPackage _unpackageOrderCmd0x01 = new ModBusRTUUnPackage();
            SlaveReplyDataBase _slaveReplyData = null;
            UnPackageError _unpackageError = _unpackageOrderCmd0x01.BuilderObjFromBytes(_orderCmd0x01, out _slaveReplyData);
            Assert.AreEqual(_slaveReplyData.OrderCmdCode, 0x01);


            byte[] _orderCmd0x03 = { 002, 0x03, 0x14, 0x00, 0x01, 0x00, 0x02, 0x00, 0x03, 0x00, 0x04, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x37, 0x57 };
            ModBusRTUUnPackage _unpackageOrderCmd0x03 = new ModBusRTUUnPackage();
            _slaveReplyData = null;
            _unpackageError = _unpackageOrderCmd0x03.BuilderObjFromBytes(_orderCmd0x03, out _slaveReplyData);
            Assert.AreEqual(_slaveReplyData.OrderCmdCode, 0x03);
        }
    }
}
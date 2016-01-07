﻿/*
 * Zazzles : A cross platform service framework
 * Copyright (C) 2014-2016 FOG Project
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 3
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace Zazzles.Tests.Process
{
    [TestFixture]
    public class ProcessHandlerTests
    {
        private const string TestEXE = "ProcessTester.exe";
        private const string IncludesDir = "tests-include";

        private string TestEXEPath;

        [SetUp]
        public void Init()
        {
            Log.Output = Log.Mode.Console;

            var loc = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            loc = loc.Replace(@"file:\", string.Empty);
            TestEXEPath = Path.Combine(loc, IncludesDir, TestEXE);
        }


        [Test]
        public void RunSTDOUT()
        {
            string[] echoTest =
            {
                "hello",
                "12345",
                "@-Q\\/"
            };

            string[] stdout;

            var filename = TestEXEPath;
            var param = "echo " + string.Join(" ", echoTest);
            MakeUnixFriendly(ref filename, ref param);
            ProcessHandler.Run(filename, param, true, out stdout);

            Assert.AreEqual(echoTest, stdout);
        }

        [Test]
        public void RunReturnCodes()
        {
            var filename = TestEXEPath;
            var param = "exit 0";
            MakeUnixFriendly(ref filename, ref param);
            var exit0 = ProcessHandler.Run(filename, param, true);

            filename = TestEXEPath;
            param = "exit 1";
            MakeUnixFriendly(ref filename, ref param);
            var exit1 = ProcessHandler.Run(filename, param, true);

            filename = TestEXEPath;
            param = "exit 2";
            MakeUnixFriendly(ref filename, ref param);
            var exit2 = ProcessHandler.Run(filename, param, true);

            Assert.AreEqual(0, exit0);
            Assert.AreEqual(1, exit1);
            Assert.AreEqual(2, exit2);
        }

        [Test]
        public void RunFakeFile()
        {
            var exit = ProcessHandler.Run("FOG_DNE_FILE.FOG_DNE_FILE", "", true);

            Assert.AreEqual(-1, exit);
        }

        [Test]
        public void NullEmptyRun()
        {
            Assert.Throws<ArgumentException>(() => ProcessHandler.Run(null, "", true));
            Assert.Throws<ArgumentNullException>(() => ProcessHandler.Run("a", null, true));
            Assert.Throws<ArgumentException>(() => ProcessHandler.Run(null, "", true));
            Assert.Throws<ArgumentException>(() => ProcessHandler.Run("", "", true));
        }

        private void MakeUnixFriendly(ref string filename , ref string param)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                return;

            param = filename + " " + param;
            filename = "mono";
        }
    }
}
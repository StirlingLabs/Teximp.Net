/*
* Copyright (c) 2016-2019 TeximpNet - Nicholas Woodfield
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using Xunit;

namespace TeximpNet.Test
{
    /// <summary>
    /// Helper for Assimp.NET testing.
    /// </summary>
    public static class TestHelper
    {
        public const float DEFAULT_TOLERANCE = 0.000001f;
        public static float Tolerance = DEFAULT_TOLERANCE;

        private static String m_rootPath = null;

        public static String RootPath
        {
            get
            {
                if (m_rootPath == null)
                {
                    /*
                    Assembly entryAssembly = Assembly.GetEntryAssembly();
                    String dirPath = String.Empty;

                    if (entryAssembly != null)
                        dirPath = Path.GetDirectoryName(entryAssembly.Location);

                    m_rootPath = dirPath;*/
                    m_rootPath = AppContext.BaseDirectory;
                }

                return m_rootPath;
            }
        }

        public static void AssertEquals(float expected, float actual)
        {
            Assert.True(Math.Abs(expected - actual) <= Tolerance);
        }

        public static void AssertEquals(float expected, float actual, String msg)
        {
            Assert.True(Math.Abs(expected - actual) <= Tolerance, msg);
        }

        //Used for identifying a batch of files that are ordered, e.g. XXX_000, XXX_001, XXX_002. So we get # of dimensions to iterate over.
        public static void GetDigits(List<int> list, int num, int minDigitsCount)
        {
            list.Clear();

            while(num > 0)
            {
                list.Add(num % 10);
                num = num / 10;
            }

            //Pad any zeros
            while(list.Count < minDigitsCount)
                list.Add(0);

            list.Reverse();
        }
    }
}

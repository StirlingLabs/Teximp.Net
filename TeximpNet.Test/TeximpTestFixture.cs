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
using System.IO;

namespace TeximpNet.Test
{
    public abstract class TeximpTestFixture
    {
        private String m_outputPath;
        private String m_inputPath;

        protected String OutputPath
        {
            get
            {
                return m_outputPath;
            }
        }

        protected String InputPath
        {
            get
            {
                return m_inputPath;
            }
        }

        public TeximpTestFixture()
        {
            m_inputPath = Path.Combine(TestHelper.RootPath, "TestFiles");
            m_outputPath = Path.Combine(TestHelper.RootPath, "OutPut", GetType().Name);

            CleanOutput();
        }

        private void CleanOutput()
        {
            if (!Directory.Exists(m_outputPath))
                Directory.CreateDirectory(m_outputPath);

            IEnumerable<String> filePaths = Directory.GetFiles(m_outputPath);

            foreach (String filePath in filePaths)
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        protected String GetOutputFile(String fileName)
        {
            return Path.Combine(m_outputPath, fileName);
        }

        protected String GetInputFile(String fileName)
        {
            return Path.Combine(m_inputPath, fileName);
        }
    }
}

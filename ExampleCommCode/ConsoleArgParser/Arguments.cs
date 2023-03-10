/*
 * Original Code grabbed from CodeProject (http://www.codeproject.com/KB/recipes/command_line.aspx)
 * Licensed under The MIT License (http://www.opensource.org/licenses/mit-license.php)
 * 
 * The MIT License 
 * 
 * Copyright (c) <year> <copyright holders>
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
 * 
 */

using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace CommandLine.Utility
{
    /// <summary>

    /// Arguments class

    /// </summary>

    public class Arguments
    {
        // Variables

        private StringDictionary m_Parameters;

        // Constructor

        public Arguments(string[] Args)
        {
            m_Parameters = new StringDictionary();
            Regex Spliter = new Regex(@"^-{1,2}|^/|=|:",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            Regex Remover = new Regex(@"^['""]?(.*?)['""]?$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            string Parameter = null;
            string[] Parts;

            // Valid parameters forms:

            // {-,/,--}param{ ,=,:}((",')value(",'))

            // Examples: 

            // -param1 value1 --param2 /param3:"Test-:-work" 

            //   /param4=happy -param5 '--=nice=--'

            foreach (string Txt in Args)
            {
                // Look for new parameters (-,/ or --) and a

                // possible enclosed value (=,:)

                Parts = Spliter.Split(Txt, 3);

                switch (Parts.Length)
                {
                    // Found a value (for the last parameter 

                    // found (space separator))

                    case 1:
                        if (Parameter != null)
                        {
                            if (!m_Parameters.ContainsKey(Parameter))
                            {
                                Parts[0] =
                                    Remover.Replace(Parts[0], "$1");

                                m_Parameters.Add(Parameter, Parts[0]);
                            }
                            Parameter = null;
                        }
                        // else Error: no parameter waiting for a value (skipped)

                        break;

                    // Found just a parameter

                    case 2:
                        // The last parameter is still waiting. 

                        // With no value, set it to true.

                        if (Parameter != null)
                        {
                            if (!m_Parameters.ContainsKey(Parameter))
                                m_Parameters.Add(Parameter, "true");
                        }
                        Parameter = Parts[1];
                        break;

                    // Parameter with enclosed value

                    case 3:
                        // The last parameter is still waiting. 

                        // With no value, set it to true.

                        if (Parameter != null)
                        {
                            if (!m_Parameters.ContainsKey(Parameter))
                                m_Parameters.Add(Parameter, "true");
                        }

                        Parameter = Parts[1];

                        // Remove possible enclosing characters (",')

                        if (!m_Parameters.ContainsKey(Parameter))
                        {
                            Parts[2] = Remover.Replace(Parts[2], "$1");
                            m_Parameters.Add(Parameter, Parts[2]);
                        }

                        Parameter = null;
                        break;
                }
            }
            // In case a parameter is still waiting

            if (Parameter != null)
            {
                if (!m_Parameters.ContainsKey(Parameter))
                    m_Parameters.Add(Parameter, "true");
            }
        }

        // Retrieve a parameter value if it exists 

        // (overriding C# indexer property)

        public string this[string Param]
        {
            get
            {
                return (m_Parameters[Param]);
            }
        }

        public StringDictionary FullArgList
        {
            get
            {
                return m_Parameters;
            }
        }
    }
}

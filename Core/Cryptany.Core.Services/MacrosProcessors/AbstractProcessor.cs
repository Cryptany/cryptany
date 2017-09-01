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
using System.Collections.Generic;
using System.Diagnostics;

namespace Cryptany.Core.MacrosProcessors
{

    /// <summary>
    /// �����-������ ��� ���� ������������ ��������.
    /// </summary>
    public abstract class MacrosProcessor
    {
        /// <summary>
        /// �����, ����������� ��������� �������
        /// </summary>
        /// <param name="parameters">��������� �������</param>
        /// <returns>������������ ��������, ������� ����� �������� ���� �������</returns>
        public abstract string Execute(OutputMessage omsg, Dictionary<string, string> parameters);
        /// <summary>
        /// ��� �������, ������������ �������-������������
        /// </summary>
        public abstract string MacrosName { get; }
    }

    /// <summary>
    /// �����, �������������� ���� �������
    /// </summary>
    public class Macros
    {
        /// <summary>
        /// �������� ������ �������� � ������ � ������������� �����������
        /// </summary>
        /// <param name="msgText">������� �����, ���������� �������</param>
        /// <returns>������ ��������, ��������� � ������</returns>
        public static List<Macros> GetMacroses(string msgText)
        {
            List<Macros> list = new List<Macros>();
            string text = new string(msgText.ToCharArray());
            int open = text.IndexOf("{#");

            while (open >= 0)
            {
                int close = text.IndexOf("#}", open);

                if (close - open < 2)
                {
                    throw new Exception("Macros markup error");

                }
                string macros = text.Substring(open /*+ "{#".Length*/, close + 2 - (open /*+ "{#".Length*/));
                ParsingResults res = Parse(macros);
                if (res.HasErrors)
                {
                    string exceptionText = string.Format(
                        "Error occured while parsing macros. Error message: \r\n {0} \r\n\r\n macros text: \r\n {1}",
                        res.ErrorMessage, res.MacrosText);
                    throw new Exception(exceptionText);
                }

                list.Add(new Macros(res, open, close));

                open = text.IndexOf("{#", close);

            }

            return list;

        }


        private readonly string _name = "";
        private readonly Dictionary<string, string> _pars = new Dictionary<string, string>();
        private readonly int _startPosition;
        private readonly int _endPosition;
        private readonly string _text;

        /// <summary>
        /// ����������� ������ Macros
        /// </summary>
        /// <param name="res">���������� �������� �������</param>
        /// <param name="startPosition">��������� ������� ���� ������� � �������� ������</param>
        /// <param name="endPosition">�������� ������� ������� � �������� ������</param>
        internal Macros(ParsingResults res, int startPosition, int endPosition)
        {
            _name = res.Name;
            _pars = res.Parameters;
            _text = res.MacrosText;
            _startPosition = startPosition;
            _endPosition = endPosition;
        }

        /// <summary>
        /// ��� �������
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }


        /// <summary>
        /// ����� �������� ���������� �������
        /// </summary>
        public Dictionary<string, string> Parameters
        {
            get
            {
                return _pars;
            }
        }

        /// <summary>
        /// ��������� ������� ���� ������� � �������� ������
        /// </summary>
        public int StartPosition
        {
            get { return _startPosition; }
        }

        /// <summary>
        /// �������� ������� ������� � �������� ������
        /// </summary>
        public int EndPosition
        {
            get { return _endPosition; }
        }

        /// <summary>
        /// ������ ����� �������
        /// </summary>
        public string Text
        {
            get { return _text; }
        }


        /// <summary>
        /// �����, �������������� ���� �������
        /// </summary>
        /// <param name="macros">���� �������</param>
        /// <returns>��������� � ������������ �������� ���� �������</returns>
        private static ParsingResults Parse(string macros)
        {
            ParsingResults res = new ParsingResults();
            res.MacrosText = macros;

            if (!(macros.StartsWith("{#") && macros.EndsWith("#}")))
            {
                res.HasErrors = true;
                res.ErrorMessage = "Leading or trailing macros delimiter not found";
                return res;
            }
            string text = macros.Replace("{#", "").Replace("#}", "").Trim();

            if (!text.Contains("("))
            {
                res.Name = text;
                return res;
            }

            int open = text.IndexOf("(");
           // int close = text.IndexOf(")");
            int close = text.LastIndexOf(")");
            if (close < open)
            {
                res.HasErrors = true;
                res.ErrorMessage = "Syntax error: opening parenthesis should preceed the closing one";
                return res;
            }

            res.Name = text.Substring(0, open).Trim();
            Trace.WriteLine("Router: ����� ������ " + res.Name);
            string pars = text.Substring(open + 1, close - open - 1);

            res.Parameters = new Dictionary<string, string>();
            if (pars.Split(';').Length == 1)
            {
                if (pars.IndexOf("=") < 0)
                    res.Parameters.Add("#default#", pars.Trim());
            }
            else
                foreach (string s in pars.Split(';'))
                {
                    string ss = s.Trim();
                    int eq = ss.IndexOf("=");

                    if (eq <= 0)
                    {
                        res.HasErrors = true;
                        res.ErrorMessage = "Syntax error: assign statement expected, but something else found";
                        return res;
                    }

                    string parName = s.Split('=')[0];
                    string parValue = s.Split('=')[1];

                    res.Parameters.Add(parName, parValue);
                }

            res.HasErrors = false;
            return res;
        }

        /// <summary>
        /// ��������� � ������������ ��������
        /// </summary>
        internal struct ParsingResults
        {
            /// <summary>
            /// ��� �������
            /// </summary>
            public string Name;

            /// <summary>
            /// ����� �������� ���������� �������
            /// </summary>
            public Dictionary<string, string> Parameters;

            /// <summary>
            /// ���� �� ������ ��� ��������
            /// </summary>
            public bool HasErrors;

            /// <summary>
            /// ��������� �� ������, ����������� �� ����� ��������
            /// </summary>
            public string ErrorMessage;

            /// <summary>
            /// ������ ����� ���� �������
            /// </summary>
            public string MacrosText;
        }
    }
}


﻿/***************************************************************************
 *  Copyright (C) 2009 by Peter L Jones                                    *
 *  pljones@users.sf.net                                                   *
 *                                                                         *
 *  This file is part of the Sims 3 Package Interface (s3pi)               *
 *                                                                         *
 *  s3pi is free software: you can redistribute it and/or modify           *
 *  it under the terms of the GNU General Public License as published by   *
 *  the Free Software Foundation, either version 3 of the License, or      *
 *  (at your option) any later version.                                    *
 *                                                                         *
 *  s3pi is distributed in the hope that it will be useful,                *
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of         *
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
 *  GNU General Public License for more details.                           *
 *                                                                         *
 *  You should have received a copy of the GNU General Public License      *
 *  along with s3pi.  If not, see <http://www.gnu.org/licenses/>.          *
 ***************************************************************************/
using System;
using System.IO;
using System.Windows.Forms;

namespace S3Pack
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
#if DEBUG
            if (Environment.CommandLine.Contains("/unpack"))
                Application.Run(new Unpack());
            else if (Environment.CommandLine.Contains("/pack"))
                Application.Run(new Pack());
#else
            if (Path.GetFileNameWithoutExtension(Application.ExecutablePath).ToLower().Equals("unpack"))
                Application.Run(new Unpack());
            else if (Path.GetFileNameWithoutExtension(Application.ExecutablePath).ToLower().Equals("pack"))
                Application.Run(new Pack());
#endif
            else
            {
                MessageBox.Show(String.Format("{0} is not recognised as a name for this program.", Path.GetFileNameWithoutExtension(Application.ExecutablePath)));
                return 1;
            }
            return 0;
        }
    }
}

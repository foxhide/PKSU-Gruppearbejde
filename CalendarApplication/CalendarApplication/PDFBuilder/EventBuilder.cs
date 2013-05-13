using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using System.Windows.Forms;

using CalendarApplication.Models.Event;

namespace CalendarApplication.PDFBuilder
{
    public class EventBuilder
    {
        private string file;
        private string pdflatexpath;
        private string auxdir;
        private List<string> input;

        /// <summary>
        /// Initialize the EventBuilder class
        /// </summary>
        /// <param name="basicevent">Model of the event to be built as pdf</param>
        public EventBuilder(BasicEvent basicevent)
        {
            this.file = basicevent.Name + ".tex";
            this.pdflatexpath = "\"C:\\Program Files\\MiKTeX 2.9\\miktex\\bin\\pdflatex.exe\"";
            this.auxdir = "C:\\Users\\Andreas\\tmp\\";
            this.input = new List<string>();

            //header
            this.input.Add("\\documentclass[a4paper,12pt]{article}");
            this.input.Add("\\usepackage[utf8]{inputenc}");
            this.input.Add("\\begin{document}");

            //body

            //epilogue
            this.input.Add("\\end{document}");

            //build pdf
            string command = this.pdflatexpath + " -aux-directory= " + auxdir +
                                "-interaction=nonstopmode -output-directory=" + auxdir + " " + auxdir + file;
            RunCommand(command,
                       "An error has occured while building " + this.file,
                       "An error has occured while trying to build " + this.file + "\n");

            //cleanup auxdir
            RunCommand("del " + auxdir,
                       "An error has occured during cleanup after " + this.file,
                       "An error has occured while trying to cleanup after " + this.file + "\n");
        }

        /// <summary>
        /// Run a given command in cmd.exe, assumes that user is using windows
        /// </summary>
        /// <param name="command">Command to be executed</param>
        /// <param name="error1">Message to be shown upon error on process exit</param>
        /// <param name="error2">Message to be shown upon error while trying to run the command</param>
        private void RunCommand(string command, string error1, string error2)
        {
            try
            {
                int ExitCode;
                ProcessStartInfo ProcessInfo;
                Process Process;

                ProcessInfo = new ProcessStartInfo("cmd.exe", "/c " + command);

                ProcessInfo.CreateNoWindow = true;
                ProcessInfo.UseShellExecute = false;

                Process = Process.Start(ProcessInfo);
                Process.WaitForExit();

                ExitCode = Process.ExitCode;

                Process.Close();

                if (ExitCode != 0)
                {
                    MessageBox.Show(error1, "ExecuteCommand");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(error2 + e.Message, "Error");
            }
        }
    }
}

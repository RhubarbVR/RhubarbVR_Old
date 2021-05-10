using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace RhubarbEngine
{
    public class CommandLineOptions
    {

        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool verbose { get; set; }

        [Option('i', "ip", Required = false, HelpText = "Ip to conect to")]
        public string ip { get; set; }
    }
}

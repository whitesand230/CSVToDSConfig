using System;
using CommandLine.Text;
using CommandLine;
namespace CSVToDSConfig
{
    public class Options
    {
        [Option('i', "inputFilePath", Required = true, HelpText = "Input file to be processed.")]
        public string inputFilePath { get; set; }

        [Option('o', "outputFilePath", Required = true, HelpText = "Output file for the dsconfig commands")]
        public string outputFilePath { get; set; }

        [Option("create", Required = false, HelpText = "If set will generate create dsconfig commands")]
        public bool create { get; set; }

        [Option("delete", Required = false, HelpText = "If set will generate delete dsconfig commands")]
        public bool delete { get; set; }
    }
}
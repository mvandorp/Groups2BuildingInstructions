using CommandLine;
using CommandLine.Text;
using System.Diagnostics.Contracts;

namespace Groups2BuildingInstructions
{
    class Options
    {
        [Option('i', "input", Required = false, DefaultValue = null,
            HelpText = "LXFML file to generate building instructions from.")]
        [ValueOption(0)]
        public string InputFile { get; set; }

        [Option('o', "output", Required = false, DefaultValue = null,
            HelpText = "File to write the output to. If this option is not the input file is overwritten instead.")]
        public string OutputFile { get; set; }

        [Option('s', "substep", Required = false, DefaultValue = 3,
            HelpText = "The maximum depth of substeps to generate.")]
        public int MaxSubstepDepth { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            Contract.Ensures(Contract.Result<string>() != null);

            return HelpText.AutoBuild(this,
                (HelpText current) =>
                {
                    current.AddPreOptionsLine(" ");
                    current.AddPreOptionsLine("Example usage:");
                    current.AddPreOptionsLine("g2bi design.lxfml");

                    HelpText.DefaultParsingErrorsHandler(this, current);
                });
        }

        public string GetMissingInputUsage()
        {
            Contract.Ensures(Contract.Result<string>() != null);

            return HelpText.AutoBuild(this,
                (HelpText current) =>
                {
                    current.AddPreOptionsLine(" ");
                    current.AddPreOptionsLine("ERROR(S):");
                    current.AddPreOptionsLine("  input option is missing.");
                    current.AddPreOptionsLine("");
                    current.AddPreOptionsLine("Example usage:");
                    current.AddPreOptionsLine("g2bi design.lxfml");

                    HelpText.DefaultParsingErrorsHandler(this, current);
                });
        }
    }

}

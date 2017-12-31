using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Groups2BuildingInstructions
{
    class Program
    {
        static void Main(string[] args)
        {
            Options options = new Options();

            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                ValidateOptions(options);

                XDocument xlfml = LoadXLFML(options);

                TransformLXFML(xlfml, options);

                SaveXLFML(options, xlfml);
            }
        }

        static XDocument LoadXLFML(Options options)
        {
            Contract.Requires(options != null);
            Contract.Ensures(Contract.Result<XDocument>() != null);

            XDocument doc = null;

            try
            {
                doc = XDocument.Load(options.InputFile);
            }
            catch (IOException ex)
            {
                Console.WriteLine("Error! Unable to read the input file: {0}", ex.Message);
                Environment.Exit(0);
            }

            return doc;
        }

        static void TransformLXFML(XDocument doc, Options options)
        {
            Contract.Requires(doc != null);
            Contract.Requires(options != null);

            XElement xlfml = doc.Descendants("LXFML").FirstOrDefault();

            if (xlfml == null)
            {
                Console.WriteLine("Error! Input file is not an LXFML document.");
                Environment.Exit(0);
            }

            RemoveOldBuildingInstructionsElements(xlfml);
            AppendNewBuildingInstructionsElement(xlfml, options);
        }

        static void SaveXLFML(Options options, XDocument doc)
        {
            Contract.Requires(options != null);
            Contract.Requires(doc != null);

            string outputFile = options.OutputFile;

            if (outputFile == null)
            {
                outputFile = options.InputFile;
            }

            doc.Save(options.OutputFile);
        }

        static void RemoveOldBuildingInstructionsElements(XElement xlfml)
        {
            Contract.Requires(xlfml != null);

            xlfml.Descendants("BuildingInstructions").Remove();
        }

        static void AppendNewBuildingInstructionsElement(XElement xlfml, Options options)
        {
            Contract.Requires(xlfml != null);
            Contract.Requires(options != null);

            XElement groupSystem = xlfml.Descendants("GroupSystem").FirstOrDefault();

            if (groupSystem == null)
            {
                Console.WriteLine("Error! LXFML file does not contain any groups. Unable to generate building instructions.");
                Environment.Exit(0);
            }

            XElement buildingInstruction = new XElement("BuildingInstruction", new XAttribute("name", "BuildingGuide1"));

            int step = 1;
            foreach (XElement group in groupSystem.Elements("Group"))
            {
                GenerateStep(group, buildingInstruction, ref step, 0, options.MaxSubstepDepth);
            }

            xlfml.Add(new XElement("BuildingInstructions", buildingInstruction));
        }

        static void GenerateStep(XElement group, XElement stepParent, ref int stepNumber, int substepDepth, int maSubstepDepthh)
        {
            Contract.Requires(group != null);
            Contract.Requires(stepParent != null);

            if (GroupHasPartRefs(group))
            {
                // Generate a step for the current group.
                XElement step = CreateStep(stepParent, ref stepNumber, substepDepth);

                foreach (string partRef in GroupGetPartRefs(group))
                {
                    step.Add(new XElement("PartRef", new XAttribute("partRef", partRef)));
                }
            }

            IEnumerable<XElement> subGroups = group.Elements("Group");

            if (subGroups.Any())
            {
                if (GroupHasSiblings(group) && substepDepth < maSubstepDepthh)
                {
                    // Generate a substep for the subgroups.
                    XElement step = CreateStep(stepParent, ref stepNumber, substepDepth);

                    int subStepNumber = 1;

                    foreach (XElement subGroup in subGroups)
                    {
                        GenerateStep(subGroup, step, ref subStepNumber, substepDepth + 1, maSubstepDepthh);
                    }
                }
                else
                {
                    // Generate steps for the subgroups.
                    foreach (XElement subGroup in subGroups)
                    {
                        GenerateStep(subGroup, stepParent, ref stepNumber, substepDepth, maSubstepDepthh);
                    }
                }
            }
        }

        private static XElement CreateStep(XElement stepParent, ref int stepNumber, int substepDepth)
        {
            Contract.Requires(stepParent != null);
            Contract.Ensures(Contract.Result<XElement>() != null);

            string stepName = substepDepth == 0 ?
                $"Step{stepNumber}" :
                $"{stepParent.Attribute("name").Value}Substep{stepNumber}";

            XElement step = new XElement("Step", new XAttribute("name", stepName));

            stepParent.Add(step);
            stepNumber++;

            return step;
        }

        static void ValidateOptions(Options options)
        {
            Contract.Requires(options != null);

            if (string.IsNullOrEmpty(options.InputFile))
            {
                Console.WriteLine(options.GetMissingInputUsage());
                Environment.Exit(0);
            }

            if (!File.Exists(options.InputFile))
            {
                Console.WriteLine("File not found: {0}", options.InputFile);
                Environment.Exit(0);
            }

            if (options.OutputFile == null)
            {
                options.OutputFile = options.InputFile;
            }
        }

        static bool GroupHasSiblings(XElement group)
        {
            Contract.Requires(group != null);
            Contract.Assume(group.Parent != null);

            return group.Parent.Elements("Group").Skip(1).Any();
        }

        static bool GroupHasPartRefs(XElement group)
        {
            XAttribute partRefs = group.Attribute("partRefs");

            return partRefs != null && !String.IsNullOrWhiteSpace(partRefs.Value);
        }

        static string[] GroupGetPartRefs(XElement group)
        {
            Contract.Requires(group != null);
            Contract.Requires(GroupHasPartRefs(group));
            Contract.Ensures(Contract.Result<string[]>() != null);

            XAttribute partRefs = group.Attribute("partRefs");

            return partRefs.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}

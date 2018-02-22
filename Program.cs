using System;
using System.Data;
using Microsoft.VisualBasic.FileIO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CommandLine;

namespace CSVToDSConfig
{
    class Program
    {
        private static Options _options = new Options();
        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
                       .WithParsed<Options>(opts => RunOptionsAndReturnExitCode(opts))
                       .WithNotParsed<Options>((errs) => HandleParseError(errs));
            //readCSV()
            //string csvPath = @"C:\Users\jamel.ahmad\Desktop\SampleCSV_update.csv"; // args[0]
            //string csvPath = arg[0]; //args[0]
            //string objectType = args[1]; //args[1]
            //string schemaName = args[2]; //args[2]
            //string outputFile = args[3]; //args[3]
            Console.WriteLine("Starting Program...");
            //DataTable csvData = GetDataTableFromCSVFile(csvPath, false, true);
            //DataTable csvData = GetDataTableFromCSVFile(csvPath, true, false);
            CreateCSVFile(_options);
            Console.WriteLine("Finished Reading CSV File");
        }

        private static void CreateCSVFile(Options options){

            string input = options.inputFilePath;
            string output = options.outputFilePath;
            string objectType = options.objectType;
            string schemaName = options.schemaName;

            //DataTable csvData = new DataTable();

            try
            {
                using (TextFieldParser csvReader = new TextFieldParser(input))
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(output))
                    {
                        csvReader.SetDelimiters(new string[] {","});
                        csvReader.HasFieldsEnclosedInQuotes = true;

                        //read columns headers
                        csvReader.ReadFields();

                        while(!csvReader.EndOfData)
                        {
                            string[] fieldData = csvReader.ReadFields();

                            string description = createDescriptionString(fieldData[4]);
                            string type = createTypeString(fieldData[8]);

                        }
                    } 
                }

            }
            catch (Exception e){
                Console.WriteLine("ERROR FOUND!!!");
                Console.WriteLine("Error Message: " + e.Message);
            }
        }





        /**
         * [0] = scim attribute
         * [1] = directory attribute
         * [2] = type -- null,simple,complex,sub
         * [3] = sub-attributes
         * [4] = description
         * [5] = required
         * [6] = Multivalue
         * [7] = searchable
         * [8] = format
         * [9] = Constraints (Should be parsed by ;)
         */
        private static DataTable GetDataTableFromCSVFile(string csvPath, Boolean create, Boolean delete)
        {
            string csvPathTest = @"C:\Users\jamel.ahmad\Desktop\SampleCSV_update.csv"; // args[0]
            string objectType = "Consumers"; //args[1]
            string schemaName = "urn:pingidentity:schemas:ciam:Consumer:1.0"; //args[2]
            string outputFile = @"C:\Users\jamel.ahmad\Desktop\output.dsconfig"; //args[3]

            DataTable csvData = new DataTable();
            try
            {
                using (TextFieldParser csvReader = new TextFieldParser(csvPathTest))
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(outputFile))
                    {
                        csvReader.SetDelimiters(new string[] { "," });
                        csvReader.HasFieldsEnclosedInQuotes = true;
                        string[] colFields = csvReader.ReadFields();

                        foreach (string column in colFields)
                        {
                            DataColumn datacolumn = new DataColumn(column);
                            datacolumn.AllowDBNull = true;
                            csvData.Columns.Add(datacolumn);
                        }

                        while (!csvReader.EndOfData)
                        {
                            string[] fieldData = csvReader.ReadFields();
                            string options = createSCIMAttributeOptionString(fieldData);
                            string options_2 = createStoreAdapterMappingOptionString(fieldData);
                            string type = createTypeString(fieldData[8]);
                            string constraints = createConstraintList(fieldData[9]);


                            if (fieldData[2].Equals("sub"))
                            {
                                if (!String.IsNullOrEmpty(constraints))
                                {
                                    string dsconfig1 = string.Format("dsconfig create-scim-subattribute --schema-name {0} --attribute-name {1} --subattribute-name {2} {3} {4} {5} --reason none", schemaName, fieldData[0], fieldData[3], options, type, constraints);
                                    file.WriteLine(dsconfig1);
                                }
                                else
                                {
                                    string dsconfig1 = string.Format("dsconfig create-scim-subattribute --schema-name {0} --attribute-name {1} --subattribute-name {2} {3} {4} --reason none", schemaName, fieldData[0], fieldData[3], options, type);
                                    file.WriteLine(dsconfig1);
                                }

                            }
                            //normal attributes
                            else
                            {
                                string dsconfig1;
                                string dsconfig2;

                                if (!String.IsNullOrEmpty(constraints))
                                {
                                    dsconfig1 = string.Format("dsconfig create-scim-attribute --schema-name {0} --attribute-name {1} --set \"description:{2}\" {3} {4} {5} --reason none", schemaName, fieldData[0], fieldData[4], type, options, constraints);
                                }
                                else
                                {
                                    dsconfig1 = string.Format("dsconfig create-scim-attribute --schema-name {0} --attribute-name {1} --set \"description:{2}\" {3} {4} --reason none", schemaName, fieldData[0], fieldData[4], type, options);
                                }
                                dsconfig2 = string.Format("dsconfig create-store-adapter-mapping --type-name {0} --mapping-name {1} --set scim-resource-type-attribute:{1} --set store-adapter-attribute:{2} {3} --reason none", objectType, fieldData[0], fieldData[1], options_2);
                                file.WriteLine(dsconfig1);
                                file.WriteLine(dsconfig2);
                                file.WriteLine();
                            }
                        }
                        file.Close();
                    }
                }
            }
            catch (Exception e)
            {
                //ToDo
            }

            return csvData;
        }

        #region Util Functions
        private static void HandleParseError(IEnumerable<Error> errs)
        {
            //todo
            throw new NotImplementedException();
        }

        private static void RunOptionsAndReturnExitCode(Options opts)
        {
            _options = opts;
        }

        private static string createTypeString(string v)
        {
            string result;
            //determine attribute type
            switch (v)
            {
                case "string":
                    result = SCIMAttributeTypes.STRING;
                    break;
                case "binary":
                    result = SCIMAttributeTypes.BINARY;
                    break;
                case "boolean":
                    result = SCIMAttributeTypes.BOOLEAN;
                    break;
                case "datetime":
                    result = SCIMAttributeTypes.DATETIME;
                    break;
                case "JSON":
                    result = SCIMAttributeTypes.COMPLEX;
                    break;
                case "complex":
                    result = SCIMAttributeTypes.COMPLEX;
                    break;
                case "integer":
                    result = SCIMAttributeTypes.INTEGER;
                    break;
                case "reference":
                    result = SCIMAttributeTypes.REFERENCE;
                    break;
                default:
                    result = SCIMAttributeTypes.STRING;
                    break;
            }
            return result;
        }

        private static string createDescriptionString(string v)
        {
            string description = "";
            if (v != string.Empty)
                description = v;
            return description;
        }

        private static string createSCIMAttributeOptionString(string[] fieldData)
        {
            string options = "";
            if (fieldData[5].Equals("Y"))
            {
                options = options + SCIMAttributeOptions.REQUIRED;
            }
            if (fieldData[6].Equals("Y"))
            {
                options = options + SCIMAttributeOptions.MULTIVALUE;
            }
            return options;
        }

        private static string createStoreAdapterMappingOptionString(string[] fieldData)
        {
            string options = "";
            if (fieldData[7].Equals("Y"))
            {
                options = options + SCIMAttributeOptions.SEARCHABLE;
            }

            options = options + SCIMAttributeOptions.AUTHORITATIVE;

            return options;
        }

        private static string createConstraintList(string v)
        {
            //determine is attribute has contraints
            string constraints = "";
            if (v != string.Empty)
            {
                string[] limits = v.Split(';');
                foreach (string limit in limits)
                {
                    constraints = constraints + " --set canonical-value:\"" + limit + "\"";
                }
                return constraints;
            }
            return constraints;
        }
        #endregion
    }
}

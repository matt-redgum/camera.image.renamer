using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Camera.Image.Renamer
{
    class Program

    {

        static Random rando = new Random();

        static void Main(string[] args)
        {

            Console.WriteLine("Camera.Image.Renamer v{0}", Assembly.GetEntryAssembly().GetName().Version);
            Console.WriteLine("Copyright {0} Matt White", DateTime.Now.Year);
            Console.WriteLine("----------------------------");
            Console.WriteLine("");
            var options = new Options();
            var canProcess = true;
            options.RecurseSubdirectories = GetRecurseSubDirectoriesArgument(args);
            options.CopyInsteadOfRename = GetCopyInsteadOfRenameArgument(args);
            options.TestOnly = GetTestArgument(args);
            options.Verbose = GetVerboseArgument(args);
            options.UseRegExFilters = GetUseRegExFiltersArgument(args);

            var suppliedOptions = GetFiltersArgument(args);

            if (!suppliedOptions.Any())
            {
                // Get the file with the list of filters
            }
            if (!suppliedOptions.Any())
            {
                // Get the default filters
                options.Filters = GetDefaultRegExFilters();
            }
            else
            {
                if (!options.UseRegExFilters)
                {
                    // Convert the supplied filters to regex ones
                    for (int i = 0; i < suppliedOptions.Count; i++)
                    {
                        options.Filters.Add(ConvertMaskToRegEx(suppliedOptions[i]));
                    }
                }
            }

            // The last argument is the source directory
            if (args != null && args.Any())
            {
                var lastArg = args.Last();
                if (string.IsNullOrWhiteSpace(lastArg))
                {
                    Console.WriteLine("Source directory not supplied");
                    canProcess = false;
                }
                else
                {
                    // Check to see if it is a valid path
                    if (System.IO.Directory.Exists(lastArg))
                    {
                        options.SourcePath = lastArg;
                    }
                    else
                    {
                        Console.WriteLine("Source directory '{0}' is not a valid or accessible directory", lastArg);
                        canProcess = false;
                    }
                }
            }
            else
            {
                // No source directory supplied
                Console.WriteLine("Source directory not supplied");
                canProcess = false;
            }

            if (options.Verbose)
            {
                for (int i = 0; i < options.Filters.Count; i++)
                {
                    Console.WriteLine("Filter: {0}", options.Filters[i]);
                }
            }
            if (canProcess)
            {
                if (options.TestOnly)
                {
                    Console.WriteLine("TEST ONLY - files will not be renamed");
                }
                Console.WriteLine("Processing path '{0}", options.SourcePath);
                Console.WriteLine("");
                ProcessFolder(options.SourcePath, options);
            }

#if DEBUG
            Console.ReadKey();
#endif
        }


        private static void ProcessFolder(string path, Options options)
        {
            // Get all the files in the folder and process
            var files = System.IO.Directory.GetFiles(path);
            for (int i = 0; i < files.Count(); i++)
            {
                ProcessFile(files[i], options);
            }
            // If required, loop through the folders 
            if (options.RecurseSubdirectories)
            {
                var dirs = System.IO.Directory.GetDirectories(path);
                for (int i = 0; i < dirs.Count(); i++)
                {
                    ProcessFolder(dirs[i], options);
                }
            }
        }

        private static void ProcessFile(string fileName, Options options)
        {
            // Match the file, then rename as required
            if (options.Verbose) { Console.WriteLine("Testing '{0}' for a filter match", fileName); }

            if (FileMatchesFilters(fileName, options.Filters))
            {
                if (options.Verbose) { Console.WriteLine("File '{0}' matches filter. Renaming...", fileName); }

                RenameFile(fileName, options);

            }
        }
        private static bool FileMatchesFilters(string filePath, List<string> filters)
        {
            var fileName = System.IO.Path.GetFileName(filePath);
            for (int i = 0; i < filters.Count; i++)
            {
                Regex mask = new Regex(filters[i]);
                if (mask.IsMatch(fileName))
                {
                    return true;
                }
            }
            return false;
        }


        private static void RenameFile(string fileName, Options options)
        {

            var path = System.IO.Path.GetDirectoryName(fileName);
            var currentDirectory = path.Replace(System.IO.Path.GetDirectoryName(path) + System.IO.Path.DirectorySeparatorChar, "");
            var currentExt = System.IO.Path.GetExtension(fileName);

            var newFileName = currentDirectory + "-" + CodeGenerator.Encode(rando.Next(1, 99999999)) + currentExt;
            var newPath = System.IO.Path.Combine(path, newFileName);
            if (options.CopyInsteadOfRename)
            {
                try
                {
                    Console.WriteLine("Copying file {0} to {1}", fileName, newFileName);
                    if (!options.TestOnly) { 
                        System.IO.File.Copy(fileName, newPath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error copying '{0}' to '{1}': {2}", fileName, newFileName, ex.GetBaseException().Message);
                }

            }
            else
            {
                try
                {
                    Console.WriteLine("Renaming file {0} to {1}", fileName, newFileName);
                    if (!options.TestOnly)
                    {
                        System.IO.File.Move(fileName, newPath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error renaming '{0}' to '{1}': {2}", fileName, newFileName, ex.GetBaseException().Message);
                }
            }
        }


        private static string ConvertMaskToRegEx(string fileMask)
        {
            return fileMask.Replace(".", "[.]").Replace("*", ".*").Replace("?", ".");
        }


        private static List<string> GetDefaultRegExFilters()
        {
            var list = new List<string>();
            list.Add(ConvertMaskToRegEx("IMG_*.JPG"));
            list.Add(ConvertMaskToRegEx("IMG_*.MOV"));
            list.Add(ConvertMaskToRegEx("DSC_*.JPG"));
            list.Add(@"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}");
            return list;
        }

        private static bool GetRecurseSubDirectoriesArgument(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                return args.Any(a => a == "--recurse" || a == "-r");
            }
            return false;
        }

        private static bool GetCopyInsteadOfRenameArgument(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                return args.Any(a => a == "--copyfile" || a == "-c");
            }
            return false;
        }

        private static bool GetVerboseArgument(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                return args.Any(a => a == "--verbose" || a == "-v");
            }
            return false;
        }

        private static bool GetUseRegExFiltersArgument(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                return args.Any(a => a == "--regex-filters" || a == "-x");
            }
            return false;
        }


        private static bool GetTestArgument(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                return args.Any(a => a == "--test" || a == "-t");
            }
            return false;
        }

        private static List<string> GetFiltersArgument(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                if (args.Any(a => a == "--filters" || a == "-f"))
                {
                    var argIdx = args.Select((v, i) => new { arg = v, index = i }).First((a) => a.arg == "--filters" || a.arg == "-f").index;
                    if (args.GetUpperBound(0) > argIdx)
                    {
                        var filterString = args[argIdx + 1];
                        // split the filter string on 
                        return new List<string>(filterString.Split(","));
                    }

                }
            }
            return new List<string>();
        }
    }

    public class CodeGenerator
    {
        private static String CON_ALPHABET = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private static int CON_BASE = 62;

        public static String Encode(int num)
        {
            StringBuilder sb = new StringBuilder();

            while (num > 0)
            {
                sb.Append(CON_ALPHABET[(num % CON_BASE)]);
                num = System.Convert.ToInt32((num / (double)CON_BASE));
            }

            StringBuilder builder = new StringBuilder();
            for (int i = sb.Length - 1; i >= 0; i += -1)
                builder.Append(sb[i]);
            return builder.ToString();
        }

        public static int Decode(String str)
        {
            int num = 0;
            int i = 0;
            int len = str.Length;
            while (i < len)
            {
                num = num * CON_BASE + CON_ALPHABET.IndexOf(str[(i)]);
                i += 1;
            }

            return num;
        }
    }
}

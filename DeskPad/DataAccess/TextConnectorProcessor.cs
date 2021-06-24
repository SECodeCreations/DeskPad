using DeskPad.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DeskPad.DataAccess.TextHelpers
{
    // Changing the namespace (adding .TextHelpers) will ensure that only files with this using statement inherit the extra stuff and eliminate any other classes from inheriting from this and getting cluttered up with the stuff that's in here (i.e. you don't want another data connector class seeing this as it would never use it).
    public static class TextConnectorProcessor
    {
        public static string FullFilePath(this string saveFileName)
        {            
            return $"{ ConfigurationManager.AppSettings["saveFilePath"] }\\{saveFileName}";
        }
        
        public static List<string> LoadFile(this string file) // Extension method (using 'this')
        {
            if (!File.Exists(file))
            {
                return new List<string>();
            }

            return File.ReadAllLines(file).ToList();
        }

        public static void SaveToNoteListFile(this List<NoteModel> models, string fileName)
        {
            List<string> lines = new List<string>();

            foreach (NoteModel p in models)
            {
                lines.Add($"{p.Id},{p.NoteFileName}");
            }
            File.WriteAllLines(fileName.FullFilePath(), lines);
        }

        public static List<NoteModel> ConvertToNotesListModels(this List<string> lines)
        {
            List<NoteModel> output = new List<NoteModel>();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                NoteModel p = new NoteModel();
                p.Id = int.Parse(cols[0]);
                p.NoteFileName = cols[1];
                output.Add(p);
            }
            return output;
        }

        // TODO - Create a method to save TEMPORARY files.

        // TODO - Create a method to load TEMPORARY files.

        // TODO - Create a method to save SHADOW COPY files.

        // TODO - Create a method to load SHADOW COPY files.
    }
}

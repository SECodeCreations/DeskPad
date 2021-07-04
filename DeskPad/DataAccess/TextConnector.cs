using DeskPad.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeskPad.DataAccess.TextHelpers;


namespace DeskPad.DataAccess
{
    public class TextConnector : IDataConnection
    {
        public const string NoteListFile = "NoteListModels.csv";

        public NoteModel SaveNote(NoteModel model, string saveFileName)
        {
            throw new NotImplementedException();
        }

        // Load USER-SAVED files.
        public List<NoteModel> GetNotes_All()
        {
            List<NoteModel> notes = NoteListFile.FullFilePath().LoadFile().ConvertToNotesListModels();

            return NoteListFile.FullFilePath().LoadFile().ConvertToNotesListModels();
        }
    }
}

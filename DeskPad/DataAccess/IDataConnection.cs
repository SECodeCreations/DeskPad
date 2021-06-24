using DeskPad.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeskPad.DataAccess
{
    public interface IDataConnection
    {
        NoteModel SaveNote(NoteModel model, string saveFileName);

        List<NoteModel> GetNotes_All();
    }
}

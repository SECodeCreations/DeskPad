using DeskPad.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeskPad.Interfaces
{
    public interface INoteRequester
    {
        void SaveNoteComplete(NoteModel model);
    }
}

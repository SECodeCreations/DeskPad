using DeskPad.DataAccess.TextHelpers;
using DeskPad.Interfaces;
using DeskPad.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace DeskPad
{

    public partial class SaveWindow : Window
    {
        private NoteModel note;
        INoteRequester callingForm;

        private const string NoteListFile = "NoteListModels.csv";

        public SaveWindow(NoteModel noteModel, INoteRequester caller)
        {
            InitializeComponent();
            note = noteModel;
            callingForm = caller;
            SaveFileNameTextBox.Focus();
        }

        public string FullFilePath()
        {
            string saveFileName = SaveFileNameTextBox.Text;

            return $"{ ConfigurationManager.AppSettings["saveFilePath"] }\\";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Set the FileNameLabel to the saveFileName.
            string saveFileName = SaveFileNameTextBox.Text;
            string filePath = FullFilePath() + $"{saveFileName}.txt";

            // Save the contents of file using "note" notemodel.
            NoteModel nm = note;
            List<string> lines = new List<string>();
            lines.Add($"{nm.NoteContent}");

            // Save the file using "saveFileName" as the file name
            File.WriteAllLines(filePath, lines);

            // Pass the save file name so that the list of files can save it.
            nm.NoteFileName = saveFileName;

            // Create an entry into a list to keep record of all notes saved.
            // Update the FileNameLabel.
            // Update the ListBox with new saved file.
            UpdateFileList(nm);

            // Update the FilePathLabel with the new saved file's path.
            nm.NoteFilePath = filePath;

            callingForm.SaveNoteComplete(nm);

            // Close the form.
            this.Close();
        }

        public NoteModel UpdateFileList(NoteModel model)
        {
            // Load list of Files.
            List<NoteModel> notes = NoteListFile.FullFilePath().LoadFile().ConvertToNotesListModels();

            int currentId = 1;

            if (notes.Count > 0)
            {
                currentId = notes.OrderByDescending(x => x.Id).First().Id + 1;
            }
            model.Id = currentId;

            // Update list with new File.
            notes.Add(model);

            // Save list with new updated list of files.
            notes.SaveToNoteListFile(NoteListFile);

            return model;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

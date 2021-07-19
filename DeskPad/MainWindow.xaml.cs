using DeskPad.Interfaces;
using DeskPad.Models;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using DeskPad.DataAccess.TextHelpers;
using System.Configuration;
using System.IO;

namespace DeskPad
{
    public partial class MainWindow : Window, INoteRequester
    {
        public const string NoteListFile = "NoteListModels.csv";
        List<NoteModel> notes = NoteListFile.FullFilePath().LoadFile().ConvertToNotesListModels();

        bool isLoadedFile;
        string loadFile;
        string currentFileOpened;
        string currentFileOpenedContents;

        public MainWindow()
        {   
            InitializeComponent();
            WireUpLists();
            isLoadedFile = false;
        }

        private void WireUpLists()
        {
            RecentNotesListBox.ItemsSource = null;
            RecentNotesListBox.ItemsSource = notes;
        }

        private void NewNoteButton_Click(object sender, RoutedEventArgs e)
        {
            // Check to see if the NotesTextBox is currently empty, if not then ask the user if they wish to save the file.
            if (NotesTextBox.Text.Length > 0 && isLoadedFile == false || isLoadedFile == true && NotesTextBox.Text != currentFileOpenedContents)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Do you wish to save the current note?",
                    "Save note?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes && isLoadedFile == false)
                {
                    // Open save prompt for user to save file.
                    SaveDialog();
                    NewNoteDialog();
                }
                else if (result == MessageBoxResult.Yes && isLoadedFile == true)
                {
                    OverwriteExistingFile();
                }
                else
                {
                    NewNoteDialog();
                }
            }
            else
            {
                NewNoteDialog();
            }
        }

        private void NewNoteDialog()
        {
            FileNameLabel.Content = "Untitled Note";
            NotesTextBox.Text = "";
            isLoadedFile = false;
            FilePathLabel.Content = "";
            currentFileOpenedContents = "";
        }

        private void DeleteNoteButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you wish to delete this note?",
                "Delete note?",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                NoteModel n = (NoteModel)RecentNotesListBox.SelectedItem;

                if (n.NoteFileName != null)
                {
                    DeleteFile(notes, n.NoteFileName);
                    notes.Remove(n);
                    WireUpLists();
                }

                // TODO - Delete note file (and any temporary / shadow copy files)

                // Reset the NotesTextBox to blank.
                NotesTextBox.Text = "";
            }
            else
            {
                return;
            }
        }

        public void DeleteFile(List<NoteModel> model, string fileName)
        {
            NoteModel newNoteFile = new NoteModel();

            foreach (NoteModel noteFile in model)
            {
                if (noteFile.NoteFileName == fileName)
                {
                    string fullFileName = $"{ fileName }" + ".txt";
                    File.Delete(fullFileName.FullFilePath());
                    newNoteFile = noteFile;
                }
            }

            UpdateFileList(newNoteFile);

            NewNoteDialog();
        }

        public void UpdateFileList(NoteModel model)
        {
            // TODO - Delete list entry for the deleted file in RecentNotesListBox.

            // Load list of Files.

            List<NoteModel> currentNoteList = NoteListFile.FullFilePath().LoadFile().ConvertToNotesListModels();

            NoteModel oldEntry = new NoteModel();

            foreach (NoteModel e in currentNoteList)
            {
                if(e.Id == model.Id)
                {
                    oldEntry = e;
                }
            }
            currentNoteList.Remove(oldEntry);

            // Update and save the new list.
            List<string> lines = new List<string>();

            foreach (NoteModel e in currentNoteList)
            {
                lines.Add($"{e.Id},{e.NoteFileName}");
            }

            File.WriteAllLines(NoteListFile.FullFilePath(), lines);
        }

        private void SaveDialog()
        {
            // Validate Form data.
            if (ValidateForm())
            {
                // Capture the content of the note and store it ready for saving.
                NoteModel nm = new NoteModel();
                nm.NoteContent = NotesTextBox.Text;

                // Open a window which prompts the user to enter a name for the file.
                SaveWindow saveWindow = new SaveWindow(nm, this);
                saveWindow.Show();
            }
        }

        private void SaveNoteButton_Click(object sender, RoutedEventArgs e)
        {
            if (isLoadedFile)
            {
                File.WriteAllText(currentFileOpened.FullFilePath() + ".txt", NotesTextBox.Text);
                currentFileOpenedContents = NotesTextBox.Text;
            }
            else
            {
                SaveDialog();
            }

        }

        private bool ValidateForm()
        {
            if (NotesTextBox.Text.Length == 0)
            {
                return false;
            }

            return true;
        }

        public void SaveNoteComplete(NoteModel model)
        {
            notes.Add(model);
            currentFileOpened = model.NoteFileName;
            currentFileOpenedContents = model.NoteContent;
            FileNameLabel.Content = model.NoteFileName;
            FilePathLabel.Content = model.NoteFilePath;
            isLoadedFile = true;
            WireUpLists();
        }

        public void OverwriteExistingFile()
        {
            File.WriteAllText(currentFileOpened.FullFilePath() + ".txt", NotesTextBox.Text);
        }

        private void RecentNotesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the selected List entry
            NoteModel nm = (NoteModel)RecentNotesListBox.SelectedItem;

            if (nm == null)
            {
                return;
            }

            // Get the List entry's file name
            if (nm.NoteFileName != null)
            {
                loadFile = FullFilePath() + $"{nm.NoteFileName}.txt";
                FilePathLabel.Content = loadFile;
            }

            // If the user has just opened the program and immediately clicks on a note from listbox (without putting any text into the NotesTextBox) just load the file (do not prompt to save).
            if (NotesTextBox.Text.Length == 0 && isLoadedFile == false)
            {
                LoadExistingFile(loadFile);
                currentFileOpened = nm.NoteFileName;
                FileNameLabel.Content = currentFileOpened;

                if (File.Exists(loadFile))
                {
                    currentFileOpenedContents = File.ReadAllText(FullFilePath() + $"{nm.NoteFileName}.txt");
                }
            }

            // If the two contents do NOT match, ask the user if they wish to save changes.
            if (!string.Equals(currentFileOpenedContents, NotesTextBox.Text) && isLoadedFile == true)
            {
                MessageBoxResult result = MessageBox.Show($"Do you wish to save changes to the current note?",
                    "Save changes to?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    OverwriteExistingFile();

                    LoadExistingFile(loadFile);
                    currentFileOpened = nm.NoteFileName;
                    FileNameLabel.Content = currentFileOpened;

                    if (File.Exists(loadFile))
                    {
                        currentFileOpenedContents = File.ReadAllText(FullFilePath() + $"{nm.NoteFileName}.txt");
                    }
                }
                else
                {
                    LoadExistingFile(loadFile);
                    currentFileOpened = nm.NoteFileName;
                    FileNameLabel.Content = currentFileOpened;

                    if (File.Exists(loadFile))
                    {
                        currentFileOpenedContents = File.ReadAllText(FullFilePath() + $"{nm.NoteFileName}.txt");
                    }
                }
            }

            // If the contents DO match, no changes have been made - therefore open the newly selected file.
            if (string.Equals(currentFileOpenedContents, NotesTextBox.Text, StringComparison.Ordinal))
            {
                LoadExistingFile(loadFile);
                if (File.Exists(loadFile))
                {
                    currentFileOpenedContents = File.ReadAllText(FullFilePath() + $"{nm.NoteFileName}.txt");

                }

                currentFileOpened = nm.NoteFileName;
                FileNameLabel.Content = currentFileOpened;
            }
        }

        private void LoadExistingFile(string fileName)
        {
            // If the file exists, set the contents of that file to the NotesTextBox.Content.
            if (File.Exists(fileName))
            {
                string readText = File.ReadAllText(fileName);
                NotesTextBox.Text = readText;
                isLoadedFile = true;
            }
            else
            {
                MessageBox.Show($"Cannot find file '{fileName}.txt'");
            }
        }

        private string FullFilePath()
        {
            return $"{ ConfigurationManager.AppSettings["saveFilePath"] }\\";
        }

    }
}

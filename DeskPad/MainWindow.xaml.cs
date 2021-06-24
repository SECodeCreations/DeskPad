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
        private const string NoteListFile = "NoteListModels.csv";
        List<NoteModel> notes = NoteListFile.FullFilePath().LoadFile().ConvertToNotesListModels();

        bool isLoadedFile;
        string loadFile;
        string currentFileOpened;
        string currentFileOpenedContents;

        public MainWindow()
        {
            InitializeComponent();
            NewNoteDialog();
            WireUpLists();
            // TODO - Load and display last opened file.
        }

        private void WireUpLists()
        {
            RecentNotesListBox.ItemsSource = null;
            RecentNotesListBox.ItemsSource = notes;
        }

        private void NewNoteButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO - check to see if the NotesTextBox is currently empty, if not then ask the user if they wish to save the file?

            if (ValidateForm())
            {
                MessageBoxResult result = MessageBox.Show(
                    "Do you wish to save the current note?",
                    "Save note?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveDialog();
                    NewNoteDialog();

                    // TODO - open save prompt for user to save file, transfer contents from temporary or shadow copy file and delete temp file.
                }
                else
                {
                    // TODO - if a temporary file has been created, delete it.

                    // TODO - Create a new temporary file (filename perhaps a datetime stamp?).

                    NewNoteDialog();
                }
            }
        }

        private void NewNoteDialog()
        {
            FileNameLabel.Content = "Untitled Note";
            NotesTextBox.Text = "";
            isLoadedFile = false;
        }

        private void DeleteNoteButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you wish to delete this note?",
                "Delete note?",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // TODO - Delete note file (and any temporary / shadow copy files)
                // TODO - Delete list entry for the deleted file in RecentNotesListBox.
                NotesTextBox.Text = "";
            }
            else
            {
                return;
            }
        }

        private void SaveDialog()
        {
            // TODO - VALIDATE FORM.
            // TODO - Capture the content of the note and store it ready for saving.

            NoteModel nm = new NoteModel();
            nm.NoteContent = NotesTextBox.Text;

            // TODO - Open a window which prompts the user to enter a name for the file.
            SaveWindow saveWindow = new SaveWindow(nm, this);
            saveWindow.Show();
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
            isLoadedFile = true;
            WireUpLists();
        }

        private void RecentNotesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the selected List entry
            NoteModel nm = (NoteModel)RecentNotesListBox.SelectedItem;

            // Get the List entry's file name
            if (nm.NoteFileName != null)
            {
                loadFile = FullFilePath() + $"{nm.NoteFileName}.txt";
            }

            // If the two contents do NOT match, ask the user if they wish to save changes.
            if (!string.Equals(currentFileOpenedContents, NotesTextBox.Text))
            {
                MessageBoxResult result = MessageBox.Show($"Do you wish to save change to the current note?",
                    "Save changes to?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    File.WriteAllText(currentFileOpened.FullFilePath() + ".txt", NotesTextBox.Text);

                    LoadExistingFile(loadFile);
                    currentFileOpened = nm.NoteFileName;
                    FileNameLabel.Content = currentFileOpened;

                    currentFileOpenedContents = File.ReadAllText(FullFilePath() + $"{nm.NoteFileName}.txt");

                }
                else
                {
                    LoadExistingFile(loadFile);
                    currentFileOpened = nm.NoteFileName;
                    FileNameLabel.Content = currentFileOpened;
                    currentFileOpenedContents = File.ReadAllText(FullFilePath() + $"{nm.NoteFileName}.txt");


                }
            }

            // If the contents DO match, no changes have been made - therefore open the newly selected file.
            if (string.Equals(currentFileOpenedContents, NotesTextBox.Text, StringComparison.Ordinal))
            {
                LoadExistingFile(loadFile);
                currentFileOpenedContents = File.ReadAllText(FullFilePath() + $"{nm.NoteFileName}.txt");

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
                MessageBox.Show("Cannot find that file.");
            }
        }

        private string FullFilePath()
        {
            return $"{ ConfigurationManager.AppSettings["saveFilePath"] }\\";
        }
    }
}

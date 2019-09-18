using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Xml.Serialization;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Repository
{
    public partial class MainWindow : MetroWindow
    {
        public List<Note> displayedNotes = new List<Note> { };
        public List<Note> notes = new List<Note> { };
        public string key = String.Empty;

        public class Note
        {
            public string Name { get; set; }
            public string Text { get; set; }
            public string Date { get; set; }

            public Note()
            {
                Name = "";
                Text = "";
                Date = DateTime.Now.ToString("dd.MM.yyyy") + " " + DateTime.Now.ToString("HH:mm");
            }
            public Note(string name, string text, string date)
            {
                Name = name;
                Text = text;
                Date = date;
            }
        }

        public class Data
        {
            public List<Note> Notes { get; set; } = new List<Note> { };

            public Data() { }
            public Data(List<Note> notes)
            {
                Notes = notes;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        public void save()
        {
            var saveData = new Data(notes);
            XmlSerializer xs = new XmlSerializer(typeof(Data));
            TextWriter tw = new StreamWriter("nt.rps");
            xs.Serialize(tw, saveData);
            tw.Close();
            string s = File.ReadAllText("nt.rps");
            s = App.Crypt(s, key);
            File.WriteAllText("nt.rps", s);
        }

        public void shake()
        {
            displayedNotes.Clear();
            foreach (Note i in notes) { if (i.Name.Contains(searchBar.Text)) displayedNotes.Add(i); }
            try { notesList.Items.Refresh(); } catch { notesList.CommitEdit(); notesList.CancelEdit(); } finally { notesList.Items.Refresh(); }
        }

        private void notesList_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            var note = notesList.SelectedItem as Note;
            if (note != null) { textBox.Document.Blocks.Clear(); textBox.AppendText(note.Text); }
            else { textBox.Document.Blocks.Clear(); }
            shake(); save();
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var note = notesList.SelectedItem as Note;
            if (note != null && new TextRange(textBox.Document.ContentStart, textBox.Document.ContentEnd).Text != null && new TextRange(textBox.Document.ContentStart, textBox.Document.ContentEnd).Text != "")
                notes.Find(x => x == note).Text = new TextRange(textBox.Document.ContentStart, textBox.Document.ContentEnd).Text;
            save();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            notes.Reverse();
            notes.Add(new Note("", "", (DateTime.Now.ToString("dd.MM.yyyy") + " " + DateTime.Now.ToString("HH:mm"))));
            notes.Reverse();
            save();
            shake();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (textBox.IsReadOnly) textBox.IsReadOnly = false;
            else textBox.IsReadOnly = true;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            string result = this.ShowModalInputExternal("Confirm", $"Are you sure about that? Type \"yes\" if so.");
            if (result != null && result.ToLower() == "yes")
            {
                var note = notesList.SelectedItem as Note;
                if (note != null) notes.Remove(note);
                shake(); save();
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            save();
        }

        private void MetroWindow_ContentRendered(object sender, EventArgs e)
        {
            if (File.Exists("nt.rps"))
            {
                bool loaded = false; string s = String.Empty;
                var loadData = new Data(notes);
                XmlSerializer xs = new XmlSerializer(typeof(Data));
                int counter = 0;
                while (!loaded)
                {
                    var result = this.ShowModalLoginExternal("Autorisation", "Enter your login and password");
                    if (result != null && result.Username != null && result.Password != null && result.Username.Length >= 4 && result.Password.Length >= 4)
                    {
                        key = App.Crypt(result.Password, result.Username);
                    }
                    else if (result != null && result.Username == null) Environment.Exit(0);
                    else {
                        //this.ShowMessageAsync("Failed to login", "Incorrect login or password. Each one must have more than 4 symbols");
                        continue;
                    }
                    s = File.ReadAllText("nt.rps");
                    s = App.Crypt(s, key);
                    File.WriteAllText("nt.rps", s);
                    try
                    {
                        using (var sr = new StreamReader("nt.rps"))
                        {
                            loadData = (Data)xs.Deserialize(sr);
                        }
                        notes = loadData.Notes;
                        loaded = true;
                    }
                    catch
                    {
                        //this.ShowMessageAsync("Failed to login", "Incorrect login or password!");
                        counter++;
                        if (counter >= 4)
                        {
                            counter = 0;
                            while (true)
                            {
                                if (File.Exists($"nt{counter}.rps")) counter++;
                                else break;
                            }
                            s = File.ReadAllText("nt.rps");
                            s = App.Crypt(s, key);
                            File.WriteAllText($"nt{counter}.rps", s);
                            File.Delete("nt.rps");
                            Environment.Exit(0);
                        }
                    }
                    finally
                    {
                        s = File.ReadAllText("nt.rps");
                        s = App.Crypt(s, key);
                        File.WriteAllText("nt.rps", s);
                    }
                }
            }
            else
            {
                bool correct = false;
                while (!correct)
                {
                    var result = this.ShowModalLoginExternal("First login", "Type your new login and password");
                    if (result != null && result.Username.Length >= 4 && result.Password.Length >= 4)
                    {
                        key = App.Crypt(result.Password, result.Username);
                        correct = true;
                    }
                    //else { this.ShowMessageAsync("Failed to login", "Incorrect login or password. Each one must have more than 4 symbols"); }
                }
            }
            save();
            notesList.ItemsSource = displayedNotes;
            shake();
        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            shake();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            searchBar.Text = "";
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            string result0 = this.ShowModalInputExternal("Confirm", $"Are you sure about that? Type \"yes\" if so.");
            if (result0 != null && result0.ToLower() == "yes")
            {
                var result = this.ShowModalLoginExternal("Confirm yourself", "Enter your current login and password");
                if (result != null && key == App.Crypt(result.Password, result.Username))
                {
                    var result1 = this.ShowModalLoginExternal("New autorisation data", "Type your new login and password");
                    if (result1 != null && result1.Username != null && result1.Password != null && result1.Username.Length >= 4 && result1.Password.Length >= 4)
                    {
                        key = App.Crypt(result1.Password, result1.Username);
                    }
                    shake(); save();
                }
            }
        }
    }
}

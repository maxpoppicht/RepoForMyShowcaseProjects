using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Trainingyaes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region <Variables etc>
        List<Item> ItemList = new List<Item>();
        XML_Config config = new XML_Config();
        FileDialog fileDialog;
        bool configCheck;
        #endregion

        public MainWindow()
        {

            InitializeComponent();

            //Check Directory and set it
            if (config.Directory == null)
            {
                config.Directory = Environment.CurrentDirectory;
            }
            else
            {
                fileDialog.InitialDirectory = config.Directory;
            }

            //Load the XML Config
            config = LoadXML();

            //Apply Config content to window
            myWindow.Height = config.WindowHeight;
            myWindow.Width = config.WindowWidth;


            for (int i = 0; i < (int)Slot.Usable; i++)
            {
                _itemslot.Items.Add((Slot)i);
            }
        }

        private void Open_File(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Multiselect = false,
                Filter = Item.LOAD
            };

            string path = "";

            if (ofd.ShowDialog() == true)
            {
                path = ofd.FileName;
            }
            else return;

            CurrentItemBox.Items.Clear();
            _textbox1.Text = "";
            _textbox2.Text = "";
            _textbox3.Text = "";
            _textbox4.Text = "";

            Item[] toLoad = Load(path);
            foreach (Item item in toLoad)
            {
                CurrentItemBox.Items.Add(item);

                _textbox1.Text = item.ItemName;
                _textbox2.Text = item.ItemAttack.ToString();
                _textbox3.Text = item.ItemDefence.ToString();
                _textbox4.Text = item.ItemPrice.ToString();
            }

        }

        private Item[] Load(string _path)
        {
            StreamReader reader = new StreamReader(_path);
            BinaryFormatter formatter = new BinaryFormatter();

            Item[] toLoad = ((Item[])formatter.Deserialize(reader.BaseStream));

            reader.Close();
            return toLoad;
        }

        //Load the XML file
        private XML_Config LoadXML()
        {
            configCheck = File.Exists(Environment.CurrentDirectory + "\\xmlConfig.xml");

            //Check if the file exists
            if (configCheck == false)
            {
                CreateXML();
            }
            //read content of file
            StreamReader reader = new StreamReader(Environment.CurrentDirectory + "\\xmlConfig.xml");
            XmlSerializer xmlserializer = new XmlSerializer(typeof(XML_Config));

            //Load the content
            XML_Config toLoad = (XML_Config)xmlserializer.Deserialize(reader.BaseStream);
            
            reader.Close();
            //Return the content
            return toLoad;
        }
        //Create the XML file
        private void CreateXML()
        {
            //Create the file by serializing the contents of XML_Config.cs
            StreamWriter xmlCreator = new StreamWriter(Environment.CurrentDirectory + "\\xmlConfig.xml");
            XmlSerializer xmlserializer = new XmlSerializer(typeof(XML_Config));

            xmlserializer.Serialize(xmlCreator.BaseStream, config);
            xmlCreator.Close();
        }

        private void Save_File(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog()
            {
                Filter = Item.SAVE
            };

            //Set the initial Directory to the dir in the XML config
            sfd.InitialDirectory = config.Directory;
            
            string path = "";

            if (sfd.ShowDialog() == true)
            {
                path = sfd.FileName;
            }
            else return;

            Item[] toSave = new Item[CurrentItemBox.Items.Count];
            CurrentItemBox.Items.CopyTo(toSave, 0);

            Save(path, toSave);
        }

        private void Save(string _path, Item[] _save)
        {
            StreamWriter writer = new StreamWriter(_path);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(writer.BaseStream, _save);

            writer.Close();
        }

        private void Exit_Programm(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void _itemslot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentItemBox.SelectedItem == null) return;
            int CBIndex = CurrentItemBox.SelectedIndex;
            Item currentItem = (Item)CurrentItemBox.SelectedItem;

            CurrentItemBox.Items.Refresh();
        }
        //Name
        private void _textbox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CurrentItemBox.SelectedItem == null) return;
            int CBIndex = CurrentItemBox.SelectedIndex;

            //Current Item
            Item currentItem = (Item)CurrentItemBox.SelectedItem;

            //Set the Name
            currentItem.ItemName = _textbox1.Text;

            //Apply Changes
            CurrentItemBox.Items[CBIndex] = currentItem;

            CurrentItemBox.SelectedIndex = CBIndex;


            //Show Real Time Change
            Binding bind = new Binding("Text");
            bind.Source = _textbox1;
            _leftbox1.SetBinding(TextBlock.TextProperty, bind);

            //Refresh
            CurrentItemBox.Items.Refresh();

        }
        //Attack
        private void _textbox2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CurrentItemBox.SelectedItem == null) return;
            int CBIndex = CurrentItemBox.SelectedIndex;

            Item currentItem = (Item)CurrentItemBox.SelectedItem;

            //Parse and set value
            int.TryParse(_textbox2.Text, out currentItem.ItemAttack);

            CurrentItemBox.Items[CBIndex] = currentItem;

            CurrentItemBox.SelectedIndex = CBIndex;

            //Show real time changes
            Binding bind = new Binding("Text");
            bind.Source = _textbox2;
            _leftbox2.SetBinding(TextBlock.TextProperty, bind);

            //refresh
            CurrentItemBox.Items.Refresh();
        }
        //Defence
        private void _textbox3_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CurrentItemBox.SelectedItem == null) return;
            int CBIndex = CurrentItemBox.SelectedIndex;

            Item currentItem = (Item)CurrentItemBox.SelectedItem;

            //Parse and set
            int.TryParse(_textbox3.Text, out currentItem.ItemDefence);

            CurrentItemBox.Items[CBIndex] = currentItem;

            CurrentItemBox.SelectedIndex = CBIndex;

            //Show real time changes
            Binding bind = new Binding("Text");
            bind.Source = _textbox3;
            _leftbox3.SetBinding(TextBlock.TextProperty, bind);

            CurrentItemBox.Items.Refresh();
        }
        //Price
        private void _textbox4_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CurrentItemBox.SelectedItem == null) return;
            int CBIndex = CurrentItemBox.SelectedIndex;

            Item currentItem = (Item)CurrentItemBox.SelectedItem;

            //Parse and set
            int.TryParse(_textbox4.Text, out currentItem.ItemPrice);

            CurrentItemBox.Items[CBIndex] = currentItem;

            CurrentItemBox.SelectedIndex = CBIndex;

            //Show real time changes
            Binding bind = new Binding("Text");
            bind.Source = _textbox4;
            _leftbox4.SetBinding(TextBlock.TextProperty, bind);

            CurrentItemBox.Items.Refresh();
        }

        private void _addButton_Click(object sender, RoutedEventArgs e)
        {
            //Create a new Item
            Item newItem = new Item();

            newItem.ItemName = _textbox1.Text;

            int.TryParse(_textbox2.Text, out newItem.ItemAttack);

            int.TryParse(_textbox3.Text, out newItem.ItemDefence);

            int.TryParse(_textbox4.Text, out newItem.ItemPrice);

            //add and refresh
            CurrentItemBox.Items.Add(newItem);
            CurrentItemBox.Items.Refresh();
        }

        private void _deleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentItemBox.SelectedItem == null) return;

            //Remove selected item
            CurrentItemBox.Items.Remove(CurrentItemBox.SelectedItem);
            CurrentItemBox.Items.Refresh();

        }
    }

    //Enum
    public enum Slot
    {
        Weapon,
        Head,
        Chest,
        Pants,
        Foot,
        Usable
    }

    [Serializable]
    public struct Item
    {
        public string ItemName;
        public Slot ItemSlot;
        public int ItemAttack;
        public int ItemDefence;
        public int ItemPrice;

        public override string ToString()
        {
            return ItemName;
        }


        public const string SAVE = "Item|*.item|Xml|*.xml";
        public const string LOAD = "Item|*.item|Xml|*.xml|All supported Files|*.item;*.xml";

    }



}

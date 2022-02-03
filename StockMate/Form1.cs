// (c) Copyright Skillage I.T.
// (c) This file is Skillage I.T. software and is made available under license.
// (c) This software is developed by: Michael Padworth
// (c) Date: 28th Jan 2021 Time: 11:34
// (c) File Name: Program.cs Version: 1-0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using Microsoft.VisualBasic.FileIO;

namespace StockMate
{   
    public partial class Form1 : Form
    {
        //Class variables that are used throughout the runtime.
        string filePath = string.Empty;
        int totalRow = 0;
        
        //Constructor. Do not change.
        public Form1()
        {
            InitializeComponent();
        }

        //Behaviour for the "Open" button
        private void openButton_Click(object sender, EventArgs e)
        {
            //Used to open a open-file window
            OpenFileDialog openFileDialog = new OpenFileDialog();

            //Set the initial folders and specify the types of file to be opened.
            openFileDialog.InitialDirectory = "c:\\stockFiles";
            openFileDialog.Filter = "csv files (*.csv)|*.csv";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;
            
            //If the dialogue is successfully opened
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //Get the path of specified file
                filePath = openFileDialog.FileName;
                try
                {
                    open(openFileDialog.FileName);

                } catch (Exception)
                {
                    //In the event of unexpected error
                    MessageBox.Show($"File failed to open. Please try again.");
                }
            }
            else
            {
                //In the event of unexpected error
                MessageBox.Show($"Could not initialise a dialogue box. Please try again.");
            }
        }

        //Behaviour for the "Save" button.
        private void saveButton_Click(object sender, EventArgs e)
        {
            //If there is more than one row in the data grid, i.e. there is data in the data grid
            if(totalRow > 1)
            {
                //Call the save function with the original file's name.
                save(filePath);
            }
            //Otherwise
            else
            {
                //Call the NoFile function.
                NoFile();
            }
        }

        //Behaviour for the "Save As" button
        private void saveAsButton_Click(object sender, EventArgs e)
        {
            ////If there is more than one row in the data grid, i.e. there is data in the data grid
            if (totalRow > 1)
            {
                //Using integrated Visual Basic feature to prompt the user to input a new file name
                string newFileAddress = Microsoft.VisualBasic.Interaction.InputBox("Please input a new file name without the extension.", "New name", "");
                
                //If the file name is empty
                if(newFileAddress != "")
                {
                    //Check for special characters not allowed in Windows 10
                    if(newFileAddress.Contains('"') || newFileAddress.Contains('?') || newFileAddress.Contains('<') || newFileAddress.Contains('>') || newFileAddress.Contains('*') || newFileAddress.Contains(':') || newFileAddress.Contains('|') || newFileAddress.Contains('/') || newFileAddress.Contains('\\'))
                    {
                        MessageBox.Show($"Invalid file name. File name must not contain any of \\ / : * ? \" < > |. Please try again.");
                        saveAsButton_Click(sender, e);
                    }

                    //If the file name is valid
                    else
                    {
                        //Format the file name
                        newFileAddress = "c:\\stockFiles\\" + newFileAddress + ".csv";

                        //Used to decide whether there exists any file with the same name, and whether the user wants to override the file
                        bool confirm = true;

                        //Check if a file with the same name exists
                        if (File.Exists(@newFileAddress))
                        {
                            //Prompt the user for overriding option
                            DialogResult dialogResult = MessageBox.Show("A file with the same name already exists. Override the file?", "Override?", MessageBoxButtons.YesNo);
                            
                            //If the user does not want to override the existing file
                            if (dialogResult == DialogResult.No)
                            {
                                confirm = false;
                            }
                        }

                        //If there is no file with the same name, or if the user wants to override the existing file
                        if (confirm)
                        {
                            //Calls the save function with the new name
                            save(newFileAddress);
                        }
                        else
                        {
                            //Prompts the user again for a new file name
                            saveAsButton_Click(sender, e);
                        }
                    }

                }
                else
                {
                    //If the user didn't specify a file name
                    MessageBox.Show($"No file name specified. Please try again.");
                }
            }

            //If there is no data.
            else
            {
                NoFile();
            }

        }

        public void open(string fileName)
        {
            //Used to store data read from the file.
            string fileContent = string.Empty;

            //Read the contents of the file into a stream
            //var fileStream = openFileDialog.OpenFile();
            Stream fileStream = File.OpenRead(fileName);


            //If the file is successfully opened, clear the existing displayed data (if any).
            totalRow = 0;
            dataGridView1.Rows.Clear();

            //Reads the entire document.
            using (StreamReader reader = new StreamReader(fileStream))
            {
                //Skip the first line (with the columns name)
                reader.ReadLine();

                //Read the rest of the data
                fileContent = reader.ReadToEnd();
                reader.Close();
            }

            // This splits the file's content by new lines.
            string[] lines = fileContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            // This is supposed to be (lines.Length - 1), as the file provided has an empty line at the end of the file.
            // But the dataGridView object in the form also contains an extra line for each field's name by design which negates the -1.
            // Therefore the -1 is not included here to avoid an unnecessary totalRow++.
            totalRow = lines.Length;

            //Split each line into 4 parts according to the design
            foreach (string a in lines)
            {
                string[] con = a.Split(',');
                if (con.Length == 4)
                {
                    //Load the data to the data grid.
                    //con[2] is parsed into an integer so the built-in sorting functions would treat it as a numeric value
                    dataGridView1.Rows.Add(con[0], con[1], Int32.Parse(con[2]), con[3]);
                }
            }
        }

        //Function used by "Save" and "Save As" buttons. Performs the saving function.
        public void save(string filename)
        {
            //Using try in case of errors
            try
            {
                //Initialising the output file stream with the file name passed to this function.
                System.IO.StreamWriter toFile = new System.IO.StreamWriter(filename);

                //Writes the first line (the columns name)
                toFile.WriteLine("Item Code,Item Description,Current Count,On Order");

                //Count the total number of columns
                int columnCount = dataGridView1.Columns.Count;

                //Write each row into the file according to the format of a .csv file
                for (int i = 1; i <= dataGridView1.Rows.Count; i++)
                {
                    for (int j = 0; j < columnCount-1; j++)
                    {
                        toFile.Write(dataGridView1.Rows[i - 1].Cells[j].Value.ToString() + ",");
                    }
                    toFile.WriteLine(dataGridView1.Rows[i - 1].Cells[columnCount - 1].Value.ToString());
                }

                //Close the stream
                toFile.Close();

                //Also sets the current opening file to the new file created.
                //Not putting this under the buttons behaviour in case of unexpected errors in the save process.
                filePath = filename;

                //Telling the user the process is successful.
                MessageBox.Show($"File successfully saved to {filename}.");
            }
            catch (Exception)
            {
                //In case error happens.
                MessageBox.Show($"File failed to save. Please try again.");
            }
        }

        //If there is no data in the data grid but the user tries to save the data into a file
        public void NoFile()
        {
            //Shows a message.
            MessageBox.Show("No file opened. Please open a file before performing this action.");
        }

        private void exportXML_Click(object sender, EventArgs e)
        {
            if (totalRow <= 1)
            {
                NoFile();
            }
            else
            {
                export(this.xmlTheme.SelectedIndex);
                Process.Start("msedge", "c:\\stockFiles\\exportedXML.xml");
            }
        }

        private void export(int style)
        {
            int columnCount = dataGridView1.Columns.Count;
            System.IO.StreamWriter toFile = new System.IO.StreamWriter("c:\\stockFiles\\exportedXML.xml");
            toFile.WriteLine("<?xml version=\"1.0\"?>");
            if(style == 0)
            {
                toFile.WriteLine("<?xml-stylesheet type=\"text/css\" href=\"dark.css\"?>");
                darkCSS();
            }
            else
            {
                toFile.WriteLine("<?xml-stylesheet type=\"text/css\" href=\"light.css\"?>");
                lightCSS();
            }
            toFile.WriteLine("<xs:schema xmlns:xs = \"http://www.w3.org/2001/XMLSchema\">");
            toFile.WriteLine("	<xs:element name=\"stocks\">");
            toFile.WriteLine("	  <xs:complexType>");
            toFile.WriteLine("		<xs:sequence>");
            toFile.WriteLine("		  <xs:element name=\"heading\" type=\"xs: string\"/>");
            toFile.WriteLine("		  <xs:element name=\"stock\" type=\"xs: string\"/>");
            toFile.WriteLine("		</xs:sequence>");
            toFile.WriteLine("	  </xs:complexType>");
            toFile.WriteLine("	</xs:element>");

            toFile.WriteLine("	<stocks>");
            toFile.WriteLine("    <heading>Item Code,Item Description,Current Count,On Order</heading>");
            for (int i = 1; i <= dataGridView1.Rows.Count; i++)
            {
                toFile.Write("        <stock>");
                for (int j = 0; j < columnCount - 1; j++)
                {
                    toFile.Write(dataGridView1.Rows[i - 1].Cells[j].Value.ToString() + ",");
                }
                toFile.Write(dataGridView1.Rows[i - 1].Cells[columnCount - 1].Value.ToString());
                toFile.WriteLine("</stock>");
            }
            toFile.WriteLine("	</stocks>");
            toFile.WriteLine("</xs:schema>");
            toFile.Close();
        }

        public void darkCSS()
        {
            System.IO.StreamWriter toFile = new System.IO.StreamWriter("c:\\stockFiles\\dark.css");
            toFile.WriteLine("books { ");
            toFile.WriteLine("     color: white; ");
            toFile.WriteLine("     background-color : black; ");
            toFile.WriteLine("     width: 100%; ");
            toFile.WriteLine("} ");
            toFile.WriteLine(" heading { ");
            toFile.WriteLine("     color: white; ");
            toFile.WriteLine("     font-size : 40px; ");
            toFile.WriteLine("     background-color : gray; ");
            toFile.WriteLine("} ");
            toFile.WriteLine(" heading, title, author, publisher, edition, price, stock { ");
            toFile.WriteLine("     display : block; ");
            toFile.WriteLine("} ");
            toFile.WriteLine(" title { ");
            toFile.WriteLine("     font-size : 25px; ");
            toFile.WriteLine("     font-weight : bold; ");
            toFile.WriteLine("} ");
            toFile.WriteLine(" stock { ");
            toFile.WriteLine("     background-color : black; ");
            toFile.WriteLine("     color: white; ");
            toFile.WriteLine("} ");
            toFile.Close();
        }

        public void lightCSS()
        {
            System.IO.StreamWriter toFile = new System.IO.StreamWriter("c:\\stockFiles\\light.css");
            toFile.WriteLine("books { ");
            toFile.WriteLine("     color: black; ");
            toFile.WriteLine("     background-color : white; ");
            toFile.WriteLine("     width: 100%; ");
            toFile.WriteLine("} ");
            toFile.WriteLine(" heading { ");
            toFile.WriteLine("     color: black; ");
            toFile.WriteLine("     font-size : 40px; ");
            toFile.WriteLine("     background-color : LightGray; ");
            toFile.WriteLine("} ");
            toFile.WriteLine(" heading, title, author, publisher, edition, price, stock { ");
            toFile.WriteLine("     display : block; ");
            toFile.WriteLine("} ");
            toFile.WriteLine(" title { ");
            toFile.WriteLine("     font-size : 25px; ");
            toFile.WriteLine("     font-weight : bold; ");
            toFile.WriteLine("} ");
            toFile.Close();
        }
    }
}

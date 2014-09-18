using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using Microsoft.Win32;
using CSGO_CfgGen.Commands;
using ICSharpCode.TextEditor.Document;

namespace CSGO_CfgGen
{
    public partial class MainForm : Form
    {
        #region Custom Form-Components

        private ToolStripMenuItem menuitemFile;
        private ToolStripSeparator menuitemSeparator;

        /// <summary>
        /// Initialisiert eigene Komponenten
        /// </summary>
        private void InitializeCustomComponent()
        {
            this.menuitemFile = new ToolStripMenuItem();
            this.menuitemFile.Text = "File";
            this.menuitemFile.Click += new System.EventHandler(this.menuitemFile_Click);
            this.menuitemSeparator = new ToolStripSeparator();
            this.menuitemSeparator.Visible = cfgFileManager.CfgFiles.Count > 0;

            //Add Load-MenuItems
            foreach (string path in cfgFileManager.cfgPaths)
            {
                ToolStripMenuItem cfgFileMenuItem = new ToolStripMenuItem();
                cfgFileMenuItem.ToolTipText = path;
                System.Text.RegularExpressions.Regex expr = new System.Text.RegularExpressions.Regex(@"(\\[^\\]+){0,4}$");
                cfgFileMenuItem.Text = "..." + expr.Match(path).ToString();
                cfgFileMenuItem.Tag = path;
                cfgFileMenuItem.Click += new EventHandler(this.cfgFileLoad_Click);
                this.menuitemLoad.DropDownItems.Add(cfgFileMenuItem);
            }
            this.menuitemLoad.DropDownItems.AddRange(
                new System.Windows.Forms.ToolStripItem[] 
                {
                    this.menuitemSeparator,
                    this.menuitemFile
                });

            //TextEditor Anpassungen
            try
            {
                FileSyntaxModeProvider csgoSyntaxProvider = new FileSyntaxModeProvider("highlighting");
                HighlightingManager.Manager.AddSyntaxModeFileProvider(csgoSyntaxProvider);
                textEditor.SetHighlighting("CSGO");
            }
            catch (IOException ioEx)
            { /*TODO*/ }
        }
        #endregion

        private ConfigFileManager cfgFileManager;

        public MainForm(ConfigFileManager cfgFileMan)
        {
            this.cfgFileManager = cfgFileMan;
            InitializeComponent();
            InitializeCustomComponent();
        }

        private void menuitemFile_Click(object sender, EventArgs e)
        {
            //Filebrowser öffnen
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = Application.StartupPath;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (dialog.CheckFileExists)
                {
                    if (cfgFileManager.CfgFiles.Any(cfg => cfg.Path == dialog.FileName))
                    {
                        //File ist bereits geladen
                        MessageBox.Show("Diese Datei wurde bereits geladen!");
                        //TODO: Ungespeicherte änderungen verwerfen und Trotzdem laden ?
                        //int id = this.cfgFileManager.reload(path);
                        //this.fillTreeView(id);
                        return;
                    }

                    //Add & load Cfg
                    int id = this.cfgFileManager.addConfig(dialog.FileName, true);
                    this.cfgFileManager.parseConfig(id, true);
                    fillTreeView(this.cfgFileManager.getConfigById(id));
                }
            }
        }

        private void cfgFileLoad_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem mItem = (ToolStripMenuItem)sender;
            string path = (string)mItem.Tag;

            int id = this.cfgFileManager.load(path);
            this.fillTreeView(this.cfgFileManager.getConfigById(id));
        }

        private void fillTreeView(ConfigFile file)
        {
            treeView.Nodes.Add(createRootNode(file));
            treeView.ExpandAll();
        }

        private void clearTreeView()
        {
            treeView.Nodes.Clear();
        }

        /// <summary>
        /// Füllt das TreeControl rekursiv
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private TreeNode createRootNode(ConfigFile file)
        {
            string filename = file.Path.Substring(file.Path.LastIndexOf('\\') + 1); //TODO filename prop
            TreeNode node = new TreeNode(filename);
            node.Tag = file.Path;

            ValidationLevel state = file.getValidationLevel();
            switch (state)
            {
                case ValidationLevel.Unknown:
                    node.BackColor = Color.LightGray;
                    break;
                case ValidationLevel.Ok:
                    break;
                case ValidationLevel.Error:
                    node.BackColor = Color.Red;
                    break;
                case ValidationLevel.Warning:
                    node.BackColor = Color.Orange;
                    break;
                default:
                    break;
            }
            
            foreach (WeakReference cfgFileRef in file.SubConfigRef)
                node.Nodes.Add(createRootNode((ConfigFile)cfgFileRef.Target));

            return node;
        }

        private void treeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            string path = (string)e.Node.Tag;
            fillTextEditorStatusBox(path);
        }

        /// <summary>
        /// Füllt TextEditor & StatusBox
        /// </summary>
        /// <param name="path"></param>
        private void fillTextEditorStatusBox(string path)
        {
            textEditor.Text = String.Empty;
            listView.Items.Clear();

            ConfigFile cfgFile = cfgFileManager.getConfigByPath(path);
            string fullConfig = String.Join(Environment.NewLine, cfgFile.Commands.Select(cmd => cmd.FullCommando));
            textEditor.Text = fullConfig;

            int fixedOffset = 0;
            foreach (Commando cmd in cfgFile.Commands)
            {
                Color bgColor;
                switch (cmd.ValidationState)
                {
                    case ValidationLevel.Unknown:
                        bgColor = Color.LightGray;
                        break;
                    case ValidationLevel.Warning:
                        bgColor = Color.Yellow;
                        break;
                    case ValidationLevel.Error:
                        bgColor = Color.Red;
                        break;
                    case ValidationLevel.Ok:
                    default:
                        fixedOffset += cmd.FullCommando.Length + Environment.NewLine.Length;
                        continue;
                }

                highlightLine(fixedOffset, bgColor);
                fixedOffset += cmd.FullCommando.Length + Environment.NewLine.Length;
                if (!string.IsNullOrEmpty(cmd.ValidationMessage))
                {
                    ListViewItem lvItem = new ListViewItem(cmd.ValidationMessage);
                    lvItem.BackColor = bgColor;
                    listView.Items.Add(lvItem);
                }
            }
            textEditor.Refresh();
        }

        /// <summary>
        /// Startet die Fehlerprüfung
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuItemRUN_Click(object sender, EventArgs e)
        {
            //Path des selektierten Nodes holen
            TreeNode selNode = treeView.SelectedNode;
            if (selNode == null) return;
            string selCfgPath = (string)selNode.Tag;

            ConfigFile currMemFile = this.cfgFileManager.CfgFiles.Find(cfg => cfg.Path == selCfgPath);

            //Änderungen speichern
            string[] newFileContent = textEditor.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            currMemFile.Commands = ConfigParser.parseLines(newFileContent, currMemFile.Path);

            ConfigFile rootFile = cfgFileManager.CfgFiles.Find(cfg => cfg.Type == ConfigFileType.AUTOEXEC);

            //Alle Files validieren
            cfgFileManager.parseConfig(rootFile.Id, false);

            clearTreeView();
            fillTreeView(rootFile);

            if (!String.IsNullOrEmpty(selCfgPath))
                fillTextEditorStatusBox(selCfgPath);
        }

        /// <summary>
        /// Highlightet eine bestimmte Line
        /// </summary>
        /// <param name="offset"></param>
        private void highlightLine(int offset, Color color)
        {
            LineSegment lineseg = textEditor.Document.GetLineSegmentForOffset(offset);
            foreach (TextWord word in lineseg.Words)
            {
                if (word.Type.Equals(TextWordType.Word))
                    word.SyntaxColor = new HighlightColor(word.Color, color, false, false);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
                
    }
}

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

namespace lab_2
{
    public partial class FormMain : Form
    {
        public List<CFigure> Templates;
        List<string> FigureNames;
        CFiguresList FiguresList;
        Bitmap Drawing;
        Graphics Canvas;
        Pen DrawingPen;
        SolidBrush DrawingBrush;
        int[] Parameters;
        // Массив параметров:
        //Parameters[0] - тип фигуры
        //Parameters[1] - цвет пера
        //Parameters[2] - толщина пера
        //Parameters[3] - цвет заливки
        //Parameters[4 - ...] - координаты для отрисовки

        List<Type> PluginShapeTypes;

        List<object> PluginShapes;
        List<Type> TypesOfShapes;

        int CurrentFigure;

        public FormMain()
        {
            InitializeComponent();
        }

        private void TextBoxX1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar) || (e.KeyChar == 8)) { return; }
            else { e.Handled = true; }
        }

        private void TextBoxY1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar) || (e.KeyChar == 8)) { return; }
            else { e.Handled = true; }
        }

        private void TextBoxX2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar) || (e.KeyChar == 8)) { return; }
            else { e.Handled = true; }
        }

        private void TextBoxY2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar) || (e.KeyChar == 8)) { return; }
            else { e.Handled = true; }
        }

        private void TextBoxWidth_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar) || (e.KeyChar == 8)) { return; }
            else { e.Handled = true; }
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            CurrentFigure = 0;
            Templates = new List<CFigure>();
            FigureNames = new List<string>();
            Parameters = new int[8];

            FiguresList = new CFiguresList();
            DrawingPen = new Pen(Color.FromArgb(0, 0, 0), 1);
            DrawingBrush = new SolidBrush(Color.FromArgb(255, 255, 255));

            Drawing = new Bitmap(PictureBoxDraw.Width, PictureBoxDraw.Height);
            Canvas = Graphics.FromImage(Drawing);

            Templates.Add(CRectangle.CreateTemplate());
            FigureNames.Add("Прямоугольник");
            Templates.Add(CEllipse.CreateTemplate());
            FigureNames.Add("Эллипс");
            Templates.Add(CLine.CreateTemplate());
            FigureNames.Add("Линия");
            Templates.Add(CTrapezium.CreateTemplate());
            FigureNames.Add("Трапеция");

            PluginShapeTypes = new List<Type>();
            PluginShapes = new List<object>();
            TypesOfShapes = new List<Type>();

            InitializePlugins();

            LabelCurrentFigure.Text = FigureNames[0];
            PictureBoxDraw.Image = Drawing;          
        }

        private void InitializePlugins() 
        {
            var PluginsList = Directory.GetFiles(Directory.GetCurrentDirectory() + "/Plugins", "*.dll");

            foreach (var Plugin in PluginsList)
            {

                Assembly AssemblyLoad = Assembly.LoadFrom(Plugin);

                Type ShapeType = AssemblyLoad.GetType("Plugin.CShape", true, true);

                object ShapeExample = Activator.CreateInstance(ShapeType, null);

                MethodInfo Method = ShapeType.GetMethod("GetName");

                object ShapeName = Method.Invoke(ShapeExample, null);

                FigureNames.Add(ShapeName.ToString());
                PluginShapeTypes.Add(ShapeType);
            }
        }

        private void ButtonPenColor_Click(object sender, EventArgs e)
        {
            ColorDialogPen.ShowDialog();
        }

        private void ButtonBrushColor_Click(object sender, EventArgs e)
        {
            ColorDialogBrush.ShowDialog();
        }

        private void ButtonBrushColor_Paint(object sender, PaintEventArgs e)
        {
            FiguresList.Draw(Canvas, DrawingPen,DrawingBrush);
            if (PluginShapes.Count > 0) 
            {
                DrawPluginShapes(Canvas,DrawingPen,DrawingBrush);
            }
            PictureBoxDraw.Image = Drawing;
        }

        private void ButtonNextFigure_Click(object sender, EventArgs e)
        {
            if (CurrentFigure + 1 >= Templates.Count + PluginShapeTypes.Count)
            {
                CurrentFigure = 0;
                LabelCurrentFigure.Text = FigureNames[0];
            }
            else
            {
                CurrentFigure++;
                LabelCurrentFigure.Text = FigureNames[CurrentFigure];
            }
        }

        private void ButtonPreviousFigure_Click(object sender, EventArgs e)
        {
            if (CurrentFigure <= 0)
            {
                CurrentFigure = Templates.Count - 1 + PluginShapeTypes.Count;

                //CurrentFigure = Templates.Count - 1;
                LabelCurrentFigure.Text = FigureNames[CurrentFigure];
            }
            else
            {
                CurrentFigure--;
                LabelCurrentFigure.Text = FigureNames[CurrentFigure];
            }
        }

        private void ButtonAdd_Click(object sender, EventArgs e)
        {
            if ((TextBoxX1.Text != "") && (TextBoxY1.Text != "") && (TextBoxX2.Text != "") && (TextBoxY2.Text != "") && (TextBoxWidth.Text != ""))
            {
                Parameters[0] = CurrentFigure;

                Parameters[1] = ColorDialogPen.Color.ToArgb();
                Parameters[2] = int.Parse(TextBoxWidth.Text);

                Parameters[3] = ColorDialogBrush.Color.ToArgb();

                Parameters[4] = int.Parse(TextBoxX1.Text);
                Parameters[5] = int.Parse(TextBoxY1.Text);
                Parameters[6] = int.Parse(TextBoxX2.Text);
                Parameters[7] = int.Parse(TextBoxY2.Text);

                if (CurrentFigure < Templates.Count)
                {
                    FiguresList.Figures.Add(Templates[CurrentFigure].Create(Parameters));
                }
                else 
                {
                    Type Shape = PluginShapeTypes[CurrentFigure - Templates.Count];
                    object ShapeObject = Activator.CreateInstance(Shape, Parameters);
                    PluginShapes.Add(ShapeObject);
                    TypesOfShapes.Add(Shape);
                }

                Refresh();

            }
            else
            {
                MessageBox.Show("Один из параметров не введен", "Внимание");//Один из параметров не введен
            }
        }

        private void PictureBoxDraw_MouseClick(object sender, MouseEventArgs e)
        {
            if (CheckBoxMouseControl.Checked)
            {
                if (e.Button == MouseButtons.Left)
                {
                    TextBoxX1.Text = e.X.ToString();
                    TextBoxY1.Text = e.Y.ToString();
                }
                if (e.Button == MouseButtons.Right)
                {
                    TextBoxX2.Text = e.X.ToString();
                    TextBoxY2.Text = e.Y.ToString();
                }
                if (e.Button == MouseButtons.Middle)
                {
                    ButtonAdd.PerformClick();
                }
            }
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FiguresList.SaveProgress();

            SavePluginShapes();
        }

        private void LoadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FiguresList.LoadProgress(Templates);

            LoadPluginShapes();

            Refresh();
        }

        private void HelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Управление мышью: \nЛевая кнопка - координаты 1 точки \nПравая кнопка - координаты 2 точки \nКолесико мыши создание фигуры", "Помощь");
        }

        private void AddPluginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialogPlugins.ShowDialog();
        }

        private void OpenFileDialogPlugins_FileOk(object sender, CancelEventArgs e)
        {
            Assembly AssemblyLoad = Assembly.LoadFrom(OpenFileDialogPlugins.FileName);

            Type ShapeType = AssemblyLoad.GetType("Plugin.CShape", true, true);

            object ShapeExample = Activator.CreateInstance(ShapeType, null);

            MethodInfo Method = ShapeType.GetMethod("GetName");

            object ShapeName = Method.Invoke(ShapeExample, null);

            FigureNames.Add(ShapeName.ToString());
            PluginShapeTypes.Add(ShapeType);

        }

        private void DrawPluginShapes(Graphics Canvas, Pen DrawingPen, SolidBrush DrawingBrush) 
        {
            MethodInfo Method;
            object Shape;
            int[] Parameters;
            object ParametersObject;

            for (int i = 0; i < PluginShapes.Count; i++)
            {
                Method = TypesOfShapes[i].GetMethod("GetDrawParameters");
                Shape = PluginShapes[i];

                ParametersObject = Method.Invoke(Shape,null);
                Parameters = (int[])ParametersObject;

                DrawPluginShape(Parameters,Canvas,DrawingPen,DrawingBrush);
            }
        }

        private void DrawPluginShape(int[] Parameters, Graphics Canvas, Pen DrawingPen, SolidBrush DrawingBrush) 
        {
            Point[] Points = new Point[(Parameters.Length - 4) / 2];
            int j = 0;

            DrawingPen.Color = Color.FromArgb(Parameters[1]);
            DrawingPen.Width = Parameters[2];
            DrawingBrush.Color = Color.FromArgb(Parameters[3]);

            for (int i = 4; i < Parameters.Length; i = i + 2)
            {
                Points[j].X = Parameters[i];
                Points[j].Y = Parameters[i + 1];
                j++;
            }

            Canvas.FillPolygon(DrawingBrush, Points);
            Canvas.DrawLines(DrawingPen, Points);
        }

        private void SavePluginShapes() 
        {
            MethodInfo Method;
            object Shape;
            int[] Parameters;
            object ParametersObject;

            StreamWriter Stream = new StreamWriter("SavedProgress/PluginSavedProgress.txt", false, System.Text.Encoding.Default);

            for (int i = 0; i < PluginShapes.Count; i++)
            {
                Method = TypesOfShapes[i].GetMethod("GetParameters");
                Shape = PluginShapes[i];

                ParametersObject = Method.Invoke(Shape, null);
                Parameters = (int[])ParametersObject;

                Stream.WriteLine(Parameters[0]);
                Stream.WriteLine(Parameters[1]);
                Stream.WriteLine(Parameters[2]);
                Stream.WriteLine(Parameters[3]);
                for (int j = 4; j < Parameters.Length; j++)
                {
                    Stream.Write(Parameters[j].ToString() + "/");
                }
                Stream.WriteLine();
                Stream.WriteLine("|");

            }
            Stream.Close();
        }

        private void LoadPluginShapes() 
        {
            string SavedPluginData = File.ReadAllText("SavedProgress/PluginSavedProgress.txt");

            string[] Shapes;
            string[] Temp;
            int AmountOfCoordinates = 0;
            int[] Parameters;

            Type Shape;
            object ShapeObject;

            Shapes = SavedPluginData.Split('|');

            for (int i = 0; i < Shapes.Length; i++)
            {

                Temp = Shapes[i].Split(new char[] {'\r','\n' },StringSplitOptions.RemoveEmptyEntries);

                if (Temp.Length > 4) 
                {
                    for (int j = 0; j < Temp[4].Length; j++)
                    {
                        if (Temp[4][j] == '/') 
                        {
                            AmountOfCoordinates++;
                        }
                    }

                    Parameters = new int[4 + AmountOfCoordinates];
                    AmountOfCoordinates = 0;

                    Parameters[0] = int.Parse(Temp[0]);
                    Parameters[1] = int.Parse(Temp[1]);
                    Parameters[2] = int.Parse(Temp[2]);
                    Parameters[3] = int.Parse(Temp[3]);

                    Temp = Temp[4].Split('/');
                    for (int j = 4; j < Parameters.Length; j++)
                    {
                        Parameters[j] = int.Parse(Temp[j-4]);
                    }

                    Shape = PluginShapeTypes[Parameters[0] - Templates.Count];
                    ShapeObject = Activator.CreateInstance(Shape, Parameters);
                    PluginShapes.Add(ShapeObject);
                    TypesOfShapes.Add(Shape);                
                }

            }

            Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(CurrentFigure.ToString());
        }

    }
}

using BoltzmannMachine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BoltzmannWpf
{
    public partial class MainWindow : Window
    {
        private String patternFilePath = "";
        Matrix<double> data;
        string[] lines;
        double sliderspeed;
        int sliderrepeat;
        TrainMachine tr;

        #region CanvasProperties
        private int canvasRowsAndColumns = 5;
        private int rectangleHeigth = 26;
        private int rectangleWidth = 26;


        private SolidColorBrush blackColor = new SolidColorBrush(Colors.Black);
        private SolidColorBrush whiteColor = new SolidColorBrush(Colors.White);
        #endregion

        public MainWindow()
        {

            InitializeComponent();
           
            setupCanvas(Canvas1);
        }

        #region Button Methods
        //=====================tab1=======================
        private void CleanButton_Click(object sender, RoutedEventArgs e)
        {
            Canvas1.Children.Clear();
            setupCanvas(Canvas1);
        }
        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Text files (*.txt)|*.txt";
            if (openFile.ShowDialog() == true)
            {
                FileLocation.Content = openFile.FileName;
                patternFilePath = openFile.FileName;
            }
        }
        private void AddPatternButton_Click(object sender, RoutedEventArgs e)
        {
            if (patternFilePath != "")
            {
                if (new FileInfo(patternFilePath).Length == 0)
                {
                    ComboBoxItem item = (ComboBoxItem)sizeComboBox.SelectedValue;
                    StreamWriter writer = new StreamWriter(patternFilePath, false);
                    writer.WriteLine(item.Content.ToString());

                    String resultString = "";
                    List<int> canvasList = canvasToList(Canvas1);
                    foreach (int element in canvasList)
                    {
                        resultString += element.ToString() + " ";
                    }
                    writer.WriteLine(resultString);
                    writer.Close();
                }
                else
                {
                    int rowsFromFile = Convert.ToInt32(File.ReadLines(patternFilePath).First());
                    if(rowsFromFile == canvasRowsAndColumns)
                    {
                        String resultString = "";
                        List<int> canvasList = canvasToList(Canvas1);
                        foreach (int element in canvasList)
                        {
                            resultString += element.ToString() + " ";
                        }

                        StreamWriter writer = new StreamWriter(patternFilePath, true);
                        writer.WriteLine(resultString);
                        writer.Close();
                    }
                    else
                    {
                        MessageBox.Show("Wielkość tablicy musi być równa wielkości tablic w pliku. Wielkość tablicy w pliku wynosi " + rowsFromFile.ToString() + ".", "Błąd", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            } else
            {
                MessageBox.Show("Wybierz plik uczący","Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void DeletePatternsButton_Click(object sender, RoutedEventArgs e)
        {
            if (patternFilePath != "")
            {
                File.WriteAllText(patternFilePath, String.Empty);
            } else
            {
                MessageBox.Show("Wybierz plik uczący", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //=====================tab2=======================
        private void SelectLearnFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Text files (*.txt)|*.txt";
            if (openFile.ShowDialog() == true)
            {
                LearnFileLocation.Content = openFile.FileName;
                learnFilePath = openFile.FileName;
            }
        }
        private void LearnButton_Click(object sender, RoutedEventArgs e)
        {
            //dla Adusia komenty, pozniej mozesz je wywalic

            if (learnFilePath != "") //sprawdza czy wybrany jest plik, jak nie to messageBox wyjezdza
            {
                if (new FileInfo(learnFilePath).Length != 0) //sprawdza czy nie jest pusty
                {
                    int hidden = Convert.ToInt32(slider2Value.Value);   //liczba neuronow ukrytych (2-10)
                    int reps = Convert.ToInt32(slider1Value.Value);     //liczba powtorzen (500-5000)

                    //========================================
                    //Tutaj zrob to zczytanie pliku tekstowego,
                    //ścieżka do niego jest w learnFilePath
                    //========================================

                } else
                {
                    MessageBox.Show("Plik uczący jest pusty!","Błąd", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            } else
            {
                MessageBox.Show("Wybierz plik uczący!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Changed Methods


        private void Sliderspeed_ValueChanged(object sender,RoutedPropertyChangedEventArgs<double> e)
        {
            // ... Get Slider reference.
            var slider = sender as Slider;
            // ... Get Value.
            sliderspeed = slider.Value;
            // ... Set Window Title.
           
        }

        private void Sliderrepeat_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // ... Get Slider reference.
            var slider = sender as Slider;
            // ... Get Value.
            sliderrepeat = (int)slider.Value;
            // ... Set Window Title.

        }

        private void canvasSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = (ComboBoxItem)sizeComboBox.SelectedValue;
            if (item.Content != null)
            {
                String selectedSize = item.Content.ToString();

                canvasRowsAndColumns = Convert.ToInt32(selectedSize);
                Canvas1.Children.Clear();
                setupCanvas(Canvas1);
            }
        }

     

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (tabItem2.IsSelected)
                {
                    patternFilePath = "";
                  
                   // setupCanvas(Canvas2);
                   //  setupCanvas(Canvas3);
                }
                if (tabItem1.IsSelected)
                {
                    patternFilePath = "";
                    // setupCanvas(Canvas2);
                    //  setupCanvas(Canvas3);
                }



            }
            catch(Exception w)
            {


            }
        }
        #endregion

        #region Canvas Methods
        private List<int> canvasToList(Canvas canv)
        {
            List<int> resultList = new List<int>();
            List<Rectangle> listofrectangles = new List<Rectangle>();

            foreach (var element in canv.Children)
            {
                if (element is Rectangle)
                {
                    listofrectangles.Add((Rectangle)element);
                }
            }

            foreach (var element in listofrectangles)
            {
                if (element.Fill.Equals(blackColor) == true)
                    resultList.Add(1);
                else
                    resultList.Add(0);
            }

            return resultList;
        }
        private void setupCanvas(Canvas Canvas)
        {
            Canvas.Height = rectangleHeigth * canvasRowsAndColumns;
            Canvas.Width = rectangleWidth * canvasRowsAndColumns;

            for (int i=0; i<canvasRowsAndColumns; i++)
            {
                for (int j=0; j<canvasRowsAndColumns; j++)
                {
                    Rectangle rectangle = new Rectangle();

                    rectangle.MouseDown += OnRectMouseLeftButtonDown;

                    rectangle.Height = rectangleHeigth;
                    rectangle.Width = rectangleWidth;

                    rectangle.Fill = whiteColor;
                    rectangle.Stroke = blackColor;
                    rectangle.StrokeThickness = 1;

                    Canvas.SetLeft(rectangle, i * rectangleWidth);
                    Canvas.SetTop(rectangle, j * rectangleHeigth);
                    Canvas.Children.Add(rectangle);
                }
            }
        }
        void OnRectMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Rectangle)
            {
                Rectangle ClickedRectangle = (Rectangle)e.OriginalSource;

                if (ClickedRectangle.Fill.Equals(blackColor) == true)
                    ClickedRectangle.Fill = whiteColor;
                else
                    ClickedRectangle.Fill = blackColor;
            }
        }
        #endregion

        private void pickfile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Text files (*.txt)|*.txt";
            if (openFile.ShowDialog() == true)
            {
                FileLocation.Content = openFile.FileName;
                patternFilePath = openFile.FileName;
            }

            if (patternFilePath != "")
            {
                using (StreamReader sr = new StreamReader(patternFilePath))
                {
                    // Read the stream to a string, and write the string to the console.
                    lines = sr.ReadToEnd().Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                 //   lines2 = new string[lines.Count()];

                    for(int i=0;i<lines.Count();i++)
                    {
                        lines[i]= lines.ElementAt(i).Replace(" ", String.Empty);

                    }

                    canvasRowsAndColumns = Convert.ToInt32(lines.ElementAt(0));
                    lines = lines.Where(w => w != lines[0]).ToArray();
                    setupCanvas(Canvas2);
                    setupCanvas(Canvas3);
                }
              
            }
            else
                MessageBox.Show("Wybierz plik uczący");


        }

        private void learn_button_Click(object sender, RoutedEventArgs e)
        {

            if (patternFilePath != "")
            {
                tr = new TrainMachine(canvasRowsAndColumns * canvasRowsAndColumns,lines.Count()-1);

                double[,] todata = new double[canvasRowsAndColumns, canvasRowsAndColumns];

                double[,] alldata= new double[lines.Count(), canvasRowsAndColumns* canvasRowsAndColumns];
                int pom;

                for (int i=0;i<lines.Count();i++)
                {
                    pom = 0;
                    for (int z=0;z<canvasRowsAndColumns;z++)
                    {

                        for (int y = 0; y < canvasRowsAndColumns; y++)
                        {

                            if (y > 0)
                            {
                                todata[z, y] = Convert.ToDouble(lines.ElementAt(i).ElementAt(y * canvasRowsAndColumns + z).ToString());
                                alldata[i, pom] = todata[z, y];
                                pom++;
                            }
                            else
                            {
                                todata[z, y] = Convert.ToDouble(lines.ElementAt(i).ElementAt(y + z).ToString());
                                alldata[i, pom] = todata[z, y];
                                pom++;
                            }
                        }
                    }

                   
                }

                data = DenseMatrix.OfArray(alldata);

                tr.train(data, sliderrepeat, sliderspeed);

                MessageBox.Show("Nauczono");



            }
            else
                MessageBox.Show("Wybierz plik uczący");

        }

        private void clearbutt_Click(object sender, RoutedEventArgs e)
        {
            Canvas2.Children.Clear();
            setupCanvas(Canvas2);

            Canvas3.Children.Clear();
            setupCanvas(Canvas3);
        }

        private void Symulation_butt_Click(object sender, RoutedEventArgs e)
        {
            

            List<int> canvasList = canvasToList(Canvas2);
            double[] tabletosim = new double[canvasList.Count()];
            int pom=0;
            for(int i=0;i< tabletosim.Count();i++)
            {
                if(i%canvasRowsAndColumns==0)
                tabletosim[i] = canvasList.ElementAt(i);

            }

          

          double[] result=(double[])DenseVector.OfArray(tr.simulation(tabletosim));

            MessageBox.Show("Wybierz plik uczący");

        }
    }
}

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
        private String learnFilePath = "";

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
            setupCanvas();
            
        }

        #region Button Methods
        //=====================tab1=======================
        private void CleanButton_Click(object sender, RoutedEventArgs e)
        {
            Canvas1.Children.Clear();
            setupCanvas();
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
                    List<int> canvasList = canvasToList();
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
                        List<int> canvasList = canvasToList();
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
        private void canvasSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = (ComboBoxItem)sizeComboBox.SelectedValue;
            if (item.Content != null)
            {
                String selectedSize = item.Content.ToString();

                canvasRowsAndColumns = Convert.ToInt32(selectedSize);
                Canvas1.Children.Clear();
                setupCanvas();
            }
        }
        #endregion

        #region Canvas Methods
        private List<int> canvasToList()
        {
            List<int> resultList = new List<int>();
            List<Rectangle> listofrectangles = new List<Rectangle>();

            foreach (var element in Canvas1.Children)
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
        private void setupCanvas()
        {
            Canvas1.Height = rectangleHeigth * canvasRowsAndColumns;
            Canvas1.Width = rectangleWidth * canvasRowsAndColumns;

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
                    Canvas1.Children.Add(rectangle);
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
    }
}

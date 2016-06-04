using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;

namespace BoltzmannMachine
{
    public class TrainMachine
    {
        double learning_rate;
        
        int num_examples, h_size, v_size;

        public Matrix<double> weights { get; set; }

        public TrainMachine( int visible_size, int hidden_size )
        {
            weights = DenseMatrix.OfArray(normalDistribution(visible_size, hidden_size, 0, 0.1));
            weights = insert(weights, 0, 0, 0);
            weights = insert(weights, 0, 0, 1);
            h_size = hidden_size;
        }

        public double[,] normalDistribution(int rows, int columns, double mean, double stddev)
        {
            Random rand = new Random();
            double[,] normDistarray = new double[rows, columns];
            int i = 0, j = 0;
            for (i = 0; i < rows; i++)
            {
                for (j = 0; j < columns; j++)
                {
                    normDistarray[i, j] = Normal.WithMeanStdDev(mean, stddev).Sample();
                }
            }
            return normDistarray;
        }

        public Matrix<double> insert(Matrix<double> data, int index, int number, int axis)
        {
            int size;
            if (axis == 0)
                size = data.RowCount;
            else            
                size = data.ColumnCount;
                        
            double[] for_vector = new double[size];

            for(int i = 0; i < size; i++)            
                for_vector[i] = number;

            if (axis == 0)
                data = data.InsertColumn(index, DenseVector.OfArray(for_vector));
            else
                data = data.InsertRow(0, DenseVector.OfArray(for_vector));

            return data;
        }

        public double[,] logisticMatrix(Matrix<double> arg)
        {
            double[,] tmp = new double[arg.RowCount, arg.ColumnCount];
            for (int i = 0; i < arg.RowCount; i++)
            {
                for (int j = 0; j < arg.ColumnCount; j++)
                {
                    tmp[i, j] = 1 / (1 + Math.Exp(-arg.At(i, j)));
                }
            }
            return tmp;
        }

        public double[] logisticVector(Vector<double> arg)
        {
            double[] tmp = new double[arg.Count];
            for (int i = 0; i < arg.Count; i++)
            {
                tmp[i] = 1 / (1 + Math.Exp(-arg.At(i)));                
            }
            return tmp;
        }

        public void train(Matrix<double> data, int epochs, double learn_rate)
        {
            Matrix<double> firststep_activations, firststep_probs, firststep_states, positive_assoc,
                secondstep_activations, secondstep_probs, thirdstep_activations, thirdstep_probs, negative_assoc;

            learning_rate = learn_rate;            
            num_examples = data.RowCount;
            data = insert(data, 0, 1, 0);

            for (int iter = 0; iter < epochs; iter++)
            {
                //Init state
                firststep_activations = data.Multiply(weights);                
                firststep_probs = DenseMatrix.OfArray(logisticMatrix(firststep_activations));
                
                double[,] tmp_states = new double[firststep_probs.RowCount, firststep_probs.ColumnCount];
                for (int i = 0; i < firststep_probs.RowCount; i++)
                {
                    for (int j = 0; j < firststep_probs.ColumnCount; j++)
                    {
                        tmp_states[i, j] = firststep_probs.At(i, j) > 0.5 ? 1 : 0;
                    }
                }
                firststep_states = DenseMatrix.OfArray(tmp_states);
                positive_assoc = data.Transpose().Multiply(firststep_probs);                
                
                //reconstruction state
                secondstep_activations = firststep_states.Multiply(weights.Transpose());                

                double[,] tmp_neg = logisticMatrix( secondstep_activations);
                for (int i = 0; i < secondstep_activations.RowCount; i++)
                {
                    tmp_neg[i, 0] = 1;
                }

                secondstep_probs = DenseMatrix.OfArray(tmp_neg);
                thirdstep_activations = secondstep_probs.Multiply(weights);              
                thirdstep_probs = DenseMatrix.OfArray(logisticMatrix(thirdstep_activations));                
                negative_assoc = secondstep_probs.Transpose().Multiply(thirdstep_probs);
                                
                weights = positive_assoc.Subtract(negative_assoc).Divide(num_examples).Multiply(learning_rate).Add(weights);
            }
        }

        public double[] simulateHidden(double[] data)
        {
            Vector<double> data_vector;
            Matrix<double> hidden_activations, hidden_probs;
            double[] tmp_data = new double[data.Length + 1];
            tmp_data[0] = 1;
            for (int i = 1; i < tmp_data.Length; i++)
                tmp_data[i] = data[i - 1];
            data_vector = DenseVector.OfArray(tmp_data);

            hidden_activations = data_vector.ToRowMatrix().Multiply(weights);
            hidden_probs = DenseMatrix.OfArray(logisticMatrix(hidden_activations));

            double[] tmp_states = new double[hidden_probs.ColumnCount];
            for (int i = 1; i < hidden_probs.ColumnCount; i++)
            {
                tmp_states[i] = hidden_probs.At(0,i) > 0.5 ? 1 : 0;
            }
            tmp_states[0] = 1;

            return tmp_states;
        }

        public double[] simulation(double[] data)
        {
            double[] hidden_states = simulateHidden(data);

            Matrix<double> visible_activations, visible_probs;
            visible_activations = DenseVector.OfArray(hidden_states).ToRowMatrix().Multiply(weights.Transpose());
            visible_probs = DenseMatrix.OfArray(logisticMatrix(visible_activations));

            double[] tmp_states = new double[visible_probs.ColumnCount - 1];
            for (int i = 1; i < visible_probs.ColumnCount; i++)
            {
                tmp_states[i-1] = visible_probs.At(0, i) > 0.5 ? 1 : 0;
            }

            return tmp_states;
        }
    }
}
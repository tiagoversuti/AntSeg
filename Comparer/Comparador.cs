using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Comparador
{
    public partial class Comparador : Form
    {
        public Comparador()
        {
            InitializeComponent();
        }

        private string file1;
        private string file2;
        private int height, width;
        private List<Point> pontos1, pontos2;

        private void file1Btn_Click(object sender, EventArgs e)
        {
            using (var open1 = new OpenFileDialog())
            {
                open1.Filter = @"Text Files(*.txt)|*.txt";

                if (open1.ShowDialog() == DialogResult.OK)
                {
                    var streamReader = new StreamReader(open1.FileName);
                    file1 = streamReader.ReadToEnd();
                }
                file1Lbl.Text = open1.FileName;
            }
        }

        private void file2Btn_Click(object sender, EventArgs e)
        {
            using (var open2 = new OpenFileDialog())
            {
                open2.Filter = @"Text Files(*.txt)|*.txt";

                if (open2.ShowDialog() == DialogResult.OK)
                {
                    var streamReader = new StreamReader(open2.FileName);
                    file2 = streamReader.ReadToEnd();
                }
                file2Lbl.Text = open2.FileName;
            }
        }

        private List<Point> LerArquivo(string file, out string tempo)
        {
            var pontos = new List<Point>();

            tempo = "";
            string temp = "";
            for (int i = 0; i<file.Length; i++)
            {
                temp += file[i];
                if (file[i] == '\n')
                    temp = "";
                switch (temp)
                {
                    case "time = ":
                        {
                            for (; file[i] != '\n'; i++)
                                temp += file[i];
                            temp = "";
                            break;
                        }
                    case "segtime = ":
                        {
                            temp = "";
                            for (; file[i] != '\n'; i++)
                                temp += file[i];
                            tempo = temp;
                            temp = "";
                            break;
                        }
                    case "height = ":
                        {
                            temp = "";
                            for (; file[i] != '\n'; i++)
                            {
                                temp += file[i];
                            }
                            if (height == 0)
                                height = Int32.Parse(temp);
                            else if (height != Int32.Parse(temp))
                                throw(new Exception("Imagem 2 tem dimensões diferentes da Imagem 1"));
                            temp = "";
                            break;
                        }
                    case "width = ":
                        {
                            temp = "";
                            for (; file[i] != '\n'; i++)
                            {
                                temp += file[i];
                            }
                            if (width == 0)
                                width = Int32.Parse(temp);
                            else if (width != Int32.Parse(temp))
                                throw (new Exception("Imagem 2 tem dimensões diferentes da Imagem 1"));
                            temp = "";
                            i++;
                            for (; i < file.Length; i++)
                                temp += file[i];
                                break;
                        }
                }
            }

            panel1.Height = height;
            panel1.Width = width;
            panel2.Height = height;
            panel2.Width = width;
            panel3.Height = height;
            panel3.Width = width;

            string str = "";
            int x = 0, y = 0;
            foreach (char t in temp)
            {
                str += t;
                if (t == '\n')
                {
                    for (int j = 0; j<str.Length; j++)
                    {
                        if (str[j] == ' ')
                        {
                            str = str.Substring(j + 1);
                            break;
                        }
                    }
                    for (int j = 0; j < str.Length; j++)
                    {
                        if (str[j] == ' ')
                        {
                            x = Int32.Parse(str.Substring(0, j));
                            str = str.Substring(j+1);
                            break;
                        }
                    }
                    for (int j = 0; j < str.Length; j++)
                    {
                        if (str[j] == '\n')
                        {
                            y = Int32.Parse(str.Substring(0, j-1));
                            str = str.Substring(j+1);
                            break;
                        }
                    }
                    pontos.Add(new Point(x, y));
                }
            }

            return pontos;
        }

        private void compararBtn_Click(object sender, EventArgs e)
        {
            string tempo1, tempo2;
            pontos1 = LerArquivo(file1, out tempo1);
            pontos2 = LerArquivo(file2, out tempo2);

            tempo1Lbl.Text = tempo1;
            tempo2Lbl.Text = tempo2;

            var matriz1 = new byte[width, height];
            var matriz2 = new byte[width, height];
            var matriz = new byte[width, height];

            foreach (Point ponto in pontos1)
                matriz1[ponto.X, ponto.Y] = 1;
            
            PreencheSolucao(matriz1);
            
            foreach (Point ponto in pontos2)
                matriz2[ponto.X, ponto.Y] = 1;
            
            PreencheSolucao(matriz2);
            
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (matriz1[i, j] != matriz2[i, j])
                        matriz[i,j] = 1;
                }
            }

            panel1.BackgroundImage = PintaImagem(matriz1);
            panel2.BackgroundImage = PintaImagem(matriz2);
            panel3.BackgroundImage = PintaImagem(matriz);

            repetibilidadeLbl.Text = CalculaRepetibilidade(matriz1, matriz2, matriz).ToString();
        }

        private void PreencheSolucao(byte[,] m)
        {
            m[0, 0] = 1;
            var fila = new List<Point> {new Point(0, 0)};

            for (int pos = 0; pos < fila.Count; pos++ )
            {
                var ponto = fila[pos];

                if (ponto.X - 1 >= 0)
                {
                    if (m[ponto.X - 1, ponto.Y] != 1)
                        {
                            fila.Add(new Point(ponto.X - 1, ponto.Y));
                            m[ponto.X - 1, ponto.Y] = 1;
                        }
                }

                if (ponto.X + 1 < width)
                {
                        if (m[ponto.X + 1, ponto.Y] != 1)
                        {
                            fila.Add(new Point(ponto.X + 1, ponto.Y));
                            m[ponto.X + 1, ponto.Y] = 1;
                        }
                }

                if (ponto.Y - 1 >= 0)
                {
                    if (m[ponto.X, ponto.Y - 1] != 1)
                    {
                        fila.Add(new Point(ponto.X, ponto.Y - 1));
                        m[ponto.X, ponto.Y - 1] = 1;
                    }
                }

                if (ponto.Y + 1 < height)
                {
                    if (m[ponto.X, ponto.Y + 1] != 1)
                    {
                        fila.Add(new Point(ponto.X, ponto.Y + 1));
                        m[ponto.X, ponto.Y + 1] = 1;
                    }
                }
            }
            
            for (int i = 0; i < m.GetLength(0); i++)
            {
                for (int j = 0; j < m.GetLength(1); j++)
                {
                    if (m[i, j] == 1)
                        m[i, j] = 0;
                    else m[i, j] = 1;
                }
            }
        }

        private Bitmap PintaImagem(byte[,] m)
        {
            var imagem = new Bitmap(width, height);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var cor = Color.FromArgb(m[i, j] * 255, m[i, j] * 255, m[i, j] * 255);
                    imagem.SetPixel(i, j, cor);
                }
            }

            return imagem;
        }

        private double CalculaRepetibilidade(byte[,] m1, byte[,] m2, byte[,] m)
        {
            int c1 = 0, c2 = 0;
            int c1XORc2 = 0;
            for (int i =0; i<width; i++)
            {
                for (int j=0; j<height; j++)
                {
                    if (m1[i, j] == 1) c1++;
                    if (m2[i, j] == 1) c2++;
                    if (m[i, j] == 1) c1XORc2++;
                }
            }
            double dif = (c1 + c2);
            dif = c1XORc2/dif;
            dif = 1 - dif;
            return dif;
        }

        private void limparBtn_Click(object sender, EventArgs e)
        {
            panel1.BackgroundImage = new Bitmap(width, height);
            panel2.BackgroundImage = new Bitmap(width, height);
            panel3.BackgroundImage = new Bitmap(width, height);
            height = 0;
            width = 0;
        }
    }
}

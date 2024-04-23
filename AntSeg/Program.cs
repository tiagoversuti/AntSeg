using System;
using System.Windows.Forms;
using System.Drawing;


namespace AntSeg
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]

        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        static public Bitmap PretoEBranco(Bitmap imagem)
        {
            var imagemPB = (Bitmap)imagem.Clone();
            for (int y = 0; y < imagem.Height; y++)
            {
                for (int x = 0; x < imagem.Width; x++)
                {
                    Color c = imagem.GetPixel(x, y);
                    var luma = (c.R + c.G + c.B)/3;
                    imagemPB.SetPixel(x, y, Color.FromArgb(luma, luma, luma));
                }
            }
            return imagemPB;
        }

        static public Bitmap Gradiente(Bitmap imagem)
        {
            var imagemG = new Bitmap(imagem);
            for (int y = 0; y < imagem.Height - 1; y++)
            {
                for (int x = 0; x < imagem.Width - 1; x++)
                {
                    int menor = int.MaxValue, maior = 0;
                    if (x - 1 >= 0)
                    {
                        Color c = imagem.GetPixel(x - 1, y);
                        GradienteMenorMaior(c, ref menor, ref maior);
                    }
                    if (y - 1 >= 0)
                    {
                        Color c = imagem.GetPixel(x, y - 1);
                        GradienteMenorMaior(c, ref menor, ref maior);
                    }
                    if (x + 1 < imagem.Width)
                    {
                        Color c = imagem.GetPixel(x + 1, y);
                        GradienteMenorMaior(c, ref menor, ref maior);
                    }
                    if (y + 1 < imagem.Height)
                    {
                        Color c = imagem.GetPixel(x, y + 1);
                        GradienteMenorMaior(c, ref menor, ref maior);
                    }
                    Color co = imagem.GetPixel(x, y);
                    int diff = 0;
                    int aux = co.R - menor;
                    if (aux > diff) 
                        diff = aux;
                    aux = maior - co.R;
                    if (aux > diff) 
                        diff = aux;
                    co = Color.FromArgb(diff, diff, diff);
                    imagemG.SetPixel(x, y, co);
                }
            }
            return imagemG;
        }

        static public Bitmap MorfGrad(Bitmap imagem, ref byte[,] matriz)
        {
            var imagemG = new Bitmap(imagem);
            for (int y = 1; y < imagem.Height - 2; y++)
            {
                for (int x = 1; x < imagem.Width - 2; x++)
                {
                    byte maior = 0;
                    byte menor = byte.MaxValue;

                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            var p = imagem.GetPixel(x + i, y + j).R;
                            if (p > maior)
                                maior = p;
                            if (p < menor)
                                menor = p;
                        }
                    }
                    var diff = (byte)(maior - menor);
                    Color co = Color.FromArgb(diff, diff, diff);
                    imagemG.SetPixel(x, y, co);
                    matriz[x, y] = diff;
                }
            }

            for (int x = 0; x < imagemG.Width; x++)
            {
                Color co = Color.FromArgb(0,0,0);
                imagemG.SetPixel(x, 0, co);
                imagemG.SetPixel(x, imagemG.Height - 1, co);
                matriz[x, 0] = 0;
                matriz[x,imagemG.Height - 1] = 0;
            }
            for (int x = 0; x < imagemG.Height; x++)
            {
                Color co = Color.FromArgb(0, 0, 0);
                imagemG.SetPixel(0, x, co);
                imagemG.SetPixel(imagemG.Width - 1, 0, co);
                matriz[0, x] = 0;
                matriz[imagemG.Height - 1, x] = 0;
            }

            return imagemG;
        }

        static private void GradienteMenorMaior(Color c, ref int menor, ref int maior)
        {
            if ((c.R) < menor)
                menor = (c.R);
            if ((c.R) > maior)
                maior = (c.R);
        }

        static public double[,] Variancia(byte[,] grad)
        {
            int width = grad.GetLength(0);
            int height = grad.GetLength(1);

            var m = new double[width, height];

            double n = width * height;

            for (int y = 1; y < height - 2; y++)
            {
                for (int x = 1; x < width - 2; x++)
                {
                    double s = 0, s2 = 0;

                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            int aux = grad[x + i, y + j];
                            s += aux;
                            s2 += aux * aux;
                        }
                    }


                    double v = (1 / (n - 1)) * (s2 - (1 / n) * s * s);
                    m[x,y] = v;
                }
            }

            return m;
        }

        static public Bitmap VarianciaImagem(double[,] grad)
        {
            var img = new Bitmap(grad.GetLength(0), grad.GetLength(1));
            double maior = 0;
            for (int i = 0; i < grad.GetLength(0); i++)
            {
                for (int j = 0; j < grad.GetLength(1); j++)
                {
                    if (grad[i, j] > maior)
                        maior = grad[i, j];
                }
            }

            maior = 255 / maior;

            for (int i = 0; i < grad.GetLength(0); i++)
            {
                for (int j = 0; j < grad.GetLength(1); j++)
                {
                    var pixel = (byte)(grad[i, j] * maior);
                    Color c = Color.FromArgb(pixel, pixel, pixel);
                    img.SetPixel(i, j, c);
                }
            }
            return img;
        }

    }
}

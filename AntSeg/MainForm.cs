using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace AntSeg
{
    public partial class MainForm : Form
    {
        private Bitmap imagem, imagemGradiente, imagemPB, imagemOriginal;
        private byte[,] matrizGradiente;
        private bool primeiroClique = true;
        int pontosAntSeg;
        private int height, width;
        Point mouse = new Point(0, 0), mouseOriginal = new Point(0, 0);
        List<Point> pontos = new List<Point>();
        List<Point> resultado = new List<Point>();
        Ant antseg;
        static List<Pen> pens = new List<Pen>();
        static int pen;
        static Pen penpadrao = new Pen(Color.Aqua);

        private DateTime t_inicioseg;
        private DateTime t_fimseg;
        DateTime t_inicio;
        DateTime t_fim;
        TimeSpan t_diferenca;

        public MainForm()
        {
            InitializeComponent();
        }

        private void abrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            try
            {
                using (var open = new OpenFileDialog())
                {
                    open.Filter = @"Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp";

                    if (open.ShowDialog() == DialogResult.OK)
                    {
                        textBox1.Text += @"Abrindo arquivo... ";
                        t_inicio = DateTime.Now;
                        imagem = new Bitmap(open.FileName);
                        imagemOriginal = imagem;
                        height = imagem.Height;
                        width = imagem.Width;
                        panel1.Height = height;
                        panel1.Width = width;
                        panel2.Height = height;
                        panel2.Width = width;
                        panel3.Height = height;
                        panel3.Width = width;
                        panel4.Height = height;
                        panel4.Width = width;
                        panel5.Height = height;
                        panel5.Width = width;
                        panel1.BackgroundImage = imagemOriginal;
                        matrizGradiente = new byte[width, height];
                        imagemPB = Program.PretoEBranco(imagemOriginal);
                        //imagemGradiente = Program.Gradiente(imagemPB);
                        imagemGradiente = Program.MorfGrad(imagemPB, ref matrizGradiente);
                        imagem = (Bitmap)imagemGradiente.Clone();
                        panel2.BackgroundImage = imagemGradiente;
                        panel3.BackgroundImage = PintaPreto();
                        panel4.BackgroundImage = (Bitmap)panel3.BackgroundImage.Clone();
                        double[,] m = Program.Variancia(matrizGradiente);
                        panel5.BackgroundImage = Program.VarianciaImagem(m);
                        primeiroClique = true;
                        pens.Add(new Pen(Color.Aqua));
                        pens.Add(new Pen(Color.Magenta));
                        pens.Add(new Pen(Color.Orange));
                        pens.Add(new Pen(Color.Lime));

                        t_fim = DateTime.Now;
                        t_diferenca = t_fim.Subtract(t_inicio);
                        textBox1.AppendText("tempo: " + t_diferenca.TotalSeconds + " segundos.\r\n");
                    }
                }
                pretoEBrancoToolStripMenuItem.Enabled = true;
                gradienteToolStripMenuItem.Enabled = true;
            }
            catch (Exception)
            {
                throw new ApplicationException("Falha ao carregar imagem");
            }

        }

        private void sairToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        
        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            {
                salvarToolStripMenuItem.Enabled = false;

                if (e.Button == MouseButtons.Left)
                {
                    if (primeiroClique)
                    {
                        t_inicioseg = DateTime.Now;
                        apagarBtn_Click(sender, e);
                        primeiroClique = false;
                        var ndi = (int)(numeroDeIteracoesNud.Value);
                        var qdf = (int)(qtdeDeFormigasNud.Value);
                        var alfa = (double)alfaNud.Value;
                        var beta = (double)betaNud.Value;
                        var gama = (int)(gamaNud.Value);
                        var ev = (double)evaporacaoNud.Value;
                        var p = (int)pNud.Value;
                        var prob = probabilidadeCbx.Checked;
                        antseg = new Ant(imagem, ndi, qdf, matrizGradiente, alfa, beta, gama, ev, p, prob);
                        panel3.BackgroundImage = PintaFeromonios(antseg);

                        resultado = new List<Point>();
                    }
                    pontos.Add(new Point(mouse.X, mouse.Y));
                    if (pontos.Count > 1)
                        ExecutaAntSeg();
                }

                else if (e.Button == MouseButtons.Right)
                {
                    if (resultado.Count > 1 && pontos.Count > 1)
                    {
                        while (resultado[resultado.Count - 1] != pontos[pontos.Count - 2])
                        {
                            resultado.RemoveAt(resultado.Count - 1);
                        }
                        pontos.RemoveAt(pontos.Count - 1);
                        PintaCaminhoInteiro();
                        if (decrementarCbx.Checked)
                            antseg.EvaporarFeromonios(ref antseg.feromonios);
                        PintaFeromonios(antseg);
                        AtualizaPen();
                        if (primeiroClique)
                            primeiroClique = false;
                    }
                }
                else if (e.Button == MouseButtons.Middle)
                {
                    if (resultado.Count > 1)
                    {
                        pontos.Add(new Point(e.X, e.Y));
                        if (pontos.Count > 1)
                            ExecutaAntSeg();
                    }
                }
            }
        }

        private void panel1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                pontos.Add(pontos[0]);
                primeiroClique = true;
                panel1.Invalidate();
                ExecutaAntSeg();
                t_fimseg = DateTime.Now;
                t_diferenca = t_fimseg.Subtract(t_inicioseg);
                textBox1.AppendText("tempo total: " + t_diferenca.TotalSeconds + " segundos.\r\n");
                salvarToolStripMenuItem.Enabled = true;
                
            }
        }

        private void AtualizaPen()
        {
            if (pen < pens.Count - 1)
                pen++;
            else pen = 0;
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            if (imagemOriginal != null && pontos.Count > 0)
            {
                using (Graphics g = panel1.CreateGraphics())
                {
                    foreach (Point t in pontos)
                    {
                    //g.DrawLine(penpadrao, pontos[i], pontos[i + 1]);
                        g.DrawEllipse(new Pen(Color.Red), t.X - 1, t.Y - 1, 3, 3);
                    }
                    for (int i = 0; i < pontos.Count - 1; i++)
                    {
                        g.DrawLine(penpadrao, pontos[i], pontos[i + 1]);
                    }
                    g.DrawLine(penpadrao, pontos[pontos.Count - 1], mouse);
                    g.DrawEllipse(penpadrao, mouse.X - 1, mouse.Y - 1, 3, 3);
                    g.DrawRectangle(penpadrao, mouseOriginal.X - 15, mouseOriginal.Y - 15, 31, 31);
                }
            }
            else using (Graphics g = panel1.CreateGraphics())
            {
                //g.DrawLine(penpadrao, posX, posY, mouse.X, mouse.Y);
                g.DrawEllipse(penpadrao, mouse.X - 1, mouse.Y - 1, 3, 3);
                g.DrawRectangle(penpadrao, mouseOriginal.X - 15, mouseOriginal.Y - 15, 31, 31);
            }
        }
            
        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Cross;
            if (matrizGradiente != null)
            {
                int deslocX = 0, deslocY = 0;

                byte grad = matrizGradiente[e.X, e.Y];

                for (int i = -15; i <= 15; i++)
                {
                    if (i + e.X >= 0 && i + e.X < width)
                        for (int j = -15; j <= 15; j++)
                        {
                            if ((j + e.Y >= 0) && (j + e.Y < height))
                                if (matrizGradiente[e.X + i, e.Y + j] > grad)
                                {
                                    grad = matrizGradiente[e.X + i, e.Y + j];
                                    deslocX = i;
                                    deslocY = j;
                                }
                        }
                }

                mouse.X = e.X + deslocX;
                mouse.Y = e.Y + deslocY;
            }

            mouseOriginal.X = e.X;
            mouseOriginal.Y = e.Y;
            
            panel1.Invalidate();
        }

        private Bitmap PintaCaminho(int pas, Bitmap img)
        {
            using (Graphics g = Graphics.FromImage(img))
            {
                if (rastroCbx.Checked)
                {
                    int variacao = resultado.Count - 1 - pas;
                    for (int i = pas; i < resultado.Count - 1; i++)
                     {
                        var tempCor = Color.FromArgb((i - pontosAntSeg) * 255 / variacao, (i - pontosAntSeg) * 255 / variacao, (i - pontosAntSeg) * 255 / variacao);
                        var tempPen = new Pen(tempCor);
                        g.DrawLine(tempPen, resultado[i], resultado[i + 1]);
                    }
                }
                else
                    for (int i = pas; i < resultado.Count - 1; i++)
                        g.DrawLine(pens[pen], resultado[i], resultado[i + 1]);
            }

            return img;
        }

        private Bitmap PintaPreto()
        {
            var img = new Bitmap(width, height);
            Color c = Color.FromArgb(0, 0, 0);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    img.SetPixel(i, j, c);
                }
            }
            return img;
        }

        private void PintaCaminhoInteiro()
        {
            imagem = (Bitmap)imagemGradiente.Clone();
            Bitmap imagemPreta = PintaPreto();
            using (Graphics.FromImage(imagem))
            {
                for (int i = 0; i <= pontos.Count - 1; i++)
                {
                    imagem = PintaCaminho(i, imagem);
                    imagemPreta = PintaCaminho(i, imagemPreta);
                    panel2.BackgroundImage = imagem;
                    panel4.BackgroundImage = imagemPreta;
                }
            }
            return;
        }

        private Bitmap PintaFeromonios(Ant ant)
        {
            var imagemFeromonios = new Bitmap(width, height);
            double maiorFeromonio = 0;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (ant.feromonios[i,j] > maiorFeromonio)
                        maiorFeromonio = ant.feromonios[i, j];
                }
            }
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var aux = (int)(ant.feromonios[i, j] * 255 / maiorFeromonio);
                    var c = Color.FromArgb(aux, aux, aux);
                    imagemFeromonios.SetPixel(i, j, c);
                }
            }
            return imagemFeromonios;
        }
        
        private void zoomMaisBtn_Click(object sender, EventArgs e)
        {
            panel1.Width *= 2;
            panel1.Height *= 2;
        }

        private void originalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panel1.BackgroundImage = imagemOriginal;
        }
        
        private void pretoEBrancoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panel1.BackgroundImage = imagemPB;
        }

        private void gradienteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panel1.BackgroundImage = imagemGradiente;
        }

        private void ExecutaAntSeg()
        {
            t_inicio = DateTime.Now;
            textBox1.AppendText("Calculando trecho " + (pontos.Count - 1) + "... ");
            textBox1.Refresh();
            pontosAntSeg = resultado.Count;
            resultado.AddRange(antseg.AntSeg(pontos[pontos.Count - 2], pontos[pontos.Count - 1], imagem));
            panel2.BackgroundImage = PintaCaminho(pontosAntSeg, (Bitmap)panel2.BackgroundImage.Clone());
            panel4.BackgroundImage = PintaCaminho(pontosAntSeg, (Bitmap)panel4.BackgroundImage.Clone());
            AtualizaPen();
            pontosAntSeg++;
            panel3.BackgroundImage = PintaFeromonios(antseg);
            t_fim = DateTime.Now;
            t_diferenca = t_fim.Subtract(t_inicio);
            textBox1.AppendText("tempo: " + t_diferenca.TotalSeconds + " segundos.\r\n");
        }

        private void apagarBtn_Click(object sender, EventArgs e)
        {
            panel2.BackgroundImage = imagemGradiente;
            pontos.Clear();
            panel1.Invalidate();
            primeiroClique = true;
            panel3.BackgroundImage = PintaPreto();
            panel4.BackgroundImage = (Bitmap)panel3.BackgroundImage.Clone();
            textBox1.Text = "";
            resultado.Clear();
        }

        private void FormPrincipal_Paint(object sender, PaintEventArgs e)
        {
        }

        private void FormPrincipal_MouseMove(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Default;
        }

        private void numeroDeIteracoesNud_ValueChanged(object sender, EventArgs e)
        {
            if (antseg != null)
                antseg.numeroDeIteracoes = (int)numeroDeIteracoesNud.Value;
        }

        private void qtdeDeFormigasNud_ValueChanged(object sender, EventArgs e)
        {
            if (antseg != null)
                antseg.qtdeDeFormigas = (int)qtdeDeFormigasNud.Value;
        }

        private void alfaNud_ValueChanged(object sender, EventArgs e)
        {
            if (antseg != null)
                antseg.alfa = (double)alfaNud.Value;
        }

        private void betaNud_ValueChanged(object sender, EventArgs e)
        {
            if (antseg != null)
                antseg.beta = (double)betaNud.Value;
        }

        private void gamaNud_ValueChanged(object sender, EventArgs e)
        {
            if (antseg != null)
                antseg.gama = (double)gamaNud.Value;
        }

        private void evaporacaoNud_ValueChanged(object sender, EventArgs e)
        {
            if (antseg != null)
                antseg.evaporacao = (double)evaporacaoNud.Value;
        }

        private void pNud_ValueChanged(object sender, EventArgs e)
        {
            if (antseg != null)
                antseg.p = (int)pNud.Value;
        }
        
        private void salvarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dir = new FolderBrowserDialog())
            {
                if (dir.ShowDialog() == DialogResult.OK)
                {
                    var path = dir.SelectedPath;
                    var time = DateTime.Now.ToString();
                    var texto = "time = " + time + Environment.NewLine + "segtime = " + t_diferenca + 
                        Environment.NewLine + "height = " + height + Environment.NewLine + "width = " + width;
                    for (int i = 0; i<resultado.Count; i++)
                    {
                        texto += Environment.NewLine + i + " " + resultado[i].X + " " + resultado[i].Y;
                    }

                    time = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() +
                           DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();
                    path += "\\" + time + ".txt";
                    //path += "\\1.txt";
                    using (var fs = new System.IO.StreamWriter(path))
                    {
                        fs.WriteLine(texto);
                    }
                }
            }
        }

        private void probabilidadeCbx_CheckedChanged(object sender, EventArgs e)
        {
            if (antseg != null)
                antseg.probabilistico = probabilidadeCbx.Checked;
        }

        
    }
}

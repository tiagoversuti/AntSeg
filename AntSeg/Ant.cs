using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace TCC
{
    class Ant
    {
        public double[,] feromonios;
        bool[,] jaCaminhado, jaCaminhadoLocal;
        public int qtdeDeFormigas, numeroDeIteracoes, p;
        public double alfa, beta, gama, evaporacao;
        byte[,] matriz;
        int width, height;
        public bool probabilistico;

        Random rand;

        public Ant(Bitmap imagem, int numeroDeIteracoes, int qtdeDeFormigas, byte[,] matriz, double alfa, double beta, double gama, double ev, int p, bool prob)
        {
            feromonios = new double[imagem.Width, imagem.Height];
            jaCaminhado = new bool[imagem.Width, imagem.Height];
            this.qtdeDeFormigas = qtdeDeFormigas;
            this.numeroDeIteracoes = numeroDeIteracoes;
            this.matriz = matriz;
            InicializaFeromonios(ref feromonios);
            this.alfa = alfa;
            this.beta = beta;
            this.gama = gama;
            evaporacao = ev;
            this.p = p;
            probabilistico = prob;

            width = imagem.Width;
            height = imagem.Height;
        }

        public IEnumerable<Point> AntSeg(Point P1, Point P2, Bitmap imagem)
        {
            var melhorSol = new List<Point>();
            double qmelhorSol = -1;
            InicializaFeromonios(ref feromonios);
            for (int i = 0; i < numeroDeIteracoes; i++)
            {
                List<Point> sol = ConstruirSolsCFormigas(P1, P2, imagem);
                double qsol = QualidadeDaSolucao(sol);
                AtualizarFeromonios(ref feromonios, sol, qsol);
                if (qsol > qmelhorSol)
                {
                    melhorSol = sol;
                    qmelhorSol = qsol;
                }
            }

            MarcarCaminho(ref melhorSol, P1);
            return melhorSol;
        }

        private double QualidadeDaSolucao(List<Point> sol)
        {
            double qsol = SomatoriaGradiente(sol) / sol.Count;
            return qsol;
        }

        private double SomatoriaGradiente(IEnumerable<Point> sol)
        {
            return sol.Aggregate<Point, double>(0, (current, ponto) => current + matriz[ponto.X, ponto.Y]);
        }

        private void InicializaFeromonios(ref double[,] Feromonios)
        {
            for (int i = 0; i < Feromonios.GetLength(0); i++)
                for (int j = 0; j < Feromonios.GetLength(1); j++)
                    Feromonios[i, j] = 1;
        }

        private void InicializaPassos(bool[,] m)
        {
            for (int i = 0; i < m.GetLength(0); i++)
                for (int j = 0; j < m.GetLength(1); j++)
                    m[i, j] = false;
        }

        private void AtualizarFeromonios(ref double[,] Feromonios, IEnumerable<Point> sol, double qsol)
        {
            EvaporarFeromonios(ref Feromonios);
            foreach (Point P in sol)
                Feromonios[P.X, P.Y] += qsol;
        }

        public void EvaporarFeromonios(ref Double[,] Feromonios)
        {
                for (int i = 0; i < Feromonios.GetLength(0); i++)
                    for (int j = 0; j < Feromonios.GetLength(1); j++)
                        Feromonios[i, j] *= (1 - evaporacao);
        }

        private List<Point> ConstruirSolsCFormigas(Point P1, Point P2, Bitmap imagem)
        {
            double qmelhorSol = -1;
            var melhorSol = new List<Point>();
            for (int k = 1; k <= qtdeDeFormigas; k++)
            {
                List<Point> solAtual = ConstruirSol(imagem, P1, P2);
                double qsolAtual = QualidadeDaSolucao(solAtual);
                if (qsolAtual > qmelhorSol)
                {
                    melhorSol = solAtual;
                    qmelhorSol = qsolAtual;
                }
            }
            return melhorSol;
        }

        private List<Point> ConstruirSol(Bitmap imagem, Point P1, Point P2)
        {
            jaCaminhadoLocal = new bool[imagem.Width, imagem.Height];

            jaCaminhado[P2.X, P2.Y] = false;

            Point P = P1;
            var sol = new List<Point> {P};
            while (P != P2)
            {
                List<Point> pontos = DefineProximosPontos(P);
                if (pontos.Count == 0)
                {
                    InicializaPassos(jaCaminhadoLocal);
                    P = P1;
                    sol = new List<Point> {P};
                    jaCaminhadoLocal[P.X, P.Y] = true;
                }
                else
                {
                    P = DecideProximoPonto(pontos, P2);
                    sol.Add(P);
                    jaCaminhadoLocal[P.X, P.Y] = true;
                }
            }
            return sol;
        }

        private void MarcarCaminho(ref List<Point> sol, Point P1)
        {
            foreach (Point P in sol)
                jaCaminhado[P.X, P.Y] = true;
            jaCaminhado[P1.X, P1.Y] = false;
        }

        private List<Point> DefineProximosPontos(Point P)
        {
            var pontos = new List<Point>();

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    bool ok = ((P.X + i < width - 1) && (P.X + i >= 0) && (P.Y + j < height) && (P.Y + j >= 0));
                    if (ok && (!jaCaminhadoLocal[P.X + i, P.Y + j]) )
                        pontos.Add(new Point(P.X + i, P.Y + j));
                }
            }

            return pontos;
        }

        private Point DecideProximoPonto(List<Point> pontos, Point P2)
        {
            var P = new Point();
            double probTotal = 0, prob = 0;
            var probs = new double[pontos.Count];
            for (int i = 0; i < pontos.Count; i++)
            {   
                double distanciax = (pontos[i].X - P2.X) * (pontos[i].X - P2.X);
                double distanciay = (pontos[i].Y - P2.Y) * (pontos[i].Y - P2.Y);
                double distanciachess = distanciax > distanciay ? distanciax : distanciay;
                //double distanciaeuclid = (distanciay + distanciax);
                
                probs[i] = (Math.Pow(feromonios[pontos[i].X, pontos[i].Y], alfa) * Math.Pow((matriz[pontos[i].X, pontos[i].Y] + 1), beta));
                if (distanciachess > p)
                    probs[i] *= Math.Pow(1 / distanciachess, gama);
                probTotal += probs[i];
            }

            if (probabilistico)
            {
                rand = new Random();
                double numeroRandom = rand.NextDouble();

                //sorteia um ponto entre os possíveis ponderado pela probabilidade
                for (int i = 0; i < pontos.Count; i++)
                {
                    P = pontos[i];
                    prob += probs[i];
                    if ((prob/probTotal) >= numeroRandom)
                        //intervalo de nro. random: [0..1). Se nro. random = 1, esta pegando o ultimo ponto da solucao.
                        break;

                }
            }
            else
            {
                //pega sempre o ponto de maior probabilidade
                P = pontos[0];
                prob = probs[0];
                for (int i = 1; i < pontos.Count; i++)
                {
                    if (probs[i] > prob)
                    {
                        P = pontos[i];
                        prob = probs[i];
                    }
                }
            }

            

            return P;
        }
    }
}
